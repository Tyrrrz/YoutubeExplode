using System;

namespace YoutubeExplode.Demo.Gui.Utils;

// Straightforward implementation of IProgress<T> that simply invokes a delegate
// without using any synchronization (unlike the built-in Progress<T> class).
// This is required in Avalonia because the built-in Progress<T> class causes race conditions.
internal class DelegateProgress<T>(Action<T> report) : IProgress<T>
{
    public void Report(T value) => report(value);
}
