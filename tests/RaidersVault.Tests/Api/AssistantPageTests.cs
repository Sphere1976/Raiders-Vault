using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace RaidersVault.Tests.Api;

public class AssistantPageTests : IClassFixture<RaidersVaultWebApplicationFactory>
{
    private readonly RaidersVaultWebApplicationFactory _factory;

    public AssistantPageTests(RaidersVaultWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SignedInUserCanOpenAssistantPage()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var loginPage = await client.GetStringAsync("/Account/Login");
        var token = Regex.Match(loginPage, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")
            .Groups[1]
            .Value;

        var loginResponse = await client.PostAsync(
            "/Account/Login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["username"] = "admin",
                ["password"] = "password",
                ["__RequestVerificationToken"] = token
            }));

        Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);

        var assistantResponse = await client.GetAsync("/Assistant/Index");
        var assistantPage = await assistantResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, assistantResponse.StatusCode);
        Assert.Contains("Raiders Vault AI", assistantPage);
        Assert.Contains("data-ai-chat", assistantPage);
    }

    [Fact]
    public async Task AssistantUsesBlueprintDatabaseForSpecificBlueprintQuestions()
    {
        var originalApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        Environment.SetEnvironmentVariable("OPENAI_API_KEY", string.Empty);

        try
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var loginPage = await client.GetStringAsync("/Account/Login");
            var loginToken = Regex.Match(loginPage, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")
                .Groups[1]
                .Value;

            await client.PostAsync(
                "/Account/Login",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["username"] = "admin",
                    ["password"] = "password",
                    ["__RequestVerificationToken"] = loginToken
                }));

            var assistantPage = await client.GetStringAsync("/Assistant/Index");
            var chatToken = Regex.Match(assistantPage, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")
                .Groups[1]
                .Value;

            var request = new HttpRequestMessage(HttpMethod.Post, "/Assistant/Ask");
            request.Headers.Add("RequestVerificationToken", chatToken);
            request.Content = new StringContent(
                JsonSerializer.Serialize(new { message = "where can i get a dolabra blueprint", page = "Assistant" }),
                Encoding.UTF8,
                "application/json");

            var response = await client.SendAsync(request);
            var payload = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Dolabra", payload);
            Assert.Contains("Found Inside Assessors", payload);
            Assert.Contains("Close Scrutiny", payload);
        }
        finally
        {
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", originalApiKey);
        }
    }

    [Fact]
    public async Task AssistantUsesLoadoutEngineForPvpGunQuestions()
    {
        var originalApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        Environment.SetEnvironmentVariable("OPENAI_API_KEY", string.Empty);

        try
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var loginPage = await client.GetStringAsync("/Account/Login");
            var loginToken = Regex.Match(loginPage, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")
                .Groups[1]
                .Value;

            await client.PostAsync(
                "/Account/Login",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["username"] = "admin",
                    ["password"] = "password",
                    ["__RequestVerificationToken"] = loginToken
                }));

            var assistantPage = await client.GetStringAsync("/Assistant/Index");
            var chatToken = Regex.Match(assistantPage, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")
                .Groups[1]
                .Value;

            var request = new HttpRequestMessage(HttpMethod.Post, "/Assistant/Ask");
            request.Headers.Add("RequestVerificationToken", chatToken);
            request.Content = new StringContent(
                JsonSerializer.Serialize(new { message = "what is best PVP gun", page = "Assistant" }),
                Encoding.UTF8,
                "application/json");

            var response = await client.SendAsync(request);
            var payload = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Tempest", payload);
            Assert.Contains("Il Toro", payload);
            Assert.Contains("Heavy Shield", payload);
            Assert.DoesNotContain("Snap Hook", payload);
        }
        finally
        {
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", originalApiKey);
        }
    }

    [Fact]
    public async Task AssistantComparesPvpAndPveInsteadOfMatchingBetterQuest()
    {
        var payload = await AskAssistantWithoutOpenAiAsync("what's better PVP or PVE format");

        Assert.Contains("PvP vs PvE", payload);
        Assert.Contains("PvP is better", payload);
        Assert.Contains("PvE is better", payload);
        Assert.Contains("Tempest", payload);
        Assert.Contains("Renegade", payload);
        Assert.DoesNotContain("A Better Use", payload);
    }

    [Fact]
    public async Task AssistantHandlesFrustrationWithoutGameplayFallback()
    {
        var payload = await AskAssistantWithoutOpenAiAsync("you're dumb");

        Assert.Contains("missed the mark", payload);
        Assert.Contains("app data", payload);
        Assert.DoesNotContain("Snap Hook", payload);
        Assert.DoesNotContain("Angled Grip", payload);
    }

    [Fact]
    public async Task AssistantCanDescribeWholeAppCoverage()
    {
        var payload = await AskAssistantWithoutOpenAiAsync("what do you know about the entire app");

        Assert.Contains("Command Center", payload);
        Assert.Contains("Global Ops", payload);
        Assert.Contains("Blueprints", payload);
        Assert.Contains("Item Database", payload);
        Assert.Contains("External source index", payload);
    }

    [Fact]
    public async Task AssistantCanRouteMetaForgeQuestionsToExternalSourceIndex()
    {
        var payload = await AskAssistantWithoutOpenAiAsync("what metaforge tools can help with arc raiders");

        Assert.Contains("MetaForge ARC Raiders hub", payload);
        Assert.Contains("database", payload, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("maps", payload, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("https://metaforge.app/arc-raiders", payload);
    }

    [Fact]
    public async Task AssistantUsesRivenTidesRecordsForMapFeatureQuestions()
    {
        var payload = await AskAssistantWithoutOpenAiAsync("what is the best feature of Riven tides map");

        Assert.Contains("Beachcombing", payload);
        Assert.Contains("Dockmaster", payload);
        Assert.Contains("buried-loot", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Snap Hook", payload);
    }

    [Fact]
    public async Task AssistantUsesMapRiskDataForDangerousMapQuestions()
    {
        var payload = await AskAssistantWithoutOpenAiAsync("what map is the most dangerous");

        Assert.Contains("Most dangerous map", payload);
        Assert.Contains("danger score", payload);
        Assert.Contains("risk", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Snap Hook", payload);
    }

    [Fact]
    public async Task AssistantUsesArcThreatDataForDangerousArcQuestions()
    {
        var payload = await AskAssistantWithoutOpenAiAsync("whats the most dangerous arc");

        Assert.Contains("Most dangerous ARC threat", payload);
        Assert.Contains("Matriarch", payload);
        Assert.Contains("Leaper", payload);
        Assert.DoesNotContain("Most dangerous map", payload);
        Assert.DoesNotContain("Snap Hook", payload);
    }

    [Fact]
    public async Task AssistantSearchesQuestDataForObjectiveQuestions()
    {
        var payload = await AskAssistantWithoutOpenAiAsync("how do i complete mixed signals");

        Assert.Contains("Mixed Signals", payload);
        Assert.Contains("Surveyor", payload);
        Assert.Contains("Objectives", payload);
        Assert.DoesNotContain("Snap Hook", payload);
    }

    [Fact]
    public async Task AssistantSearchesTrialDataForWeeklyQuestions()
    {
        var payload = await AskAssistantWithoutOpenAiAsync("how should i do the damage leapers trial");

        Assert.Contains("Damage Leapers", payload);
        Assert.Contains("Weekly Trials", payload);
        Assert.Contains("open sightlines", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Snap Hook", payload);
    }

    [Fact]
    public async Task AssistantSearchesSkillDataForSkillQuestions()
    {
        var payload = await AskAssistantWithoutOpenAiAsync("what skill helps with wasps and turrets");

        Assert.Contains("Flyswatter", payload);
        Assert.Contains("Skills", payload);
        Assert.Contains("Wasps and Turrets", payload);
        Assert.DoesNotContain("Snap Hook", payload);
    }

    [Fact]
    public async Task AssistantMatchesPluralItemQuestions()
    {
        var payload = await AskAssistantWithoutOpenAiAsync("where do i find batteries");

        Assert.Contains("Battery", payload);
        Assert.Contains("Research/Admin shelves", payload);
        Assert.Contains("Item Database", payload);
    }

    [Fact]
    public async Task AssistantExplainsAppPagesWhenAsked()
    {
        var payload = await AskAssistantWithoutOpenAiAsync("what can i do on the map conditions page");

        Assert.Contains("Map Conditions", payload);
        Assert.Contains("condition-aware loadouts", payload);
        Assert.Contains("app area", payload, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AssistantSummarizesCurrentPriorities()
    {
        var payload = await AskAssistantWithoutOpenAiAsync("what should i focus on next");

        Assert.Contains("Current app-grounded priorities", payload);
        Assert.Contains("Inventory priority", payload);
        Assert.Contains("Blueprint", payload);
        Assert.DoesNotContain("Ask me for a farm route", payload);
    }

    private async Task<string> AskAssistantWithoutOpenAiAsync(string message)
    {
        var originalApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        Environment.SetEnvironmentVariable("OPENAI_API_KEY", string.Empty);

        try
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var loginPage = await client.GetStringAsync("/Account/Login");
            var loginToken = Regex.Match(loginPage, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")
                .Groups[1]
                .Value;

            await client.PostAsync(
                "/Account/Login",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["username"] = "admin",
                    ["password"] = "password",
                    ["__RequestVerificationToken"] = loginToken
                }));

            var assistantPage = await client.GetStringAsync("/Assistant/Index");
            var chatToken = Regex.Match(assistantPage, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")
                .Groups[1]
                .Value;

            var request = new HttpRequestMessage(HttpMethod.Post, "/Assistant/Ask");
            request.Headers.Add("RequestVerificationToken", chatToken);
            request.Content = new StringContent(
                JsonSerializer.Serialize(new { message, page = "Assistant" }),
                Encoding.UTF8,
                "application/json");

            var response = await client.SendAsync(request);
            var payload = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return payload;
        }
        finally
        {
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", originalApiKey);
        }
    }
}
