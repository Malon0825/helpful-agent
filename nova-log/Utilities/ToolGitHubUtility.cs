using nova_log.DataAccess;
using nova_log.Logic;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace nova_log.Utilities
{
    public class ToolGitHubUtility
    {
        public async Task GetGithubProjectFieldId(List<ChatMessage> chatHistory, ChatToolCall toolCall)
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
                chatHistory.Add(new SystemChatMessage(projectFieldsJson));
                return;
            }
            catch (Exception ex)
            {
                chatHistory.Add(new SystemChatMessage($"Error in CheckGithubProjectFields implementation: {ex.Message}"));
                string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
                return;
            }
        }

        public async Task GetGithubRepoId(List<ChatMessage> chatHistory, ChatToolCall toolCall)
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

                string response = await new OpenAIService().SendChatPrompt(chatHistory);
                chatHistory.Add(new AssistantChatMessage(response));
                return;
            }
            catch (Exception ex)
            {
                chatHistory.Add(new SystemChatMessage($"Error in GetGithubRepoId implementation: {ex.Message}"));
                string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
                return;
            }
        }

        public async Task GetGithubAssigneeId(List<ChatMessage> chatHistory, ChatToolCall toolCall)
        {
            chatHistory.Add(new ToolChatMessage(toolCall.Id, "Tool call to get github assignee id."));
            using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);

            try
            {
                string userName = argumentsJson.RootElement.GetProperty("userName").GetString();
                chatHistory.Add(new SystemChatMessage($"Fetching GitHub user ID for {userName}..."));

                AgentTask agentTask = new();
                string userId = await agentTask.GetGithubAssigneeId(userName);
                chatHistory.Add(new AssistantChatMessage(userId));
                return;
            }
            catch (Exception ex)
            {
                chatHistory.Add(new SystemChatMessage($"Error in GetGithubAssigneeId implementation: {ex.Message}"));
                string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
                return;
            }
        }

        public async Task CreateGithubIssue(List<ChatMessage> chatHistory, ChatToolCall toolCall)
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
                chatHistory.Add(new AssistantChatMessage(issueResponse));
                return;
            }
            catch (Exception ex)
            {
                chatHistory.Add(new SystemChatMessage($"Error in CreateGithubIssue implementation: {ex.Message}"));
                string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
                return;
            }
        }

        public async Task AddIssueToProject(List<ChatMessage> chatHistory, ChatToolCall toolCall)
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
                chatHistory.Add(new AssistantChatMessage(result));
                return;
            }
            catch (Exception ex)
            {
                chatHistory.Add(new SystemChatMessage($"Error in AddIssueToProject implementation: {ex.Message}"));
                string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
                return;
            }
        }

        public async Task UpdateAssignee(List<ChatMessage> chatHistory, ChatToolCall toolCall)
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
                chatHistory.Add(new AssistantChatMessage(result));
                return;
            }
            catch (Exception ex)
            {
                chatHistory.Add(new SystemChatMessage($"Error in UpdateAssignee implementation: {ex.Message}"));
                string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
                return;
            }
        }
    }
}
