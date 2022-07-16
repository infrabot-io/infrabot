using infrabot.PluginEditor.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using infrabot.PluginSystem;
using infrabot.PluginSystem.Utils;
using infrabot.PluginSystem.Execution;
using infrabot.PluginSystem.Enums;
using System.Diagnostics;

namespace infrabot.PluginEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isEditing = false;
        private string userTempPath = System.IO.Path.GetTempPath();
        public Plugin _plugin;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            Button? helpButton = sender as Button;

            if(helpButton != null) 
            { 
                HelpDialog helpDialog = new HelpDialog(helpButton.Name);
                helpDialog.ShowDialog();
            }
        }

        private void InitFormData(Plugin plugin)
        {
            // Enable Main menu items
            FileSavePluginMenuItem.IsEnabled = true;
            FileClosePluginMenuItem.IsEnabled = true;

            // Enable Form items
            MainPanelData.IsEnabled = true;

            // Select first Tab in TabControl
            MainTabControl.SelectedIndex = 0;

            // Insert data to boxes
            PluginName.Text = plugin.Name;
            PluginGuid.Text = plugin.Guid;
            PluginAuthor.Text = plugin.Author;
            PluginVersion.Text = plugin.Version;
            PluginWebSite.Text = plugin.WebSite;
            PluginHelp.Text = plugin.Help;
            PluginHelpShort.Text = plugin.HelpShort;
            PluginExecutionCommand.Text = plugin.PluginExecution.ExecutionCommand;
            PluginExecuteFile.Text = plugin.PluginExecution.ExecuteFile;
            PluginDefaultErrorMessage.Text = plugin.PluginExecution.DefaultErrorMessage;
            PluginExecuteType.SelectedIndex = plugin.PluginExecution.ExecuteType;

            // Other actions
            AddTextToEventsList("Loaded form data.");
        }

        private void ClearFormData()
        {
            // Disable Main menu items
            FileSavePluginMenuItem.IsEnabled = false;
            FileClosePluginMenuItem.IsEnabled = false;

            // Disable Form items
            MainPanelData.IsEnabled = false;
            MainPluginJsonDataScroll.ScrollToHome();

            // Select first Tab in TabControl
            MainTabControl.SelectedIndex = 0;

            // Insert data to boxes
            PluginName.Text = "";
            PluginGuid.Text = ""; ;
            PluginAuthor.Text = "";
            PluginVersion.Text = "";
            PluginWebSite.Text = "";
            PluginHelp.Text = "";
            PluginHelpShort.Text = "";
            PluginExecutionCommand.Text = "";
            PluginExecuteFile.Text = "";
            PluginDefaultErrorMessage.Text = "";
            PluginExecuteType.SelectedIndex = 0;

            // Other actions
            AddTextToEventsList("Unloaded form data.");
        }

        public void AddTextToEventsList(string text)
        {
            if (text == "" || text == null)
            {
                return;
            }

            EventsList.Items.Add(text);
            EventsList.ScrollIntoView(EventsList.Items.Count - 1);
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if(_plugin.Guid != null)
            {
                Process.Start("explorer.exe", userTempPath + _plugin.Guid);
            }
        }

        private void PluginExecuteResults_Click(object sender, RoutedEventArgs e)
        {
            ExecuteResultsWindow executeResultsWindow = new ExecuteResultsWindow(_plugin);
            executeResultsWindow.Show();
        }


        #region Main Menu Handlers

        private void FileNewPluginMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditing)
            {
                if (MessageBox.Show("Are you sure that you want to finish editing this file? All changes will not be saved!", "Attention", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _isEditing = false;
                }
                else
                {
                    AddTextToEventsList("Create new file is cancelled by user.");
                    return;
                }
            }

            List<ExecuteResult> executeResults = new List<ExecuteResult>
            {
                new ExecuteResult
                {
                    ResultValue = "0",
                    ResultOutput = "Server {DATA} was successfully restarted!",
                    ResultCheckType = (int) CommandResultCheckTypes.EqualsTo
                },
                new ExecuteResult
                {
                    ResultValue = "1",
                    ResultOutput = "Server {DATA} was not restarted! Can not find such server!",
                    ResultCheckType = (int) CommandResultCheckTypes.EqualsTo
                },
                new ExecuteResult
                {
                    ResultValue = "2",
                    ResultOutput = "Server {DATA} was not restarted! There are still logged in users!",
                    ResultCheckType = (int) CommandResultCheckTypes.EqualsTo
                },
                new ExecuteResult
                {
                    ResultValue = "3",
                    ResultOutput = "Server {DATA} was not restarted! Script output was: {RESULT}",
                    ResultCheckType = (int) CommandResultCheckTypes.EqualsTo
                }
            };

            PluginExecution pluginExecution = new PluginExecution()
            {
                ExecutionCommand = "/performaction",
                ExecuteFile = "performaction.ps1",
                DefaultErrorMessage = "You entered `{DATA}`! Unexpected error! Result was: {RESULT}",
                ExecuteType = (int) CommandExecuteTypes.PSScript,
                ExecuteResults = executeResults
            };

            _plugin = null;

            _plugin = new Plugin
            {
                Name = "Simple name",
                Guid = Guid.NewGuid().ToString(),
                Author = "Author",
                Version = "1.0.0.0",
                WebSite = "https://somesite.com",
                Help = "Perform action. Write \"/performaction\" to restart the server",
                HelpShort = "Restarts the specified server",
                PluginExecution = pluginExecution,
                PluginFiles = null
            };

            if (Directory.Exists(userTempPath + _plugin.Guid))
            {
                Directory.Delete(userTempPath + _plugin.Guid);
                AddTextToEventsList("Folder with old cache for this plugin still exists. Clearing it at " + userTempPath + _plugin.Guid);
            }

            Directory.CreateDirectory(userTempPath + _plugin.Guid);
            _isEditing = true;

            ClearFormData();
            InitFormData(_plugin);

            AddTextToEventsList("Created new plugin");
        }

        private void FileOpenPluginMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditing)
            {
                if (MessageBox.Show("Are you sure that you want to finish editing this file? All changes will not be saved!", "Attention", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _isEditing = false;
                }
                else
                {
                    AddTextToEventsList("Open another file is cancelled by user.");
                    return;
                }
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".plug";
            openFileDialog.Filter = "Plugin Files (*.plug)|*.plug|All files (*.*)|*.*";
            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                try
                {
                    // Get our plugin file
                    _plugin = PluginActions.GetPlugin(openFileDialog.FileName);

                    if (_plugin != null)
                    {
                        // Remove old temp folder if exists
                        if (Directory.Exists(userTempPath + _plugin.Guid))
                        {
                            Directory.Delete(userTempPath + _plugin.Guid, true);
                            AddTextToEventsList("Folder with old cache for this plugin still exists. Clearing it at " + userTempPath + _plugin.Guid);
                        }

                        // Create new temp folder
                        Directory.CreateDirectory(userTempPath + _plugin.Guid);

                        // Extract files to the directory
                        if (Directory.Exists(userTempPath + _plugin.Guid))
                        {
                            if(_plugin.PluginFiles != null)
                            {
                                PluginActions.ExtractPluginFiles(_plugin, userTempPath + _plugin.Guid);
                            }
                        }

                        _isEditing = true;
                        InitFormData(_plugin);
                        AddTextToEventsList("Loaded file: " + openFileDialog.FileName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    AddTextToEventsList("Error occured: " + ex.Message);
                }
            }
        }

        private void FileSavePluginMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if(PluginExecutionCommand.Text.Length == 0 || PluginExecutionCommand.Text == null)
            {
                MessageBox.Show("Please insert Execution Command value. It must not be empty!", "Error");
                return;
            }

            if (PluginExecuteFile.Text.Length == 0 || PluginExecuteFile.Text == null)
            {
                MessageBox.Show("Please insert Execute File value. It must not be empty!", "Error");
                return;
            }

            if (!PluginExecutionCommand.Text.StartsWith("/"))
            {
                MessageBox.Show("Execution Command value must start with / character.", "Error");
                return;
            }

            try 
            {
                // Save file dialog
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Plugin Files (*.plug)|*.plug";
                if (saveFileDialog.ShowDialog() == true)
                {
                    _plugin.Name = PluginName.Text;
                    _plugin.Author = PluginAuthor.Text;
                    _plugin.Version = PluginVersion.Text;
                    _plugin.WebSite = PluginWebSite.Text;
                    _plugin.Help = PluginHelp.Text;
                    _plugin.HelpShort = PluginHelpShort.Text;
                    _plugin.PluginExecution.ExecutionCommand = PluginExecutionCommand.Text;
                    _plugin.PluginExecution.ExecuteFile = PluginExecuteFile.Text;
                    _plugin.PluginExecution.DefaultErrorMessage = PluginDefaultErrorMessage.Text;
                    _plugin.PluginExecution.ExecuteType = PluginExecuteType.SelectedIndex;

                    _plugin.PluginFiles = PluginActions.ImportPluginFiles(userTempPath + _plugin.Guid);

                    PluginActions.SavePlugin(_plugin, saveFileDialog.FileName);

                    AddTextToEventsList("File saved: " + saveFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FileClosePluginMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditing)
            {
                if (MessageBox.Show("Are you sure that you want to close this file? All changes will not be saved!", "Attention", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _isEditing = false;
                }
                else
                {
                    AddTextToEventsList("Close current file is cancelled by user.");
                    return;
                }
            }

            try
            {
                if (Directory.Exists(userTempPath + _plugin.Guid))
                {
                    Directory.Delete(userTempPath + _plugin.Guid);
                    AddTextToEventsList("Cleared old cache folder at: " + userTempPath + _plugin.Guid);
                }
            }
            catch (Exception ex)
            {
                AddTextToEventsList("Clearing plugin`s temporary folder was not successfull. Consider to clean it manually. Error: " + ex.Message);
            }

            ClearFormData();
            _plugin = null;
            AddTextToEventsList("File closed.");
        }

        private void FileExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditing)
            {
                if (MessageBox.Show("Are you sure that you want to exit application? All changes will not be saved!", "Attention", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    return;
                }
            }

            Application.Current.Shutdown();
        }


        private void HelpAboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutDialog aboutDialog = new AboutDialog();
            aboutDialog.ShowDialog();
            AddTextToEventsList("About dialog opened.");
        }

        #endregion

    }
}
