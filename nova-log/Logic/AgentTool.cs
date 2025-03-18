using Google.Apis.Sheets.v4.Data;
using nova_log.DataAccess;
using nova_log.Models;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nova_log.Logic
{
    public class AgentTool
    {
        AgentTask agentTask = new();

        public static readonly ChatTool GetGoogleSheetTask = ChatTool.CreateFunctionTool(
            functionName: nameof(agentTask.GetGoogleSheetTask),
            functionDescription: "Retrieves user's task from Google Sheet",
            functionParameters: BinaryData.FromBytes("""
            {
                "type": "object",
                "properties": {
                    "spreadSheetId": {
                        "type": "string",
                        "description": "The ID of the spreadsheet where the user wants to log his/her task."
                    },
                    "sheetName": {
                        "type": "string",
                        "description": "The name of the sheet where the user wants to log his/her task."
                    },
                    "sheetRangeFrom": {
                        "type": "string",
                        "enum": [ "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" ],
                        "description": "The starting column the user wants to log his/her task."
                    },
                    "sheetRangeTo": {
                        "type": "string",
                        "enum": [ "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" ],
                        "description": "The ending column the user wants to log his/her task."
                    }
                },
                "required": ["spreadSheetId", "sheetName", "sheetRangeFrom", "sheetRangeTo"]
            }
            """u8.ToArray())
        );

        public static readonly ChatTool CreateGoogleSheetTask = ChatTool.CreateFunctionTool(
            functionName: nameof(agentTask.CreateGoogleSheetTask),
            functionDescription: "Creates and logs Google Sheet task for the user.",
            functionParameters: BinaryData.FromBytes("""
            {
                "type": "object",
                "properties": {
                    "spreadSheetId": {
                        "type": "string",
                        "description": "The ID of the spreadsheet where the user wants to create the task."
                    },
                    "sheetName": {
                        "type": "string",
                        "description": "The name of the sheet where the user wants to create the task."
                    },
                    "taskDetails": {
                        "type": "string",
                        "description": "Details of tasks performed by user."
                    },
                    "taskCount": {
                        "type": "integer",
                        "description": "Number of task the user want's to generate."
                    }
                },
                "required": ["spreadSheetId", "sheetName"]
            }
            """u8.ToArray())
        );

        public static readonly ChatTool GetCurrentDate = ChatTool.CreateFunctionTool(
            functionName: nameof(AgentTask.GetCurrentDate),
            functionDescription: "Returns the current date in yyyy-MM-dd format.",
            functionParameters: BinaryData.FromBytes("""
                {
                    "type": "object",
                    "properties": {},
                    "required": []
                }
                """u8.ToArray())
        );

        public static readonly ChatTool GetGithubProjectFieldId = ChatTool.CreateFunctionTool(
            functionName: nameof(AgentTask.GetGithubProjectFieldId),
            functionDescription: "Fetches all field IDs for a given GitHub project and returns them as JSON.",
            functionParameters: BinaryData.FromBytes("""
                {
                    "type": "object",
                    "properties": {
                        "repoOwner": {
                            "type": "string",
                            "description": "The owner of the GitHub repository."
                        },
                        "projectNumber": {
                            "type": "integer",
                            "description": "The number of the project in the repository."
                        }
                    },
                    "required": ["repoOwner", "projectNumber"]
                }
                """u8.ToArray())
        );

        public static readonly ChatTool GetGithubRepoId = ChatTool.CreateFunctionTool(
            functionName: nameof(AgentTask.GetGithubRepoId),
            functionDescription: "Retrieves all repositories under a given repository owner and returns them as JSON, including repository IDs.",
            functionParameters: BinaryData.FromBytes("""
                {
                    "type": "object",
                    "properties": {
                        "repoOwner": {
                            "type": "string",
                            "description": "The username or organization name of the repository owner."
                        }
                    },
                    "required": ["repoOwner"]
                }
                """u8.ToArray())
        );

        public static readonly ChatTool CreateGithubIssue = ChatTool.CreateFunctionTool(
            functionName: nameof(AgentTask.CreateGithubIssue),
            functionDescription: "Creates a new GitHub issue in a given repository.",
            functionParameters: BinaryData.FromBytes("""
                {
                    "type": "object",
                    "properties": {
                        "repositoryId": {
                            "type": "string",
                            "description": "The unique ID of the GitHub repository where the issue will be created."
                        },
                        "title": {
                            "type": "string",
                            "description": "The title of the GitHub issue."
                        },
                        "body": {
                            "type": "string",
                            "description": "The detailed description or body of the issue."
                        }
                    },
                    "required": ["repositoryId", "title", "body"]
                }
                """u8.ToArray())
        );


        public static readonly ChatTool GetGithubAssigneeId = ChatTool.CreateFunctionTool(
            functionName: nameof(AgentTask.GetGithubAssigneeId),
            functionDescription: "Retrieves the GitHub user ID for a given username.",
            functionParameters: BinaryData.FromBytes("""
                {
                    "type": "object",
                    "properties": {
                        "userName": {
                            "type": "string",
                            "description": "The GitHub username of the assignee."
                        }
                    },
                    "required": ["userName"]
                }
                """u8.ToArray())
        );

        public static readonly ChatTool AddIssueToProject = ChatTool.CreateFunctionTool(
            functionName: nameof(AgentTask.AddIssueToProject),
            functionDescription: "Adds an issue to a specified GitHub project.",
            functionParameters: BinaryData.FromBytes("""
                {
                    "type": "object",
                    "properties": {
                        "projectId": {
                            "type": "string",
                            "description": "The ID of the GitHub project."
                        },
                        "contentId": {
                            "type": "string",
                            "description": "The ID of the issue to be added to the project."
                        }
                    },
                    "required": ["projectId", "contentId"]
                }
                """u8.ToArray())
        );

        public static readonly ChatTool UpdateAssignee = ChatTool.CreateFunctionTool(
            functionName: nameof(AgentTask.UpdateAssignee),
            functionDescription: "Updates the assignee of a GitHub issue or pull request.",
            functionParameters: BinaryData.FromBytes("""
                {
                    "type": "object",
                    "properties": {
                        "contentId": {
                            "type": "string",
                            "description": "The ID of the issue or pull request to update."
                        },
                        "assigneeId": {
                            "type": "string",
                            "description": "The ID of the user to assign to the issue."
                        }
                    },
                    "required": ["contentId", "assigneeId"]
                }
                """u8.ToArray())
        );





    }
}
