using System.Windows;
using GalaSoft.MvvmLight.Threading;

namespace YoutubeExplode.DemoWpf
{
    public partial class App
    {
        static App()
        {
            DispatcherHelper.Initialize();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            Locator.Cleanup();
        }
    }
}