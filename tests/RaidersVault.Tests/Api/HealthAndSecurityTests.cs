using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using RaidersVault.Tests;
using Xunit;

namespace RaidersVault.Tests.Api;

public class HealthAndSecurityTests : IClassFixture<RaidersVaultWebApplicationFactory>
{
    private readonly RaidersVaultWebApplicationFactory _factory;

    public HealthAndSecurityTests(RaidersVaultWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthEndpointReturnsSuccess()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GlobalOpsApiRequiresAuthentication()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/api/v1/global-ops");

        Assert.True(
            response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Redirect,
            $"Expected unauthorized or redirect, got {(int)response.StatusCode}.");
    }

    [Fact]
    public async Task LoginPageIncludesSecurityHeaders()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/Account/Login");

        Assert.True(response.Headers.Contains("X-Frame-Options"));
        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
    }
}
