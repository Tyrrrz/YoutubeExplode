using System;
using System.Windows.Input;

namespace YoutubeExplode.Demo.Gui.ViewModels.Framework;

public class RelayCommand<T>(Action<T> execute, Func<T, bool> canExecute) : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public RelayCommand(Action<T> execute)
        : this(execute, _ => true) { }

    public bool CanExecute(object? parameter) =>
        canExecute(parameter is not null ? (T)parameter : default!);

    public void Execute(object? parameter) =>
        execute(parameter is not null ? (T)parameter : default!);

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public class RelayCommand(Action execute, Func<bool> canExecute) : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public RelayCommand(Action execute)
        : this(execute, () => true) { }

    public bool CanExecute(object? parameter) => canExecute();

    public void Execute(object? parameter) => execute();

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
