using FluentAssertions;
using HPMS.Modules.Billing.Entities;

namespace HPMS.Tests.Unit;

public class BillingLogicTests
{
    [Fact]
    public void NewInvoice_ShouldDefaultTo_PendingStatus()
    {
        // Arrange & Act
        var invoice = new Invoice();

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Pending);
        invoice.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void LedgerEntry_ShouldDefaultTo_UtcNow()
    {
        // Arrange & Act
        var ledger = new FinancialLedger();

        // Assert
        ledger.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}