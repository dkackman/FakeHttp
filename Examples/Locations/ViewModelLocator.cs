using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;

using Microsoft.Practices.ServiceLocation;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;

using Locations.ViewModels;

using BingGeoCoder.Client;

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