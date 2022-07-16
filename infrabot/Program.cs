using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using infrabot.Utils;

namespace infrabot
{
    public class Program
    {
        public const string ServiceName = "infrabot.io";
        public static ConfigManager ConfigManagerInstance;
        public static PluginsManager PluginsManagerInstance;
        public static CommandManager CommandManagerInstance;
        public static TelegramBotClient BotClient = null;

        public class Service : ServiceBase
        {
            public Service()
            {
                ServiceName = Program.ServiceName;
            }

            protected override void OnStart(string[] args)
            {
                Program.Start(args);
            }

            protected override void OnStop()
            {
                Program.Stop();
            }
        }

        static void Main(string[] args)
        {
            if (!Environment.UserInteractive)
            {
                using (var service = new Service())
                {
                    ServiceBase.Run(service);
                }
            }
            else
            {
                Start(args);
                Stop();
            }
        }

        private static void Start(string[] args)
        {
            // Execute startup args
            StartupArgsExecutor.ExecuteArgs(args);

            // Init modules
            ConfigManagerInstance = new ConfigManager();
            PluginsManagerInstance = new PluginsManager();
            CommandManagerInstance = new CommandManager();

            // Init Telegram Bot client
            BotClient = new TelegramBotClient(ConfigManagerInstance.Config.telegram_bot_token);
            using var cts = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() };
            BotClient.StartReceiving(updateHandler: CommandManagerInstance.HandleCommand, pollingErrorHandler: CommandManagerInstance.HandleError, receiverOptions: receiverOptions, cancellationToken: cts.Token);
            var me = BotClient.GetMeAsync();

            // Detect if we are executed from the console
            if (Environment.UserInteractive)
            {
                Console.WriteLine("********************************");
                Console.WriteLine("           InfraBot.IO          ");
                Console.WriteLine("********************************");
                Console.WriteLine("");
                Console.WriteLine("Successfully connected to Telegram with the specified Token!");
                Console.WriteLine("");
                Console.WriteLine($"I am user {me.Id} and my name is {me.Result.Username}");
                Console.WriteLine("Now you can send commands to me from telegram!");
                Console.WriteLine("");
                Console.WriteLine("Press any key to exit application.");

                Console.Read();
                
                cts.Cancel();
            }
        }

        private static void Stop()
        {
            if (BotClient != null)
            {
                BotClient.CloseAsync();
            }
        }

        public static void SetMyCommandsForChat(List<BotCommand> botCommands)
        {
            BotClient.SetMyCommandsAsync(botCommands);
        }
    }
}
