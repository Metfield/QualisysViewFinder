using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Urho;
using Urho.Forms;
using Xamarin.Forms;

namespace Arqus
{
	public class MarkerPageViewModel : BindableBase, INavigatedAware
	{
        private INavigationService _navigationService;

        public MarkerPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public async void OnNavigatedFrom(NavigationParameters parameters)
        {
        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {
            Debug.WriteLine("Navigated to Marker PageViewModel");
        }
    }
}
