using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using PowerKit.Extensions;

namespace YoutubeExplode.Demo.Gui.Utils.Extensions;

internal static class AvaloniaExtensions
{
    extension(IApplicationLifetime lifetime)
    {
        public Control? TryGetMainView() =>
            lifetime switch
            {
                IClassicDesktopStyleApplicationLifetime desktopLifetime =>
                    desktopLifetime.MainWindow,

                ISingleViewApplicationLifetime singleViewLifetime => singleViewLifetime.MainView,

                _ => null,
            };

        public TopLevel? TryGetTopLevel() => lifetime.TryGetMainView()?.Pipe(TopLevel.GetTopLevel);
    }
}
