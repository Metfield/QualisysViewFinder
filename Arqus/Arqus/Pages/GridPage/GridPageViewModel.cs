using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Arqus
{
    class GridPageViewModel
    {
        private ICameraService cameraService;
        private ISettingsService settingsService;
        private INavigationService navigationService;

        public GridPageViewModel(INavigationService navigationService, ICameraService cameraService)
        {
            this.navigationService = navigationService;
            this.cameraService = cameraService;
        }

    }
}
