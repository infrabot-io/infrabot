using Infrabot.PluginSystem;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Infrabot.PluginSystem.Enums;
using Infrabot.PluginSystem.Utils;
using Infrabot.PluginSystem.Execution;
using Infrabot.PluginSystem.Data;
using Infrabot.PluginEditor.Windows;
using System.Text.Json;

namespace Infrabot.PluginEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isEditing = false;
        private string userTempPath = Path.GetTempPath();
        public Plugin _plugin;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitFormData()
        {
            // Enable Main menu items
            FileSavePluginMenuItem.IsEnabled = true;
            FileClosePluginMenuItem.IsEnabled = true;
            FileImportPluginMetadataMenuItem.IsEnabled = true;
            FileExportPluginMetadataMenuItem.IsEnabled = true;

            // Enable Form items
            MainPanelData.IsEnabled = true;

            // Select first Tab in TabControl
            MainTabControl.SelectedIndex = 0;

            // Insert data to boxes
            PluginName.Text = _plugin.Name;
            PluginGuid.Text = _plugin.Guid.ToString();
            PluginId.Text = _plugin.Id;
            PluginDescription.Text = _plugin.Description;
            PluginType.SelectedItem = _plugin.PluginType.ToString();
            PluginAuthor.Text = _plugin.Author;
            PluginVersion.Text = _plugin.Version.ToString();
            PluginWebSite.Text = _plugin.WebSite;

            // Other actions
            AddTextToEventsList("Initialized form data.");
        }

        private void ClearFormData()
        {
            // Disable Main menu items
            FileSavePluginMenuItem.IsEnabled = false;
            FileClosePluginMenuItem.IsEnabled = false;
            FileImportPluginMetadataMenuItem.IsEnabled = false;
            FileExportPluginMetadataMenuItem.IsEnabled = false;

            // Disable Form items
            MainPanelData.IsEnabled = false;
            MainPluginDataScroll.ScrollToHome();

            // Select first Tab in TabControl
            MainTabControl.SelectedIndex = 0;

            // Insert data to boxes
            PluginName.Text = "";
            PluginGuid.Text = "";
            PluginId.Text = "";
            PluginDescription.Text = "";
            PluginType.SelectedItem = "Automation";
            PluginAuthor.Text = "";
            PluginVersion.Text = "";
            PluginWebSite.Text = "";

            // Other actions
            AddTextToEventsList("Unloaded form data.");
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

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (_plugin.Guid != null)
            {
                Process.Start("explorer.exe", userTempPath + _plugin.Guid);
                AddTextToEventsList($"Opened folder {userTempPath + _plugin.Guid} in Windows Explorer.");
            }
        }

        private void PluginExecuteCommands_Click(object sender, RoutedEventArgs e)
        {
            _isEditing = true;
            ExecuteCommandsDialog executeCommandsDialog = new ExecuteCommandsDialog(_plugin);
            executeCommandsDialog.ShowDialog();
            AddTextToEventsList("Execute commands window opened.");
        }

        private void PluginSettings_Click(object sender, RoutedEventArgs e)
        {
            _isEditing = true;
            PluginSettingsDialog pluginSettingsDialog = new PluginSettingsDialog(_plugin);
            pluginSettingsDialog.ShowDialog();
            AddTextToEventsList("Plugin settings window opened.");
        }

        public void AddTextToEventsList(string text)
        {
            if (text == "" || text == null)
            {
                return;
            }

            EventsList.Items.Add(text);
            EventsList.ScrollIntoView(text);
        }

        private void EventsListMenuItemCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is MenuItem menuItem && menuItem.DataContext is string itemText && !string.IsNullOrWhiteSpace(itemText))
                {
                    Clipboard.SetText(itemText);
                }
            }
            catch {}
        }

        #region Main Menu Handlers

        private void FileNewPluginMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditing)
            {
                if (MessageBox.Show("Are you sure that you want to finish editing this file? All changes will not be saved!", "Attention", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _isEditing = false;

                    if (Directory.Exists(userTempPath + _plugin.Guid))
                    {
                        Directory.Delete(userTempPath + _plugin.Guid);
                        AddTextToEventsList("Cleaning old cache folder for this plugin at " + userTempPath + _plugin.Guid);
                    }
                }
                else
                {
                    AddTextToEventsList("Create new file cancelled by user.");
                    return;
                }
            }

            _plugin = null;

            _plugin = new Plugin
            {
                Name = "New plugin",
                Guid = Guid.NewGuid(),
                Id = PluginUtility.GenerateUniquePluginId(),
                Description = "This plugin deletes universe. Be careful",
                PluginType = PluginSystem.Enums.PluginType.Automation,
                Author = "Walter White",
                Version = 0,
                WebSite = "https://somesite.com",
                PluginExecutions = new List<PluginExecution>()
                {
                    new PluginExecution
                    {
                        CommandName = "/somecommand",
                        Help = "Write /somecommand to do cool stuff",
                        ExecutionFilePath = "some_script.ps1",
                        ExecutionTimeout = 10,
                        DefaultErrorMessage = "Default error message on fail or timeout",
                        ExecuteType = CommandExecuteTypes.PSScript
                    }
                },
                PluginFiles = new List<PluginFile>()
            };

            if (Directory.Exists(userTempPath + _plugin.Guid))
            {
                Directory.Delete(userTempPath + _plugin.Guid);
            }

            Directory.CreateDirectory(userTempPath + _plugin.Guid);
            _isEditing = true;

            ClearFormData();
            InitFormData();

            AddTextToEventsList("Created new plugin");
        }

        private async void FileOpenFileMenuItem_Click(object sender, RoutedEventArgs e)
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
                    // Get plugin file
                    _plugin = await PluginUtility.GetPlugin(openFileDialog.FileName);

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
                            if (_plugin.PluginFiles != null)
                            {
                                await PluginUtility.ExtractPluginFiles(_plugin, userTempPath + _plugin.Guid);
                            }
                        }

                        _isEditing = true;
                        InitFormData();
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

        private async void FileSavePluginMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Check if commands do not start with / character
            var invalidCommands = _plugin.PluginExecutions.Where(exec => !exec.CommandName.StartsWith("/")).ToList();
            if (invalidCommands.Any())
            {
                var sb = new StringBuilder();
                sb.AppendLine("Error: The following commands do not start with '/'. Please fix them:");
                foreach (var exec in invalidCommands)
                {
                    sb.AppendLine($"\n{exec.CommandName}");
                }
                MessageBox.Show(sb.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Check if not unique values speicified
            var duplicates = _plugin.PluginExecutions.GroupBy(exec => exec.CommandName, StringComparer.OrdinalIgnoreCase).Where(group => group.Count() > 1).Select(group => group.Key).ToList();
            if (duplicates.Any())
            {
                var message = new StringBuilder();
                message.AppendLine("Error: The following commands are duplicated. Please remove these duplicates:");
                foreach (var d in duplicates)
                {
                    message.AppendLine($"\n{d}");
                }
                MessageBox.Show(message.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    _plugin.Description = PluginDescription.Text;

                    if (PluginType.SelectedIndex == 0)
                        _plugin.PluginType = PluginSystem.Enums.PluginType.Monitoring;
                    else if (PluginType.SelectedIndex == 1)
                        _plugin.PluginType = PluginSystem.Enums.PluginType.Alerting;
                    else if (PluginType.SelectedIndex == 2)
                        _plugin.PluginType = PluginSystem.Enums.PluginType.Logging;
                    else if (PluginType.SelectedIndex == 3)
                        _plugin.PluginType = PluginSystem.Enums.PluginType.Automation;
                    else if (PluginType.SelectedIndex == 4)
                        _plugin.PluginType = PluginSystem.Enums.PluginType.Infrastructure;
                    else if (PluginType.SelectedIndex == 5)
                        _plugin.PluginType = PluginSystem.Enums.PluginType.Configuration;
                    else if (PluginType.SelectedIndex == 6)
                        _plugin.PluginType = PluginSystem.Enums.PluginType.Administration;
                    else if (PluginType.SelectedIndex == 7)
                        _plugin.PluginType = PluginSystem.Enums.PluginType.ComplianceAndAudit;
                    else if (PluginType.SelectedIndex == 8)
                        _plugin.PluginType = PluginSystem.Enums.PluginType.Other;

                    _plugin.Author = PluginAuthor.Text;
                    _plugin.Version = Convert.ToInt32(PluginVersion.Text);
                    _plugin.WebSite = PluginWebSite.Text;

                    _plugin.PluginFiles = await PluginUtility.ImportPluginFiles(userTempPath + _plugin.Guid);

                    await PluginUtility.SavePlugin(_plugin, saveFileDialog.FileName);

                    _isEditing = false;

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

        private void FileExportPluginMetadataMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Save file dialog
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "JSON Files (*.json)|*.json";
                if (saveFileDialog.ShowDialog() == true)
                {
                    _plugin.Name = PluginName.Text;
                    _plugin.Description = PluginDescription.Text;

                    if (PluginType.SelectedIndex == 0)
                        _plugin.PluginType = PluginSystem.Enums.PluginType.Monitoring;
                    else if (PluginType.SelectedIndex == 1)
                        _plugin.PluginType = PluginSystem.Enums.PluginType.Alerting;
                    else if (PluginType.SelectedIndex == 2)
                        _plugin.PluginType = PluginSystem.Enums.PluginType.Logging;
                    else if (PluginType.SelectedIndex == 3)
                        _plugin.PluginType = PluginSystem.Enums.PluginType.Automation;
                    else if (PluginType.SelectedIndex == 4)
                        _plugin.PluginType = PluginSystem.Enums.PluginType.Infrastructure;
                    else if (PluginType.SelectedIndex == 5)
                        _plugin.PluginType = PluginSystem.Enums.PluginType.Configuration;
                    else if (PluginType.SelectedIndex == 6)
                        _plugin.PluginType = PluginSystem.Enums.PluginType.Administration;
                    else if (PluginType.SelectedIndex == 7)
                        _plugin.PluginType = PluginSystem.Enums.PluginType.ComplianceAndAudit;
                    else if (PluginType.SelectedIndex == 8)
                        _plugin.PluginType = PluginSystem.Enums.PluginType.Other;

                    _plugin.Author = PluginAuthor.Text;
                    _plugin.Version = Convert.ToInt32(PluginVersion.Text);
                    _plugin.WebSite = PluginWebSite.Text;

                    Plugin tempPlugin = _plugin;
                    tempPlugin.PluginFiles = null;

                    var json = JsonSerializer.Serialize(tempPlugin, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                    File.WriteAllText(saveFileDialog.FileName, json);

                    AddTextToEventsList("JSON metadata saved: " + saveFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void FileImportPluginMetadataMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditing)
            {
                if (MessageBox.Show("Are you sure that you want to import metadata? All changes will be lost!", "Attention", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    return;
                }
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".json";
            openFileDialog.Filter = "JSON Files (*.json)|*.json|All files (*.*)|*.*";
            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                try
                {
                    var json = File.ReadAllText(openFileDialog.FileName);
                    Plugin tempPlugin = JsonSerializer.Deserialize<Plugin>(json);
                    tempPlugin.PluginFiles = null;

                    _plugin.Name = tempPlugin.Name;
                    _plugin.Description = tempPlugin.Description;
                    _plugin.PluginType = tempPlugin.PluginType;
                    _plugin.Author = tempPlugin.Author;
                    _plugin.Version = tempPlugin.Version;
                    _plugin.WebSite = tempPlugin.WebSite;
                    _plugin.PluginExecutions = tempPlugin.PluginExecutions;
                    _plugin.Settings = tempPlugin.Settings;

                    // Set Data to UI
                    PluginName.Text = _plugin.Name;
                    PluginDescription.Text = _plugin.Description;

                    if (_plugin.PluginType == PluginSystem.Enums.PluginType.Monitoring)
                        PluginType.SelectedIndex = 0;
                    else if (_plugin.PluginType == PluginSystem.Enums.PluginType.Alerting )
                        PluginType.SelectedIndex = 1;
                    else if (_plugin.PluginType == PluginSystem.Enums.PluginType.Logging )
                        PluginType.SelectedIndex = 2;
                    else if (_plugin.PluginType == PluginSystem.Enums.PluginType.Automation)
                        PluginType.SelectedIndex = 3 ;
                    else if (_plugin.PluginType == PluginSystem.Enums.PluginType.Infrastructure )
                        PluginType.SelectedIndex = 4;
                    else if (_plugin.PluginType == PluginSystem.Enums.PluginType.Configuration)
                        PluginType.SelectedIndex = 5 ;
                    else if (_plugin.PluginType == PluginSystem.Enums.PluginType.Administration )
                        PluginType.SelectedIndex = 6;
                    else if (_plugin.PluginType == PluginSystem.Enums.PluginType.ComplianceAndAudit )
                        PluginType.SelectedIndex = 7;
                    else if (_plugin.PluginType == PluginSystem.Enums.PluginType.Other)
                        PluginType.SelectedIndex = 8;

                    PluginAuthor.Text = _plugin.Author;
                    PluginVersion.Text = _plugin.Version.ToString();
                    PluginWebSite.Text = _plugin.WebSite;

                    AddTextToEventsList("JSON metadata loaded from file: " + openFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    AddTextToEventsList("Error occured: " + ex.Message);
                }
            }
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