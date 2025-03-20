using Google.Apis.Sheets.v4.Data;
using nova_log.DataAccess;
using nova_log.Models;
using nova_log.Utilities;
using OpenAI.Assistants;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Google.Apis.Requests.BatchRequest;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace nova_log.Logic
{
    public class PromptHandler
    {
        private readonly ChatCompletionOptions _options;
        private OpenAIService _openAIService;
        private NovaCoreUtility _promptUtility;
        public PromptHandler()
        {
            _openAIService = new OpenAIService();
            _promptUtility = new NovaCoreUtility();


            _options = new ChatCompletionOptions();
            _options.Tools.Add(AgentTool.GetGoogleSheetTask);
            _options.Tools.Add(AgentTool.CreateGoogleSheetTask);
            //_options.Tools.Add(AgentTool.GetGithubProjectFieldId);
            //_options.Tools.Add(AgentTool.GetGithubRepoId);
            //_options.Tools.Add(AgentTool.CreateGithubIssue);
            //_options.Tools.Add(AgentTool.GetGithubAssigneeId);
            //_options.Tools.Add(AgentTool.AddIssueToProject);
            //_options.Tools.Add(AgentTool.UpdateAssignee);
        }

        public async Task<List<ChatMessage>> SendChatHistoryWithTools(List<ChatMessage> chatHistory)
        {
            try
            {
                chatHistory = await _promptUtility.EvaluateAgentResponseWithLoop(chatHistory, _options);
            }
            catch (Exception ex)
            {
                chatHistory.Add(new SystemChatMessage(ex.Message));
                string agentResponse = await _openAIService.SendUtilityChatPrompt(ex.Message);
                chatHistory.Add(new AssistantChatMessage(agentResponse));
            }
            return chatHistory;
        }

        public async Task<List<ChatMessage>> SendChatIntroPrompt(List<ChatMessage> chatHistory)
        {
            try
            {
                chatHistory.Add(new SystemChatMessage(OpenAIPromptModel.IntroductionSystemInstruction()));
                string agentResponse = await _openAIService.SendIntroPrompt(chatHistory);
                chatHistory.Add(new AssistantChatMessage(agentResponse));             
            }
            catch (Exception ex)
            {
                chatHistory.Add(new SystemChatMessage(ex.Message));
                string agentResponse = await _openAIService.SendUtilityChatPrompt(ex.Message);
                chatHistory.Add(new AssistantChatMessage(agentResponse));
            }
            return chatHistory;
        }

        //public async Task<RequestModel> SendChatHistoryWithTools(RequestModel clientRequest)
        //{
        //    try
        //    {
        //        OpenAIService openAIService = new();
        //        NovaCoreUtility promptUtility = new();

        //        List<ChatMessage> chatHistory = ConvertionUtility.ConvertJsonToChatHistory(clientRequest.ChatHistory);

        //        chatHistory.Add(new UserChatMessage(clientRequest.UserPrompt));
        //        chatHistory = await promptUtility.EvaluateAgentResponseWithLoop(chatHistory, _options);
        //        clientRequest.ChatHistory = ConvertionUtility.ConvertChatHistoryToJson(chatHistory);
        //        clientRequest.AgentResponse = chatHistory.Last().Content[0].Text;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //    return clientRequest;
        //}

        //public async Task<RequestModel> SendChatIntroPrompt()
        //{
        //    List<ChatMessage> chatHistory = new();
        //    OpenAIService openAIService = new();
        //    RequestModel clientRequest = new();
        //    try
        //    {
        //        chatHistory.Add(new SystemChatMessage(OpenAIPromptModel.IntroductionSystemInstruction()));

        //        string agentResponse = await openAIService.SendIntroPrompt(chatHistory);
        //        chatHistory.Add(new AssistantChatMessage(agentResponse));

        //        clientRequest.ChatHistory = ConvertionUtility.ConvertChatHistoryToJson(chatHistory);
        //        clientRequest.AgentResponse = chatHistory.Last().Content[0].Text;
        //        return clientRequest;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }

        //}

        public async Task<string> SendUserPrompt(List<ChatMessage> chatHitory)
        {
            try
            {
                OpenAIService openAIService = new();
                return await openAIService.SendChatPrompt(chatHitory);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
    }
}
