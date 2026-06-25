using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using RaidersVault.Tests;
using Xunit;

namespace RaidersVault.Tests.Api;

public class LoginFlowTests : IClassFixture<RaidersVaultWebApplicationFactory>
{
    private readonly RaidersVaultWebApplicationFactory _factory;

    public LoginFlowTests(RaidersVaultWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DemoUserCanSignInAndReachCommandCenter()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var loginPage = await client.GetStringAsync("/Account/Login");
        var token = Regex.Match(loginPage, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")
            .Groups[1]
            .Value;

        Assert.False(string.IsNullOrWhiteSpace(token));

        var response = await client.PostAsync(
            "/Account/Login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["username"] = "admin",
                ["password"] = "password",
                ["__RequestVerificationToken"] = token
            }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Home/Index", response.Headers.Location?.OriginalString ?? string.Empty);
    }
}
