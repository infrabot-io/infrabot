using Infrabot.PluginSystem.Enums;
using ProtoBuf;

namespace Infrabot.PluginSystem.Execution
{
    [ProtoContract]
    public class PluginSetting
    {
        [ProtoMember(1)]
        public string Key { get; set; } // Setting name

        [ProtoMember(2)]
        public string Value { get; set; } // Setting value

        [ProtoMember(3)]
        public PluginSettingType SettingType { get; set; } // Setting type

        [ProtoMember(4)]
        public string Description { get; set; } // Description of the setting
    }
}
