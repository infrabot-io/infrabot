using System;
using System.ServiceProcess;
using Telegram.Bot;
using Telegram.Bot.Args;
using InfraBot.Core;

namespace InfraBot
{
    public static class Program
    {
        public const string ServiceName = "InfraBot.IO";
        static CommandCenter commandCenter;
        static ITelegramBotClient botClient;
        static PluginsManager pluginsManager;

        public class Service : ServiceBase
        {
            public Service() { ServiceName = Program.ServiceName; }
            protected override void OnStart(string[] args) { Program.Start(args); }
            protected override void OnStop() { Program.Stop(); }
        }

        static void Main(string[] args)
        {
            if (!Environment.UserInteractive)
            {
                // running as service
                using (var service = new Service())
                {
                    ServiceBase.Run(service);
                }
            }
            else
            {
                // running as console app
                Start(args);
                Stop();
            }
        }

        private static void Start(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "--install" || args[0] == "/i" || args[0] == "-i")
                {
                    Console.WriteLine("Got " + args[0] + " argument!");
                    Console.WriteLine("Starting service installation");
                    Console.WriteLine("");

                    Console.WriteLine("Service Install output:");
                    if (InfraBotInstaller.InstallService())
                    {
                        Console.WriteLine("");
                    }
                    else
                    {
                        Console.WriteLine("Something went wrong. Service has not been installed. Make sure that you have admin rights!");
                        Console.WriteLine("");
                    }
                    Console.WriteLine("Task finished!");
                    return;
                }
            }

            pluginsManager = new PluginsManager();
            commandCenter = new CommandCenter();
            botClient = new TelegramBotClient(commandCenter.GetTelegramToken());
            var me = botClient.GetMeAsync().Result;
            if (Environment.UserInteractive)
            {
                Console.WriteLine($"**********************");
                Console.WriteLine($"     InfraBot.IO      ");
                Console.WriteLine($"**********************");
                Console.WriteLine($"");
                Console.WriteLine($"Successfully connected to Telegram with the specified Token!");
                Console.WriteLine($"");
                Console.WriteLine($"I am user {me.Id} and my name is {me.FirstName}");
                Console.WriteLine($"Now you can send commands to me from telegram! Lets see what happens! Good luck!");
                Console.WriteLine($"And please do not spill coffee on your laptop while playing with me!");
                Console.WriteLine($"");
                Console.WriteLine("Press any key to exit application.");
            }
            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();
            Console.Read();
        }

        private static void Stop()
        {
            if (botClient != null)
            {
                botClient.StopReceiving();
            }
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text != null && e.Message.Text != "")
            {
                await commandCenter.ExecuteCommand(botClient, sender, e);
            }
        }
    }
}
