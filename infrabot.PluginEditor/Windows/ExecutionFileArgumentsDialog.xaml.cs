using Infrabot.PluginEditor.Notifiers;
using Infrabot.PluginSystem.Execution;
using Infrabot.PluginSystem;
using System.Windows;
using System.Windows.Controls;

namespace Infrabot.PluginEditor.Windows
{
    /// <summary>
    /// Interaction logic for ExecutionFileArgumentsDialog.xaml
    /// </summary>
    public partial class ExecutionFileArgumentsDialog : Window
    {
        private Plugin _plugin;
        private PluginExecutionWrapper _pluginExecutionWrapper;

        public ExecutionFileArgumentsDialog(Plugin plugin, PluginExecutionWrapper pluginExecutionWrapper)
        {
            InitializeComponent();

            _plugin = plugin;
            _pluginExecutionWrapper = pluginExecutionWrapper;

            // Set the ItemsSource for the ListBox
            ArgumentsList.ItemsSource = _pluginExecutionWrapper.ExecutionFileArguments;

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

        private void ArgumentsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedFileArgument = ArgumentsList.SelectedItem as ExecutionFileArgumentWrapper;
            PropertiesPanel.DataContext = selectedFileArgument;
            PropertiesPanel.IsEnabled = selectedFileArgument != null;
        }

        private void AddNew_Click(object sender, RoutedEventArgs e)
        {
            var newFileArgument = new ExecutionFileArgument
            {
                Name = "arg1",
                Value = "",
                Description = ""
            };

            var wrapper = new ExecutionFileArgumentWrapper(newFileArgument);
            _pluginExecutionWrapper.ExecutionFileArguments.Add(wrapper);
            ArgumentsList.SelectedItem = wrapper;
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var selectedExecution = ArgumentsList.SelectedItem as ExecutionFileArgumentWrapper;
            if (selectedExecution != null)
            {
                _pluginExecutionWrapper.ExecutionFileArguments.Remove(selectedExecution);
            }
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ExecutionFileArgumentWrapper fileArgument)
            {
                _pluginExecutionWrapper.ExecutionFileArguments.Remove(fileArgument);
            }
        }
    }
}
