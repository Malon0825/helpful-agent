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
                                            chatHistory = await googleSheetUtility.GetGoogleSheetTask(chatHistory, toolCall);
                                            break;

                                        case nameof(AgentTool.CreateGoogleSheetTask):
                                            chatHistory = await googleSheetUtility.CreateGoogleSheetTask(chatHistory, toolCall);
                                            break;

                                        case nameof(AgentTool.GetGithubProjectFieldId):
                                            chatHistory = await gitHubUtility.GetGithubProjectFieldId(chatHistory, toolCall);
                                            break;

                                        case nameof(AgentTool.GetGithubRepoId):
                                            chatHistory = await gitHubUtility.GetGithubRepoId(chatHistory, toolCall);
                                            break;

                                        case nameof(AgentTool.GetGithubAssigneeId):
                                            chatHistory = await gitHubUtility.GetGithubAssigneeId(chatHistory, toolCall);
                                            break;

                                        case nameof(AgentTool.CreateGithubIssue):
                                            chatHistory = await gitHubUtility.CreateGithubIssue(chatHistory, toolCall);
                                            break;

                                        case nameof(AgentTool.AddIssueToProject):
                                            chatHistory = await gitHubUtility.AddIssueToProject(chatHistory, toolCall);
                                            break;

                                        case nameof(AgentTool.UpdateAssignee):
                                            chatHistory = await gitHubUtility.UpdateAssignee(chatHistory, toolCall);
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
    }
}
