using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using nova_log.DataAccess;
using nova_log.Logic;
using nova_log.Models;
using OpenAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace nova_log.Utilities
{
    public class NovaCoreUtility
    {
        public async Task<List<ChatMessage>> EvaluateAgentResponseWithLoop(List<ChatMessage> chatHistory, ChatCompletionOptions options)
        {
            OpenAIService openAIService = new();
            bool requiresAction;
            RequestModel history = new();
            do
            {

                requiresAction = false;
                try
                {

                    history.ChatHistory = ConvertionUtility.ConvertChatHistoryToJson(chatHistory);
                    ChatCompletion agentResponse = await openAIService.SendChatPrompt(chatHistory, options);


                    switch (agentResponse.FinishReason)
                {
                    case ChatFinishReason.Stop:
                        {
                            chatHistory.Add(new AssistantChatMessage(agentResponse));
                            break;
                        }

                    case ChatFinishReason.ToolCalls:
                        {
                            chatHistory.Add(new AssistantChatMessage(agentResponse));

                            foreach (ChatToolCall toolCall in agentResponse.ToolCalls)
                            {
                                switch (toolCall.FunctionName)
                                {
                                    case nameof(AgentTool.GetGoogleSheetTask):
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

                                            }
                                            else
                                            {
                                                try
                                                {
                                                    AgentTask agentTask = new();
                                                    var (dataTable, error) = await agentTask.GetGoogleSheetTask(
                                                        sheetRangeFrom.GetString(),
                                                        sheetRangeTo.GetString(),
                                                        spreadSheetId.GetString(), 
                                                        sheetName.GetString()
                                                    );
                                                    chatHistory.Add(new SystemChatMessage($"Fetched data from google sheet."));
                                                    if (error != null)
                                                    {
                                                        chatHistory.Add(new SystemChatMessage($"An error occurred: {error.Message}"));
                                                        string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                                                        chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
                                                    }
                                                    else
                                                    {
                                                      
                                                        string taskJson = ConvertionUtility.ConvertToJson(dataTable);
                                                        chatHistory.Add(new SystemChatMessage($"Task list has been retrieved from google sheet: {taskJson}"));
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    chatHistory.Add(new SystemChatMessage($"Give a user friendly message about what might be caussing the error: {ex}"));
                                                    string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                                                    chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
                                                }
                                                
                                            }
                                            break;
                                        }

                                    case nameof(AgentTool.CreateGoogleSheetTask):
                                        {
                                            chatHistory.Add(new ToolChatMessage(toolCall.Id, "Tool create task in google sheet."));
                                            using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);

                                            bool hasSpreadsheetId = argumentsJson.RootElement.TryGetProperty("spreadSheetId", out JsonElement spreadSheetId);
                                            bool hasSheetName = argumentsJson.RootElement.TryGetProperty("sheetName", out JsonElement sheetName);


                                            if (!hasSpreadsheetId || !hasSheetName)
                                            {
                                                chatHistory.Add(new SystemChatMessage($"Missing required arguments: spreadSheetId or sheetName."));
                                                string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                                                chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
                                            }

                                            try
                                            {
                                                string jsonArguments = ConvertionUtility.ConvertToJson(argumentsJson);
                                                chatHistory.Add(new SystemChatMessage(OpenAIPromptModel.GenerateDynamicJsonTaskSystemInstruction()));

                                                    string structuredResponse = await new OpenAIService().SendChatPromptReturnString(chatHistory, googleSheetStructuredResponse);
                                                    chatHistory.Add(new AssistantChatMessage(structuredResponse));

                                                DataTable taskList = ConvertionUtility.ConvertJsonToDynamicDataTable(structuredResponse);

                                                AgentTask agent = new();

                                                bool isTaskInserted = await agent.CreateGoogleSheetTask(taskList, spreadSheetId.GetString(), sheetName.GetString());
                                                if (isTaskInserted)
                                                {
                                                    chatHistory.Add(new SystemChatMessage("Task has been inserted to google sheet."));
                                                }
                                                else
                                                {
                                                    chatHistory.Add(new SystemChatMessage("Failed to insert task."));
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                chatHistory.Add(new SystemChatMessage($"Give a user friendly message about what might be caussing the error: {ex}"));
                                                string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                                                chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
                                            }
                                            break;
                                        }

                                    case nameof(AgentTool.GetCurrentDate):
                                        {
                                            chatHistory.Add(new ToolChatMessage(toolCall.Id, "Tool call to get current date."));
                                            using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);

                                            try
                                            {
                                                string jsonArguments = ConvertionUtility.ConvertToJson(argumentsJson);
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
                                            break;
                                        }

                                    case nameof(AgentTool.GetGithubProjectFieldId):
                                        {
                                                chatHistory.Add(new ToolChatMessage(toolCall.Id, "Tool call to get all github projects field id."));
                                                using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);

                                            try
                                            {
                                                string jsonArguments = ConvertionUtility.ConvertToJson(argumentsJson);
                                                chatHistory.Add(new SystemChatMessage("Processing CheckGithubProjectFields."));

                                                string repoOwner = argumentsJson.RootElement.GetProperty("repoOwner").GetString();
                                                string projectNumber = argumentsJson.RootElement.GetProperty("projectNumber").GetString();

                                                AgentTask projectHelper = new();
                                                string projectFieldsJson = await projectHelper.GetGithubProjectFieldId(repoOwner, projectNumber);

                                            }
                                            catch (Exception ex)
                                            {
                                                chatHistory.Add(new SystemChatMessage($"Error in CheckGithubProjectFields implementation: {ex.Message}"));

                                                string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                                                chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
                                            }
                                            break;
                                        }

                                    case nameof(AgentTool.GetGithubRepoId):
                                        {
                                                chatHistory.Add(new ToolChatMessage(toolCall.Id, "Tool call to get all github repo id."));
                                                using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);

                                            try
                                            {
                                                string repoOwner = argumentsJson.RootElement.GetProperty("repoOwner").GetString();
                                                chatHistory.Add(new SystemChatMessage($"Fetching repositories for {repoOwner}..."));

                                                AgentTask agentTask = new();
                                                string repoJson = await agentTask.GetGithubRepoId(repoOwner);
                                                chatHistory.Add(new SystemChatMessage(repoJson));
                                                chatHistory.Add(new SystemChatMessage("Based on the response in json. Create a list of repository and give that back to the user."));
                                                chatHistory.Add(new AssistantChatMessage(await new OpenAIService().SendChatPrompt(chatHistory)));
                                            }
                                            catch (Exception ex)
                                            {
                                                chatHistory.Add(new SystemChatMessage($"Error in GetGithubRepoId implementation: {ex.Message}"));
                                                string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                                                chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
                                            }
                                            break;
                                        }

                                    case nameof(AgentTool.GetGithubAssigneeId):
                                            {
                                                chatHistory.Add(new ToolChatMessage(toolCall.Id, "Tool call to get github assignee id."));
                                                using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);

                                                try
                                                {
                                                    string userName = argumentsJson.RootElement.GetProperty("userName").GetString();

                                                    chatHistory.Add(new SystemChatMessage($"Fetching GitHub user ID for {userName}..."));

                                                    AgentTask agentTask = new();
                                                    string userId = await agentTask.GetGithubAssigneeId(userName);

                                                }
                                                catch (Exception ex)
                                                {
                                                    chatHistory.Add(new SystemChatMessage($"Error in GetGithubAssigneeId implementation: {ex.Message}"));
                                                    string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                                                    chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
                                                }
                                                break;
                                            }

                                    case nameof(AgentTool.CreateGithubIssue):
                                        {
                                                chatHistory.Add(new ToolChatMessage(toolCall.Id, "Tool call to create github issue."));
                                                using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);

                                            try
                                            {
                                                string repositoryId = argumentsJson.RootElement.GetProperty("repositoryId").GetString();
                                                string title = argumentsJson.RootElement.GetProperty("title").GetString();
                                                string body = argumentsJson.RootElement.GetProperty("body").GetString();

                                                chatHistory.Add(new SystemChatMessage($"Creating a new GitHub issue in repository {repositoryId}..."));

                                                AgentTask agentTask = new();
                                                string issueResponse = await agentTask.CreateGithubIssue(repositoryId, title, body);
                                            }
                                            catch (Exception ex)
                                            {
                                                chatHistory.Add(new SystemChatMessage($"Error in CreateGithubIssue implementation: {ex.Message}"));
                                                string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                                                chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
                                            }
                                            break;
                                        }

                                    case nameof(AgentTool.AddIssueToProject):
                                        {
                                                chatHistory.Add(new ToolChatMessage(toolCall.Id, "Tool call to add issues to github projects"));
                                                using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);

                                            try
                                            {
                                                string projectId = argumentsJson.RootElement.GetProperty("projectId").GetString();
                                                string contentId = argumentsJson.RootElement.GetProperty("contentId").GetString();

                                                chatHistory.Add(new SystemChatMessage($"Adding issue {contentId} to project {projectId}..."));

                                                AgentTask agentTask = new();
                                                string result = await agentTask.AddIssueToProject(projectId, contentId);
                                            }
                                            catch (Exception ex)
                                            {
                                                chatHistory.Add(new SystemChatMessage($"Error in AddIssueToProject implementation: {ex.Message}"));
                                                string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                                                chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
                                            }
                                            break;
                                        }

                                    case nameof(AgentTool.UpdateAssignee):
                                        {
                                                chatHistory.Add(new ToolChatMessage(toolCall.Id, "Tool call to update assignee on github projects."));
                                                using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);

                                            try
                                            {
                                                string contentId = argumentsJson.RootElement.GetProperty("contentId").GetString();
                                                string assigneeId = argumentsJson.RootElement.GetProperty("assigneeId").GetString();

                                                chatHistory.Add(new SystemChatMessage($"Updating assignee for content {contentId} to user {assigneeId}..."));

                                                AgentTask agentTask = new();
                                                string result = await agentTask.UpdateAssignee(contentId, assigneeId);
                                            }
                                            catch (Exception ex)
                                            {
                                                chatHistory.Add(new SystemChatMessage($"Error in UpdateAssignee implementation: {ex.Message}"));
                                                string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                                                chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
                                            }
                                            break;
                                        }



                                    default:
                                        {
                                            throw new NotImplementedException($"Unexpected tool function: {toolCall.FunctionName}");
                                        }
                                }
                            }

                            requiresAction = true;
                            break;
                        }

                    case ChatFinishReason.Length:
                        throw new NotImplementedException("Incomplete model output due to MaxTokens parameter or token limit exceeded.");

                    case ChatFinishReason.ContentFilter:
                        throw new NotImplementedException("Omitted content due to a content filter flag.");

                    case ChatFinishReason.FunctionCall:
                        throw new NotImplementedException("Deprecated in favor of tool calls.");

                    default:
                        throw new NotImplementedException(agentResponse.FinishReason.ToString());
                }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            } while (requiresAction);
            return chatHistory;
        }

        ChatCompletionOptions googleSheetStructuredResponse = new()
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: "task_list",
                jsonSchema: BinaryData.FromBytes("""
            {
                "type": "object",
                "properties": {
                    "tasks": {
                        "type": "array",
                        "items": {
                            "type": "object",
                            "properties": {
                                "task_list": { "type": "string" },
                                "task_type": { "type": "string" },
                                "estimate_hours": { "type": "string" },
                                "actual_start_date": { "type": "string" },
                                "actual_end_date": { "type": "string" },
                                "priority_level": { "type": "string" },
                                "status": { "type": "string" },
                                "remarks": { "type": "string" }
                            },
                            "required": [
                                "task_list", "task_type", "estimate_hours", "actual_start_date", "actual_end_date", 
                                "priority_level", "status", "remarks"
                            ],
                            "additionalProperties": false
                        }
                    }
                },
                "required": ["tasks"],
                "additionalProperties": false
            }
            """u8.ToArray()),
                jsonSchemaIsStrict: true)
        };


      

    }
}
