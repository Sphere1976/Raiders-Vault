using System.Net;
using System.Text.RegularExpressions;
using RaidersVault.ViewModels;

namespace RaidersVault.Services;

public class ArcRaidersLiveOpsService
{
    private const string OfficialHomeUrl = "https://arcraiders.com/";
    private const string OfficialMapConditionsUrl = "https://arcraiders.com/map-conditions";

    private readonly HttpClient _httpClient;
    private readonly ILogger<ArcRaidersLiveOpsService> _logger;

    public ArcRaidersLiveOpsService(
        HttpClient httpClient,
        ILogger<ArcRaidersLiveOpsService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ArcRaidersLiveOpsViewModel> BuildAsync()
    {
        var model = BuildFallback();

        try
        {
            var homeTask = ReadOfficialPageAsync(OfficialHomeUrl);
            var conditionsTask = ReadOfficialPageAsync(OfficialMapConditionsUrl);
            await Task.WhenAll(homeTask, conditionsTask);

            var newsItems = ParseNews(homeTask.Result);
            var conditions = ParseMapConditions(conditionsTask.Result);

            if (newsItems.Any())
            {
                model.NewsItems = newsItems;
            }

            if (conditions.Active.Any())
            {
                model.ActiveConditions = conditions.Active;
            }

            if (conditions.Upcoming.Any())
            {
                model.UpcomingConditions = conditions.Upcoming;
            }
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Using embedded ARC Raiders live ops fallback data.");
        }

        model.GeneratedAtUtc = DateTimeOffset.UtcNow;
        return model;
    }

    private async Task<string> ReadOfficialPageAsync(string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.UserAgent.ParseAdd("RaidersVault/1.0 (+local companion dashboard)");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
    }

    private static List<EmbarkNewsItem> ParseNews(string html)
    {
        var items = new List<EmbarkNewsItem>();
        var regex = new Regex(
            @"href=""(?<url>/news/[^""]+)""[^>]*>.*?(?<category>Store Update|Patch Notes|Update|News|Notes)[^<]*</[^>]+>\s*<[^>]+>\s*(?<title>[^<]+?)\s*</[^>]+>\s*<[^>]+>\s*(?<date>[A-Z][a-z]+\s+\d{1,2},\s+\d{4})",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        foreach (Match match in regex.Matches(html).Take(4))
        {
            items.Add(new EmbarkNewsItem
            {
                Category = Clean(match.Groups["category"].Value),
                Title = Clean(match.Groups["title"].Value),
                PublishedAt = Clean(match.Groups["date"].Value),
                Summary = "Official ARC Raiders update from Embark.",
                Url = $"https://arcraiders.com{match.Groups["url"].Value}"
            });
        }

        return items;
    }

    private static (List<LiveMapConditionItem> Active, List<LiveMapConditionItem> Upcoming) ParseMapConditions(string html)
    {
        var active = new List<LiveMapConditionItem>();
        var upcoming = new List<LiveMapConditionItem>();
        var compact = Regex.Replace(html, @"\s+", " ");

        foreach (var item in ParseConditionSection(compact, "Active now", "Coming up").Take(4))
        {
            item.Status = "Active";
            active.Add(item);
        }

        foreach (var item in ParseConditionSection(compact, "Coming up", "Embark Studios").Take(8))
        {
            item.Status = "Upcoming";
            upcoming.Add(item);
        }

        return (active, upcoming);
    }

    private static IEnumerable<LiveMapConditionItem> ParseConditionSection(
        string html,
        string startText,
        string endText)
    {
        var start = html.IndexOf(startText, StringComparison.OrdinalIgnoreCase);
        if (start < 0)
        {
            yield break;
        }

        var end = html.IndexOf(endText, start + startText.Length, StringComparison.OrdinalIgnoreCase);
        var section = end > start ? html[start..end] : html[start..];
        var regex = new Regex(
            @"(?<remaining>\d{1,2}:\d{2}(?::\d{2})?)\s+(?<condition>Close Scrutiny|Electromagnetic Storm|Hidden Bunker|Hurricane|Locked Gate|Night Raid|Beachcombing|Bird City|Harvester|Launch Tower Loot|Lush Blooms|Matriarch|Prospecting Probes)\s+(?<map>Buried City|Dam Battlegrounds|Riven Tides|Spaceport|Stella Montis|The Blue Gate)\s+(?<window>[A-Z][a-z]{2}\s+\d{1,2}\s+·\s+[^<]+?(?:AM|PM)\s+-\s+[^<]+?(?:AM|PM))",
            RegexOptions.IgnoreCase);

        foreach (Match match in regex.Matches(section))
        {
            yield return new LiveMapConditionItem
            {
                ConditionName = Clean(match.Groups["condition"].Value),
                MapName = Clean(match.Groups["map"].Value),
                TimeRemaining = Clean(match.Groups["remaining"].Value),
                Window = Clean(match.Groups["window"].Value),
                SourceUrl = OfficialMapConditionsUrl
            };
        }
    }

    private static ArcRaidersLiveOpsViewModel BuildFallback()
    {
        return new ArcRaidersLiveOpsViewModel
        {
            SourceName = "Official ARC Raiders",
            SourceUrl = OfficialHomeUrl,
            ActiveConditions = new List<LiveMapConditionItem>
            {
                new() { ConditionName = "Harvester", MapName = "The Blue Gate", TimeRemaining = "Live", Window = "Official conditions feed", Status = "Active" },
                new() { ConditionName = "Night Raid", MapName = "Dam Battlegrounds", TimeRemaining = "Live", Window = "Official conditions feed", Status = "Active" },
                new() { ConditionName = "Prospecting Probes", MapName = "Riven Tides", TimeRemaining = "Live", Window = "Official conditions feed", Status = "Active" }
            },
            UpcomingConditions = new List<LiveMapConditionItem>
            {
                new() { ConditionName = "Bird City", MapName = "Buried City", TimeRemaining = "Next", Window = "Official conditions feed", Status = "Upcoming" },
                new() { ConditionName = "Hidden Bunker", MapName = "Spaceport", TimeRemaining = "Next", Window = "Official conditions feed", Status = "Upcoming" },
                new() { ConditionName = "Hurricane", MapName = "Buried City", TimeRemaining = "Soon", Window = "Official conditions feed", Status = "Upcoming" }
            },
            NewsItems = new List<EmbarkNewsItem>
            {
                new() { Category = "Store Update", Title = "Store Update 1.32.0", PublishedAt = "June 9, 2026", Summary = "Macrame Set colour variants, Lob Bangs hairstyle, and Ermal offer rotation.", Url = "https://arcraiders.com/news/store-update-1-32-0" },
                new() { Category = "Store Update", Title = "Store Update 1.31.0", PublishedAt = "June 2, 2026", Summary = "Official weekly store update from Embark.", Url = "https://arcraiders.com/news/store-update-1-31-0" },
                new() { Category = "Store Update", Title = "Store Update 1.30.0", PublishedAt = "May 26, 2026", Summary = "Official weekly store update from Embark.", Url = "https://arcraiders.com/news/store-update-1-30-0" },
                new() { Category = "Notes", Title = "Notes on The Matchmaking System", PublishedAt = "May 20, 2026", Summary = "Official notes from Embark on matchmaking.", Url = "https://arcraiders.com/news/notes-on-the-matchmaking-system" }
            }
        };
    }

    private static string Clean(string value)
    {
        return WebUtility.HtmlDecode(value).Trim();
    }
}
