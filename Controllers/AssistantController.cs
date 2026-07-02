using Microsoft.AspNetCore.Mvc;
using RaidersVault.Services;

namespace RaidersVault.Controllers;

public class AssistantController : BaseController
{
    private readonly AiChatService _aiChatService;

    public AssistantController(AiChatService aiChatService)
    {
        _aiChatService = aiChatService;
    }

    public IActionResult Index()
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Ask([FromBody] AiChatPromptRequest request, CancellationToken cancellationToken)
    {
        if (!IsLoggedIn())
        {
            return Unauthorized(new { reply = "Please sign in before using Raiders Vault AI." });
        }

        var message = InputSanitizer.Clean(request.Message);
        if (string.IsNullOrWhiteSpace(message))
        {
            return BadRequest(new { reply = "Ask a question first." });
        }

        if (message.Length > 1200)
        {
            return BadRequest(new { reply = "Keep chat prompts under 1,200 characters." });
        }

        var result = await _aiChatService.AskAsync(
            message,
            request.Page,
            HttpContext.Session.GetString("User"),
            cancellationToken);

        return Json(result);
    }
}

public sealed class AiChatPromptRequest
{
    public string? Message { get; set; }

    public string? Page { get; set; }
}
