using Newtonsoft.Json.Linq;
using nova_log.Models;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace nova_log.Utilities
{
    public static class ConvertionUtility
    {

        /// <summary>
        /// Converts a JSON string representing an array of objects into a DataTable.
        /// The method handles dynamic JSON structure by adding new columns if they appear.
        /// It also extracts the JSON content between the first '[' and last ']'.
        /// </summary>
        /// <param name="json">The JSON string to convert.</param>
        /// <returns>A DataTable populated with the JSON data.</returns>
        public static DataTable ConvertJsonToDynamicDataTable(string json)
        {
            try
            {
                // Extract valid JSON array by finding the first '[' and last ']'
                int startIndex = json.IndexOf('[');
                int endIndex = json.LastIndexOf(']');

                if (startIndex == -1 || endIndex == -1 || startIndex > endIndex)
                {
                    throw new Exception("Invalid JSON format: Unable to locate valid array boundaries.");
                }

                json = json.Substring(startIndex, (endIndex - startIndex) + 1);

                DataTable dt = new DataTable();
                JArray jsonArray = JArray.Parse(json);

                foreach (JObject obj in jsonArray)
                {
                    foreach (var property in obj.Properties())
                    {
                        if (!dt.Columns.Contains(property.Name))
                        {
                            dt.Columns.Add(property.Name, typeof(string)); // Use string as default type
                        }
                    }

                    DataRow row = dt.NewRow();
                    foreach (var property in obj.Properties())
                    {
                        row[property.Name] = property.Value?.ToString() ?? DBNull.Value.ToString();
                    }
                    dt.Rows.Add(row);
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting JSON to DataTable: {ex.Message}");
            }
        }


        /// <summary>
        /// Converts a list of ChatMessage objects to a JSON string suitable for client consumption.
        /// </summary>
        /// <param name="chatHistory">The list of chat messages.</param>
        /// <returns>A JSON string representation of the chat history.</returns>
        public static string ConvertChatHistoryToJson(List<ChatMessage> chatHistory)
        {
            try
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

                        chatHistoryModel.ToolCallId = ((OpenAI.Chat.ToolChatMessage)chatMessage).ToolCallId.ToString();
                    }

                    if (chatMessage.Content != null && chatMessage.Content.Count > 0)
                    {
                        chatHistoryModel.Content = chatMessage.Content[0].Text;
                    }
                    else
                    {
                        chatHistoryModel.Content = string.Empty;
                    }

                    chatHistoryModelList.Add(chatHistoryModel);
                }

                string json = JsonSerializer.Serialize(
                    chatHistoryModelList,
                    new JsonSerializerOptions { WriteIndented = true }
                );

                return json;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Converts a JSON string into a list of ChatMessage objects.
        /// </summary>
        /// <param name="json">A JSON string representation of the chat history.</param>
        /// <returns>A List of ChatMessage objects reconstructed from the JSON.</returns>
        public static List<ChatMessage> ConvertJsonToChatHistory(string json)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<ChatMessage>();
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var modelList = JsonSerializer.Deserialize<List<ChatHistoryModel>>(json, options);

                List<ChatMessage> chatMessages = new();

                if (modelList is null)
                    return chatMessages;

                foreach (var model in modelList)
                {
                    ChatMessage message;

                    if (string.IsNullOrWhiteSpace(model.Content))
                    {
                        model.Content = string.Empty; 
                    }

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
                            message = new ToolChatMessage(model.ToolCallId, model.Content);
                            break;
                        default:
                            message = new SystemChatMessage("Not implemented Chat Message Role");
                            break;
                    }

                    chatMessages.Add(message);
                }
                return chatMessages;
            }
            catch(Exception ex)
            {
                throw new Exception (ex.Message);
            }
           
        }

        ///// <summary>
        ///// Converts a JSON string representing an array of objects into a DataTable.
        ///// The method handles dynamic JSON structure by adding new columns if they appear.
        ///// </summary>
        ///// <param name="json">The JSON string to convert.</param>
        ///// <returns>A DataTable populated with the JSON data.</returns>
        //public static DataTable ConvertJsonToDynamicDataTable(string json)
        //{
        //    try
        //    {
        //        DataTable dt = new DataTable();
        //        JObject jsonObject = JObject.Parse(json);

        //        if (!jsonObject.ContainsKey("tasks") || !(jsonObject["tasks"] is JArray jsonArray))
        //        {
        //            throw new Exception("Invalid JSON format: 'tasks' array not found.");
        //        }

        //        foreach (JObject obj in jsonArray)
        //        {
        //            foreach (var property in obj.Properties())
        //            {
        //                if (!dt.Columns.Contains(property.Name))
        //                {
        //                    dt.Columns.Add(property.Name, typeof(string)); // Use string as default type
        //                }
        //            }

        //            DataRow row = dt.NewRow();
        //            foreach (var property in obj.Properties())
        //            {
        //                row[property.Name] = property.Value?.ToString() ?? DBNull.Value.ToString();
        //            }
        //            dt.Rows.Add(row);
        //        }

        //        return dt;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Error converting JSON to DataTable: {ex.Message}");
        //    }
        //}


        /// <summary>
        /// Converts a DataTable to a JSON string representation.
        /// </summary>
        /// <param name="dataTable">The DataTable to convert.</param>
        /// <returns>A JSON string representation of the DataTable.</returns>
        public static string ConvertToJson(DataTable dataTable)
        {
            try
            {
                var rows = new List<Dictionary<string, object>>();

                foreach (DataRow row in dataTable.Rows)
                {
                    var rowDictionary = new Dictionary<string, object>();
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        rowDictionary[column.ColumnName] = row[column] ?? DBNull.Value;
                    }
                    rows.Add(rowDictionary);
                }

                var options = new JsonSerializerOptions { WriteIndented = true };
                var withTrailingQoute = '"' + System.Text.Json.JsonSerializer.Serialize(rows, options) + '"';
                return withTrailingQoute;
            }
            catch (Exception ex)
            {
                throw new Exception("Error converting DataTable to JSON", ex);
            }
        }


        /// <summary>
        /// Converts a JsonDocument to a JSON string representation.
        /// </summary>
        /// <param name="argumentsJson">The JsonDocument to convert.</param>
        /// <returns>A JSON string representation of the JsonDocument.</returns>
        public static string ConvertToJson(JsonDocument argumentsJson)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception("Error converting JsonDocument to JSON", ex);
            }
        }




    }
}
