using Google.Apis.Sheets.v4.Data;
using nova_log.DataAccess;
using nova_log.Logic;
using nova_log.Models;
using nova_log.Utilities;
using OpenAI.Chat;
using System;
using System.Diagnostics.Tracing;
using System.Threading.Tasks;
using static Google.Apis.Requests.BatchRequest;

class Program
{
    static async Task Main(string[] args)
    {

        AgentTask agenttask = new AgentTask();
        await agenttask.CreateGithubTask();

    }

    //static async Task Main(string[] args)
    //{
    //    try
    //    {
    //        List<ChatMessage> _chatHistory = new();
    //        PromptHandler promptHandler = new();


    //        _chatHistory = await promptHandler.SendChatIntroPrompt(_chatHistory);

    //        string res = _chatHistory.Last().Content[0].Text;
    //        Console.WriteLine(res);

    //        string userPrompt;

    //        do
    //        {
    //            Console.WriteLine("\nEnter your message ('exit' to quit): ");
    //            userPrompt = Console.ReadLine();

    //            if (userPrompt?.ToLower() == "exit")
    //            {
    //                break;
    //            }
    //            _chatHistory.Add(new UserChatMessage(userPrompt));

    //            _chatHistory = await promptHandler.SendChatHistoryWithTools(_chatHistory);
    //            Console.WriteLine("\n");
    //            res = _chatHistory.Last().Content[0].Text;
    //            Console.WriteLine(res);


    //        } while (true);

    //        Console.WriteLine("\nConversation ended. Press any key to exit...");
    //        Console.ReadKey();
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"{ex.Message}");
    //        Console.ReadKey();
    //    }
    //}


}
