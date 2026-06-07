using HPMS.Modules.Billing.Data;
using HPMS.Modules.Billing.Entities;
using HPMS.SharedKernel.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace HPMS.Modules.Billing;

public static class BillingModule
{
    public static IEndpointRouteBuilder MapBillingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/billing")
            .WithTags("Billing")
            .RequireAuthorization(HpmsPolicies.BillingStaff);

        // Get all Invoices
        group.MapGet("/invoices", async (BillingDbContext db) => 
            await db.Invoices.ToListAsync());

        // Get the Ledger (Audit Trail)
        group.MapGet("/ledger", async (BillingDbContext db) => 
            await db.FinancialLedgers
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync());

        // Record Payment (Transition from Pending to Paid)
        group.MapPost("/invoices/{id:guid}/pay", async (Guid id, BillingDbContext db) =>
        {
            var invoice = await db.Invoices.FindAsync(id);
            if (invoice is null) return Results.NotFound();

            if (invoice.Status == InvoiceStatus.Paid) 
                return Results.BadRequest("Invoice is already paid.");

            invoice.Status = InvoiceStatus.Paid;

            // Add a Credit entry to the ledger to balance the original Debit
            db.FinancialLedgers.Add(new FinancialLedger
            {
                TenantId = invoice.TenantId,
                InvoiceId = invoice.Id,
                Amount = invoice.Amount,
                Type = "Credit", // Payment received
                CreatedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
            return Results.Ok(new { Message = "Payment recorded and ledger updated." });
        });
        
        // Billing Summary (For Billing Managers)
        group.MapGet("/summary/revenue", async (BillingDbContext db) =>
        {
            var last7Days = DateTime.UtcNow.AddDays(-7).Date;

            // Aggregate revenue by day for the chart
            var chartData = await db.FinancialLedgers
                .Where(l => l.CreatedAt >= last7Days && l.Type == "Debit")
                .GroupBy(l => l.CreatedAt.Date)
                .Select(g => new { 
                    Date = g.Key.ToString("MM-dd"), 
                    Amount = g.Sum(x => x.Amount) 
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            var totalToday = chartData.FirstOrDefault(x => x.Date == DateTime.UtcNow.ToString("MM-dd"))?.Amount ?? 0;

            return Results.Ok(new
            {
                TodayRevenue = totalToday,
                ChartLabels = chartData.Select(x => x.Date),
                ChartValues = chartData.Select(x => x.Amount)
            });
        });

        return endpoints;
    }
}