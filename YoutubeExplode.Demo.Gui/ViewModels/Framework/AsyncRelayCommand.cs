using System;
using System.Threading.Tasks;

namespace YoutubeExplode.Demo.Gui.ViewModels.Framework;

public class AsyncRelayCommand<T> : RelayCommand<T>
{
    public AsyncRelayCommand(Func<T, Task> executeAsync, Func<T, bool> canExecute)
        : base(async x => await executeAsync(x), canExecute) { }

    public AsyncRelayCommand(Func<T, Task> executeAsync)
        : this(executeAsync, _ => true) { }
}

public class AsyncRelayCommand : RelayCommand
{
    public AsyncRelayCommand(Func<Task> executeAsync, Func<bool> canExecute)
        : base(async () => await executeAsync(), canExecute) { }

    public AsyncRelayCommand(Func<Task> executeAsync)
        : this(executeAsync, () => true) { }
}
