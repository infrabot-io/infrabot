using ProtoBuf;

namespace infrabot.PluginSystem.Data
{
    [ProtoContract]
    public class PluginFile
    {
        [ProtoMember(1)]
        public string PluginFileName;

        [ProtoMember(2)]
        public string PluginFilePath;

        [ProtoMember(3)]
        public byte[] PluginFileData;
    }
}
