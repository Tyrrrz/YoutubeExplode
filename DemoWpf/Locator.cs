using DemoWpf.ViewModels;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace DemoWpf
{
    public class Locator
    {
        public static void Init()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<IMainViewModel, MainViewModel>();
        }

        public static void Cleanup()
        {
        }

        public IMainViewModel MainViewModel => ServiceLocator.Current.GetInstance<IMainViewModel>();
    }
}