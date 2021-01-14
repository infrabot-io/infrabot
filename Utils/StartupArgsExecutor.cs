using System;
using System.IO;

namespace InfraBot.Core
{
    public static class StartupArgsExecutor
    {
        public static void ExecuteArgs(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "--install" || args[0] == "/i" || args[0] == "-i" || args[0] == "i")
                {
                    Console.WriteLine("**                     infrabot.io                                    **");
                    Console.WriteLine(@"**You can find more on https://infrabot.io/documentation/gettingstarted**");
                    Console.WriteLine("Got " + args[0] + " argument!");
                    Console.WriteLine("Starting service installation");
                    Console.WriteLine("");
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
                    Environment.Exit(0);
                }
                else if (args[0] == "--cleanplugins" || args[0] == "/c" || args[0] == "-c" || args[0] == "c")
                {
                    Console.WriteLine("**                     infrabot.io                                    **");
                    Console.WriteLine(@"**You can find more on https://infrabot.io/documentation/gettingstarted**");
                    Console.WriteLine("Got " + args[0] + " argument!");
                    Console.WriteLine("Starting plugins folder cleanup");
                    Console.WriteLine("");
                    try
                    {
                        DirectoryInfo di = new DirectoryInfo(PluginsManager.PluginsPath);
                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                            Console.WriteLine("Deleting: " + file.FullName);
                        }
                        foreach (DirectoryInfo dir in di.GetDirectories())
                        {
                            dir.Delete(true);
                            Console.WriteLine("Deleting: " + dir.FullName);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Something went wrong: " + ex.Message);
                    }
                    Console.WriteLine("");
                    Console.WriteLine("Task finished!");
                    Environment.Exit(0);
                }
                else if (args[0] == "--help" || args[0] == "/h" || args[0] == "-h" || args[0] == "h" || args[0] == "-?" || args[0] == "/?" || args[0] == "?")
                {
                    Console.WriteLine("**                     infrabot.io - HELP MANUAL                       **");
                    Console.WriteLine(@"**You can find more on https://infrabot.io/documentation/gettingstarted**");
                    Console.WriteLine("");
                    Console.WriteLine("There are the following options available:");
                    Console.WriteLine(@"     --help (-h | /h | h | -? | ? | /?) - Shows help manual");
                    Console.WriteLine(@"     --install (-i | /i | i )            - Reinstall infrabot service");
                    Console.WriteLine(@"               Deletes 'infrabot.io' service, and then creates a new one");
                    Console.WriteLine(@"               with the current TelegramBot.exe file path");
                    Console.WriteLine(@"     --cleanplugins (-c | /c | c) - Cleans all plugins in Plugins directory");
                    Console.WriteLine(@"               Make sure that you have backups of your plugins directory");
                    Console.WriteLine(@"               before executing this command. All files and folders in");
                    Console.WriteLine(@"               plugins directory will be permanently deleted!");
                    Console.WriteLine(@"");
                    Console.WriteLine(@"");
                    Console.WriteLine(@"");
                    Console.WriteLine(@"-- End of help --");
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine("**                     infrabot.io                                    **");
                    Console.WriteLine(@"**You can find more on https://infrabot.io/documentation/gettingstarted**");
                    Console.WriteLine("");
                    Console.WriteLine("This command not found. Write --help to get more info!");
                    Environment.Exit(0);
                }
            }
        }
    }
}