using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using infrabot.ConfigEditor.Serialization;
using infrabot.ConfigEditor.Windows;
using Microsoft.Win32;

namespace infrabot.ConfigEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isEditing;
        private Config _config;

        public MainWindow()
        {
            InitializeComponent();
            _isEditing = false;
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

        public void AddTextToEventsList(string text)
        {
            if (text == "" || text == null)
            {
                return;
            }

            EventsList.Items.Add(text);
            EventsList.ScrollIntoView(EventsList.Items.Count - 1);
        }

        private void AddItemToAllowedUsersIDList(object sender, RoutedEventArgs e)
        {
            try
            {
                Button? button = (Button)sender;
                if (button.Name == "AllowedUsersIDButton")
                {
                    AllowedUsersIDList.Items.Add(new AllowedIDs { ID = Convert.ToInt64(AllowedUsersIDText.Text) });
                    AllowedUsersIDText.Text = "";
                    AllowedUsersIDText.Focus();
                }
                else if (button.Name == "AllowedUsersIDButtonEmergency")
                {
                    AllowedUsersIDListEmergency.Items.Add(new AllowedIDs { ID = Convert.ToInt64(AllowedUsersIDTextEmergency.Text) });
                    AllowedUsersIDTextEmergency.Text = "";
                    AllowedUsersIDTextEmergency.Focus();
                }
                else if (button.Name == "AllowedUsersIDButtonReloadConfig")
                {
                    AllowedUsersIDListReloadConfig.Items.Add(new AllowedIDs { ID = Convert.ToInt64(AllowedUsersIDTextReloadConfig.Text) });
                    AllowedUsersIDTextReloadConfig.Text = "";
                    AllowedUsersIDTextReloadConfig.Focus();
                }
                else if (button.Name == "AllowedUsersIDButtonGetCommands")
                {
                    AllowedUsersIDListGetCommands.Items.Add(new AllowedIDs { ID = Convert.ToInt64(AllowedUsersIDTextGetCommands.Text) });
                    AllowedUsersIDTextGetCommands.Text = "";
                    AllowedUsersIDTextGetCommands.Focus();
                }

                _isEditing = true;
            }
            catch { }
        }

        private void DeleteItemFromAllowedList(object sender, RoutedEventArgs e)
        {
            try
            {
                Button? button = sender as Button;
                if (button.Name == "AllowedUsersIDListItem")
                    AllowedUsersIDList.Items.Remove((AllowedIDs)button.DataContext);
                else if (button.Name == "AllowedUsersIDListItemEmergency")
                    AllowedUsersIDListEmergency.Items.Remove((AllowedIDs)button.DataContext);
                else if (button.Name == "AllowedUsersIDListItemReloadConfig")
                    AllowedUsersIDListReloadConfig.Items.Remove((AllowedIDs)button.DataContext);
                else if (button.Name == "AllowedUsersIDListItemGetCommands")
                    AllowedUsersIDListGetCommands.Items.Remove((AllowedIDs)button.DataContext);

                _isEditing = true;
            }
            catch { }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void InitFormFromConfig(Config config)
        {
            if(config == null)
            {
                AddTextToEventsList("Config file is null. Could not initiate form.");
                return;
            }

            _isEditing = true;
            MainPluginJsonDataScroll.IsEnabled = true;
            FileSaveConfigMenuItem.IsEnabled = true;
            FileCloseConfigMenuItem.IsEnabled = true;
            MainPluginJsonDataScroll.ScrollToHome();

            AllowedUsersIDList.Items.Clear();
            AllowedUsersIDListEmergency.Items.Clear();
            AllowedUsersIDListReloadConfig.Items.Clear();
            AllowedUsersIDListGetCommands.Items.Clear();
            AllowedUsersIDText.Text = "";
            AllowedUsersIDTextEmergency.Text = "";
            AllowedUsersIDTextReloadConfig.Text = "";
            AllowedUsersIDTextGetCommands.Text = "";

            TelegramBotToken.Text = config.telegram_bot_token;

            if(config.telegram_enable_logging == true)
            {
                EnableLogging.SelectedIndex = 0;
            }
            else
            {
                EnableLogging.SelectedIndex = 1;
            }

            if (config.telegram_enable_reloadconfig == true)
            {
                EnableReloadConfig.SelectedIndex = 0;
            }
            else
            {
                EnableReloadConfig.SelectedIndex = 1;
            }

            if (config.telegram_enable_emergency == true)
            {
                EnableEmergency.SelectedIndex = 0;
            }
            else
            {
                EnableEmergency.SelectedIndex = 1;
            }

            if (config.telegram_enable_showmyid == true)
            {
                EnableShowMyID.SelectedIndex = 0;
            }
            else
            {
                EnableShowMyID.SelectedIndex = 1;
            }

            PowerShellDefaultPath.Text = config.telegram_powershell_default_path;
            PowerShellArguments.Text = config.telegram_powershell_arguments;
            ResultMaxLength.Text = config.telegram_result_max_length.ToString();

            foreach (int allowedUser in config.telegram_allowed_users_id)
            {
                AllowedUsersIDList.Items.Add(new AllowedIDs { ID = Convert.ToInt64(allowedUser) });
            }

            foreach (int allowedUser in config.telegram_allowed_users_id_emergency)
            {
                AllowedUsersIDListEmergency.Items.Add(new AllowedIDs { ID = Convert.ToInt64(allowedUser) });
            }

            foreach (int allowedUser in config.telegram_allowed_users_id_reloadconfig)
            {
                AllowedUsersIDListReloadConfig.Items.Add(new AllowedIDs { ID = Convert.ToInt64(allowedUser) });
            }

            foreach (int allowedUser in config.telegram_allowed_users_id_getcommands)
            {
                AllowedUsersIDListGetCommands.Items.Add(new AllowedIDs { ID = Convert.ToInt64(allowedUser) });
            }
        }

        private void DeinitForm()
        {
            _isEditing = false;
            _config = null;
            AllowedUsersIDList.Items.Clear();
            AllowedUsersIDListEmergency.Items.Clear();
            AllowedUsersIDListReloadConfig.Items.Clear();
            AllowedUsersIDListGetCommands.Items.Clear();
            AllowedUsersIDText.Text = "";
            AllowedUsersIDTextEmergency.Text = "";
            AllowedUsersIDTextReloadConfig.Text = "";
            AllowedUsersIDTextGetCommands.Text = "";
            TelegramBotToken.Text = "";
            EnableLogging.SelectedIndex = 1;
            EnableReloadConfig.SelectedIndex = 0;
            EnableEmergency.SelectedIndex = 1;
            EnableShowMyID.SelectedIndex = 1;
            PowerShellDefaultPath.Text = "";
            PowerShellArguments.Text = "";
            ResultMaxLength.Text = "";
            FileSaveConfigMenuItem.IsEnabled = false;
            FileCloseConfigMenuItem.IsEnabled = false;
            MainPluginJsonDataScroll.IsEnabled = false;
            MainPluginJsonDataScroll.ScrollToHome();
        }

        private void ConfigTextChanged(object sender, TextChangedEventArgs e)
        {
            _isEditing = true;
        }

        private void ConfigSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _isEditing = true;
        }


        #region Main Menu Handlers

        private void FileNewConfigMenuItem_Click(object sender, RoutedEventArgs e)
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

            _config = null;
            _config = new Config() 
            {
                telegram_bot_token = "Your_Bot_token",
                telegram_enable_logging = false,
                telegram_enable_reloadconfig = true,
                telegram_enable_emergency = true,
                telegram_enable_showmyid = false,
                telegram_powershell_default_path = "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe",
                telegram_powershell_arguments = "-ExecutionPolicy Unrestricted",
                telegram_result_max_length = 12000,
                telegram_allowed_users_id = new List<long>(),
                telegram_allowed_users_id_emergency = new List<long>(),
                telegram_allowed_users_id_reloadconfig = new List<long>(),
                telegram_allowed_users_id_getcommands = new List<long>()
            };
            InitFormFromConfig(_config);

            AddTextToEventsList("New configuration created");
        }

        private void FileOpenConfigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditing)
            {
                if (MessageBox.Show("Are you sure that you want to finish editing this file? All changes will not be saved!", "Attention", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    AddTextToEventsList("Open another file is cancelled by user.");
                    return;
                }
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".plug";
            openFileDialog.Filter = "Config Files (*.json)|*.json|All files (*.*)|*.*";
            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                try
                {
                    string configJson = File.ReadAllText(openFileDialog.FileName);
                    _config = JsonSerializer.Deserialize<Config>(configJson);
                    InitFormFromConfig(_config);
                    AddTextToEventsList("Loaded configuration file: " + openFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    DeinitForm();
                    AddTextToEventsList("Error occured: " + ex.Message);
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void FileSaveConfigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Save file dialog
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Config Files (*.json)|*.json|All files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == true)
                {
                    _isEditing = false;

                    // Set bot token
                    _config.telegram_bot_token = TelegramBotToken.Text;

                    // Set if to enable logging
                    if(EnableLogging.SelectedIndex == 0)
                    {
                        _config.telegram_enable_logging = true;
                    }
                    else
                    {
                        _config.telegram_enable_logging = false;
                    }

                    // Set if to enable reload config
                    if (EnableReloadConfig.SelectedIndex == 0)
                    {
                        _config.telegram_enable_reloadconfig = true;
                    }
                    else
                    {
                        _config.telegram_enable_reloadconfig = false;
                    }

                    // Set if to enable emergency
                    if (EnableEmergency.SelectedIndex == 0)
                    {
                        _config.telegram_enable_emergency = true;
                    }
                    else
                    {
                        _config.telegram_enable_emergency = false;
                    }

                    // Set if to enable show my id
                    if (EnableShowMyID.SelectedIndex == 0)
                    {
                        _config.telegram_enable_showmyid = true;
                    }
                    else
                    {
                        _config.telegram_enable_showmyid = false;
                    }

                    // Set default powershell config
                    _config.telegram_powershell_default_path = PowerShellDefaultPath.Text;

                    // Set telegram powershell arguments
                    _config.telegram_powershell_arguments = PowerShellArguments.Text;

                    // Set telegram max result return length
                    _config.telegram_result_max_length = Convert.ToInt32(ResultMaxLength.Text);

                    // Set telegram allowed users id
                    if(_config.telegram_allowed_users_id == null)
                        _config.telegram_allowed_users_id = new List<long>();
                    _config.telegram_allowed_users_id.Clear();
                    foreach (AllowedIDs allowedIDs in AllowedUsersIDList.Items)
                    {
                        _config.telegram_allowed_users_id.Add(allowedIDs.ID);
                    }

                    // Set telegram allowed emergency users id
                    if (_config.telegram_allowed_users_id_emergency == null)
                        _config.telegram_allowed_users_id_emergency = new List<long>();
                    _config.telegram_allowed_users_id_emergency.Clear();
                    foreach (AllowedIDs allowedIDs in AllowedUsersIDListEmergency.Items)
                    {
                        _config.telegram_allowed_users_id_emergency.Add(allowedIDs.ID);
                    }

                    // Set telegram allowed reloadconfig users id
                    if (_config.telegram_allowed_users_id_reloadconfig == null)
                        _config.telegram_allowed_users_id_reloadconfig = new List<long>();
                    _config.telegram_allowed_users_id_reloadconfig.Clear();
                    foreach (AllowedIDs allowedIDs in AllowedUsersIDListReloadConfig.Items)
                    {
                        _config.telegram_allowed_users_id_reloadconfig.Add(allowedIDs.ID);
                    }

                    // Set telegram allowed getcommands users id
                    if (_config.telegram_allowed_users_id_getcommands == null)
                        _config.telegram_allowed_users_id_getcommands = new List<long>();
                    _config.telegram_allowed_users_id_getcommands.Clear();
                    foreach (AllowedIDs allowedIDs in AllowedUsersIDListGetCommands.Items)
                    {
                        _config.telegram_allowed_users_id_getcommands.Add(allowedIDs.ID);
                    }

                    // Serialize config and write to file
                    string jsonConfig = JsonSerializer.Serialize<Config>(_config);
                    File.WriteAllText(saveFileDialog.FileName, jsonConfig);

                    AddTextToEventsList("File saved: " + saveFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FileCloseConfigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditing)
            {
                if (MessageBox.Show("Are you sure that you want to close this file? All changes will not be saved!", "Attention", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    AddTextToEventsList("Close current file is cancelled by user.");
                    return;
                }
            }

            try
            {
                _isEditing = false;
                DeinitForm();
                AddTextToEventsList("File closed");
            }
            catch (Exception ex)
            {
                AddTextToEventsList("Clearing plugin`s temporary folder was not successfull. Consider to clean it manually. Error: " + ex.Message);
            }

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
