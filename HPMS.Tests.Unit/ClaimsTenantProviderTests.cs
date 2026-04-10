using System.Security.Claims;
using FluentAssertions;
using HPMS.Web.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace HPMS.Tests.Unit;

public class ClaimsTenantProviderTests
{
    [Fact]
    public void GetTenantId_ShouldReturnCorrectGuid_WhenClaimIsPresent()
    {
        // Arrange: Create a fake tenant ID and simulate a user with that claim
        var expectedTenantId = Guid.NewGuid();
        
        // Simulate a logged-in user with a "TenantId" claim
        var claims = new List<Claim> 
        { 
            new Claim("TenantId", expectedTenantId.ToString()) 
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Mock HttpContextAccessor to return our fake user
        var mockAccessor = new Mock<IHttpContextAccessor>();
        var mockContext = new DefaultHttpContext { User = principal };
        mockAccessor.Setup(x => x.HttpContext).Returns(mockContext);

        var provider = new ClaimsTenantProvider(mockAccessor.Object);

        // Act
        var result = provider.GetTenantId();

        // Assert
        result.Should().Be(expectedTenantId);
    }

    [Fact]
    public void GetTenantId_ShouldReturnEmptyGuid_WhenClaimIsMissing()
    {
        // Arrange: No claims
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        var mockAccessor = new Mock<IHttpContextAccessor>();
        var mockContext = new DefaultHttpContext { User = principal };
        mockAccessor.Setup(x => x.HttpContext).Returns(mockContext);

        var provider = new ClaimsTenantProvider(mockAccessor.Object);

        // Act
        var result = provider.GetTenantId();

        // Assert
        result.Should().Be(Guid.Empty);
    }
}