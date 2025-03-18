using Microsoft.AspNetCore.Mvc;
using nova_log.api.Service;
using nova_log.Models;
using OpenAI.Chat;

namespace nova_log.api.Controllers
{
    [Route("[controller]")]
    public class AgentTasksController : ControllerBase
    {
        private readonly NovaService _chatProcessingService;

        public AgentTasksController(NovaService chatProcessingService)
        {
            _chatProcessingService = chatProcessingService;
        }

        [HttpPost("AgentNova")]
        public async Task<IActionResult> Chat([FromBody] RequestModel request)
        {
            if (request.ChatHistory == null || request.UserPrompt == string.Empty)
                return Ok(await _chatProcessingService.ProcessIntroChatAsync());

            return Ok(await _chatProcessingService.ProcessMainChatAsync(request));
        }
    }
}
