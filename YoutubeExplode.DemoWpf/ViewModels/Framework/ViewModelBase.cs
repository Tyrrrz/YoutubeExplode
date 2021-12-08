using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace YoutubeExplode.DemoWpf.ViewModels.Framework;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void RaisePropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected void Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        RaisePropertyChanged(propertyName);
    }
}