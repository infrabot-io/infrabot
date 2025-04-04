using Infrabot.PluginEditor.Notifiers;
using Infrabot.PluginSystem.Enums;
using Infrabot.PluginSystem.Execution;
using Infrabot.PluginSystem;
using System.Windows;
using System.Windows.Controls;

namespace Infrabot.PluginEditor.Windows
{
    /// <summary>
    /// Interaction logic for ExecuteCommandsDialog.xaml
    /// </summary>
    public partial class ExecuteCommandsDialog : Window
    {
        private Plugin _plugin;
        private PluginWrapper _pluginWrapper;

        public ExecuteCommandsDialog(Plugin plugin)
        {
            this.Resources.Add("ExecuteTypes", Enum.GetValues(typeof(CommandExecuteTypes)));
            InitializeComponent();

            _plugin = plugin;
            _pluginWrapper = new PluginWrapper(_plugin);

            // Set the ItemsSource for the ListBox
            ExecutionsList.ItemsSource = _pluginWrapper.PluginExecutions;

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

        private void ExecutionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedExecution = ExecutionsList.SelectedItem as PluginExecutionWrapper;
            PropertiesPanel.DataContext = selectedExecution;
            PropertiesPanel.IsEnabled = selectedExecution != null;
        }

        private void AddNew_Click(object sender, RoutedEventArgs e)
        {
            int randomNumber = Random.Shared.Next(0, 100);

            var newExecution = new PluginExecution
            {
                CommandName = $"/newcommand{randomNumber}",
                ExecutionFilePath = $"new_command_script{randomNumber}.ps1",
                ExecutionTimeout = randomNumber,
                Help = "",
                DefaultErrorMessage = "",
                ExecuteType = CommandExecuteTypes.PSScript,
                ExecutionFileArguments = new List<ExecutionFileArgument>()
            };

            var wrapper = new PluginExecutionWrapper(newExecution);
            _pluginWrapper.PluginExecutions.Add(wrapper);
            ExecutionsList.SelectedItem = wrapper;
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var selectedExecution = ExecutionsList.SelectedItem as PluginExecutionWrapper;
            if (selectedExecution != null)
            {
                _pluginWrapper.PluginExecutions.Remove(selectedExecution);
            }
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (_pluginWrapper.PluginExecutions.Count <= 1)
            {
                return;
            }

            if (sender is Button button && button.Tag is PluginExecutionWrapper execution)
            {
                _pluginWrapper.PluginExecutions.Remove(execution);
            }
        }

        private void ExecutionFileArguments_Click(object sender, RoutedEventArgs e)
        {
            PluginExecutionWrapper? selectedExecution = ExecutionsList.SelectedItem as PluginExecutionWrapper;
            
            ExecutionFileArgumentsDialog executionFileArgumentsDialog = new ExecutionFileArgumentsDialog(_plugin, selectedExecution);
            executionFileArgumentsDialog.ShowDialog();
        }
    }
}
