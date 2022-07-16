using System.Collections.Generic;
using infrabot.PluginSystem.Execution;
using infrabot.PluginSystem.Data;
using ProtoBuf;

namespace infrabot.PluginSystem
{
    [ProtoContract]
    public class Plugin
    {
        [ProtoMember(1)]
        public string Name;

        [ProtoMember(2)]
        public string Guid;

        [ProtoMember(3)]
        public string Author;

        [ProtoMember(4)]
        public string Version;

        [ProtoMember(5)]
        public string WebSite;

        [ProtoMember(6)]
        public string Help;

        [ProtoMember(7)]
        public string HelpShort;

        [ProtoMember(8)]
        public PluginExecution PluginExecution;

        [ProtoMember(9)]
        public List<PluginFile> PluginFiles;
    }
}
