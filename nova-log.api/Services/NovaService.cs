using nova_log.Logic;
using nova_log.Models;
using OpenAI.Chat;

namespace nova_log.api.Service
{
    public class NovaService
    {
        private readonly PromptHandler _promptHandler;

        public NovaService()
        {
            _promptHandler = new PromptHandler();
        }


        /// <summary>
        /// Process the full conversation history provided by the client and generate a new assistant message.
        /// </summary>
        /// <param name="chatHistory">The entire conversation from client side.</param>
        /// <returns>A string representing the assistant's response.</returns>
        public async Task<RequestModel> ProcessMainChatAsync(RequestModel clientRequest)
        {
            return await _promptHandler.SendChatHistoryWithTools(clientRequest);
        }

        /// <summary>
        /// Generate introduction assistant message.
        /// </summary>
        /// <param name="chatHistory">The entire conversation from client side.</param>
        /// <returns>A string representing the assistant's response.</returns>
        public async Task<RequestModel> ProcessIntroChatAsync()
        {
            return await _promptHandler.SendChatIntroPrompt();
        }
    }
}
