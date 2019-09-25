using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using YoutubeExplode.DemoWpf.ViewModels;

namespace YoutubeExplode.DemoWpf
{
    public class Locator
    {
        public static void Init()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<MainViewModel>();
        }

        public MainViewModel MainViewModel => ServiceLocator.Current.GetInstance<MainViewModel>();
    }
}