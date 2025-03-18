using nova_log.Logic;
using nova_log.Models;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nova_log.DataAccess
{
    public class OpenAIService
    {
        private ChatClient _chatClient;

        public OpenAIService()
        {
            _chatClient = new(model: SecretsConfiguration.GetOpenAIModel(), apiKey: SecretsConfiguration.GetOpenAIKey());
        }

        public async Task<string> SendIntroPrompt(List<ChatMessage> chatMessage)
        {
            try
            {
                ChatCompletion completion = await _chatClient.CompleteChatAsync(chatMessage);
                return completion.Content[0].Text;

            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ChatCompletion> SendChatPrompt(List<ChatMessage> chatHistory, ChatCompletionOptions options)
        {
            try
            {
                return await _chatClient.CompleteChatAsync(chatHistory, options);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> SendChatPrompt(List<ChatMessage> chatHistory)
        {
            try
            {
                ChatCompletion completion = await _chatClient.CompleteChatAsync(chatHistory);
                return completion.Content[0].Text;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> SendChatPromptReturnString(List<ChatMessage> chatHistory, ChatCompletionOptions options)
        {
            try
            {
                ChatCompletion completion = await _chatClient.CompleteChatAsync(chatHistory, options);
                return completion.Content[0].Text;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
