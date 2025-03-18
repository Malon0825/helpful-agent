using Newtonsoft.Json.Linq;
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
    public static class ConvertionUtility
    {

        /// <summary>
        /// Converts a list of ChatMessage objects to a JSON string suitable for client consumption.
        /// </summary>
        /// <param name="chatHistory">The list of chat messages.</param>
        /// <returns>A JSON string representation of the chat history.</returns>
        public static string ConvertChatHistoryToJson(List<ChatMessage> chatHistory)
        {
            List<ChatHistoryModel> chatHistoryModelList = new();

            foreach (ChatMessage chatMessage in chatHistory)
            {
                ChatHistoryModel chatHistoryModel = new();

                if (chatMessage is UserChatMessage)
                {
                    chatHistoryModel.Role = Enums.ChatRoleEnum.User;
                }
                else if (chatMessage is AssistantChatMessage)
                {
                    chatHistoryModel.Role = Enums.ChatRoleEnum.Assistant;
                }
                else if (chatMessage is SystemChatMessage)
                {
                    chatHistoryModel.Role = Enums.ChatRoleEnum.System;
                }
                else
                {
                    chatHistoryModel.Role = Enums.ChatRoleEnum.Tool;
                }

                chatHistoryModel.Content = chatMessage.Content[0].Text;
                chatHistoryModelList.Add(chatHistoryModel);
            }

            string json = JsonSerializer.Serialize(
                chatHistoryModelList,
                new JsonSerializerOptions { WriteIndented = true }
            );

            return json;
        }

        /// <summary>
        /// Converts a JSON string into a list of ChatMessage objects.
        /// </summary>
        /// <param name="json">A JSON string representation of the chat history.</param>
        /// <returns>A List of ChatMessage objects reconstructed from the JSON.</returns>
        public static List<ChatMessage> ConvertJsonToChatHistory(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<ChatMessage>();
            }

            // Set up options for JSON deserialization.
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Deserialize the JSON string into a list of ChatHistoryModel objects.
            var modelList = JsonSerializer.Deserialize<List<ChatHistoryModel>>(json, options);

            List<ChatMessage> chatMessages = new();

            if (modelList is null)
                return chatMessages;

            // Convert each ChatHistoryModel to its respective ChatMessage type.
            foreach (var model in modelList)
            {
                ChatMessage message;

                switch (model.Role)
                {
                    case Enums.ChatRoleEnum.User:
                        message = new UserChatMessage(model.Content);
                        break;
                    case Enums.ChatRoleEnum.Assistant:
                        message = new AssistantChatMessage(model.Content);
                        break;
                    case Enums.ChatRoleEnum.System:
                        message = new SystemChatMessage(model.Content);
                        break;
                    case Enums.ChatRoleEnum.Tool:
                        message = new ToolChatMessage(model.Content);
                        break;
                    default:
                        message = new ToolChatMessage(model.Content);
                        break;
                }

                chatMessages.Add(message);
            }
            return chatMessages;
        }

        /// <summary>
        /// Converts a JSON string representing an array of objects into a DataTable.
        /// The method handles dynamic JSON structure by adding new columns if they appear.
        /// </summary>
        /// <param name="json">The JSON string to convert.</param>
        /// <returns>A DataTable populated with the JSON data.</returns>
        public static DataTable ConvertJsonToDynamicDataTable(string json)
        {
            // Create a new DataTable instance.
            DataTable dt = new DataTable();

            // Parse the JSON string into a JArray.
            JArray jsonArray = JArray.Parse(json);

            // Iterate over each JObject in the array.
            foreach (JObject obj in jsonArray)
            {
                // For each property in the JObject, add a DataColumn if it doesn't already exist.
                foreach (var property in obj.Properties())
                {
                    if (!dt.Columns.Contains(property.Name))
                    {
                        // You can change the data type as needed; here we use string for simplicity.
                        dt.Columns.Add(property.Name, typeof(string));
                    }
                }

                // Once columns are set, create a new DataRow.
                DataRow row = dt.NewRow();

                // For each property, assign the value to the corresponding column.
                foreach (var property in obj.Properties())
                {
                    // If the property value is null, save DBNull.Value.
                    row[property.Name] = property.Value != null ? property.Value.ToString() : DBNull.Value;
                }

                // Add the row to the DataTable.
                dt.Rows.Add(row);
            }

            return dt;
        }


        public static DataTable ConvertJsonToDataTable(string jsonResponse)
        {
            DataTable dataTable = new DataTable();

            // Define the columns based on JSON structure
            dataTable.Columns.Add("Task List", typeof(string));
            dataTable.Columns.Add("Task Type", typeof(string));
            dataTable.Columns.Add("Estimate (hours)", typeof(string));
            dataTable.Columns.Add("Actual Start Date", typeof(string));
            dataTable.Columns.Add("Actual End Date", typeof(string));
            dataTable.Columns.Add("Priority Level", typeof(string));
            dataTable.Columns.Add("Status", typeof(string));
            dataTable.Columns.Add("Remarks", typeof(string));

            try
            {
                // Deserialize JSON response
                var document = JsonDocument.Parse(jsonResponse);
                var tasksArray = document.RootElement.GetProperty("tasks");

                foreach (var task in tasksArray.EnumerateArray())
                {
                    DataRow row = dataTable.NewRow();
                    row["Task List"] = task.GetProperty("task_list").GetString();
                    row["Task Type"] = task.GetProperty("task_type").GetString();
                    row["Estimate (Hours)"] = task.GetProperty("estimate_hours").GetString();
                    row["Actual Start Date"] = task.GetProperty("actual_start_date").GetString();
                    row["Actual End Date"] = task.GetProperty("actual_end_date").GetString();
                    row["Priority Level"] = task.GetProperty("priority_level").GetString();
                    row["Status"] = task.GetProperty("status").GetString();
                    row["Remarks"] = task.GetProperty("remarks").GetString();

                    dataTable.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error parsing JSON to DataTable", ex);
            }

            return dataTable;
        }

        public static string ConvertToJson(DataTable dataTable)
        {
            var rows = new List<Dictionary<string, object>>();

            // Iterate over each row in the DataTable.
            foreach (DataRow row in dataTable.Rows)
            {
                var rowDictionary = new Dictionary<string, object>();

                // Add each column's data into the dictionary.
                foreach (DataColumn column in dataTable.Columns)
                {
                    rowDictionary[column.ColumnName] = row[column] ?? DBNull.Value; // Handle nulls.
                }

                rows.Add(rowDictionary);
            }

            // Serialize the list of dictionaries to JSON.
            var options = new JsonSerializerOptions
            {
                WriteIndented = true, // Makes the JSON pretty (optional).
            };

            return System.Text.Json.JsonSerializer.Serialize(rows, options);
        }

        public static string ConvertToJson(JsonDocument argumentsJson)
        {
            var jsonObject = new Dictionary<string, object>();

            if (argumentsJson.RootElement.TryGetProperty("spreadSheetId", out JsonElement spreadSheetId))
                jsonObject["spreadSheetId"] = spreadSheetId.GetString();

            if (argumentsJson.RootElement.TryGetProperty("sheetName", out JsonElement sheetName))
                jsonObject["sheetName"] = sheetName.GetString();

            if (argumentsJson.RootElement.TryGetProperty("taskDetails", out JsonElement taskDetails))
                jsonObject["taskDetails"] = taskDetails.GetString();

            if (argumentsJson.RootElement.TryGetProperty("taskCount", out JsonElement taskCount))
                jsonObject["taskCount"] = taskCount.GetInt32();

            return System.Text.Json.JsonSerializer.Serialize(jsonObject, new JsonSerializerOptions { WriteIndented = true });
        }



    }
}
