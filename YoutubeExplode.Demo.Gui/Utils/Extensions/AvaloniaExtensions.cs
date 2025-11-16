using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.VisualTree;

namespace YoutubeExplode.Demo.Gui.Utils.Extensions;

internal static class AvaloniaExtensions
{
    extension(IApplicationLifetime lifetime)
    {
        public Window? TryGetMainWindow() =>
            lifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime
                ? desktopLifetime.MainWindow
                : null;

        public TopLevel? TryGetTopLevel() =>
            lifetime.TryGetMainWindow()
            ?? (lifetime as ISingleViewApplicationLifetime)?.MainView?.GetVisualRoot() as TopLevel;
    }
}
