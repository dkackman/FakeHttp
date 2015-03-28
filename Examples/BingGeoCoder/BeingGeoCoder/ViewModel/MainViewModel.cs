using System;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using BeingGeoCoder.Common;
using BeingGeoCoder.Model;

using BingGeoCoder.Client;

namespace BeingGeoCoder.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// The <see cref="WelcomeTitle" /> property's name.
        /// </summary>
        public const string WelcomeTitlePropertyName = "WelcomeTitle";

        private readonly IGeoCoder _geoCoder;
        private readonly INavigationService _navigationService;

        private RelayCommand _navigateCommand;
        private string _originalTitle;
        private string _welcomeTitle = string.Empty;

        /// <summary>
        /// Gets the NavigateCommand.
        /// </summary>
        public RelayCommand NavigateCommand
        {
            get
            {
                return _navigateCommand
                       ?? (_navigateCommand = new RelayCommand(
                           () => _navigationService.NavigateTo(ViewModelLocator.SecondPageKey)));
            }
        }

        /// <summary>
        /// Gets the WelcomeTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string WelcomeTitle
        {
            get
            {
                return _welcomeTitle;
            }

            set
            {
                Set(ref _welcomeTitle, value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(
            IGeoCoder geoCoder,
            INavigationService navigationService)
        {
            _geoCoder = geoCoder;
            _navigationService = navigationService;
            Initialize();
        }

        private string _address;
        public string CurrentAddress
        {
            get { return _address; }
            set { Set<String>(ref _address, value); }
        }

        public void Load(DateTime lastVisit)
        {
            if (lastVisit > DateTime.MinValue)
            {
                WelcomeTitle = string.Format(
                    "{0} (last visit on the {1})",
                    _originalTitle,
                    lastVisit);
            }
        }

        private async Task Initialize()
        {
            try
            {
                WelcomeTitle = "Geo Coding";
            }
            catch (Exception ex)
            {
                // Report error here
            }
        }
    }
}