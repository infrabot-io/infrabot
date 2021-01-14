using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using Newtonsoft.Json;
using Telegram.Bot.Args;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using InfraBot.Enums;
using InfraBot.Serialization;

namespace InfraBot.Core
{
    public class CommandCenter
    {
        public static Config config;
        static string JsonConfigFileName = "config.json";
        static string JsonConfigFile = "";
        static CommandCenter()
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + JsonConfigFileName))
            {
                Console.WriteLine("File \"" + AppDomain.CurrentDomain.BaseDirectory + JsonConfigFileName + "\" was not found. Please check if this file exists!");
                Environment.Exit(0);
            }

            JsonConfigFile = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + JsonConfigFileName);
            config = JsonConvert.DeserializeObject<Config>(JsonConfigFile);
            config.telegram_commands = PluginsManager.LoadPlugins();
        }

        public async Task ExecuteCommand(ITelegramBotClient botClient, object sender, MessageEventArgs e)
        {
            long FromChatId = e.Message.Chat.Id;
            int FromUserId = e.Message.From.Id;
            string FromUserName = e.Message.From.Username;

            // SHOWMYID
            if (e.Message.Text.ToLower() == "/showmyid" && config.telegram_enable_showmyid == true)
            {
                WriteToLog("Somebody with `" + FromUserId.ToString() + "` from chat with id `" + FromChatId.ToString() + "` sent /showmyid command!");
                await botClient.SendTextMessageAsync(
                    chatId: e.Message.Chat,
                    text: "Your command was: " + e.Message.Text + "\n" + "Result: Your id is: " + FromUserId.ToString()
                );
                return;
            }

            if (config.telegram_allowed_users_id.Count > 0)
            {
                if (config.telegram_allowed_users_id.Contains(FromUserId) == false)
                {
                    WriteToLog("User with Username `" + FromUserName + "` and id `" + FromUserId.ToString() + "` sent message to Bot. Command was not executed, because user is not in the access list!");
                    return;
                }
            }

            // GETCOMMANDS
            if (e.Message.Text.ToLower() == "/getcommands")
            {
                if (config.telegram_allowed_users_id_getcommands.Count > 0 && !config.telegram_allowed_users_id_getcommands.Contains(FromUserId))
                {
                    WriteToLog("User is not in access Users list for /getcommands");
                    return;
                }
                string CommandsList = "";
                foreach (Command command in config.telegram_commands)
                {
                    if (command.command_show_in_get_commands_list == true)
                    {
                        CommandsList = CommandsList + "*" + command.command_starts_with + "* : _" + command.command_help_short + "_" + Environment.NewLine;
                    }
                }
                for (int i = 0; i < CommandsList.Length; i += 4096)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                        parseMode: ParseMode.Markdown,
                        text: CommandsList.Substring(i, Math.Min(4096, CommandsList.Length - i))
                    );
                    if (i > config.telegram_result_max_length)
                    {
                        break;
                    }
                }
            }

            // RELOADCONFIG
            if (e.Message.Text.ToLower() == "/reloadconfig" && config.telegram_enable_reloadconfig == true)
            {
                if (config.telegram_allowed_users_id_reloadconfig.Count > 0 && !config.telegram_allowed_users_id_reloadconfig.Contains(FromUserId))
                {
                    WriteToLog("User is not in the access Users list for /reloadconfig");
                    return;
                }

                JsonConfigFile = null;
                JsonConfigFile = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\" + JsonConfigFileName);
                config = null;
                config = JsonConvert.DeserializeObject<Config>(JsonConfigFile);
                config.telegram_commands = PluginsManager.LoadPlugins();
                WriteToLog("Somebody with `" + FromUserId.ToString() + "` from chat with id `" + FromChatId.ToString() + "` sent /reloadconfig command!");
                await botClient.SendTextMessageAsync(
                    chatId: e.Message.Chat,
                    text: "Your command was: " + e.Message.Text + "\n" + "Result: Config has been reloaded!"
                );
                return;
            }

            // EMERGENCY
            if (e.Message.Text.ToLower() == "/emergency" && config.telegram_enable_emergency == true)
            {
                if (config.telegram_allowed_users_id_emergency.Count > 0 && !config.telegram_allowed_users_id_emergency.Contains(FromUserId))
                {
                    WriteToLog("User is not in the access Users list for /emergency");
                    return;
                }
                WriteToLog("Somebody with Username `" + FromUserName + "` and with ID `" + FromUserId.ToString() + "` from chat with id `" + FromChatId.ToString() + "` sent /emergency command! Shutting down application!");
                await botClient.SendTextMessageAsync(
                    chatId: e.Message.Chat,
                    text: "Your command was: " + e.Message.Text + "\n" + "Result: Application emergency Exit Succeded. Anyways try to send any valid command again. If nothing comes, then everything is OK! You will have to run application manually!"
                );
                Environment.Exit(0);
            }

            // REMINDME
            if (e.Message.Text.ToLower().StartsWith("/remindme ") && config.telegram_enable_reminder == true)
            {
                if (config.telegram_allowed_users_id_remindme.Count > 0 && !config.telegram_allowed_users_id_remindme.Contains(FromUserId))
                {
                    WriteToLog("User is not in the access Users list for /emergency");
                    return;
                }
                WriteToLog("Somebody with Username `" + FromUserName + "` and with ID `" + FromUserId.ToString() + "` from chat with id `" + FromChatId.ToString() + "` created Reminder Task in Task Scheduler.");
                int hour = 0, min = 0;
                string Message = e.Message.Text.ToLower().Replace("/remindme ", "").Replace("\"", "");
                string[] MessageParts = Message.Split(" ");
                string MessageRemindText = "";
                if (Message == "?")
                {
                    await botClient.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                        text: "This command will remind you something at the specified time. Usage '/remindme 16 45 \"Must restart server\"'"
                    );
                    return;
                }
                if (MessageParts.Length > 2)
                {
                    try
                    {
                        hour = Convert.ToInt32(MessageParts[0]);
                        min = Convert.ToInt32(MessageParts[1]);
                    }
                    catch
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: e.Message.Chat,
                            text: "Please provide normal arguments. Hour and minute must be number! Example: `/remindme 16 45 Say hi to world`. Write `/remindme ?` for help"
                        );
                        return;
                    }
                }
                for (int i = 2; i < MessageParts.Length; i++)
                {
                    MessageRemindText = MessageRemindText + MessageParts[i] + " ";
                }
                if (MessageRemindText.EndsWith(" "))
                {
                    MessageRemindText = MessageRemindText.Remove(MessageRemindText.Length - 1);
                }
                SchedulerService.ScheduleTask(hour, min, 0, () =>
                {
                    botClient.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                        text: "Reminder: " + MessageRemindText
                    );
                });
                await botClient.SendTextMessageAsync(
                    chatId: e.Message.Chat,
                    text: "Reminder has been set at: " + hour.ToString() + ":" + min.ToString()
                );
            }

            /*if (e.Message.Text.ToLower().StartsWith("/scheduletask"))
            {
                WriteToLog("Somebody with Username `" + FromUserName + "` and with ID `" + FromUserId.ToString() + "` from chat with id `" + FromChatId.ToString() + "` created Task in Task Scheduler.");
                string Message = e.Message.Text.ToLower().Replace("/scheduletask ", "").Replace("\"", "");
                SchedulerService.ScheduleTask(15, 43, 0, () =>
                {
                    botClient.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                        text: Message
                    );
                });
            }*/

            foreach (Command command in config.telegram_commands)
            {
                if (e.Message.Text.ToLower().StartsWith(command.command_starts_with) && ((command.command_allowed_users_id.Count > 0 && command.command_allowed_users_id.Contains(FromUserId)) || command.command_allowed_users_id.Count == 0))
                {
                    WriteToLog("Got command: `" + e.Message.Text + "`. This command is in your commands list! Processing further!");
                    if (command.command_execute_type == (int)CommandExecuteTypes.AppWithArgs || command.command_execute_type == (int)CommandExecuteTypes.PSScriptWithArgs)
                    {
                        WriteToLog("Got command: `" + e.Message.Text + "`. This command execute type is `" + command.command_execute_type.ToString() + "`!");
                        string Message = e.Message.Text.ToLower().Replace("/", "");
                        string[] MessageParts = Message.Split(" ");

                        foreach (string MessagePart in MessageParts)
                        {
                            if (MessagePart == "?")
                            {
                                await botClient.SendTextMessageAsync(
                                    chatId: e.Message.Chat,
                                    text: "Your command was: " + e.Message.Text + "\n" + "Help Manual: " + command.command_help_manual
                                );
                                return;
                            }
                        }

                        if (MessageParts.Length < FindMaxNumber(command.command_data_id))
                        {
                            WriteToLog("Got command: `" + e.Message.Text + "`. Result: Command was wrong! Normal command was not provided!");
                            await botClient.SendTextMessageAsync(
                                chatId: e.Message.Chat,
                                text: "Your command was: " + e.Message.Text + "\n" + "Result: Your command was wrong! Please provide normal command!"
                            );
                            break;
                        }

                        string MessagesData = "";
                        foreach (int MessagePart in command.command_data_id)
                        {
                            MessagesData += MessageParts[MessagePart - 1] + " ";
                        }
                        if (MessagesData.EndsWith(" "))
                        {
                            MessagesData = MessagesData.Remove(MessagesData.Length - 1);
                        }

                        WriteToLog("Got command: `" + e.Message.Text + "`. Message calculated arguments are: " + MessagesData);
                        Task<string> aa = null;
                        var ExecuteCommand = aa;
                        if (command.command_execute_type == 1)
                        {
                            WriteToLog("Got command: `" + e.Message.Text + "`. Command calculated arguments are: " + command.command_execute_file);
                            ExecuteCommand = ExecAsync(command.command_execute_file, MessagesData);
                        }
                        else if (command.command_execute_type == 3)
                        {
                            WriteToLog("Got command: `" + e.Message.Text + "`. Command calculated arguments are: " + config.telegram_powershell_path + " " + config.telegram_powershell_executionpolicy + " -File \"" + command.command_execute_file + "\" " + MessagesData);
                            ExecuteCommand = ExecAsync(config.telegram_powershell_path, " " + config.telegram_powershell_executionpolicy + " -File \"" + command.command_execute_file + "\" " + MessagesData);
                        }

                        if (command.command_execute_results.Count == 0)
                        {
                            WriteToLog("Command `" + command.command_starts_with + "` does not have any results!");
                            break;
                        }

                        bool ContainsResultType1 = false;
                        bool ContainsResultType2 = false;
                        bool ContainsResultType3 = false;
                        bool ContainsResultType4 = false;
                        foreach (ExecuteResult executeresult in command.command_execute_results)
                        {
                            if (executeresult.result_checktype == (int)ResultCheckTypes.Equals)
                            {
                                if (ExecuteCommand.Result == executeresult.result_value)
                                {
                                    ContainsResultType1 = true;
                                }
                            }
                            else if (executeresult.result_checktype == (int)ResultCheckTypes.Contains)
                            {
                                if (ExecuteCommand.Result.Contains(executeresult.result_value))
                                {
                                    ContainsResultType2 = true;
                                }
                            }
                            else if (executeresult.result_checktype == (int)ResultCheckTypes.StartsWith)
                            {
                                if (ExecuteCommand.Result.StartsWith(executeresult.result_value))
                                {
                                    ContainsResultType3 = true;
                                }
                            }
                            else if (executeresult.result_checktype == (int)ResultCheckTypes.EndsWith)
                            {
                                if (ExecuteCommand.Result.EndsWith(executeresult.result_value))
                                {
                                    ContainsResultType4 = true;
                                }
                            }
                        }
                        if (ContainsResultType1 == false && ContainsResultType2 == false && ContainsResultType3 == false && ContainsResultType4 == false)
                        {
                            WriteToLog("Got command: `" + e.Message.Text + "`. Result: " + command.command_default_error.Replace("{DATA}", MessagesData).Replace("{RESULT}", ExecuteCommand.Result));
                            await botClient.SendTextMessageAsync(
                                chatId: e.Message.Chat,
                                text: "Your command was: " + e.Message.Text + "\n" + "Result: " + command.command_default_error.Replace("{DATA}", MessagesData).Replace("{RESULT}", ExecuteCommand.Result)
                            );
                            break;
                        }

                        foreach (ExecuteResult executeresult in command.command_execute_results)
                        {
                            if (executeresult.result_checktype == (int)ResultCheckTypes.Equals)
                            {
                                if (ExecuteCommand.Result == executeresult.result_value)
                                {
                                    WriteToLog("Got command: `" + e.Message.Text + "`. Result check type Equals. Result: " + executeresult.result_output.Replace("{DATA}", MessagesData).Replace("{RESULT}", ExecuteCommand.Result));
                                    await botClient.SendTextMessageAsync(
                                        chatId: e.Message.Chat,
                                        text: "Your command was: " + e.Message.Text + "\n" + "Result: " + executeresult.result_output.Replace("{DATA}", MessagesData).Replace("{RESULT}", ExecuteCommand.Result)
                                    );
                                    break;
                                }
                            }
                            else if (executeresult.result_checktype == (int)ResultCheckTypes.Contains)
                            {
                                if (ExecuteCommand.Result.Contains(executeresult.result_value))
                                {
                                    WriteToLog("Got command: `" + e.Message.Text + "`. Result check type Contains. Result: " + executeresult.result_output.Replace("{DATA}", MessagesData).Replace("{RESULT}", ExecuteCommand.Result));
                                    await botClient.SendTextMessageAsync(
                                        chatId: e.Message.Chat,
                                        text: "Your command was: " + e.Message.Text + "\n" + "Result: " + executeresult.result_output.Replace("{DATA}", MessagesData).Replace("{RESULT}", ExecuteCommand.Result)
                                    );
                                    break;
                                }
                            }
                            else if (executeresult.result_checktype == (int)ResultCheckTypes.StartsWith)
                            {
                                if (ExecuteCommand.Result.StartsWith(executeresult.result_value))
                                {
                                    WriteToLog("Got command: `" + e.Message.Text + "`. Result check type Starts With. Result: " + executeresult.result_output.Replace("{DATA}", MessagesData).Replace("{RESULT}", ExecuteCommand.Result));
                                    await botClient.SendTextMessageAsync(
                                        chatId: e.Message.Chat,
                                        text: "Your command was: " + e.Message.Text + "\n" + "Result: " + executeresult.result_output.Replace("{DATA}", MessagesData).Replace("{RESULT}", ExecuteCommand.Result)
                                    );
                                    break;
                                }
                            }
                            else if (executeresult.result_checktype == (int)ResultCheckTypes.EndsWith)
                            {
                                if (ExecuteCommand.Result.EndsWith(executeresult.result_value))
                                {
                                    WriteToLog("Got command: `" + e.Message.Text + "`. Result check type Ends With. Result: " + executeresult.result_output.Replace("{DATA}", MessagesData).Replace("{RESULT}", ExecuteCommand.Result));
                                    await botClient.SendTextMessageAsync(
                                        chatId: e.Message.Chat,
                                        text: "Your command was: " + e.Message.Text + "\n" + "Result: " + executeresult.result_output.Replace("{DATA}", MessagesData).Replace("{RESULT}", ExecuteCommand.Result)
                                    );
                                    break;
                                }
                            }
                        }
                    }
                    else if (command.command_execute_type == (int)CommandExecuteTypes.AppWithoutArgs || command.command_execute_type == (int)CommandExecuteTypes.PSScriptWithoutArgs)
                    {
                        WriteToLog("Got command: `" + e.Message.Text + "`. Result execute type is " + command.command_execute_type.ToString());
                        string Message = e.Message.Text.ToLower().Replace("/", "");
                        string[] MessageParts = Message.Split(" ");
                        if (MessageParts.Length > 1)
                        {
                            if (MessageParts[1] == "?" && MessageParts[1] != "" && MessageParts[1] != null)
                            {
                                await botClient.SendTextMessageAsync(
                                    chatId: e.Message.Chat,
                                    text: "Your command was: " + e.Message.Text + "\n" + "Help Manual: " + command.command_help_manual
                                );
                                break;
                            }
                        }

                        Task<string> aa = null;
                        var ExecuteCommand = aa;
                        if (command.command_execute_type == 2)
                        {
                            WriteToLog("Got command: `" + e.Message.Text + "`. Command calculated arguments are: " + command.command_execute_file);
                            ExecuteCommand = ExecAsync(command.command_execute_file, "");
                        }
                        else if (command.command_execute_type == 4)
                        {
                            WriteToLog("Got command: `" + e.Message.Text + "`. Command calculated arguments are: " + config.telegram_powershell_path + " " + config.telegram_powershell_executionpolicy + " -File \"" + command.command_execute_file + "\"");
                            ExecuteCommand = ExecAsync(config.telegram_powershell_path, " " + config.telegram_powershell_executionpolicy + " -File \"" + command.command_execute_file + "\"");
                        }
                        WriteToLog("Got command: `" + e.Message.Text + "`. Result was sent to user");

                        for (int i = 0; i < ExecuteCommand.Result.Length; i += 4000)
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: e.Message.Chat,
                                text: "Your command was: " + e.Message.Text + "\n" + "Result (" + i.ToString() + "): " + ExecuteCommand.Result.Substring(i, Math.Min(4000, ExecuteCommand.Result.Length - i))
                            );
                            if (i > config.telegram_result_max_length)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        public string GetTelegramToken()
        {
            return config.telegram_bot_token;
        }

        public static int FindMaxNumber(List<int> array)
        {
            int maxInt = Int32.MinValue;
            for (int i = 0; i < array.Count; i++)
            {
                int value = array[i];
                if (value > maxInt)
                {
                    maxInt = value;
                }
            }
            return maxInt;
        }

        public static void WriteToLog(string Log)
        {
            if (config.telegram_enable_logging)
            {
                try
                {
                    if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "logs") == false)
                    {
                        Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "logs");
                    }
                    DateTime localDate = DateTime.Now;
                    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + @"logs\logs.log", localDate.ToString() + ": " + Log + Environment.NewLine);
                }
                catch { }
            }
        }

        public static async Task<string> ExecAsync(string command, string args)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = command;
            psi.Arguments = args;
            psi.RedirectStandardOutput = true;

            using (Process proc = Process.Start(psi))
            {
                await proc.WaitForExitAsync();
                return proc.StandardOutput.ReadToEnd();
            }
        }
    }
}