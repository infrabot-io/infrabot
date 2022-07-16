using ProtoBuf;

namespace infrabot.PluginSystem.Execution
{
    [ProtoContract]
    public class ExecuteResult
    {
        [ProtoMember(1)]
        public string ResultValue { get; set; }

        [ProtoMember(2)]
        public string ResultOutput { get; set; }

        [ProtoMember(3)]
        public int ResultCheckType { get; set; }
    }
}
