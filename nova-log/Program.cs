using Google.Apis.Sheets.v4.Data;
using nova_log.DataAccess;
using nova_log.Logic;
using nova_log.Models;
using nova_log.Utilities;
using OpenAI.Chat;
using System;
using System.Diagnostics.Tracing;
using static Google.Apis.Requests.BatchRequest;

class Program
{
    static async Task Main(string[] args)
    {

        //AgentTask agentTask = new AgentTask();
        //await agentTask.CreateGithubTask();


        List<ChatMessage> _chatHistory = new();
        PromptHandler promptHandler = new();


        RequestModel response = await promptHandler.SendChatIntroPrompt();

        List<ChatMessage> chatHistory = ConvertionUtility.ConvertJsonToChatHistory(response.ChatHistory);
        string res = chatHistory.Last().Content[0].Text;
        Console.WriteLine(res);

        string userPrompt;

        do
        {
            Console.WriteLine("\nEnter your message ('exit' to quit): ");
            response.UserPrompt = Console.ReadLine();

            if (response.UserPrompt?.ToLower() == "exit")
            {
                break;
            }

            response = await promptHandler.SendChatHistoryWithTools(response);
            Console.WriteLine("\n");
            chatHistory = ConvertionUtility.ConvertJsonToChatHistory(response.ChatHistory);
            Console.WriteLine(chatHistory.Last().Content[0].Text);


        } while (true);

        Console.WriteLine("\nConversation ended. Press any key to exit...");
        Console.ReadKey();
    }
}
