using System;
using CliWrap;

namespace infrabot.Utils
{
    public class InfrabotInstaller
    {
        public static bool InstallService()
        {
            bool result = false;

            try
            {
                Cli.Wrap(Environment.SystemDirectory + @"\cmd.exe").WithArguments("/c sc delete infrabot.io").WithWorkingDirectory(Environment.SystemDirectory).WithValidation(CommandResultValidation.None).ExecuteAsync();
                Cli.Wrap(Environment.SystemDirectory + @"\cmd.exe").WithArguments("/c sc create infrabot.io binpath= \"" + AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName + "\" start= auto").WithWorkingDirectory(Environment.SystemDirectory).WithValidation(CommandResultValidation.None).ExecuteAsync();
                Cli.Wrap(Environment.SystemDirectory + @"\cmd.exe").WithArguments("/c sc description infrabot.io \"Service for infrabot\"").WithWorkingDirectory(Environment.SystemDirectory).WithValidation(CommandResultValidation.None).ExecuteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result = false;
            }

            return result;
        }

        public static bool RemoveService()
        {
            bool result = false;

            try
            {
                Cli.Wrap(Environment.SystemDirectory + @"\cmd.exe").WithArguments("/c sc delete infrabot.io").WithWorkingDirectory(Environment.SystemDirectory).WithValidation(CommandResultValidation.None).ExecuteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result = false;
            }

            return result;
        }
    }
}
