namespace Infrabot.PluginSystem.Enums
{
    public enum CommandExecuteTypes
    {
        AppExecutable = 0, // Executable file
        PSScript = 1,      // PowerShell script
        CSharpScript = 2,  // C# script (in-memory)
        BashScript = 3,  // Bash script
        PythonScript = 4  // Python script
    }
}
