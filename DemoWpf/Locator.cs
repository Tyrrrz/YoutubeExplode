using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using YoutubeExplode.DemoWpf.ViewModels;

namespace YoutubeExplode.DemoWpf
{
    public class Locator
    {
        public static void Init()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register(() => new YoutubeClient());

            SimpleIoc.Default.Register<IMainViewModel, MainViewModel>();
        }

        public static void Cleanup()
        {
        }

        public IMainViewModel MainViewModel => ServiceLocator.Current.GetInstance<IMainViewModel>();
    }
}