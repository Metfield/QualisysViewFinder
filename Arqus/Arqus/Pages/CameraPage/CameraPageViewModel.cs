﻿using Arqus.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using QTMRealTimeSDK;
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

        public MarkerPageViewModel(INavigationService navigationService, IRESTfulQTMService service)
        {
            _navigationService = navigationService;

            SetCameraModeToMarkerCommand = new DelegateCommand(() => { Task.Run(() => service.SetCameraMode(20400, CameraMode.ModeMarker)); });
            SetCameraModeToVideoCommand = new DelegateCommand(() => { Task.Run(() => service.SetCameraMode(20400, CameraMode.ModeVideo)); });
            SetCameraModeToIntensityCommand = new DelegateCommand(() => { Task.Run(() => service.SetCameraMode(20400, CameraMode.ModeMarkerIntensity)); });
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {
            Debug.WriteLine("Navigated to Marker PageViewModel");
        }

        public DelegateCommand SetCameraModeToMarkerCommand { get; set; }
        public DelegateCommand SetCameraModeToVideoCommand { get; set; }
        public DelegateCommand SetCameraModeToIntensityCommand { get; set; }
    }
}
