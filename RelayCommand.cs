/*
 @ 0xCCCCCCCC
*/

using System;
using System.Windows.Input;

namespace EasyKeeper {
    // Provides a general implementation for commands.
    // T must be of nullable, since the `param` in methods might be null.
    class RelayCommand<T> : ICommand {
        private readonly Action<T> _action;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> action, Predicate<T> canExecute = null)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object param)
        {
            return _canExecute == null || _canExecute((T)param);
        }

        public void Execute(object param)
        {
            _action((T)param);
        }
    }
}
