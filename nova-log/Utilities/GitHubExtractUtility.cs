using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace nova_log.Utilities
{
    public static class GithubExtractUtility
    {
        public static string ExtractFieldOptionsId(string responseBody, string fieldId)
        {
            using JsonDocument doc = JsonDocument.Parse(responseBody);

            if (!doc.RootElement.TryGetProperty("data", out JsonElement dataElement) ||
                !dataElement.TryGetProperty("node", out JsonElement nodeElement) ||
                !nodeElement.TryGetProperty("fields", out JsonElement fieldsElement) ||
                !fieldsElement.TryGetProperty("nodes", out JsonElement nodesElement))
            {
                Console.WriteLine("Error: Unexpected JSON structure.");
                return "{}";
            }

            var fieldsDict = new Dictionary<string, string>();

            foreach (var field in nodesElement.EnumerateArray())
            {
                if (field.TryGetProperty("id", out var idElement) && idElement.GetString() == fieldId)
                {
                    if (field.TryGetProperty("options", out var optionsElement))
                    {
                        foreach (var option in optionsElement.EnumerateArray())
                        {
                            if (option.TryGetProperty("id", out var optionIdElement) &&
                                option.TryGetProperty("name", out var nameElement))
                            {
                                string optionId = optionIdElement.GetString();
                                string optionName = nameElement.GetString();

                                if (!string.IsNullOrEmpty(optionId) && !string.IsNullOrEmpty(optionName))
                                {
                                    fieldsDict[optionName] = optionId;
                                }
                            }
                        }
                    }
                }
            }

            return JsonSerializer.Serialize(fieldsDict, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
