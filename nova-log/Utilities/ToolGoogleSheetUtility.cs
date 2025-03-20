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
        public async Task<List<ChatMessage>> GetGoogleSheetTask(List<ChatMessage> chatHistory, ChatToolCall toolCall)
        {
            chatHistory.Add(new ToolChatMessage(toolCall.Id, "Tool call to get all task in Google sheet."));
            chatHistory.Add(new SystemChatMessage("Getting properties on JsonDocument RootElement"));

            using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);
            bool hasSpreadsheetId = argumentsJson.RootElement.TryGetProperty("spreadSheetId", out JsonElement spreadSheetId);
            bool hasSheetName = argumentsJson.RootElement.TryGetProperty("sheetName", out JsonElement sheetName);
            bool hasSheetRangeFrom = argumentsJson.RootElement.TryGetProperty("sheetRangeFrom", out JsonElement sheetRangeFrom);
            bool hasSheetRangeTo = argumentsJson.RootElement.TryGetProperty("sheetRangeTo", out JsonElement sheetRangeTo);

            if (!hasSpreadsheetId || !hasSheetName || !hasSheetRangeFrom || !hasSheetRangeTo)
            {
                chatHistory.Add(new SystemChatMessage("Missing required arguments: spreadSheetId, sheetName, sheetRangeFrom, or sheetRangeTo."));
                return chatHistory;
            }
            else
            {
                try
                {
                    AgentTask agentTask = new();
                    var result = await agentTask.GetGoogleSheetTask(
                        sheetRangeFrom.GetString(),
                        sheetRangeTo.GetString(),
                        spreadSheetId.GetString(),
                        sheetName.GetString()
                    );

                    chatHistory.Add(new SystemChatMessage($"Fetched data from Google sheet."));

                    if (result.Item2 != null)
                    {
                        chatHistory.Add(new SystemChatMessage($"An error occurred: {result.Item2.Message}"));
                    }
                    else
                    {
                        string taskJson = ConvertionUtility.ConvertToJson(result.Item1);
                        chatHistory.Add(new SystemChatMessage($"Task list has been retrieved from Google sheet: {taskJson}"));
                    }

                    return chatHistory;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        public async Task<List<ChatMessage>> CreateGoogleSheetTask(List<ChatMessage> chatHistory, ChatToolCall toolCall)
        {
            chatHistory.Add(new ToolChatMessage(toolCall.Id, "Tool create task in Google sheet."));
            using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);

            bool hasSpreadsheetId = argumentsJson.RootElement.TryGetProperty("spreadSheetId", out JsonElement spreadSheetId);
            bool hasSheetName = argumentsJson.RootElement.TryGetProperty("sheetName", out JsonElement sheetName);
            bool hasSheetData = argumentsJson.RootElement.TryGetProperty("sheetName", out JsonElement sheetPreviousData);

            if (!hasSpreadsheetId || !hasSheetName || !hasSheetData)
            {
                chatHistory.Add(new SystemChatMessage($"Missing required arguments: spreadSheetId or sheetName."));
                return chatHistory;
            }

            try
            {
                chatHistory.Add(new SystemChatMessage(sheetPreviousData.GetString()));
                string jsonArguments = ConvertionUtility.ConvertToJson(argumentsJson);
                chatHistory.Add(new SystemChatMessage(jsonArguments));
                chatHistory.Add(new SystemChatMessage(OpenAIPromptModel.GenerateDynamicJsonTaskSystemInstruction()));

                string his = ConvertionUtility.ConvertChatHistoryToJson(chatHistory);
                string structuredResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                chatHistory.Add(new ToolChatMessage(toolCall.Id, "Tool to create a structured json list of task."));
                chatHistory.Add(new AssistantChatMessage(structuredResponse));

                DataTable taskList = ConvertionUtility.ConvertJsonToDynamicDataTable(structuredResponse);

                AgentTask agent = new();
                bool isTaskInserted = await agent.CreateGoogleSheetTask(taskList, spreadSheetId.GetString(), sheetName.GetString());

                if (isTaskInserted)
                {
                    chatHistory.Add(new SystemChatMessage("Task has been inserted to Google sheet."));
                }
                else
                {
                    chatHistory.Add(new SystemChatMessage("Failed to insert task."));
                }

                return chatHistory;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }

}
