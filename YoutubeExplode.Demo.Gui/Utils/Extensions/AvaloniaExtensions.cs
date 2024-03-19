using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.VisualTree;

namespace YoutubeExplode.Demo.Gui.Utils.Extensions;

internal static class AvaloniaExtensions
{
    public static TopLevel? TryGetTopLevel(this IApplicationLifetime lifetime) =>
        (lifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow
        ?? (lifetime as ISingleViewApplicationLifetime)?.MainView?.GetVisualRoot() as TopLevel;
}
