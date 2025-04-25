using Infrabot.PluginSystem.Enums;
using ProtoBuf;

namespace Infrabot.PluginSystem.Execution
{
    [ProtoContract]
    public class PluginExecution
    {
        [ProtoMember(1)]
        public string CommandName { get; set; } // Name of the command executed by the plugin

        [ProtoMember(2)]
        public string? Help { get; set; } = string.Empty; // Show help information about command

        [ProtoMember(3)]
        public string ExecutionFilePath { get; set; } // Path to the executable/script

        [ProtoMember(4)]
        public int ExecutionTimeout { get; set; } = 10; // Default timeout in seconds

        [ProtoMember(5)]
        public string? DefaultErrorMessage { get; set; } // Default error message

        [ProtoMember(6)]
        public CommandExecuteTypes ExecuteType { get; set; } // Type of execution (e.g., script, binary)

        [ProtoMember(7)]
        public List<ExecutionFileArgument>? ExecutionFileArguments { get; set; } = new(); // Dynamic argument logic
    }
}
