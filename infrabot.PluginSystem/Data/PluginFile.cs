using ProtoBuf;

namespace Infrabot.PluginSystem.Data
{
    [ProtoContract]
    public class PluginFile
    {
        [ProtoMember(1)]
        public string FileName { get; set; } // File name

        [ProtoMember(2)]
        public string FilePath { get; set; } // Path within the plugin's directory

        [ProtoMember(3)]
        public string FileHash { get; set; } // File hash

        [ProtoMember(4)]
        public byte[] FileData { get; set; } // File content
    }
}
