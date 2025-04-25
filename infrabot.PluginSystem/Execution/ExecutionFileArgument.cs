using ProtoBuf;
namespace Infrabot.PluginSystem.Execution
{
    [ProtoContract]
    public class ExecutionFileArgument
    {
        [ProtoMember(1)]
        public string Name { get; set; } // Name or key of the argument

        [ProtoMember(2)]
        public string? Value { get; set; } // Value of the argument, can be templated

        [ProtoMember(3)]
        public string? Description { get; set; } = string.Empty; // Description
    }
}
