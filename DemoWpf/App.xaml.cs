using System.Windows;
using GalaSoft.MvvmLight.Threading;

namespace DemoWpf
{
    public partial class App
    {
        static App()
        {
            DispatcherHelper.Initialize();
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            Locator.Init();
        }
    }
}