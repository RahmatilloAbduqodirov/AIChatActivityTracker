using AIChatActivityTracker.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AIChatActivityTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService;

    private static readonly ChatHistory _chatHistory = new(
        "You are a helpful activity management assistant. " +
        "You help users create, view, update, and delete their scheduled activities. " +
        "When users ask about activities, use the available functions to interact with the activity system. " +
        "Be concise and friendly in your responses.");

    public ChatController(Kernel kernel, IChatCompletionService chatCompletionService)
    {
        _kernel = kernel;
        _chatCompletionService = chatCompletionService;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponse>> Chat([FromBody] ChatRequest request)
    {
        _chatHistory.AddUserMessage(request.Message);

        var executionSettings = new PromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var response = await _chatCompletionService.GetChatMessageContentAsync(
            _chatHistory,
            executionSettings,
            _kernel);

        _chatHistory.AddAssistantMessage(response.Content ?? string.Empty);

        return Ok(new ChatResponse { Reply = response.Content ?? string.Empty });
    }
}
