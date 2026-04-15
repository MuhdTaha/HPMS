using MediatR;
using HPMS.SharedKernel.Events;
using HPMS.Modules.Billing.Data;
using HPMS.Modules.Billing.Entities;

namespace HPMS.Modules.Billing.Handlers;

// This class "listens" for the AppointmentCompletedEvent
public class CreateInvoiceOnAppointmentCompleted(BillingDbContext db) 
    : INotificationHandler<AppointmentCompletedEvent>
{
    public async Task Handle(AppointmentCompletedEvent notification, CancellationToken ct)
    {
        // 1. Create a new Invoice based on the details from the completed appointment
        var invoice = new Invoice
        {
            TenantId = notification.TenantId,
            AppointmentId = notification.AppointmentId,
            PatientId = notification.PatientId,
            Amount = notification.VisitFee,
            Status = InvoiceStatus.Pending, // Initial state from Enum
        };
        
        // 2. Create Ledger Entry (Immutable Record)
        var ledgerEntry = new FinancialLedger
        {
            TenantId = notification.TenantId,
            InvoiceId = invoice.Id,
            Amount = notification.VisitFee,
            Type = "Debit", // Debit for the patient
            CreatedAt = DateTime.UtcNow
        };

        // 3. Save both the Invoice and the Ledger Entry to the database
        db.Invoices.Add(invoice);
        db.FinancialLedgers.Add(ledgerEntry);
        
        // This saves the invoice to the 'Invoices' table in the Billing database
        await db.SaveChangesAsync(ct);
        
        // Log it to the console so you can see it working during testing
        Console.WriteLine($"[EVENT] Billing Module: Created Invoice {invoice.Id} for Appointment {notification.AppointmentId}");
    }
}