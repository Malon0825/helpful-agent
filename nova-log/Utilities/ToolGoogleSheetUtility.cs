using nova_log.DataAccess;
using nova_log.Logic;
using nova_log.Models;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace nova_log.Utilities
{
    public class ToolGoogleSheetUtility
    {
        public async Task GetGoogleSheetTask(List<ChatMessage> chatHistory, ChatToolCall toolCall)
        {
            chatHistory.Add(new ToolChatMessage(toolCall.Id, "Tool call to get all task in google sheet."));
            chatHistory.Add(new SystemChatMessage($"Getting properties on JsonDocument RootElement"));

            using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);
            bool hasSpreadsheetId = argumentsJson.RootElement.TryGetProperty("spreadSheetId", out JsonElement spreadSheetId);
            bool hasSheetName = argumentsJson.RootElement.TryGetProperty("sheetName", out JsonElement sheetName);
            bool hasSheetRangeFrom = argumentsJson.RootElement.TryGetProperty("sheetRangeFrom", out JsonElement sheetRangeFrom);
            bool hasSheetRangeTo = argumentsJson.RootElement.TryGetProperty("sheetRangeTo", out JsonElement sheetRangeTo);

            var gSheetId = spreadSheetId.GetString();
            var gSheetName = sheetName.GetString();
            var gSheetFrom = sheetRangeFrom.GetString();
            var gSheetTo = sheetRangeTo.GetString();

            chatHistory.Add(new SystemChatMessage($"Sheet Id: {gSheetId}, Sheet Name: {gSheetName}"));

            if (!hasSpreadsheetId || !hasSheetName || !hasSheetRangeFrom || !hasSheetRangeTo)
            {
                chatHistory.Add(new SystemChatMessage("Missing required arguments: spreadSheetId, sheetName, sheetRangeFrom, or sheetRangeTo."));
                string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
                return;
            }

            try
            {
                AgentTask agentTask = new();
                var result = await agentTask.GetGoogleSheetTask(
                    sheetRangeFrom.GetString(),
                    sheetRangeTo.GetString(),
                    spreadSheetId.GetString(),
                    sheetName.GetString()
                );

                chatHistory.Add(new SystemChatMessage($"Fetched data from google sheet."));

                if (result.Item2 != null)
                {
                    chatHistory.Add(new SystemChatMessage($"An error occurred: {result.Item2.Message}"));
                    string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                    chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
                }
                else
                {
                    string taskJson = ConvertionUtility.ConvertToJson(result.Item1);
                    chatHistory.Add(new SystemChatMessage($"Task list has been retrieved from google sheet: {taskJson}"));
                }

                return;
            }
            catch (Exception ex)
            {
                chatHistory.Add(new SystemChatMessage($"Give a user friendly message about what might be caussing the error: {ex}"));
                string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
                return;
            }
        }

        public async Task CreateGoogleSheetTask(List<ChatMessage> chatHistory, ChatToolCall toolCall, ChatCompletionOptions googleSheetStructuredResponse)
        {
            chatHistory.Add(new ToolChatMessage(toolCall.Id, "Tool create task in google sheet."));
            using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);

            bool hasSpreadsheetId = argumentsJson.RootElement.TryGetProperty("spreadSheetId", out JsonElement spreadSheetId);
            bool hasSheetName = argumentsJson.RootElement.TryGetProperty("sheetName", out JsonElement sheetName);
            bool hasSheetdata = argumentsJson.RootElement.TryGetProperty("sheetName", out JsonElement sheetPreviousdata);

            if (!hasSpreadsheetId || !hasSheetName || !hasSheetdata)
            {
                chatHistory.Add(new SystemChatMessage($"Missing required arguments: spreadSheetId or sheetName."));
                string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
                return;
            }

            try
            {
                chatHistory.Add(new SystemChatMessage(sheetPreviousdata.GetString()));
                string jsonArguments = ConvertionUtility.ConvertToJson(argumentsJson);
                chatHistory.Add(new SystemChatMessage(jsonArguments));
                chatHistory.Add(new SystemChatMessage(OpenAIPromptModel.GenerateJsonTaskSystemInstruction()));
                string structuredResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                chatHistory.Add(new AssistantChatMessage(structuredResponse));

                DataTable taskList = ConvertionUtility.ConvertJsonToDynamicDataTable(structuredResponse);

                AgentTask agent = new();
                bool isTaskInserted = await agent.CreateGoogleSheetTask(taskList, spreadSheetId.GetString(), sheetName.GetString());

                if (isTaskInserted)
                {
                    chatHistory.Add(new SystemChatMessage("Task has been inserted to google sheet."));
                    return;
                }
                else
                {
                    chatHistory.Add(new SystemChatMessage("Failed to insert task."));
                    return;
                }
            }
            catch (Exception ex)
            {
                chatHistory.Add(new SystemChatMessage($"Give a user friendly message about what might be caussing the error: {ex}"));
                string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
                return;
            }
        }

        public async Task GetCurrentDate(List<ChatMessage> chatHistory, ChatToolCall toolCall)
        {
            chatHistory.Add(new ToolChatMessage(toolCall.Id, "Tool call to get current date."));
            using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);

            try
            {
                string jsonArguments = ConvertionUtility.ConvertToJson(argumentsJson);
                chatHistory.Add(new SystemChatMessage(jsonArguments));
                chatHistory.Add(new SystemChatMessage("Processing GetCurrentDateTool."));

                string currentDate = AgentTask.GetCurrentDate();
                chatHistory.Add(new SystemChatMessage(currentDate));
            }
            catch (Exception ex)
            {
                chatHistory.Add(new SystemChatMessage($"Error in GetCurrentDateTool implementation: {ex.Message}"));
                string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
            }
        }
    }
}
