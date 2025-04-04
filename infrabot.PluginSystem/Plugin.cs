using Infrabot.PluginSystem.Data;
using Infrabot.PluginSystem.Enums;
using Infrabot.PluginSystem.Execution;
using ProtoBuf;

namespace Infrabot.PluginSystem
{
    [ProtoContract]
    public class Plugin
    {
        [ProtoMember(1)]
        public Guid Guid { get; set; } // Plugin GUID

        [ProtoMember(2)]
        public string Id { get; set; } // Plugin ID, used to specify from the chat

        [ProtoMember(3)]
        public string Name { get; set; } // Plugin name

        [ProtoMember(4)]
        public string? Description { get; set; } = string.Empty; // Description

        [ProtoMember(5)]
        public PluginType PluginType { get; set; } // Type of the plugin

        [ProtoMember(6)]
        public string? Author { get; set; } = string.Empty; // Author name

        [ProtoMember(7)]
        public int Version { get; set; } = 0; // Plugin version

        [ProtoMember(8)]
        public string? WebSite { get; set; } = string.Empty; // WebSite url of the author

        [ProtoMember(9)]
        public List<PluginExecution> PluginExecutions { get; set; } = new(); // List of commands which infrabot reacts to

        [ProtoMember(10)]
        public List<PluginFile> PluginFiles { get; set; } // List of embed plugin files

        [ProtoMember(11)]
        public List<PluginSetting>? Settings { get; set; } = new(); // Configurable plugin settings

        [ProtoMember(12)]
        public string Checksum { get; set; } // Plugin checksum to check against attacks
    }
}
