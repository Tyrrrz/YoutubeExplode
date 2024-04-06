using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.VisualTree;

namespace YoutubeExplode.Demo.Gui.Utils.Extensions;

internal static class AvaloniaExtensions
{
    public static Window? TryGetMainWindow(this IApplicationLifetime lifetime) =>
        lifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime
            ? desktopLifetime.MainWindow
            : null;

    public static TopLevel? TryGetTopLevel(this IApplicationLifetime lifetime) =>
        lifetime.TryGetMainWindow()
        ?? (lifetime as ISingleViewApplicationLifetime)?.MainView?.GetVisualRoot() as TopLevel;
}
