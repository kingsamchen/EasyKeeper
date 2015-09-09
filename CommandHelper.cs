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

    // Provides an approach for View to execute commands in ViewModel.
    // Unlike uses of RelayCommand, these commands are manually invoked.
    class ExecuteCommand<T, TResult> : ICommand {
        // Unfortunately, we don't need it.
#pragma warning disable 0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore 0067

        private readonly Func<T, TResult> _fn;
        private readonly Predicate<T> _canExecute;
        private TResult _result;

        public ExecuteCommand(Func<T, TResult> fn, Predicate<T> canExecute = null)
        {
            _fn = fn;
            _canExecute = canExecute;
        }

        public TResult Result
        {
            get { return _result; }
        }

        public bool CanExecute(object param)
        {
            return _canExecute == null || _canExecute((T)param);
        }

        public void Execute(object param)
        {
            ResetResult();
           _result = _fn((T)param);
        }

        private void ResetResult()
        {
            _result = default(TResult);
        }
    }
}
