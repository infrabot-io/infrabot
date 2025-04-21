using Infrabot.TelegramService.Core;
using Infrabot.PluginSystem;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Microsoft.EntityFrameworkCore;
using Infrabot.PluginSystem.Execution;
using System.Text;
using CustomExtensions;
using Infrabot.PluginSystem.Utils;
using Infrabot.PluginSystem.Enums;
using Newtonsoft.Json;
using CliWrap;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Infrabot.Common.Contexts;

namespace Infrabot.TelegramService.Managers
{
    public class CommandManager
    {
        private readonly ILogger<CommandManager> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly List<ICommandHandler> _builtInHandlers;
        private ITelegramBotClient _botClient;
        private Common.Models.Configuration _configuration;
        private ITelegramResponder _telegramResponder;
        private IPluginRegistry _pluginRegistry;

        public CommandManager(ILogger<CommandManager> logger, ICommandHandlerFactory commandFactory)
        {
            _logger = logger;
            _builtInHandlers = commandFactory.GetBuiltInCommands();
            _botClient = commandFactory.GetBotClient();
            _configuration = commandFactory.GetConfiguration();
            _telegramResponder = commandFactory.GetTelegramResponder();
            _pluginRegistry = commandFactory.GetPluginRegistry();
            _scopeFactory = commandFactory.GetServiceScopeFactory();
            _logger.LogInformation("Init: Command manager");
        }

        public async Task HandleCommand(Message message)
        {
            // Check if user has permission to write to bot
            if (HasPermissionToWriteToBot(Convert.ToInt32(message.From?.Id), message) == false)
            {
                _logger.LogDebug($"User {message.From?.Username} with id {message.From?.Id} does not have permission to write to bot");
                return;
            }

            // Parse the command
            if (TryParseCommand(message.Text, out var command, out var botName, out var pluginId, out bool isConfidential, out var arguments))
            {
                // Check if command was adressed to bot
                if (!string.IsNullOrEmpty(botName))
                {
                    var me = await _botClient.GetMe();
                    if (me.Username?.ToLower() != botName)
                    {
                        // Ignoring message because it was not addressed to bot
                        _logger.LogDebug($"Command: {command} from {message.From?.Username}. Will not execute because command was not adressed to bot {me.Username?.ToLower()}");
                        return;
                    }
                }
                
                // Built-in commands handling
                var handler = _builtInHandlers.FirstOrDefault(h => h.Command == command);
                if (handler != null)
                {
                    await handler.ExecuteAsync(message);
                    return;
                }

                if (_pluginRegistry.Plugins.Count > 0)
                {
                    foreach (Plugin plugin in _pluginRegistry.Plugins)
                    {
                        if (!string.IsNullOrEmpty(pluginId))
                        {
                            if (pluginId != plugin.Id)
                            {
                                _logger.LogDebug($"Command: {command} from {message.From?.Username}. Will not execute plugin {plugin.Id} because command was not adressed to it.");
                                continue;
                            }
                        }

                        if (HasPermissionToPlugin(plugin.Guid, Convert.ToInt32(message.From?.Id)) == false)
                        {
                            _logger.LogInformation($"User {message.From?.Username} with id {message.From?.Id} does not have permission to plugin {plugin.Name} with GUID {plugin.Guid}");
                            return;
                        }

                        if (plugin.PluginExecutions.Count > 0)
                        {
                            foreach (PluginExecution pluginExecution in plugin.PluginExecutions)
                            {
                                if (pluginExecution.CommandName == command)
                                {
                                    // Add telegram message to logs
                                    await AddLogToTelegramMessages(message, isConfidential);

                                    // Show plugin info
                                    if (arguments.Count > 0 && arguments.First() == "??")
                                    {
                                        await _telegramResponder.SendPlain(message.Chat, $"Plugin: {plugin.Name}\nId: {plugin.Id}\nAuthor: {plugin.Author}\nGuid: {plugin.Guid.ToString()}\nDescription: {plugin.Description}\nVersion: {plugin.Version}\nWebsite: {plugin.WebSite}");
                                        _logger.LogDebug($"Command: {command} from {message.From?.Username}. First argument is ??, so will send plugin information to user");
                                        return;
                                    }

                                    // Show help manual if requested
                                    if (arguments.Count > 0 && arguments.First() == "?")
                                    {
                                        await ShowHelpManual(message, pluginExecution);
                                        return;
                                    }

                                    // Check if execution file actually exists
                                    string executionFile = Path.Combine(_pluginRegistry.GetPluginDirectory(), plugin.Guid.ToString(), pluginExecution.ExecutionFilePath);
                                    if (!File.Exists(executionFile))
                                    {
                                        await _telegramResponder.SendMarkdown(message.Chat, $"❌ Error: File *{executionFile.Replace("\\", "\\\\")}* does not exist.\n🔄 Make sure that file specified in the plugins configuration exists or redeploy the plugin.".EscapeMarkdown());
                                        _logger.LogDebug($"Command: {command} from {message.From?.Username}. Error: The command was aborted because execution file {executionFile} from the plugin is missing. Please redeploy the plugin.");
                                        return;
                                    }

                                    // Check execution file hash to prevent security breach
                                    string executableFileHash = HashUtility.CalculateSHA256(executionFile);
                                    if (plugin.PluginFiles == null || plugin.PluginFiles.All(file => file.FileHash != executableFileHash))
                                    {
                                        await _telegramResponder.SendMarkdown(message.Chat, $"❌ `Error:` The file *{executionFile.Replace("\\", "\\\\")}* does not match the original version.\n🔄 Please redeploy the plugin.".EscapeMarkdown());
                                        _logger.LogDebug($"Command: {command} from {message.From?.Username}. Error: The command was aborted because execution file hash differs from the original file. Please redeploy the plugin.");
                                        return;
                                    }

                                    // Check if arguments metadata specified
                                    // If specified check if user specified all of them
                                    if (pluginExecution.ExecutionFileArguments != null && pluginExecution.ExecutionFileArguments.Count > 0)
                                    {
                                        if (arguments.Count > pluginExecution.ExecutionFileArguments.Count)
                                        {
                                            await _telegramResponder.SendMarkdown(message.Chat, $"❌ `Error:` You specified more arguments *({arguments.Count})* than this command supports *({pluginExecution.ExecutionFileArguments.Count})*.\n ✅ Write '{command} ?' to get information.".EscapeMarkdown());
                                            _logger.LogDebug($"Command: {command} from {message.From?.Username}. Error: The command was aborted because user specified more arguments {arguments.Count} than this command supports ({pluginExecution.ExecutionFileArguments.Count}).");
                                            return;
                                        }

                                        var missing = new List<ExecutionFileArgument>();
                                        for (int i = 0; i < pluginExecution.ExecutionFileArguments.Count; i++)
                                        {
                                            // If arguments doesn't have an element at index 'i'
                                            // it means pluginExecution.ExecutionFileArguments[i] is missing from arguments
                                            if (i >= arguments.Count)
                                            {
                                                missing.Add(pluginExecution.ExecutionFileArguments[i]);
                                            }
                                        }

                                        if (missing.Any())
                                        {
                                            string missingArguments = "";
                                            foreach (ExecutionFileArgument executionFileArgument in missing)
                                            {
                                                missingArguments += $"   *'{executionFileArgument.Name}'* - {executionFileArgument.Description} (Example: {executionFileArgument.Value})\n";
                                            }

                                            await _telegramResponder.SendMarkdown(message.Chat, $"❌ `Error:` There are some missing arguments :\n {missingArguments}".EscapeMarkdown());
                                            _logger.LogDebug($"Command: {command} from {message.From?.Username}. Error: The command was aborted because there are some missing arguments {missingArguments}");
                                            return;
                                        }
                                    }

                                    // If plugin execution path somehow became absent, use C:\Windows\System32 as a default working directory
                                    string workingDirectory = Path.Combine(_pluginRegistry.GetPluginDirectory(), plugin.Guid.ToString());
                                    if (!Directory.Exists(workingDirectory))
                                    {
                                        workingDirectory = Environment.SystemDirectory;
                                        _logger.LogDebug($"Command: {command} from {message.From?.Username}. Working directory was set to {workingDirectory} because plugin execution path somehow became absent");
                                    }

                                    // Define cancellation token for execution timeout
                                    using var cliCts = new CancellationTokenSource();
                                    if (pluginExecution.ExecutionTimeout == 0 || pluginExecution.ExecutionTimeout > 43200)
                                    {
                                        cliCts.CancelAfter(TimeSpan.FromSeconds(43200));
                                        _logger.LogDebug($"Command: {command} from {message.From?.Username}. Execution timeout for {pluginExecution.CommandName} was set to 43200 because its value in the configuration was {pluginExecution.ExecutionTimeout}");
                                    }
                                    else
                                    {
                                        cliCts.CancelAfter(TimeSpan.FromSeconds(pluginExecution.ExecutionTimeout));
                                        _logger.LogDebug($"Command: {command} from {message.From?.Username}. Execution timeout for {pluginExecution.CommandName} was set to {pluginExecution.ExecutionTimeout}");
                                    }

                                    // Define environment variables from settings
                                    IReadOnlyDictionary<string, string> environmentVariables = AddCliWrapEnvironmentVariables(plugin.Settings);

                                    // Extract ConfigFile settings to json file
                                    ExtractSettingsToConfigFile(plugin);

                                    // Execution Output and Error datastore
                                    var stdOutBuffer = new StringBuilder();
                                    var stdErrBuffer = new StringBuilder();

                                    switch (pluginExecution.ExecuteType)
                                    {
                                        case CommandExecuteTypes.PSScript:
                                            {
                                                if (await CheckEnvironmentExecutable(message, _configuration.TelegramPowerShellPath, command) == false)
                                                    continue;

                                                _logger.LogDebug($"Command: {command} from {message.From?.Username}. Starting execution of PowerShell command {pluginExecution.CommandName} - {plugin.Name} {plugin.Guid}");

                                                try
                                                {
                                                    var cliTask = Cli.Wrap(_configuration.TelegramPowerShellPath)
                                                        .WithArguments(args =>
                                                        {
                                                            if (_configuration.TelegramPowerShellArguments?.Length > 0)
                                                            {
                                                                string[] powershellArgs = _configuration.TelegramPowerShellArguments.Split(' ');
                                                                foreach (string arg in powershellArgs)
                                                                {
                                                                    args.Add(arg);
                                                                }
                                                            }

                                                            args.Add("-File");
                                                            args.Add(executionFile);

                                                            if (arguments.Count > 0)
                                                            {
                                                                foreach (string arg in arguments)
                                                                {
                                                                    args.Add(arg);
                                                                }
                                                            }

                                                            if (plugin.Settings != null && plugin.Settings.Count > 0)
                                                            {
                                                                foreach (PluginSetting pluginSetting in plugin.Settings)
                                                                {
                                                                    if (pluginSetting.SettingType == PluginSettingType.Argument)
                                                                    {
                                                                        args.Add(pluginSetting.Key);
                                                                        args.Add(pluginSetting.Value);
                                                                    }
                                                                }
                                                            }
                                                        })
                                                        .WithWorkingDirectory(workingDirectory)
                                                        .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                                                        .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                                                        .WithValidation(CommandResultValidation.None)
                                                        .WithEnvironmentVariables(environmentVariables);

                                                    await cliTask.ExecuteAsync(cliCts.Token);
                                                }
                                                catch (OperationCanceledException)
                                                {
                                                    _logger.LogWarning($"The command {command} was aborted due to timeout ({pluginExecution.ExecutionTimeout}) expiration.\nOutput: {stdOutBuffer.ToString()}\nError: {stdErrBuffer.ToString()}"); 
                                                    await _telegramResponder.SendPlain(message.Chat, $"⚠️ `Warning:` The command {command} was aborted due to timeout ({pluginExecution.ExecutionTimeout}) expiration.\nOutput: {stdOutBuffer.ToString()}\nError: {stdErrBuffer.ToString()}");
                                                    return;
                                                }
                                                catch (Exception ex)
                                                {
                                                    _logger.LogError($"The command {command} was aborted due to an error: " + ex.Message);
                                                    await _telegramResponder.SendPlain(message.Chat, $"❌ `Error:` The command {command} was aborted due to an error: " + ex.Message);
                                                    return;
                                                }
                                            }
                                            break;
                                        case CommandExecuteTypes.BashScript:
                                            {
                                                if (await CheckEnvironmentExecutable(message, _configuration.TelegramLinuxShellPath, command) == false)
                                                    continue;

                                                _logger.LogDebug($"Command: {command} from {message.From?.Username}. Starting execution of Bash Script command {pluginExecution.CommandName} - {plugin.Name} {plugin.Guid}");

                                                try
                                                {
                                                    var cliTask = Cli.Wrap(_configuration.TelegramLinuxShellPath)
                                                        .WithArguments(args =>
                                                        {
                                                            if (_configuration.TelegramLinuxShellArguments?.Length > 0)
                                                            {
                                                                string[] shellArgs = _configuration.TelegramLinuxShellArguments.Split(' ');
                                                                foreach (string arg in shellArgs)
                                                                {
                                                                    args.Add(arg);
                                                                }
                                                            }

                                                            args.Add(executionFile);

                                                            if (arguments.Count > 0)
                                                            {
                                                                foreach (string arg in arguments)
                                                                {
                                                                    args.Add(arg);
                                                                }
                                                            }

                                                            if (plugin.Settings != null && plugin.Settings.Count > 0)
                                                            {
                                                                foreach (PluginSetting pluginSetting in plugin.Settings)
                                                                {
                                                                    if (pluginSetting.SettingType == PluginSettingType.Argument)
                                                                    {
                                                                        args.Add(pluginSetting.Key);
                                                                        args.Add(pluginSetting.Value);
                                                                    }
                                                                }
                                                            }
                                                        })
                                                        .WithWorkingDirectory(workingDirectory)
                                                        .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                                                        .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                                                        .WithValidation(CommandResultValidation.None)
                                                        .WithEnvironmentVariables(environmentVariables);

                                                    await cliTask.ExecuteAsync(cliCts.Token);
                                                }
                                                catch (OperationCanceledException)
                                                {
                                                    _logger.LogWarning($"The command {command} was aborted due to timeout ({pluginExecution.ExecutionTimeout}) expiration.\nOutput: {stdOutBuffer.ToString()}\nError: {stdErrBuffer.ToString()}");
                                                    await _telegramResponder.SendPlain(message.Chat, $"⚠️ `Warning:` The command {command} was aborted due to timeout ({pluginExecution.ExecutionTimeout}) expiration.\nOutput: {stdOutBuffer.ToString()}\nError: {stdErrBuffer.ToString()}");
                                                    return;
                                                }
                                                catch (Exception ex)
                                                {
                                                    _logger.LogError($"The command {command} was aborted due to an error: " + ex.Message);
                                                    await _telegramResponder.SendPlain(message.Chat, $"❌ `Error:` The command {command} was aborted due to an error: " + ex.Message);
                                                    return;
                                                }
                                            }
                                            break;
                                        case CommandExecuteTypes.PythonScript:
                                            {
                                                if (await CheckEnvironmentExecutable(message, _configuration.TelegramPythonPath, command) == false)
                                                    continue;

                                                _logger.LogDebug($"Command: {command} from {message.From?.Username}. Starting execution of Python script command {pluginExecution.CommandName} - {plugin.Name} {plugin.Guid}");

                                                try
                                                {
                                                    var cliTask = Cli.Wrap(_configuration.TelegramPythonPath)
                                                        .WithArguments(args =>
                                                        {
                                                            if (_configuration.TelegramPythonArguments?.Length > 0)
                                                            {
                                                                string[] shellArgs = _configuration.TelegramPythonArguments.Split(' ');
                                                                foreach (string arg in shellArgs)
                                                                {
                                                                    args.Add(arg);
                                                                }
                                                            }

                                                            args.Add(executionFile);

                                                            if (arguments.Count > 0)
                                                            {
                                                                foreach (string arg in arguments)
                                                                {
                                                                    args.Add(arg);
                                                                }
                                                            }

                                                            if (plugin.Settings != null && plugin.Settings.Count > 0)
                                                            {
                                                                foreach (PluginSetting pluginSetting in plugin.Settings)
                                                                {
                                                                    if (pluginSetting.SettingType == PluginSettingType.Argument)
                                                                    {
                                                                        args.Add(pluginSetting.Key);
                                                                        args.Add(pluginSetting.Value);
                                                                    }
                                                                }
                                                            }
                                                        })
                                                        .WithWorkingDirectory(workingDirectory)
                                                        .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                                                        .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                                                        .WithValidation(CommandResultValidation.None)
                                                        .WithEnvironmentVariables(environmentVariables);

                                                    await cliTask.ExecuteAsync(cliCts.Token);
                                                }
                                                catch (OperationCanceledException)
                                                {
                                                    _logger.LogWarning($"The command {command} was aborted due to timeout ({pluginExecution.ExecutionTimeout}) expiration.\nOutput: {stdOutBuffer.ToString()}\nError: {stdErrBuffer.ToString()}");
                                                    await _telegramResponder.SendPlain(message.Chat, $"⚠️ `Warning:` The command {command} was aborted due to timeout ({pluginExecution.ExecutionTimeout}) expiration.\nOutput: {stdOutBuffer.ToString()}\nError: {stdErrBuffer.ToString()}");
                                                    return;
                                                }
                                                catch (Exception ex)
                                                {
                                                    _logger.LogError($"The command {command} was aborted due to an error: " + ex.Message);
                                                    await _telegramResponder.SendPlain(message.Chat, $"❌ `Error:` The command {command} was aborted due to an error: " + ex.Message);
                                                    return;
                                                }
                                            }
                                            break;
                                        case CommandExecuteTypes.AppExecutable:
                                            {
                                                _logger.LogDebug($"Command: {command} from {message.From?.Username}. Starting execution of application executable command {pluginExecution.CommandName} - {plugin.Name} {plugin.Guid}");

                                                try
                                                {
                                                    var cliTask = Cli.Wrap(executionFile)
                                                       .WithArguments(args =>
                                                       {
                                                           if (arguments.Count > 0)
                                                           {
                                                               foreach (string arg in arguments)
                                                               {
                                                                   args.Add(arg);
                                                               }
                                                           }

                                                           if (plugin.Settings != null && plugin.Settings.Count > 0)
                                                           {
                                                               foreach (PluginSetting pluginSetting in plugin.Settings)
                                                               {
                                                                   if (pluginSetting.SettingType == PluginSettingType.Argument)
                                                                   {
                                                                       args.Add(pluginSetting.Key);
                                                                       args.Add(pluginSetting.Value);
                                                                   }
                                                               }
                                                           }
                                                           //Console.WriteLine($"Arguments: {args.Build()}");
                                                       })
                                                       .WithWorkingDirectory(workingDirectory)
                                                       .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                                                       .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                                                       .WithValidation(CommandResultValidation.None)
                                                       .WithEnvironmentVariables(environmentVariables);

                                                    await cliTask.ExecuteAsync(cliCts.Token);
                                                }
                                                catch (OperationCanceledException)
                                                {
                                                    _logger.LogWarning($"The command {command} was aborted due to timeout ({pluginExecution.ExecutionTimeout}) expiration.\nOutput: {stdOutBuffer.ToString()}\nError: {stdErrBuffer.ToString()}");
                                                    await _telegramResponder.SendPlain(message.Chat, $"⚠️ `Warning:` The command {command} was aborted due to timeout ({pluginExecution.ExecutionTimeout}) expiration.\nOutput: {stdOutBuffer.ToString()}\nError: {stdErrBuffer.ToString()}");
                                                    return;
                                                }
                                                catch (Exception ex)
                                                {
                                                    _logger.LogError($"The command {command} was aborted due to an error: " + ex.Message);
                                                    await _telegramResponder.SendPlain(message.Chat, $"❌ `Error:` The command {command} was aborted due to an error: " + ex.Message);
                                                    return;
                                                }
                                            }
                                            break;
                                        case CommandExecuteTypes.CSharpScript:
                                            {
                                                _logger.LogDebug($"Command: {command} from {message.From?.Username}. Starting execution of CSharp Script command {pluginExecution.CommandName} - {plugin.Name} {plugin.Guid}");
                                                
                                                try
                                                {
                                                    object executionResult = await CSharpScript.EvaluateAsync(File.ReadAllText(executionFile));
                                                    stdOutBuffer.Append(executionResult);
                                                }
                                                catch (CompilationErrorException e)
                                                {
                                                    _logger.LogError($"Error during execution of C# script: {e.Message}");
                                                    stdErrBuffer.Append(e.Message);
                                                }
                                                catch (Exception ex)
                                                {
                                                    _logger.LogError($"The command {command} was aborted due to an error: " + ex.Message);
                                                    await _telegramResponder.SendPlain(message.Chat, $"❌ `Error:` The command {command} was aborted due to an error: " + ex.Message);
                                                    return;
                                                }
                                            }
                                            break;
                                    }

                                    if (stdErrBuffer.ToString().Length == 0)
                                    {
                                        await _telegramResponder.SendPlain(message.Chat, stdOutBuffer.ToString());
                                    }
                                    else
                                    {
                                        if (stdErrBuffer.ToString().Length > 0)
                                            await _telegramResponder.SendPlain(message.Chat, stdErrBuffer.ToString());
                                        else
                                            await _telegramResponder.SendPlain(message.Chat, pluginExecution.DefaultErrorMessage);
                                    }

                                    _logger.LogInformation($"User {message.From?.Username} executed command {command} from plugin {plugin.Guid} with name {plugin.Name}");
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool TryParseCommand(string? input, out string command, out string botName, out string pluginId, out bool isConfidential, out List<string> arguments)
        {
            command = null;
            botName = null;
            pluginId = null;
            isConfidential = false;
            arguments = new List<string>();

            // Check if input is null or empty
            if (string.IsNullOrEmpty(input))
                return false;

            // Match command with optional @botName and !pluginId
            Match match = Regex.Match(input, @"^(\/\w+)(?:@(\w+))?(?:!(\w+))?(#)?");

            if (!match.Success) return false;

            command = match.Groups[1].Value.ToLower();  // Extract command
            botName = match.Groups[2].Success ? match.Groups[2].Value.ToLower() : null;  // Extract botName if present
            pluginId = match.Groups[3].Success ? match.Groups[3].Value : null;  // Extract pluginId if present
            isConfidential = match.Groups[4].Success ? true : false;  // Extract if confidential flag present

            // Extract arguments (handles quoted arguments properly)
            string argsPart = input.Substring(match.Length).Trim();
            if (!string.IsNullOrEmpty(argsPart))
            {
                var argsMatch = Regex.Matches(argsPart, @"(\"".+?\""|\S+)");

                foreach (Match arg in argsMatch)
                {
                    string cleanedArg = arg.Value.Trim();

                    // Check if argument starts with a quote but doesn't close properly
                    if (cleanedArg.StartsWith("\"") && !cleanedArg.EndsWith("\""))
                    {
                        return false; // Invalid argument format (unmatched quotes)
                    }

                    // Remove surrounding quotes if present
                    if (cleanedArg.StartsWith("\"") && cleanedArg.EndsWith("\""))
                    {
                        cleanedArg = cleanedArg.Substring(1, cleanedArg.Length - 2);
                    }

                    arguments.Add(cleanedArg);
                }
            }

            return true;
        }

        private bool HasPermissionToWriteToBot(int telegramUserId, Message message)
        {
            using var scope = _scopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<InfrabotContext>();

            // Check if /showmyid command sent while it is enabled.
            if (_configuration.TelegramEnableShowMyId && message.Text?.ToLower() == "/showmyid")
            {
                _logger.LogDebug($"Command: {message?.Text} from {message?.From?.Username}. User has permission to write to bot, because Show My Id is enabled even if he is not in the Telegram Users list");
                return true; 
            }

            // Check if user is in the list.
            bool isInTheList = _context.TelegramUsers.Any(t => t.TelegramId == telegramUserId);
            _logger.LogDebug($"Command: {message?.Text} from {message?.From?.Username}. User {(isInTheList ? "has" : "does not have")} permission to write to bot {(isInTheList ? "because he is in the Telegram Users list" : "because he is not in the Telegram Users list")}");

            return isInTheList;
        }

        private bool HasPermissionToPlugin(Guid pluginGuid, int telegramUserId)
        {
            using var scope = _scopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<InfrabotContext>();

            var plugin = _context.Plugins.FirstOrDefault(p => p.Guid == pluginGuid);
            if (plugin == null)
            {
                return false;
            }

            // Load all PermissionAssignments that include the plugin,
            // including the directly assigned TelegramUsers and Groups.
            var assignments = _context.PermissionAssignments
                .Include(pa => pa.TelegramUsers)
                .Include(pa => pa.Groups)
                .Where(pa => pa.Plugins.Any(p => p.Id == plugin.Id))
                .ToList();

            // Check if any assignment directly includes the Telegram user.
            bool hasDirectAccess = assignments.Any(pa => pa.TelegramUsers.Any(u => u.TelegramId == telegramUserId));

            // Get all unique groups from these assignments.
            var groupsInAssignments = assignments.SelectMany(pa => pa.Groups)
                                                   .Select(g => g.Id)
                                                   .Distinct()
                                                   .ToList();

            if (!groupsInAssignments.Any())
            {
                _logger.LogDebug($"Telegram user with {telegramUserId} id has direct permission to plugin");
                return hasDirectAccess;
            }

            // Instead of relying on g.UserGroups (which might not be loaded), 
            // query the UserGroups table to get the groups that the user belongs to,
            // but only among those groups referenced by the assignments.
            var userGroupIds = _context.UserGroups
                .Include(ug => ug.TelegramUser)
                .Where(ug => ug.TelegramUser.TelegramId == telegramUserId && groupsInAssignments.Contains(ug.GroupId))
                .Select(ug => ug.GroupId)
                .Distinct()
                .ToList();

            bool hasGroupAccess = groupsInAssignments.Any(gid => userGroupIds.Contains(gid));
            _logger.LogDebug($"Telegram user with {telegramUserId} id has permission to plugin via group");

            return hasDirectAccess || hasGroupAccess;
        }

        private async Task AddLogToTelegramMessages(Message message, bool isConfidential)
        {
            using var scope = _scopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<InfrabotContext>();

            if(isConfidential)
                _context.TelegramMessages.Add(new Common.Models.TelegramMessage { Message = message.Text?.Split(' ')[0].ToString(), TelegramUserId = message.From?.Id, TelegramUserUsername = message.From?.Username, CreatedDate = DateTime.Now });
            else
                _context.TelegramMessages.Add(new Common.Models.TelegramMessage { Message = message.Text, TelegramUserId = message.From?.Id, TelegramUserUsername = message.From?.Username, CreatedDate = DateTime.Now });

            await _context.SaveChangesAsync();
            _logger.LogDebug($"Command: {message?.Text} from {message?.From?.Username}. Will add Log to messages. Log mode is {(isConfidential ? "Confidential" : "Not Confidential")}.");
        }

        private async Task ShowHelpManual(Message message, PluginExecution pluginExecution)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\n\n🧾 Arguments:");
            if (pluginExecution.ExecutionFileArguments != null && pluginExecution.ExecutionFileArguments.Count > 0)
            {
                foreach (ExecutionFileArgument executionFileArgument in pluginExecution.ExecutionFileArguments)
                {
                    sb.AppendLine($"🔹     '{executionFileArgument.Name}' - {executionFileArgument.Description}");
                }
            }
            else
            {
                sb.AppendLine("     (no arguments for this command)");
            }
            await _telegramResponder.SendPlain(message.Chat, $"ℹ️ Help: {pluginExecution.Help}{sb.ToString()}");
            _logger.LogDebug($"Command: {message?.Text} from {message?.From?.Username}.  First argument is ?, so will send command information to user");
        }

        private Dictionary<string, string> AddCliWrapEnvironmentVariables(List<PluginSetting> pluginSettings)
        {
            Dictionary<string, string> environmentVariables = new Dictionary<string, string>();

            if (pluginSettings != null && pluginSettings.Count > 0)
            {
                foreach (PluginSetting pluginSetting in pluginSettings)
                {
                    if (pluginSetting.SettingType == PluginSettingType.EnvironmentVariable)
                    {
                        environmentVariables.Add(pluginSetting.Key, pluginSetting.Value);
                    }
                }
            }

            return environmentVariables;
        }

        private void ExtractSettingsToConfigFile(Plugin plugin)
        {
            List<PluginSetting> pluginSettingsConfigFile = new List<PluginSetting>();
            if (plugin.Settings != null && plugin.Settings.Count > 0)
            {
                foreach (PluginSetting pluginSetting in plugin.Settings)
                {
                    if (pluginSetting.SettingType == PluginSettingType.ConfigFile)
                    {
                        pluginSettingsConfigFile.Add(pluginSetting);
                    }
                }

                try
                {
                    // Get temp file path
                    string tempFile = Path.Combine(_pluginRegistry.GetPluginDirectory(), plugin.Guid.ToString(), "tempConfigFile.json");

                    // Delete if old file exists
                    if (File.Exists(tempFile)) { File.Delete(tempFile); }

                    // Convert to json settings
                    var jsonSettings = JsonConvert.SerializeObject(pluginSettingsConfigFile);

                    // Save data to file
                    File.WriteAllText(tempFile, jsonSettings);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error during settings to config file extraction: {ex.Message}");
                }
            }
        }

        private async Task<bool> CheckEnvironmentExecutable(Message message, string executableFilePath, string command)
        {
            if (File.Exists(executableFilePath))
                return true;

            await _telegramResponder.SendMarkdown(message.Chat, $"❌ `Error:` The command {command} was aborted because file *{executableFilePath.Replace("\\", "\\\\").Replace(".", "\\.")}* is missing\\. Fix it in the configuration from the UI interface\\.");
            _logger.LogDebug($"Command: {command} from {message.From?.Username}. Error: The command {command} was aborted because file {executableFilePath} is missing.");

            return false;
        }
    }
}
