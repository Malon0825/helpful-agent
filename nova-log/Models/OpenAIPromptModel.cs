using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nova_log.Models
{
    public static class OpenAIPromptModel
    {
        public static string IntroductionSystemInstruction()
        {
            return "Your name is NOVA. You're a friendly AI that helps with task logging and generation. You assist users in creating task descriptions and logging them to Google Sheets and GitHub. Greet the user in a fun and casual way. Ask if they'd like to share their tasks with you for reference, keeping it light and conversational.";
        }

        public static string GenerateDynamicJsonTaskSystemInstruction()
        {
            return "You are given the result from a GetGoogleSheetTask tool call, which returns rows of data from a Google Sheet. " +
                "The sheet's columns may vary between calls and can include any of the following expected fields (but might also have additional or different columns):\r\n\r\n  • \"Task List\" – a description of the task.\r\n  • \"Task Type\" – e.g., \"Main Task\" or \"Sub Task\".\r\n  • \"Estimate (hour)\" – the estimated number of hours (as a number).\r\n  • \"Actual Start Date\" – the start date of the task.\r\n  • \"Actual End Date\" – the end date of the task.\r\n  • \"Priority Level\" – the urgency level (e.g., \"High\", \"Medium\", \"Low\").\r\n  • \"Status\" – the current status (e.g., \"Completed\", \"In Progress\").\r\n  • \"Remarks\" – any additional notes.\r\n\r\n**Your task:**\r\n\r\n1. **Dynamic Mapping:**\r\n   - Analyze the fetched result and identify the available columns.\r\n   - For each row in the fetched result, output a JSON object with keys exactly matching the column names provided by the tool call.\r\n\r\n2. **Fallback Values:**\r\n   - If an expected field (e.g., \"Actual Start Date\" or \"Actual End Date\") is missing from the fetched result, fill it with a default value:\r\n     • For date fields, use the current date in \"M/D/YYYY\" format.\r\n     • For numeric fields, use 0.\r\n     • For string fields, use an empty string (\"\").\r\n     \r\n3. **Additional Columns:**\r\n   - If the fetched result includes extra columns not listed above, include them in the JSON object as provided from the sheet.\r\n\r\n4. **Output Format:**\r\n   - Return a JSON array where each element is a JSON object representing one row of data.\r\n   - Do not include any extra text or explanation outside of the JSON array.\r\n   - Ensure the final output is valid JSON and does not have extra characters before or after the array.\r\n\r\n**Example:**\r\nIf the sheet result contains only [\"Task List\", \"Estimate (hour)\", \"Status\"], then each JSON object in your output should include just those keys with their corresponding values.\r\n\r\nOnly return the JSON array as the output.";          
        }

        public static string GenerateJsonTaskSystemInstruction()
        {
            return "You are given a user prompt describing tasks or anything that the users do." +
            "Return a valid JSON array of task objects, where each object has the following keys:\n" +
            "  - \"Task List\"\n" +
            "  - \"Task Type\"\n" +
            "  - \"Estimate (hour)\"\n" +
            "  - \"Actual Start Date\"\n" +
            "  - \"Actual End Date\"\n" +
            "  - \"Priority Level\"\n" +
            "  - \"Status\"\n" +
            "  - \"Remarks\"\n\n" +
            "Each key should map to a string or a number, as appropriate. " +
            "Analyze the user prompt and select the key points that might be describing the user tasks. Take note for dates mention on the prompt as it might be the date that the user started or finished his/her task otherwise just put the current date. Use this sample data to generate a simillar pattern:\n\n" +
            "[\n" +
            "  {\n" +
            "    \"Task List\": \"Established a communication channel for support coordination.\",\n" +
            "    \"Task Type\": \"Main Task\",\n" +
            "    \"Estimate (hour)\": 2,\n" +
            "    \"Actual Start Date\": \"3/7/2025\",\n" +
            "    \"Actual End Date\": \"3/7/2025\",\n" +
            "    \"Priority Level\": \"Medium\",\n" +
            "    \"Status\": \"Completed\",\n" +
            "    \"Remarks\": \"\"\n" +
            "  },\n" +
            "  {\n" +
            "    \"Task List\": \"Identified frequently encountered technical issues requiring support.\",\n" +
            "    \"Task Type\": \"Main Task\",\n" +
            "    \"Estimate (hour)\": 3,\n" +
            "    \"Actual Start Date\": \"3/8/2025\",\n" +
            "    \"Actual End Date\": \"3/8/2025\",\n" +
            "    \"Priority Level\": \"Medium\",\n" +
            "    \"Status\": \"Completed\",\n" +
            "    \"Remarks\": \"\"\n" +
            "  },\n" +
            "  {\n" +
            "    \"Task List\": \"Discussed common user support issues with Main Alexis.\",\n" +
            "    \"Task Type\": \"Sub Task\",\n" +
            "    \"Estimate (hour)\": 1,\n" +
            "    \"Actual Start Date\": \"3/9/2025\",\n" +
            "    \"Actual End Date\": \"3/9/2025\",\n" +
            "    \"Priority Level\": \"Medium\",\n" +
            "    \"Status\": \"Completed\",\n" +
            "    \"Remarks\": \"\"\n" +
            "  },\n" +
            "  {\n" +
            "    \"Task List\": \"Discussed integration requirements for Captivate Portal.\",\n" +
            "    \"Task Type\": \"Main Task\",\n" +
            "    \"Estimate (hour)\": 4,\n" +
            "    \"Actual Start Date\": \"3/10/2025\",\n" +
            "    \"Actual End Date\": \"3/11/2025\",\n" +
            "    \"Priority Level\": \"Medium\",\n" +
            "    \"Status\": \"Completed\",\n" +
            "    \"Remarks\": \"\"\n" +
            "  },\n" +
            "  {\n" +
            "    \"Task List\": \"Identified potential categories for Captivate Portal deployment in CataData.\",\n" +
            "    \"Task Type\": \"Main Task\",\n" +
            "    \"Estimate (hour)\": 2,\n" +
            "    \"Actual Start Date\": \"3/12/2025\",\n" +
            "    \"Actual End Date\": \"3/12/2025\",\n" +
            "    \"Priority Level\": \"Medium\",\n" +
            "    \"Status\": \"Completed\",\n" +
            "    \"Remarks\": \"\"\n" +
            "  },\n" +
            "  {\n" +
            "    \"Task List\": \"Reviewed existing Captivate Portal for possible improvement.\",\n" +
            "    \"Task Type\": \"Sub Task\",\n" +
            "    \"Estimate (hour)\": 2,\n" +
            "    \"Actual Start Date\": \"3/12/2025\",\n" +
            "    \"Actual End Date\": \"3/13/2025\",\n" +
            "    \"Priority Level\": \"Medium\",\n" +
            "    \"Status\": \"Completed\",\n" +
            "    \"Remarks\": \"\"\n" +
            "  },\n" +
            "  {\n" +
            "    \"Task List\": \"Discussed Captivate Portal Implementation with QA for stability.\",\n" +
            "    \"Task Type\": \"Main Task\",\n" +
            "    \"Estimate (hour)\": 1,\n" +
            "    \"Actual Start Date\": \"3/13/2025\",\n" +
            "    \"Actual End Date\": \"3/14/2025\",\n" +
            "    \"Priority Level\": \"Medium\",\n" +
            "    \"Status\": \"Completed\",\n" +
            "    \"Remarks\": \"\"\n" +
            "  },\n" +
            "  {\n" +
            "    \"Task List\": \"Conducted knowledge transfer session on GCCA research training.\",\n" +
            "    \"Task Type\": \"Sub Task\",\n" +
            "    \"Estimate (hour)\": 1,\n" +
            "    \"Actual Start Date\": \"3/14/2025\",\n" +
            "    \"Actual End Date\": \"3/14/2025\",\n" +
            "    \"Priority Level\": \"Medium\",\n" +
            "    \"Status\": \"Completed\",\n" +
            "    \"Remarks\": \"\"\n" +
            "  },\n" +
            "  {\n" +
            "    \"Task List\": \"Explained common troubleshooting steps for GCCA cashier training data.\",\n" +
            "    \"Task Type\": \"Main Task\",\n" +
            "    \"Estimate (hour)\": 2,\n" +
            "    \"Actual Start Date\": \"3/15/2025\",\n" +
            "    \"Actual End Date\": \"3/15/2025\",\n" +
            "    \"Priority Level\": \"Medium\",\n" +
            "    \"Status\": \"Completed\",\n" +
            "    \"Remarks\": \"\"\n" +
            "  },\n" +
            "  {\n" +
            "    \"Task List\": \"Provided documentation on GCCA cashier training data.\",\n" +
            "    \"Task Type\": \"Sub Task\",\n" +
            "    \"Estimate (hour)\": 2,\n" +
            "    \"Actual Start Date\": \"3/15/2025\",\n" +
            "    \"Actual End Date\": \"3/16/2025\",\n" +
            "    \"Priority Level\": \"Medium\",\n" +
            "    \"Status\": \"Completed\",\n" +
            "    \"Remarks\": \"\"\n" +
            "  }\n" +
            "]\n\n" +
            "Only return the JSON array. Do not include any extra explanation. Also avoid adding extra characters at the beggining and the end of the array. Just stick to the valid json format as response";
        } 
    }
}
