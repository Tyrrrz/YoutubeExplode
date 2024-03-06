using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Material.Styles.Themes;
using YoutubeExplode.Demo.Gui.Views;

namespace YoutubeExplode.Demo.Gui;

public class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow();

        base.OnFrameworkInitializationCompleted();

        // Set custom theme colors
        this.LocateMaterialTheme<MaterialThemeBase>().CurrentTheme = Theme.Create(
            Theme.Light,
            Color.Parse("#343838"),
            Color.Parse("#F9A825")
        );
    }
}
