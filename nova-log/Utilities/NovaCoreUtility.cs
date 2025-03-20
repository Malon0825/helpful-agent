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

            // Initialize utility classes
            ToolGoogleSheetUtility googleSheetUtility = new ToolGoogleSheetUtility();
            ToolGitHubUtility gitHubUtility = new ToolGitHubUtility();

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
                                            await googleSheetUtility.GetGoogleSheetTask(chatHistory, toolCall);
                                            break;

                                        case nameof(AgentTool.CreateGoogleSheetTask):
                                            await googleSheetUtility.CreateGoogleSheetTask(chatHistory, toolCall, googleSheetStructuredResponse);
                                            break;

                                        case nameof(AgentTool.GetCurrentDate):
                                            await googleSheetUtility.GetCurrentDate(chatHistory, toolCall);
                                            break;

                                        case nameof(AgentTool.GetGithubProjectFieldId):
                                            await gitHubUtility.GetGithubProjectFieldId(chatHistory, toolCall);
                                            break;

                                        case nameof(AgentTool.GetGithubRepoId):
                                            await gitHubUtility.GetGithubRepoId(chatHistory, toolCall);
                                            break;

                                        case nameof(AgentTool.GetGithubAssigneeId):
                                            await gitHubUtility.GetGithubAssigneeId(chatHistory, toolCall);
                                            break;

                                        case nameof(AgentTool.CreateGithubIssue):
                                            await gitHubUtility.CreateGithubIssue(chatHistory, toolCall);
                                            break;

                                        case nameof(AgentTool.AddIssueToProject):
                                            await gitHubUtility.AddIssueToProject(chatHistory, toolCall);
                                            break;

                                        case nameof(AgentTool.UpdateAssignee):
                                            await gitHubUtility.UpdateAssignee(chatHistory, toolCall);
                                            break;

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
