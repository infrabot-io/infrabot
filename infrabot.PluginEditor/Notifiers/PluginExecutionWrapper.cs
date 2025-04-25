using Infrabot.PluginSystem.Enums;
using Infrabot.PluginSystem.Execution;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Infrabot.PluginEditor.Notifiers
{
    public class PluginExecutionWrapper : INotifyPropertyChanged
    {
        private readonly PluginExecution _execution;
        private ObservableCollection<ExecutionFileArgumentWrapper> _executionFileArguments;

        public PluginExecution OriginalExecution => _execution;

        public event PropertyChangedEventHandler? PropertyChanged;

        public PluginExecutionWrapper(PluginExecution execution)
        {
            _execution = execution;

            // Initialize the ObservableCollection
            _executionFileArguments = new ObservableCollection<ExecutionFileArgumentWrapper>(
                _execution.ExecutionFileArguments?.Select(arg => new ExecutionFileArgumentWrapper(arg)) ??
                new List<ExecutionFileArgumentWrapper>());

            // Sync collections
            _executionFileArguments.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    _execution.ExecutionFileArguments ??= new List<ExecutionFileArgument>();
                    foreach (ExecutionFileArgumentWrapper item in e.NewItems!)
                    {
                        _execution.ExecutionFileArguments.Add(item.OriginalArgument);
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    if (_execution.ExecutionFileArguments != null)
                    {
                        foreach (ExecutionFileArgumentWrapper item in e.OldItems!)
                        {
                            _execution.ExecutionFileArguments.Remove(item.OriginalArgument);
                        }
                    }
                }
            };
        }

        public string CommandName
        {
            get => _execution.CommandName;
            set
            {
                if (_execution.CommandName != value)
                {
                    _execution.CommandName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? Help
        {
            get => _execution.Help;
            set
            {
                if (_execution.Help != value)
                {
                    _execution.Help = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ExecutionFilePath
        {
            get => _execution.ExecutionFilePath;
            set
            {
                if (_execution.ExecutionFilePath != value)
                {
                    _execution.ExecutionFilePath = value;
                    OnPropertyChanged();
                }
            }
        }

        public int ExecutionTimeout
        {
            get => _execution.ExecutionTimeout;
            set
            {
                if (_execution.ExecutionTimeout != value)
                {
                    _execution.ExecutionTimeout = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? DefaultErrorMessage
        {
            get => _execution.DefaultErrorMessage;
            set
            {
                if (_execution.DefaultErrorMessage != value)
                {
                    _execution.DefaultErrorMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public CommandExecuteTypes ExecuteType
        {
            get => _execution.ExecuteType;
            set
            {
                if (_execution.ExecuteType != value)
                {
                    _execution.ExecuteType = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<ExecutionFileArgumentWrapper> ExecutionFileArguments
        {
            get => _executionFileArguments;
            set
            {
                _executionFileArguments = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
