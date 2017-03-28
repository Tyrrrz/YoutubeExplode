using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using YoutubeExplode.DemoWpf.ViewModels;

namespace YoutubeExplode.DemoWpf
{
    public class Locator
    {
        static Locator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register(() => new YoutubeClient());

            SimpleIoc.Default.Register<IMainViewModel, MainViewModel>();
        }

        public static void Cleanup()
        {
        }

        public static T Resolve<T>() => ServiceLocator.Current.GetInstance<T>();
        public static T Resolve<T>(string id) => ServiceLocator.Current.GetInstance<T>(id);

        public IMainViewModel MainViewModel => Resolve<IMainViewModel>();
    }
}