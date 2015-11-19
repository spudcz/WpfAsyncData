using System;
using System.Windows.Input;

namespace WpfAsyncDataSample.Model
{
    public class RelayCommand : ICommand
    {
        public RelayCommand() { }

        public RelayCommand(Action<object> executeAction)
        {
            ExecuteAction = executeAction;
        }

        public RelayCommand(Action<object> executeAction, Predicate<object> canExecuteAction)
            : this (executeAction)
        {
            CanExecuteAction = canExecuteAction;
        }

        public Action<object> ExecuteAction { get; set; }
        public Predicate<object> CanExecuteAction { get; set; }

        public bool CanExecute(object parameter)
        {
            if (CanExecuteAction != null) {
                return CanExecuteAction(parameter);
            }
            return true;
        }

        public void Execute(object parameter)
        {
            if (ExecuteAction != null) {
                ExecuteAction(parameter);
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}