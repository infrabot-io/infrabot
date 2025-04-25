using Infrabot.PluginSystem.Execution;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Infrabot.PluginEditor.Notifiers
{
    public class ExecutionFileArgumentWrapper : INotifyPropertyChanged
    {
        private readonly ExecutionFileArgument _argument;
        public ExecutionFileArgument OriginalArgument => _argument;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ExecutionFileArgumentWrapper(ExecutionFileArgument argument)
        {
            _argument = argument;
        }

        public string Name
        {
            get => _argument.Name;
            set
            {
                if (_argument.Name != value)
                {
                    _argument.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? Value
        {
            get => _argument.Value;
            set
            {
                if (_argument.Value != value)
                {
                    _argument.Value = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? Description
        {
            get => _argument.Description;
            set
            {
                if (_argument.Description != value)
                {
                    _argument.Description = value;
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
