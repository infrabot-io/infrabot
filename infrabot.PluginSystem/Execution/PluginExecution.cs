using System.Collections.Generic;
using ProtoBuf;

namespace infrabot.PluginSystem.Execution
{
    [ProtoContract]
    public class PluginExecution
    {
        [ProtoMember(1)]
        public string ExecutionCommand;

        [ProtoMember(2)]
        public string ExecuteFile;

        [ProtoMember(3)]
        public string DefaultErrorMessage;

        [ProtoMember(4)]
        public int ExecuteType;

        [ProtoMember(5)]
        public List<ExecuteResult> ExecuteResults;
    }
}
