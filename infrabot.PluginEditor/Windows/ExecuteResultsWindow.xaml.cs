using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using infrabot.PluginSystem;
using infrabot.PluginSystem.Enums;
using infrabot.PluginSystem.Execution;

namespace infrabot.PluginEditor.Windows
{
    /// <summary>
    /// Interaction logic for ExecuteResultsWindow.xaml
    /// </summary>
    public partial class ExecuteResultsWindow : Window
    {
        private Plugin _plugin = null;
        private int selectedResult = 0;

        public ExecuteResultsWindow(Plugin plugin)
        {
            InitializeComponent();
            _plugin = plugin;
            if(_plugin != null)
            {
                if(_plugin.PluginExecution.ExecuteResults != null)
                {
                    if (_plugin.PluginExecution.ExecuteResults.Count > 0)
                    {
                        foreach (ExecuteResult executeResult in _plugin.PluginExecution.ExecuteResults)
                        {
                            CommandExecuteResultList.Items.Add(new ExecuteResult
                            {
                                ResultCheckType = executeResult.ResultCheckType,
                                ResultOutput = executeResult.ResultOutput,
                                ResultValue = executeResult.ResultValue
                            });
                        }

                        ExecuteResultCheckType.SelectedIndex = _plugin.PluginExecution.ExecuteResults[0].ResultCheckType;
                        ExecuteResultValue.Text = _plugin.PluginExecution.ExecuteResults[0].ResultValue;
                        ExecuteResultOutput.Text = _plugin.PluginExecution.ExecuteResults[0].ResultOutput;
                        CommandExecuteResultList.SelectedIndex = 0;
                    }
                }
            }
        }

        private void CommandExecuteResultListItemDelete(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = sender as Button;
                CommandExecuteResultList.Items.Remove((ExecuteResult)button.DataContext);
                EmptyDataInForm();
            }
            catch { }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            Button helpButton = sender as Button;

            if (helpButton != null)
            {
                HelpDialog helpDialog = new HelpDialog(helpButton.Name);
                helpDialog.ShowDialog();
            }
        }

        private void AddExecuteResult_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CommandExecuteResultList.Items.Add(new ExecuteResult
                {
                    ResultValue = "0",
                    ResultOutput = "Some result",
                    ResultCheckType = (int) CommandResultCheckTypes.Contains
                });
            }
            catch { }
        }

        private void SaveExecuteResults_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<ExecuteResult> newExecuteResults = new List<ExecuteResult>() { };
                for (int i = 0; i < CommandExecuteResultList.Items.Count; i++)
                {
                    ExecuteResult newExecuteResult = CommandExecuteResultList.Items[i] as ExecuteResult;
                    newExecuteResults.Add(newExecuteResult);
                }

                for (int i = 0; i < newExecuteResults.Count; i++)
                {
                    if (i == selectedResult)
                    {
                        newExecuteResults[i].ResultCheckType = ExecuteResultCheckType.SelectedIndex;
                        newExecuteResults[i].ResultValue = ExecuteResultValue.Text;
                        newExecuteResults[i].ResultOutput = ExecuteResultOutput.Text;
                    }
                }

                _plugin.PluginExecution.ExecuteResults = newExecuteResults;
                CommandExecuteResultList.Items.Clear();

                if (_plugin.PluginExecution.ExecuteResults != null)
                {
                    foreach (ExecuteResult executeResult in _plugin.PluginExecution.ExecuteResults)
                    {
                        CommandExecuteResultList.Items.Add(new ExecuteResult
                        {
                            ResultCheckType = executeResult.ResultCheckType,
                            ResultOutput = executeResult.ResultOutput,
                            ResultValue = executeResult.ResultValue
                        });
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void EmptyDataInForm()
        {
            ExecuteResultCheckType.SelectedIndex = 0;
            ExecuteResultValue.Text = "";
            ExecuteResultOutput.Text = "";
        }

        private void CommandExecuteResultListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedResult = CommandExecuteResultList.SelectedIndex;

            try
            {
                if (CommandExecuteResultList.SelectedItem != null)
                {
                    ExecuteResult executeResult = (CommandExecuteResultList.SelectedItem as ExecuteResult);
                    if (executeResult != null)
                    {
                        ExecuteResultCheckType.SelectedIndex = executeResult.ResultCheckType;
                        ExecuteResultValue.Text = executeResult.ResultValue;
                        ExecuteResultOutput.Text = executeResult.ResultOutput;
                    }
                }
            }
            catch { }
        }
    }
}
