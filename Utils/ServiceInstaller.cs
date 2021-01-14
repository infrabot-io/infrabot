using System;
using System.Diagnostics;

namespace InfraBot.Core
{
    public static class InfraBotInstaller
    {
        public static bool InstallService()
        {
            bool result = false;
            try
            {
                RunCmdWithArguments("/c sc delete infrabot.io");
                RunCmdWithArguments("/c sc create infrabot.io binpath= \"" + AppDomain.CurrentDomain.BaseDirectory + "TelegramBot.exe" + "\" start= auto");
                RunCmdWithArguments("/c sc description infrabot.io \"Service for infrabot\"");
                result = true;
            }
            catch
            {
                result = false;
            }
            return result;
        }

        private static void RunCmdWithArguments(string arguments)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = arguments;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            Console.WriteLine(process.StandardOutput.ReadToEnd());
        }
    }
}