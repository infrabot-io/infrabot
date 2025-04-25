using Infrabot.PluginSystem.Enums;
using Infrabot.PluginSystem.Execution;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Infrabot.PluginEditor.Notifiers
{
    public class PluginSettingWrapper : INotifyPropertyChanged
    {
        private readonly PluginSetting _setting;
        public PluginSetting OriginalSetting => _setting;

        public event PropertyChangedEventHandler? PropertyChanged;

        public PluginSettingWrapper(PluginSetting setting)
        {
            _setting = setting;
        }

        public string Key
        {
            get => _setting.Key;
            set
            {
                if (_setting.Key != value)
                {
                    _setting.Key = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Value
        {
            get => _setting.Value;
            set
            {
                if (_setting.Value != value)
                {
                    _setting.Value = value;
                    OnPropertyChanged();
                }
            }
        }

        public PluginSettingType SettingType
        {
            get => _setting.SettingType;
            set
            {
                if (_setting.SettingType != value)
                {
                    _setting.SettingType = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Description
        {
            get => _setting.Description;
            set
            {
                if (_setting.Description != value)
                {
                    _setting.Description = value;
                    OnPropertyChanged();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
