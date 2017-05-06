using System;
using System.Threading.Tasks;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using BingGeoCoder.Client;

namespace Locations.ViewModels
{
    public sealed class MainViewModel : ViewModelBase
    {
        private readonly IGeoCoder _geocoder;

        public MainViewModel(IGeoCoder geocoder)
        {
            _geocoder = geocoder;

            LookupAddress = new RelayCommand(() => Lookup(), () => !string.IsNullOrEmpty(Landmark));
        }

        private string _landmark;
        public string Landmark
        {
            get { return _landmark; }
            set { _landmark = value; LookupAddress.RaiseCanExecuteChanged(); }
        }

        private string _erroMessage;
        public string ErrorMessage { get { return _erroMessage; } private set { _erroMessage = value; RaisePropertyChanged(); } }

        private bool _isBusy;
        public bool IsBusy { get { return _isBusy; } private set { _isBusy = value; RaisePropertyChanged(); } }

        private string _location;
        public string Location { get { return _location; } private set { _location = value; RaisePropertyChanged(); } }

        public RelayCommand LookupAddress { get; private set; }

        public async Task Lookup()
        {
            IsBusy = true;
            ErrorMessage = "";
            Location = "";

            try
            {
                var coord = await _geocoder.GetCoordinate(Landmark);
                Location = $"{coord.Item1}, {coord.Item2}";
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                Location = "";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}