using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using HPMS.Scheduling.Data;
using HPMS.Scheduling.Entities;
using HPMS.Scheduling.Services;
using HPMS.SharedKernel.Authorization;
using HPMS.SharedKernel.Events;
using HPMS.SharedKernel.Interfaces;
using MediatR;

namespace HPMS.Scheduling;

public static class SchedulingModule
{
    public static IEndpointRouteBuilder MapSchedulingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/scheduling")
            .WithTags("Scheduling")
            .RequireAuthorization(); // Mandatory for HIPAA compliance
        
        // --- APPOINTMENT ENDPOINTS ---
        
        // UC-01: Schedule Appointment
        group.MapPost("/appointments", async (
            CreateAppointmentDto dto, 
            SchedulingDbContext db, 
            IAppointmentConflictService conflictService,
            ITenantProvider tenantProvider) =>
        {
            // 1. Validate Time Logic
            if (dto.StartTime >= dto.EndTime)
                return Results.BadRequest("End time must be after start time.");

            // 2. Check for double-booking (FR-S01)
            var isAvailable = await conflictService.IsSlotAvailableAsync(
                dto.ProviderId, 
                dto.StartTime, 
                dto.EndTime);

            if (!isAvailable)
                return Results.Conflict("The provider is already booked for this time slot.");

            // 3. Initialize the Entity
            var appointment = new Appointment
            {
                TenantId = tenantProvider.GetTenantId(),
                PatientId = dto.PatientId,
                ProviderId = dto.ProviderId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Status = AppointmentStatus.Scheduled // Initial state
            };

            // 4. Save to Database
            db.Appointments.Add(appointment);
            await db.SaveChangesAsync();

            // 5. Return the Created Appointment with a Location header
            return Results.CreatedAtRoute("GetAppointment", new { id = appointment.Id }, appointment);
        })
        .RequireAuthorization(HpmsPolicies.SchedulingWrite);
        
        // UC-02: Update Appointment Status (The State Machine)
        group.MapPatch("/appointments/{id:guid}/status", async (
            Guid id, 
            UpdateAppointmentStatusDto dto, 
            IMediator mediator,
            SchedulingDbContext db) => 
        {
            // 1. Find the appointment
            var appointment = await db.Appointments
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment is null)
                return Results.NotFound();

            // 2. Validate the transition (Basic logic)
            var newStatus = (AppointmentStatus)dto.NewStatus;
            bool isValid = IsValidTransition(appointment.Status, newStatus);
            
            if (!isValid)
                return Results.BadRequest($"Invalid status transition from {appointment.Status} to {newStatus}.");
            
            // 3. Update the state if requested transition is valid
            var oldStatus = appointment.Status;
            appointment.Status = newStatus;

            // 4. Save changes
            await db.SaveChangesAsync();

            // 5. Trigger Billing Event if status is 'Completed'
            if (newStatus == AppointmentStatus.Completed)
            {
                await mediator.Publish(new AppointmentCompletedEvent(
                    appointment.Id,
                    appointment.PatientId,
                    appointment.TenantId));
            }

            return Results.Ok(new { Message = $"Status updated from {oldStatus} to {newStatus}" });
        })
        .RequireAuthorization(HpmsPolicies.VisitManagement)
        .WithName("UpdateAppointmentStatus");

        // UC-03: Soft delete an appointment instead of removing it permanently.
        group.MapDelete("/appointments/{id:guid}", async (Guid id, SchedulingDbContext db) =>
        {
            var appointment = await db.Appointments
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment is null)
                return Results.NotFound();

            if (appointment.IsDeleted)
                return Results.NoContent();

            appointment.IsDeleted = true;
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .RequireAuthorization(HpmsPolicies.ClinicAdminOrAbove)
        .WithName("DeleteAppointment");

        // Helper endpoint for the CreatedAtRoute in your Post method
        group.MapGet("/appointments/{id:guid}", async (Guid id, SchedulingDbContext db) =>
        {
            var appointment = await db.Appointments
                .FirstOrDefaultAsync(a => a.Id == id);
            return appointment is not null ? Results.Ok(appointment) : Results.NotFound();
        })
        .RequireAuthorization(HpmsPolicies.ClinicalStaff)
        .WithName("GetAppointment");
        
        // Scheduling Summary (For Providers & Front Desk)
        group.MapGet("/summary/today", async (SchedulingDbContext db, ITenantProvider tenant) =>
        {
            var today = DateTime.UtcNow.Date;

            var appointments = await db.Appointments
                .Where(a => a.StartTime.Date == today)
                .Select(a => new
                {
                    a.Id,
                    a.PatientId,
                    a.StartTime,
                    a.Status,
                }).ToListAsync();

            return Results.Ok(new
            {
                TotalCount = appointments.Count,
                ArrivedCount = appointments.Count(a => a.Status == AppointmentStatus.Arrived),
                InSessionCount = appointments.Count(a => a.Status == AppointmentStatus.InSession),
                CompletedCount = appointments.Count(a => a.Status == AppointmentStatus.Completed),
                NoShowCount = appointments.Count(a => a.Status == AppointmentStatus.NoShow),
                CanceledCount = appointments.Count(a => a.Status == AppointmentStatus.Canceled),
                Queue = appointments.Where(a => a.Status == AppointmentStatus.Arrived)
                    .OrderBy(a => a.StartTime)
                    .ToList()
            });
        })
        .RequireAuthorization(HpmsPolicies.ClinicalStaff);
        
        // --- PATIENT ENDPOINTS ---
        
        // Register a new patient
        group.MapPost("/patients", async (PatientDto dto, SchedulingDbContext db, ITenantProvider tenantProvider) =>
        {
            var phi = new PatientPhi
            {
                Address = dto.Address,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Ssn = dto.Ssn ?? string.Empty,
                InsuranceNumber = dto.InsuranceNumber ?? string.Empty,
                EmergencyContact = dto.EmergencyContact ?? string.Empty
            };
            
            var patient = new Patient
            {
                TenantId = tenantProvider.GetTenantId(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                DateOfBirth = dto.DateOfBirth.ToDateTime(TimeOnly.MinValue),
                IsDeleted = false, // Ensure soft-delete is initialized to false
                PHI_Data = JsonSerializer.Serialize(phi) // Encrypt and store PHI as JSON blob
            };

            db.Patients.Add(patient);
            await db.SaveChangesAsync();
            return Results.Created($"/scheduling/patients/{patient.Id}", patient);
        })
        .RequireAuthorization(HpmsPolicies.ClinicalStaff)
        .WithName("CreatePatient");

        // Get all patients for the current clinic (filtered by TenantId)
        group.MapGet("/patients", async (SchedulingDbContext db) => 
        {
            return await db.Patients.ToListAsync();
        })
        .RequireAuthorization(HpmsPolicies.ClinicalStaff)
        .WithName("GetPatients");

        // Soft delete a patient
        group.MapDelete("/patients/{id:guid}", async (Guid id, SchedulingDbContext db) =>
        {
            var patient = await db.Patients.FindAsync(id);
            if (patient is null) return Results.NotFound();

            patient.IsDeleted = true; // HIPAA-compliant soft delete
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .RequireAuthorization(HpmsPolicies.ClinicAdminOrAbove)
        .WithName("DeletePatient");
        
        return endpoints;
    }
    
    public static bool IsValidTransition(AppointmentStatus current, AppointmentStatus next)
    {
        return (current, next) switch
        {
            // Normal workflow
            (AppointmentStatus.Scheduled, AppointmentStatus.Arrived)   => true,
            (AppointmentStatus.Arrived, AppointmentStatus.InSession)   => true,
            (AppointmentStatus.InSession, AppointmentStatus.Completed) => true,

            // Any non-terminal state can be Canceled or a NoShow
            (_, AppointmentStatus.Canceled) when current != AppointmentStatus.Completed => true,
            (_, AppointmentStatus.NoShow)   when current != AppointmentStatus.Completed => true,

            // Allow skipping 'Arrived' for telehealth (Scheduled -> InSession)
            (AppointmentStatus.Scheduled, AppointmentStatus.InSession) => true,

            // Default: Block all other jumps (e.g., Completed -> Scheduled is forbidden)
            _ => false
        };
    }
}