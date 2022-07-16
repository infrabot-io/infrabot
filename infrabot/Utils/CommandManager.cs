using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using infrabot.PluginSystem.Enums;
using infrabot.PluginSystem.Execution;
using infrabot.PluginSystem;
using CliWrap;

namespace infrabot.Utils
{
    public class CommandManager
    {
        private List<Plugin> Plugins = null;

        public CommandManager()
        {
            Plugins = PluginsManager.Plugins;
        }

        public async Task HandleCommand(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Only process Message updates
            if (update.Message is not { } message)
                return;

            // Only process text messages
            if (message.Text is not { } messageText)
                return;

            // Get chat id, user`s name and id
            long chatId = message.Chat.Id;
            long userId = message.From.Id;
            string userName = message.From.Username;

            // /showmyid command logic
            if(messageText.ToLower() == "/showmyid" && Program.ConfigManagerInstance.Config.telegram_enable_showmyid == true)
            {
                WriteToLog("ShowMyId is requested by " + userId.ToString() + " with name " + userName);
                SendTelegramMessage(botClient, update, cancellationToken, chatId, "Your command was: " + messageText + "\n" + "Result: Your id is: " + userId.ToString() + " and your username is " + userName);
            }

            // Check if user has access to send commands to telegram bot
            if(Program.ConfigManagerInstance.Config.telegram_allowed_users_id.Count > 0 && Program.ConfigManagerInstance.Config.telegram_allowed_users_id.Contains(userId) == false)
            {
                WriteToLog("User " + userId.ToString() + " with name " + userName + " does not have access to send commands to bot. Sent message: " + messageText + ". Chat id is: " + chatId.ToString(), "WARNING");
                return;
            }

            // /getcommands command logic
            if (((Program.ConfigManagerInstance.Config.telegram_allowed_users_id_getcommands.Count > 0 && Program.ConfigManagerInstance.Config.telegram_allowed_users_id_getcommands.Contains(userId) == true) ||
                (Program.ConfigManagerInstance.Config.telegram_allowed_users_id_getcommands.Count == 0)) && messageText.ToLower() == "/getcommands")
            {
                string commandsList = "";
                foreach (Plugin plugin in Plugins)
                {
                    commandsList = commandsList + "*" + plugin.PluginExecution.ExecutionCommand + "* : _" + plugin.HelpShort + "_" + Environment.NewLine;
                }

                WriteToLog("GetCommands is requested by " + userId.ToString() + " with name " + userName);
                SendTelegramMessage(botClient, update, cancellationToken, chatId, commandsList, true);
            }

            // /reloadconfig command logic
            if (messageText.ToLower() == "/reloadconfig" && Program.ConfigManagerInstance.Config.telegram_enable_reloadconfig == true)
            {
                // Reload configuration
                Program.ConfigManagerInstance.LoadConfig();

                // Reload plugins
                PluginsManager.ReloadPlugins();

                // Send message
                WriteToLog("ReloadConfig is requested by " + userId.ToString() + " with name " + userName);
                SendTelegramMessage(botClient, update, cancellationToken, chatId, "Your command was: " + messageText + "\n" + "Result: Config and plugins have been reloaded!");
            }

            // /emergency command logic
            if (((Program.ConfigManagerInstance.Config.telegram_allowed_users_id_emergency.Count > 0 && Program.ConfigManagerInstance.Config.telegram_allowed_users_id_emergency.Contains(userId) == true) ||
                (Program.ConfigManagerInstance.Config.telegram_allowed_users_id_emergency.Count == 0)) && messageText.ToLower() == "/emergency")
            {
                // If we find specified file then delete it, and prevent application exit. 
                // In any other case application goes to exit loop and can not stop receiving /emergency command
                if (System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + "emergency.lock") == true)
                {
                    System.IO.File.Delete(AppDomain.CurrentDomain.BaseDirectory + "emergency.lock");
                    return;
                }

                // Send message that we got emergency exit
                WriteToLog("Emergency is requested by " + userId.ToString() + " with name " + userName, "WARNING");
                SendTelegramMessage(botClient, update, cancellationToken, chatId, "Your command was: " + messageText + "\n" + "Result: Exiting application now!");

                // Create temp file to prevent exit loop
                if (System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + "emergency.lock") == false)
                {
                    System.IO.File.Create(AppDomain.CurrentDomain.BaseDirectory + "emergency.lock");
                }
                
                // Exit application
                Environment.Exit(0);
            }
            
            // Execute logic from loaded plugins
            foreach(Plugin plugin in Plugins)
            {
                if (messageText.ToLower().Split(" ")[0].Equals(plugin.PluginExecution.ExecutionCommand.ToLower()))
                {
                    string[] messageParts = messageText.Split(" ");
                    string argumentsData = "";

                    // Show help manual if '?' argument specified
                    if (messageParts.Length > 1)
                    {
                        if (messageParts[1] == "?")
                        {
                            WriteToLog("Help manual is requested by " + userId.ToString() + " with name " + userName + " for command " + messageParts[0]);
                            SendTelegramMessage(botClient, 
                                update, 
                                cancellationToken, 
                                chatId, 
                                "Your command was: " + messageText + 
                                    "\nPlugin: " + plugin.Name + 
                                    "\nGuid: " + plugin.Guid +
                                    "\nAuthor: " + plugin.Author +
                                    "\nVersion: " + plugin.Version +
                                    "\nWebSite: " + plugin.WebSite +
                                    "\nHelp Manual: " + plugin.Help
                                );
                            return;
                        }
                    }

                    // Determine arguments based on inserted data
                    if (messageParts.Length > 0)
                    {
                        for (int i = 0; i < messageParts.Length; i += 1)
                        {
                            if (i == 0)
                                continue;

                            if(i == messageParts.Length - 1)
                                argumentsData = argumentsData + messageParts[i];
                            else
                                argumentsData = argumentsData + messageParts[i] + " ";
                        }
                    }

                    // Check if execution file actually exists
                    string executionFile = AppDomain.CurrentDomain.BaseDirectory + "plugins" + "\\" + plugin.Guid + "\\" + plugin.PluginExecution.ExecuteFile;
                    if (!System.IO.File.Exists(executionFile))
                    {
                        WriteToLog(messageParts[0] + " command execution is requested by " + userId.ToString() + " with name " + userName + " failed. File does not exist: " + executionFile, "ERROR");
                        SendTelegramMessage(botClient, update, cancellationToken, chatId, "Your command was: " + messageText + "\n" + "Error. File '" + executionFile + "' for execution does not exist. Make sure that file specified in the plugin`s configuration exists or redeploy plugin.");
                        return;
                    }

                    // If plugin execution path somehow became absent, use C:\Windows\System32 as a default working directory
                    string workingDirectory = AppDomain.CurrentDomain.BaseDirectory + "plugins" + "\\" + plugin.Guid;
                    if (!Directory.Exists(workingDirectory))
                        workingDirectory = Environment.SystemDirectory;

                    // Execution Output and Error datastore
                    var stdOutBuffer = new StringBuilder();
                    var stdErrBuffer = new StringBuilder();

                    if (plugin.PluginExecution.ExecuteType == (int)CommandExecuteTypes.PSScript)
                    {
                        WriteToLog(messageParts[0] + " command execution is requested by " + userId.ToString() + " with name " + userName + ". Executed PowerShell:" 
                            + Environment.NewLine + "    PowerShell path: " + Program.ConfigManagerInstance.Config.telegram_powershell_default_path
                            + Environment.NewLine + "    File path: " + executionFile
                            + Environment.NewLine + "    Config aguments: " + Program.ConfigManagerInstance.Config.telegram_powershell_arguments
                            + Environment.NewLine + "    Full aguments: " + Program.ConfigManagerInstance.Config.telegram_powershell_arguments + " -File \"" + executionFile + "\" " + argumentsData
                            + Environment.NewLine + "    Working directory: " + workingDirectory);

                        // Execute PowerShell script
                        await Cli.Wrap(Program.ConfigManagerInstance.Config.telegram_powershell_default_path)
                            .WithArguments(Program.ConfigManagerInstance.Config.telegram_powershell_arguments + " -File \"" + executionFile + "\" " + argumentsData)
                            .WithWorkingDirectory(workingDirectory)
                            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                            .WithValidation(CommandResultValidation.None)
                            .ExecuteAsync();
                    }
                    else if (plugin.PluginExecution.ExecuteType == (int)CommandExecuteTypes.AppExecutable)
                    {
                        WriteToLog(messageParts[0] + " command execution is requested by " + userId.ToString() + " with name " + userName + ". Executed app:"
                            + Environment.NewLine + "    File path: " + executionFile
                            + Environment.NewLine + "    Full aguments: " + argumentsData
                            + Environment.NewLine + "    Working directory: " + workingDirectory);

                        // Execute custom application
                        await Cli.Wrap(executionFile)
                            .WithArguments(argumentsData)
                            .WithWorkingDirectory(workingDirectory)
                            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                            .WithValidation(CommandResultValidation.None)
                            .ExecuteAsync();
                    }
                    
                    if (plugin.PluginExecution.ExecuteResults != null)
                    {
                        if (plugin.PluginExecution.ExecuteResults.Count > 0)
                        {
                            bool isResultExecuted = false;

                            // Output data based on the execution result
                            foreach (ExecuteResult executeResult in plugin.PluginExecution.ExecuteResults)
                            {
                                if (executeResult.ResultCheckType == (int)CommandResultCheckTypes.EqualsTo)
                                {
                                    if (stdOutBuffer.ToString().Equals(executeResult.ResultValue))
                                    {
                                        WriteToLog(messageParts[0] + " command execution is requested by " + userId.ToString() + " with name " + userName + ". Execute result check succeeded: EqualsTo");
                                        isResultExecuted = true;
                                        SendTelegramMessage(botClient, update, cancellationToken, chatId, "Your command was: " + messageText + "\n" + "Result: " + executeResult.ResultOutput.Replace("{DATA}", argumentsData).Replace("{RESULT}", stdOutBuffer.ToString()));
                                        return;
                                    }
                                }
                                else if (executeResult.ResultCheckType == (int)CommandResultCheckTypes.NotEquals)
                                {
                                    if (!stdOutBuffer.ToString().Equals(executeResult.ResultValue))
                                    {
                                        WriteToLog(messageParts[0] + " command execution is requested by " + userId.ToString() + " with name " + userName + ". Execute result check succeeded: NotEquals");
                                        isResultExecuted = true;
                                        SendTelegramMessage(botClient, update, cancellationToken, chatId, "Your command was: " + messageText + "\n" + "Result: " + executeResult.ResultOutput.Replace("{DATA}", argumentsData).Replace("{RESULT}", stdOutBuffer.ToString()));
                                        return;
                                    }
                                }
                                else if (executeResult.ResultCheckType == (int)CommandResultCheckTypes.GreaterThan)
                                {
                                    try
                                    {
                                        isResultExecuted = true;
                                        int outputToInt = Convert.ToInt32(stdOutBuffer.ToString().Trim());
                                        int checkToInt = Convert.ToInt32(executeResult.ResultValue.ToString().Trim());

                                        if (outputToInt > checkToInt)
                                        {
                                            WriteToLog(messageParts[0] + " command execution is requested by " + userId.ToString() + " with name " + userName + ". Execute result check succeeded: GreaterThan");
                                            SendTelegramMessage(botClient, update, cancellationToken, chatId, "Your command was: " + messageText + "\n" + "Result: " + executeResult.ResultOutput.Replace("{DATA}", argumentsData).Replace("{RESULT}", stdOutBuffer.ToString()));
                                            return;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        SendTelegramMessage(botClient, update, cancellationToken, chatId, "Could not compare. Output was: " + stdOutBuffer.ToString().Trim() + ". Error: " + ex.Message);
                                    }
                                }
                                else if (executeResult.ResultCheckType == (int)CommandResultCheckTypes.LessThan)
                                {
                                    try
                                    {
                                        isResultExecuted = true;
                                        int outputToInt = Convert.ToInt32(stdOutBuffer.ToString().Trim());
                                        int checkToInt = Convert.ToInt32(executeResult.ResultValue.ToString().Trim());

                                        if (outputToInt < checkToInt)
                                        {
                                            WriteToLog(messageParts[0] + " command execution is requested by " + userId.ToString() + " with name " + userName + ". Execute result check succeeded: LessThan");
                                            SendTelegramMessage(botClient, update, cancellationToken, chatId, "Your command was: " + messageText + "\n" + "Result: " + executeResult.ResultOutput.Replace("{DATA}", argumentsData).Replace("{RESULT}", stdOutBuffer.ToString()));
                                            return;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        SendTelegramMessage(botClient, update, cancellationToken, chatId, "Could not compare. Output was: " + stdOutBuffer.ToString().Trim() + ". Error: " + ex.Message);
                                    }
                                }
                                else if (executeResult.ResultCheckType == (int)CommandResultCheckTypes.Contains)
                                {
                                    if (stdOutBuffer.ToString().Contains(executeResult.ResultValue))
                                    {
                                        WriteToLog(messageParts[0] + " command execution is requested by " + userId.ToString() + " with name " + userName + ". Execute result check succeeded: Contains");
                                        isResultExecuted = true;
                                        SendTelegramMessage(botClient, update, cancellationToken, chatId, "Your command was: " + messageText + "\n" + "Result: " + executeResult.ResultOutput.Replace("{DATA}", argumentsData).Replace("{RESULT}", stdOutBuffer.ToString()));
                                        return;
                                    }
                                }
                                else if (executeResult.ResultCheckType == (int)CommandResultCheckTypes.StartsWith)
                                {
                                    if (stdOutBuffer.ToString().StartsWith(executeResult.ResultValue))
                                    {
                                        WriteToLog(messageParts[0] + " command execution is requested by " + userId.ToString() + " with name " + userName + ". Execute result check succeeded: StartsWith");
                                        isResultExecuted = true;
                                        SendTelegramMessage(botClient, update, cancellationToken, chatId, "Your command was: " + messageText + "\n" + "Result: " + executeResult.ResultOutput.Replace("{DATA}", argumentsData).Replace("{RESULT}", stdOutBuffer.ToString()));
                                        return;
                                    }
                                }
                                else if (executeResult.ResultCheckType == (int)CommandResultCheckTypes.EndsWith)
                                {
                                    if (stdOutBuffer.ToString().EndsWith(executeResult.ResultValue))
                                    {
                                        WriteToLog(messageParts[0] + " command execution is requested by " + userId.ToString() + " with name " + userName + ". Execute result check succeeded: EndsWith");
                                        isResultExecuted = true;
                                        SendTelegramMessage(botClient, update, cancellationToken, chatId, "Your command was: " + messageText + "\n" + "Result: " + executeResult.ResultOutput.Replace("{DATA}", argumentsData).Replace("{RESULT}", stdOutBuffer.ToString()));
                                        return;
                                    }
                                }
                                else if (executeResult.ResultCheckType == (int)CommandResultCheckTypes.NotContains)
                                {
                                    if (!stdOutBuffer.ToString().Contains(executeResult.ResultValue))
                                    {
                                        WriteToLog(messageParts[0] + " command execution is requested by " + userId.ToString() + " with name " + userName + ". Execute result check succeeded: NotContains");
                                        isResultExecuted = true;
                                        SendTelegramMessage(botClient, update, cancellationToken, chatId, "Your command was: " + messageText + "\n" + "Result: " + executeResult.ResultOutput.Replace("{DATA}", argumentsData).Replace("{RESULT}", stdOutBuffer.ToString()));
                                        return;
                                    }
                                }
                                else if (executeResult.ResultCheckType == (int)CommandResultCheckTypes.NotStartsWith)
                                {
                                    if (!stdOutBuffer.ToString().StartsWith(executeResult.ResultValue))
                                    {
                                        WriteToLog(messageParts[0] + " command execution is requested by " + userId.ToString() + " with name " + userName + ". Execute result check succeeded: NotStartsWith");
                                        isResultExecuted = true;
                                        SendTelegramMessage(botClient, update, cancellationToken, chatId, "Your command was: " + messageText + "\n" + "Result: " + executeResult.ResultOutput.Replace("{DATA}", argumentsData).Replace("{RESULT}", stdOutBuffer.ToString()));
                                        return;
                                    }
                                }
                                else if (executeResult.ResultCheckType == (int)CommandResultCheckTypes.NotEndsWith)
                                {
                                    if (!stdOutBuffer.ToString().EndsWith(executeResult.ResultValue))
                                    {
                                        WriteToLog(messageParts[0] + " command execution is requested by " + userId.ToString() + " with name " + userName + ". Execute result check succeeded: NotEndsWith");
                                        isResultExecuted = true;
                                        SendTelegramMessage(botClient, update, cancellationToken, chatId, "Your command was: " + messageText + "\n" + "Result: " + executeResult.ResultOutput.Replace("{DATA}", argumentsData).Replace("{RESULT}", stdOutBuffer.ToString()));
                                        return;
                                    }
                                }
                                else if (executeResult.ResultCheckType == (int)CommandResultCheckTypes.IgnoreCaseEqualsTo)
                                {
                                    if (stdOutBuffer.ToString().Equals(executeResult.ResultValue, StringComparison.OrdinalIgnoreCase))
                                    {
                                        WriteToLog(messageParts[0] + " command execution is requested by " + userId.ToString() + " with name " + userName + ". Execute result check succeeded: IgnoreCaseEqualsTo");
                                        isResultExecuted = true;
                                        SendTelegramMessage(botClient, update, cancellationToken, chatId, "Your command was: " + messageText + "\n" + "Result: " + executeResult.ResultOutput.Replace("{DATA}", argumentsData).Replace("{RESULT}", stdOutBuffer.ToString()));
                                        return;
                                    }
                                }
                                else if (executeResult.ResultCheckType == (int)CommandResultCheckTypes.IgnoreCaseContains)
                                {
                                    if (stdOutBuffer.ToString().Contains(executeResult.ResultValue, StringComparison.OrdinalIgnoreCase))
                                    {
                                        WriteToLog(messageParts[0] + " command execution is requested by " + userId.ToString() + " with name " + userName + ". Execute result check succeeded: IgnoreCaseContains");
                                        isResultExecuted = true;
                                        SendTelegramMessage(botClient, update, cancellationToken, chatId, "Your command was: " + messageText + "\n" + "Result: " + executeResult.ResultOutput.Replace("{DATA}", argumentsData).Replace("{RESULT}", stdOutBuffer.ToString()));
                                        return;
                                    }
                                }
                                else if (executeResult.ResultCheckType == (int)CommandResultCheckTypes.IgnoreCaseStartsWith)
                                {
                                    if (stdOutBuffer.ToString().StartsWith(executeResult.ResultValue, StringComparison.OrdinalIgnoreCase))
                                    {
                                        WriteToLog(messageParts[0] + " command execution is requested by " + userId.ToString() + " with name " + userName + ". Execute result check succeeded: IgnoreCaseStartsWith");
                                        isResultExecuted = true;
                                        SendTelegramMessage(botClient, update, cancellationToken, chatId, "Your command was: " + messageText + "\n" + "Result: " + executeResult.ResultOutput.Replace("{DATA}", argumentsData).Replace("{RESULT}", stdOutBuffer.ToString()));
                                        return;
                                    }
                                }
                                else if (executeResult.ResultCheckType == (int)CommandResultCheckTypes.IgnoreCaseEndsWith)
                                {
                                    if (stdOutBuffer.ToString().EndsWith(executeResult.ResultValue, StringComparison.OrdinalIgnoreCase))
                                    {
                                        WriteToLog(messageParts[0] + " command execution is requested by " + userId.ToString() + " with name " + userName + ". Execute result check succeeded: IgnoreCaseEndsWith");
                                        isResultExecuted = true;
                                        SendTelegramMessage(botClient, update, cancellationToken, chatId, "Your command was: " + messageText + "\n" + "Result: " + executeResult.ResultOutput.Replace("{DATA}", argumentsData).Replace("{RESULT}", stdOutBuffer.ToString()));
                                        return;
                                    }
                                }
                                else if (executeResult.ResultCheckType == (int)CommandResultCheckTypes.IgnoreCaseNotContains)
                                {
                                    if (!stdOutBuffer.ToString().Contains(executeResult.ResultValue, StringComparison.OrdinalIgnoreCase))
                                    {
                                        WriteToLog(messageParts[0] + " command execution is requested by " + userId.ToString() + " with name " + userName + ". Execute result check succeeded: IgnoreCaseNotContains");
                                        isResultExecuted = true;
                                        SendTelegramMessage(botClient, update, cancellationToken, chatId, "Your command was: " + messageText + "\n" + "Result: " + executeResult.ResultOutput.Replace("{DATA}", argumentsData).Replace("{RESULT}", stdOutBuffer.ToString()));
                                        return;
                                    }
                                }
                                else if (executeResult.ResultCheckType == (int)CommandResultCheckTypes.IgnoreCaseNotStartsWith)
                                {
                                    if (!stdOutBuffer.ToString().StartsWith(executeResult.ResultValue, StringComparison.OrdinalIgnoreCase))
                                    {
                                        WriteToLog(messageParts[0] + " command execution is requested by " + userId.ToString() + " with name " + userName + ". Execute result check succeeded: IgnoreCaseNotStartsWith");
                                        isResultExecuted = true;
                                        SendTelegramMessage(botClient, update, cancellationToken, chatId, "Your command was: " + messageText + "\n" + "Result: " + executeResult.ResultOutput.Replace("{DATA}", argumentsData).Replace("{RESULT}", stdOutBuffer.ToString()));
                                        return;
                                    }
                                }
                                else if (executeResult.ResultCheckType == (int)CommandResultCheckTypes.IgnoreCaseNotEndsWith)
                                {
                                    if (!stdOutBuffer.ToString().EndsWith(executeResult.ResultValue, StringComparison.OrdinalIgnoreCase))
                                    {
                                        WriteToLog(messageParts[0] + " command execution is requested by " + userId.ToString() + " with name " + userName + ". Execute result check succeeded: IgnoreCaseNotEndsWith");
                                        isResultExecuted = true;
                                        SendTelegramMessage(botClient, update, cancellationToken, chatId, "Your command was: " + messageText + "\n" + "Result: " + executeResult.ResultOutput.Replace("{DATA}", argumentsData).Replace("{RESULT}", stdOutBuffer.ToString()));
                                        return;
                                    }
                                }
                            }

                            // After check of all execute results, if still dont find match, just show default error message
                            if (isResultExecuted == false)
                            {
                                if(stdErrBuffer.ToString().Length == 0)
                                {
                                    WriteToLog(messageParts[0] + " command execution is requested by " + userId.ToString() + " with name " + userName + ". No execute result check succeeded. ErrorBuffer is empty. Default error returned: " + plugin.PluginExecution.DefaultErrorMessage.Replace("{DATA}", messageText).Replace("{RESULT}", stdOutBuffer.ToString()), "WARNING");
                                    SendTelegramMessage(botClient, update, cancellationToken, chatId, plugin.PluginExecution.DefaultErrorMessage.Replace("{DATA}", messageText).Replace("{RESULT}", stdOutBuffer.ToString()));
                                }
                                else 
                                { 
                                    WriteToLog(messageParts[0] + " command execution is requested by " + userId.ToString() + " with name " + userName + ". No execute result check succeeded. Returning ErrorBuffer since it is not empty: " + stdErrBuffer.ToString(), "ERROR");
                                    SendTelegramMessage(botClient, update, cancellationToken, chatId, "Error occured. Output was: '" + stdOutBuffer.ToString() + "'. Error was: " + stdErrBuffer.ToString());
                                }
                            }
                        }
                    }
                    else
                    {
                        // If we dont check for execute results, then show just output. If error occured show error message.
                        if (stdErrBuffer.ToString().Length == 0)
                        {
                            WriteToLog(messageParts[0] + " command execution is requested by " + userId.ToString() + " with name " + userName + ". Returning all output: " + stdOutBuffer.ToString());
                            SendTelegramMessage(botClient, update, cancellationToken, chatId, "Your command was: " + messageText + "\n" + "Result: " + stdOutBuffer.ToString());
                        }
                        else
                        {
                            WriteToLog(messageParts[0] + " command execution is requested by " + userId.ToString() + " with name " + userName + ". Returning ErrorBuffer since it is not empty: " + stdErrBuffer.ToString(), "ERROR");
                            SendTelegramMessage(botClient, update, cancellationToken, chatId, "Error occured. Output was: '" + stdOutBuffer.ToString() + "'. Error was: " + stdErrBuffer.ToString());
                        }
                    }
                }
            }
        }

        public Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            WriteToLog("Error in handle telegram message occured: " + ErrorMessage, "ERROR");
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private async void SendTelegramMessage(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken, long chatId, string message, bool useParseMode = false)
        {
            for (int i = 0; i < message.Length; i += 4096)
            {
                if (useParseMode == true)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        parseMode: ParseMode.Markdown,
                        text: message.Substring(i, Math.Min(4096, message.Length - i)),
                        cancellationToken: cancellationToken);
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: message.Substring(i, Math.Min(4096, message.Length - i)),
                        cancellationToken: cancellationToken);
                }
                if (i > Program.ConfigManagerInstance.Config.telegram_result_max_length)
                {
                    break;
                }
            }
        }

        public static void WriteToLog(string log, string logType = "INFO")
        {
            if (Program.ConfigManagerInstance.Config.telegram_enable_logging == true)
            {
                try
                {
                    // Create directory for logs if exists
                    if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "logs") == false)
                    {
                        Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "logs");
                    }

                    // Get current date
                    DateTime localDate = DateTime.Now;
                    System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + @"logs\log-" + localDate.Day + "-" + localDate.Month + "-" + localDate.Year + ".txt", localDate.ToString() + " [" + logType + "] : " + log + Environment.NewLine);
                }
                catch { }
            }
        }
    }
}
