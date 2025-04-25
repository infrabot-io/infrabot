using Infrabot.PluginSystem;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Infrabot.PluginEditor.Notifiers
{
    public class PluginWrapper : INotifyPropertyChanged
    {
        private readonly Plugin _plugin;
        private ObservableCollection<PluginExecutionWrapper> _pluginExecutions;
        private ObservableCollection<PluginSettingWrapper> _pluginSettings;

        public event PropertyChangedEventHandler? PropertyChanged;

        public PluginWrapper(Plugin plugin)
        {
            _plugin = plugin;

            // Sync plugin executions
            _pluginExecutions = new ObservableCollection<PluginExecutionWrapper>(
                _plugin.PluginExecutions.Select(pe => new PluginExecutionWrapper(pe)));

            _pluginExecutions.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (PluginExecutionWrapper item in e.NewItems!)
                    {
                        _plugin.PluginExecutions.Add(item.OriginalExecution);
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (PluginExecutionWrapper item in e.OldItems!)
                    {
                        _plugin.PluginExecutions.Remove(item.OriginalExecution);
                    }
                }
            };

            // Sync plugin settings
            _pluginSettings = new ObservableCollection<PluginSettingWrapper>(
                _plugin.Settings.Select(pe => new PluginSettingWrapper(pe)));

            _pluginSettings.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (PluginSettingWrapper item in e.NewItems!)
                    {
                        _plugin.Settings.Add(item.OriginalSetting);
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (PluginSettingWrapper item in e.OldItems!)
                    {
                        _plugin.Settings.Remove(item.OriginalSetting);
                    }
                }
            };
        }

        public ObservableCollection<PluginExecutionWrapper> PluginExecutions
        {
            get => _pluginExecutions;
            set
            {
                _pluginExecutions = value;
                OnPropertyChanged(nameof(PluginExecutions));
            }
        }

        public ObservableCollection<PluginSettingWrapper> PluginSettings
        {
            get => _pluginSettings;
            set
            {
                _pluginSettings = value;
                OnPropertyChanged(nameof(PluginSettings));
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
