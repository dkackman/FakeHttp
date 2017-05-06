using BingGeoCoder.Client;
using GalaSoft.MvvmLight.Ioc;
using Locations.ViewModels;
using Microsoft.Practices.ServiceLocation;
using System.Net.Http;
using Windows.Storage;

namespace Locations
{
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            //if (ViewModelBase.IsInDesignModeStatic)
            //{
            //    SimpleIoc.Default.Register<IDataService, Design.DesignDataService>();
            //    SimpleIoc.Default.Register<INavigationService,
            //    Design.DesignNavigationService>();
            //}
            //else
            //{
            //    SimpleIoc.Default.Register<IDataService, DataService>();
            //    SimpleIoc.Default.Register<INavigationService>(() => new NavigationService());
            //}
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register(() => GeoCoderFactory());
        }

        private static IGeoCoder GeoCoderFactory()
        {
            ViewModelLocator locator = App.Current.Resources["Locator"] as ViewModelLocator;
            return new GeoCoder(new HttpClientHandler(), locator.BingMapsApiKey);
        }

        public MainViewModel Main => SimpleIoc.Default.GetInstance<MainViewModel>();

        public string BingMapsApiKey
        {
            get { return ApplicationData.Current.LocalSettings.Values.TryGetValue("BingMapsApiKey", out object o) ? o.ToString() : ""; }
            set { ApplicationData.Current.LocalSettings.Values["BingMapsApiKey"] = value; }
        }
    }
}