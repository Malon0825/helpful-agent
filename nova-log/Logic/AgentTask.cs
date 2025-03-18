using Google.Apis.Sheets.v4.Data;
using nova_log.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace nova_log.Logic
{
    public class AgentTask
    {
        private string _spreadsheetId;
        private string _spreadsheetName;

        public AgentTask()
        {
            _spreadsheetId = "15sF4UAoLBju1aXSdo58HTcHMGtRExRu3-CLRzxr2RQY";
            _spreadsheetName = "Sheet1";
        }

        public AgentTask(string spreadSheetId, string spreadSheetName)
        {
            _spreadsheetId = spreadSheetId;
            _spreadsheetName = spreadSheetName;
        }

        public async Task<bool> CreateGoogleSheetTask(DataTable taskTable)
        {
            GoogleSheetService sheetService = new(_spreadsheetId, _spreadsheetName);
            return await sheetService.AppendToSheet(taskTable);
        }

        public async Task<(DataTable dataTable, Exception? error)> GetGoogleSheetTask(string columnFrom, string columnTo)
        {
            GoogleSheetService sheetService = new(_spreadsheetId, _spreadsheetName);
            return await sheetService.GetTaskListAsDataTableAsync(columnFrom, columnTo);
        }

        public static string GetCurrentDate()
        {
            // Customize the date format if necessary.
            return DateTime.Now.ToString("yyyy-MM-dd");
        }

        public async Task<string> GetGithubProjectFieldId(string repoOwner, string projectNumber)
        {
            GitHubIssueCreator issueCreator = new();
            string projectId = await issueCreator.GetOrgProjectIdAsync(repoOwner, projectNumber);
            return await issueCreator.GetAllFieldsAsJson(projectId);
        }

        public async Task<string> GetGithubRepoId(string repoOwner)
        {
            GitHubIssueCreator issueCreator = new();
            return await issueCreator.GetGithubRepoId(repoOwner);
        }

        public async Task<string> CreateGithubIssue(string repositoryId, string title, string body)
        {
            GitHubIssueCreator issueCreator = new();
            return await issueCreator.CreateIssueAsync(repositoryId, title, body);
        }

        public async Task<string> GetGithubAssigneeId(string userName)
        {
            GitHubIssueCreator issueCreator = new();
            return await issueCreator.GetUserIdAsync(userName);
        }

        public async Task<string> AddIssueToProject(string projectId, string contentId)
        {
            GitHubIssueCreator issueCreator = new();
            return await  issueCreator.AddIssueToProjectAsync(projectId, contentId);
        }

        public async Task<string> UpdateAssignee(string contentId, string assigneeId)
        {
            GitHubIssueCreator issueCreator = new();
            return await issueCreator.UpdateAssigneeAsync(contentId, assigneeId);
        }

        public async Task CreateGithubTask()
        {
            try
            {

                string repoOwner = "nbspi";
                string userName = "ItsMark-SE";
                string projectNumber = "171";
                string repoName = "queue-counter-db";
                string status = "In Progress";
                string[] labels = { "bug", "high-priority" };
                string startDate = "2024-03-10";
                string endDate = "2024-03-15";
                string title = "Test issue 6";
                string body = "This is a test issue created via GraphQL API";


                GitHubIssueCreator issueCreator = new();
                string repositoryId = await issueCreator.GetRepositoryIdAsync(repoOwner, repoName);
                Console.WriteLine($"Repository ID: {repositoryId}");

                // Step 2: Create the issue in the repository

                Console.WriteLine("Creating issue...");
                string createIssueResponse = await issueCreator.CreateIssueAsync(repositoryId, title, body);
                Console.WriteLine($"Issue created response: {createIssueResponse}");

                // Extract issue ID for later use in adding to project
                string contentId = issueCreator.ExtractIssueId(createIssueResponse);
                int issueNumber = issueCreator.ExtractIssueNumber(createIssueResponse);
                string issueUrl = issueCreator.ExtractIssueUrl(createIssueResponse);
                Console.WriteLine($"Issue created with ID: {contentId}");
                Console.WriteLine($"Issue number: {issueNumber}");
                Console.WriteLine($"Issue URL: {issueUrl}");

                // Step 3: Get the project ID (assuming it's a user project)
                // Note: If it's an organization project, use GetOrgProjectIdAsync instead
                // This should be the number of the project (queue-counter-version1.6.0)
                Console.WriteLine("Getting project ID...");
                string projectId = await issueCreator.GetOrgProjectIdAsync(repoOwner, projectNumber);
                Console.WriteLine($"Project ID: {projectId}");

                // Step 4: Add the issue to the project
                Console.WriteLine("Adding issue to project...");



                string assigneeId = await issueCreator.GetUserIdAsync(userName);

                string addToProjectResponse = await issueCreator.AddIssueToProjectAsync(projectId, contentId);
                Console.WriteLine($"Add to project response: {addToProjectResponse}");


                string updateAssigneesAsyncResponse = await issueCreator.UpdateAssigneeAsync(contentId, assigneeId);
                Console.WriteLine($"Add to project response: {updateAssigneesAsyncResponse}");

                string fieldIdByResponse = await issueCreator.GetAllFieldsAsJson(projectId);
                Console.WriteLine($"Add to project response: {fieldIdByResponse}");

                //string updateStatusAsyncResponse = await issueCreator.UpdateStatusAsync(projectId, contentId, fieldId, statusOptionid);
                //Console.WriteLine($"Add to project response: {updateAssigneesAsyncResponse}");


                Console.WriteLine("Operation completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine(ex.StackTrace);
            }
        }

    }
}
