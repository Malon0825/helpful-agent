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
    /// <summary>
    /// Utility class for interacting with GitHub through various tools.
    /// </summary>
    public class ToolGitHubUtility
    {
        /// <summary>
        /// Gets the field ID of a GitHub project.
        /// </summary>
        /// <param name="chatHistory">The list of chat messages to log activity.</param>
        /// <param name="toolCall">The tool call containing the necessary parameters.</param>
        /// <returns>The updated chat history.</returns>
        public async Task<List<ChatMessage>> GetGithubProjectFieldId(List<ChatMessage> chatHistory, ChatToolCall toolCall)
        {
            chatHistory.Add(new ToolChatMessage(toolCall.Id, "Tool call to get all GitHub projects field ID."));
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
            }
            catch (Exception ex)
            {
                chatHistory.Add(new SystemChatMessage($"Error in CheckGithubProjectFields implementation: {ex.Message}"));
                string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
            }

            return chatHistory;
        }

        /// <summary>
        /// Gets the repository IDs for a given GitHub user or organization.
        /// </summary>
        /// <param name="chatHistory">The list of chat messages to log activity.</param>
        /// <param name="toolCall">The tool call containing the necessary parameters.</param>
        /// <returns>The updated chat history.</returns>
        public async Task<List<ChatMessage>> GetGithubRepoId(List<ChatMessage> chatHistory, ChatToolCall toolCall)
        {
            chatHistory.Add(new ToolChatMessage(toolCall.Id, "Tool call to get all GitHub repo IDs."));
            using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);

            try
            {
                string repoOwner = argumentsJson.RootElement.GetProperty("repoOwner").GetString();
                chatHistory.Add(new SystemChatMessage($"Fetching repositories for {repoOwner}..."));

                AgentTask agentTask = new();
                string repoJson = await agentTask.GetGithubRepoId(repoOwner);
                chatHistory.Add(new SystemChatMessage(repoJson));
                chatHistory.Add(new SystemChatMessage("Based on the response in JSON. Create a list of repositories and return it to the user."));

                string response = await new OpenAIService().SendChatPrompt(chatHistory);
                chatHistory.Add(new AssistantChatMessage(response));
            }
            catch (Exception ex)
            {
                chatHistory.Add(new SystemChatMessage($"Error in GetGithubRepoId implementation: {ex.Message}"));
                string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
            }

            return chatHistory;
        }

        /// <summary>
        /// Gets the GitHub user ID for a given assignee.
        /// </summary>
        /// <param name="chatHistory">The list of chat messages to log activity.</param>
        /// <param name="toolCall">The tool call containing the necessary parameters.</param>
        /// <returns>The updated chat history.</returns>
        public async Task<List<ChatMessage>> GetGithubAssigneeId(List<ChatMessage> chatHistory, ChatToolCall toolCall)
        {
            chatHistory.Add(new ToolChatMessage(toolCall.Id, "Tool call to get GitHub assignee ID."));
            using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);

            try
            {
                string userName = argumentsJson.RootElement.GetProperty("userName").GetString();
                chatHistory.Add(new SystemChatMessage($"Fetching GitHub user ID for {userName}..."));

                AgentTask agentTask = new();
                string userId = await agentTask.GetGithubAssigneeId(userName);
                chatHistory.Add(new AssistantChatMessage(userId));
            }
            catch (Exception ex)
            {
                chatHistory.Add(new SystemChatMessage($"Error in GetGithubAssigneeId implementation: {ex.Message}"));
                string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
            }

            return chatHistory;
        }

        /// <summary>
        /// Creates a new issue in a GitHub repository.
        /// </summary>
        /// <param name="chatHistory">The list of chat messages to log activity.</param>
        /// <param name="toolCall">The tool call containing the necessary parameters.</param>
        /// <returns>The updated chat history.</returns>
        public async Task<List<ChatMessage>> CreateGithubIssue(List<ChatMessage> chatHistory, ChatToolCall toolCall)
        {
            chatHistory.Add(new ToolChatMessage(toolCall.Id, "Tool call to create GitHub issue."));
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
            }
            catch (Exception ex)
            {
                chatHistory.Add(new SystemChatMessage($"Error in CreateGithubIssue implementation: {ex.Message}"));
                string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
            }

            return chatHistory;
        }

        /// <summary>
        /// Adds an issue to a GitHub project.
        /// </summary>
        /// <param name="chatHistory">The list of chat messages to log activity.</param>
        /// <param name="toolCall">The tool call containing the necessary parameters.</param>
        /// <returns>The updated chat history.</returns>
        public async Task<List<ChatMessage>> AddIssueToProject(List<ChatMessage> chatHistory, ChatToolCall toolCall)
        {
            chatHistory.Add(new ToolChatMessage(toolCall.Id, "Tool call to add issues to GitHub projects."));
            using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);

            try
            {
                string projectId = argumentsJson.RootElement.GetProperty("projectId").GetString();
                string contentId = argumentsJson.RootElement.GetProperty("contentId").GetString();

                chatHistory.Add(new SystemChatMessage($"Adding issue {contentId} to project {projectId}..."));

                AgentTask agentTask = new();
                string result = await agentTask.AddIssueToProject(projectId, contentId);
                chatHistory.Add(new AssistantChatMessage(result));
            }
            catch (Exception ex)
            {
                chatHistory.Add(new SystemChatMessage($"Error in AddIssueToProject implementation: {ex.Message}"));
                string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
            }

            return chatHistory;
        }

        /// <summary>
        /// Updates the assignee of a GitHub issue or project item.
        /// </summary>
        /// <param name="chatHistory">The list of chat messages to log activity.</param>
        /// <param name="toolCall">The tool call containing the necessary parameters.</param>
        /// <returns>The updated chat history.</returns>
        public async Task<List<ChatMessage>> UpdateAssignee(List<ChatMessage> chatHistory, ChatToolCall toolCall)
        {
            chatHistory.Add(new ToolChatMessage(toolCall.Id, "Tool call to update assignee on GitHub projects."));
            using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);

            try
            {
                string contentId = argumentsJson.RootElement.GetProperty("contentId").GetString();
                string assigneeId = argumentsJson.RootElement.GetProperty("assigneeId").GetString();

                chatHistory.Add(new SystemChatMessage($"Updating assignee for content {contentId} to user {assigneeId}..."));

                AgentTask agentTask = new();
                string result = await agentTask.UpdateAssignee(contentId, assigneeId);
                chatHistory.Add(new AssistantChatMessage(result));
            }
            catch (Exception ex)
            {
                chatHistory.Add(new SystemChatMessage($"Error in UpdateAssignee implementation: {ex.Message}"));
                string agentErrorResponse = await new OpenAIService().SendChatPrompt(chatHistory);
                chatHistory.Add(new AssistantChatMessage(agentErrorResponse));
            }

            return chatHistory;
        }
    }

}
