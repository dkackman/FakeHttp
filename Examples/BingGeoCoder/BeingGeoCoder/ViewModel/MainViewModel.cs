using System;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using BeingGeoCoder.Common;

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

        private string _welcomeTitle = string.Empty;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IGeoCoder geoCoder, INavigationService navigationService)
        {
            _geoCoder = geoCoder;
            _navigationService = navigationService;
            Initialize();
        }

        /// <summary>
        /// Gets the NavigateCommand.
        /// </summary>
        public RelayCommand NavigateCommand
        {
            get
            {
                return _navigateCommand
                       ?? (_navigateCommand = new RelayCommand(() => _navigationService.NavigateTo(ViewModelLocator.SecondPageKey)));
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

        private string _address;
        public string CurrentAddress
        {
            get { return _address; }
            set { Set<String>(ref _address, value); }
        }

        public void Load(DateTime lastVisit)
        {
        }

        private async Task Initialize()
        {
            try
            {
                WelcomeTitle = "Fake Http Geo Coding";
                CurrentAddress = await _geoCoder.GetFormattedAddress(44.9108238220215, -93.1702041625977);
            }
            catch (Exception ex)
            {
                CurrentAddress = ex.Message;
            }
        }
    }
}