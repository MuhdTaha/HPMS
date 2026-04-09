using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using HPMS.Scheduling.Data;
using HPMS.Scheduling.Entities;
using HPMS.Scheduling.Services;
using HPMS.SharedKernel.Interfaces;

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
        });
        
        // UC-02: Update Appointment Status (The State Machine)
        group.MapPatch("/appointments/{id:guid}/status", async (
            Guid id, 
            UpdateAppointmentStatusDto dto, 
            SchedulingDbContext db) => 
        {
            // 1. Find the appointment
            var appointment = await db.Appointments
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment is null)
                return Results.NotFound();

            // 2. Validate the transition (Basic logic)
            if (appointment.Status == AppointmentStatus.Completed || 
                appointment.Status == AppointmentStatus.Canceled)
            {
                return Results.BadRequest("Cannot change the status of a terminal appointment.");
            }

            // 3. Update the state
            var oldStatus = appointment.Status;
            appointment.Status = (AppointmentStatus)dto.NewStatus;

            // 4. Save changes
            await db.SaveChangesAsync();

            // 5. TODO: Trigger Billing Event if status is 'Completed'
            if (appointment.Status == AppointmentStatus.Completed)
            {
                // This is where Phase 3 (Event-Driven Architecture) begins!
            }

            return Results.Ok(new { Message = $"Status updated from {oldStatus} to {appointment.Status}" });
        })
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
        .WithName("DeleteAppointment");

        // Helper endpoint for the CreatedAtRoute in your Post method
        group.MapGet("/appointments/{id:guid}", async (Guid id, SchedulingDbContext db) =>
        {
            var appointment = await db.Appointments
                .FirstOrDefaultAsync(a => a.Id == id);
            return appointment is not null ? Results.Ok(appointment) : Results.NotFound();
        })
        .WithName("GetAppointment");
        
        // --- PATIENT ENDPOINTS ---
        
        // Register a new patient
        group.MapPost("/patients", async (PatientDto dto, SchedulingDbContext db, ITenantProvider tenantProvider) =>
        {
            var patient = new Patient
            {
                TenantId = tenantProvider.GetTenantId(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                DateOfBirth = dto.DateOfBirth.ToDateTime(TimeOnly.MinValue),
                IsDeleted = false // Ensure soft-delete is initialized to false
            };

            db.Patients.Add(patient);
            await db.SaveChangesAsync();

            return Results.Created($"/scheduling/patients/{patient.Id}", patient);
        })
        .WithName("CreatePatient");

        // Get all patients for the current clinic (filtered by TenantId)
        group.MapGet("/patients", async (SchedulingDbContext db) => 
        {
            return await db.Patients.ToListAsync();
        })
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
        .WithName("DeletePatient");
        
        return endpoints;
    }
}