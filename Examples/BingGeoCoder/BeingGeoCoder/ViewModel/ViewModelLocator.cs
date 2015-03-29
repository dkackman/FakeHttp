/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocatorTemplate xmlns:vm="using:BeingGeoCoder.ViewModel"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/
using System;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;

using Microsoft.Practices.ServiceLocation;

using BingGeoCoder.Client;

using MockHttp;

namespace BeingGeoCoder.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ViewModelLocator
    {
        public const string SecondPageKey = "SecondPage";

        /// <summary>
        /// Gets the Main property.
        /// </summary>
        [SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (ViewModelBase.IsInDesignModeStatic)
            {
                MessageHandlerFactory.Mode = MessageHandlerMode.Mock;                
            }
            else
            {
                MessageHandlerFactory.Mode = MessageHandlerMode.Mock;
            }

            var mockFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var store = new StorageFolderResponseStore(mockFolder, (name, value) => name == "key");
            SimpleIoc.Default.Register<HttpMessageHandler>(() => MessageHandlerFactory.CreateMessageHandler(store));
            SimpleIoc.Default.Register<IGeoCoder>(() => new GeoCoder(SimpleIoc.Default.GetInstance<HttpMessageHandler>(), "key"));

            var nav = new NavigationService();
            nav.Configure(ViewModelLocator.SecondPageKey, typeof(SecondPage));
            SimpleIoc.Default.Register<INavigationService>(() => nav);

            SimpleIoc.Default.Register<IDialogService, DialogService>();
            SimpleIoc.Default.Register<MainViewModel>();
        }

        /// <summary>
        /// Cleans up all the resources.
        /// </summary>
        public static void Cleanup()
        {
        }
    }
}