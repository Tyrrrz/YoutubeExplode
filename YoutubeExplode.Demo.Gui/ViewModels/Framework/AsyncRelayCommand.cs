using System;
using System.Threading.Tasks;

namespace YoutubeExplode.Demo.Gui.ViewModels.Framework;

public class AsyncRelayCommand<T>(Func<T, Task> executeAsync, Func<T, bool> canExecute)
    : RelayCommand<T>(async x => await executeAsync(x), canExecute)
{
    public AsyncRelayCommand(Func<T, Task> executeAsync)
        : this(executeAsync, _ => true) { }
}

public class AsyncRelayCommand(Func<Task> executeAsync, Func<bool> canExecute)
    : RelayCommand(async () => await executeAsync(), canExecute)
{
    public AsyncRelayCommand(Func<Task> executeAsync)
        : this(executeAsync, () => true) { }
}
