using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using YoutubeExplode.Demo.Gui.ViewModels;

namespace YoutubeExplode.Demo.Gui.Views;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    public new MainViewModel DataContext
    {
        get =>
            (MainViewModel)(
                base.DataContext ?? throw new InvalidOperationException("DataContext is not set.")
            );
        set => base.DataContext = value;
    }

    private void Window_OnLoaded(object? sender, RoutedEventArgs args) => QueryTextBox.Focus();
}
