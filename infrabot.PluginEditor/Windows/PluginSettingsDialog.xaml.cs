using Infrabot.PluginEditor.Notifiers;
using Infrabot.PluginSystem.Enums;
using Infrabot.PluginSystem.Execution;
using Infrabot.PluginSystem;
using System.Windows;
using System.Windows.Controls;
namespace Infrabot.PluginEditor.Windows
{
    /// <summary>
    /// Interaction logic for PluginSettingsDialog.xaml
    /// </summary>
    public partial class PluginSettingsDialog : Window
    {
        private Plugin _plugin;
        private PluginWrapper _pluginWrapper;

        public PluginSettingsDialog(Plugin plugin)
        {
            this.Resources.Add("SettingType", Enum.GetValues(typeof(PluginSettingType)));
            InitializeComponent();

            _plugin = plugin;
            _pluginWrapper = new PluginWrapper(_plugin);

            // Set the ItemsSource for the ListBox
            SettingsList.ItemsSource = _pluginWrapper.PluginSettings;

            // Disable properties panel initially
            PropertiesPanel.IsEnabled = false;
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            Button? helpButton = sender as Button;

            if (helpButton != null)
            {
                HelpDialog helpDialog = new HelpDialog(helpButton.Name);
                helpDialog.ShowDialog();
            }
        }

        private void SettingsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedSetting = SettingsList.SelectedItem as PluginSettingWrapper;
            PropertiesPanel.DataContext = selectedSetting;
            PropertiesPanel.IsEnabled = selectedSetting != null;
        }

        private void AddNew_Click(object sender, RoutedEventArgs e)
        {
            var newSetting = new PluginSetting
            {
                Key = "botName",
                Value = "Value for botName environment variable is accessible from the script",
                SettingType = PluginSettingType.EnvironmentVariable,
                Description = ""
            };

            var wrapper = new PluginSettingWrapper(newSetting);
            _pluginWrapper.PluginSettings.Add(wrapper);
            SettingsList.SelectedItem = wrapper;
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var selectedSetting = SettingsList.SelectedItem as PluginSettingWrapper;
            if (selectedSetting != null)
            {
                _pluginWrapper.PluginSettings.Remove(selectedSetting);
            }
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is PluginSettingWrapper setting)
            {
                _pluginWrapper.PluginSettings.Remove(setting);
            }
        }
    }
}
