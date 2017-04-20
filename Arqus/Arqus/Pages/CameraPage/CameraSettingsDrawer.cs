using QTMRealTimeSDK;
using Arqus.Helpers;

namespace Arqus
{
    /// <summary>
    /// This class is in charge of handling the layout for the settings drawer
    /// in the cameraPage according the the currently selected mode
    /// </summary>
    class CameraSettingsDrawer
    {
        // Holds reference to CameraPageViewModel so that bindings
        // can be changed here
        CameraPageViewModel pageViewModel;

        public CameraSettingsDrawer(CameraPageViewModel cpvm, CameraMode mode)
        {
            pageViewModel = cpvm;
            ChangeDrawerMode(mode);
        }

        public void ChangeDrawerMode(CameraMode newMode)
        {
            // ATTENTION! It is very important to set Max value first, otherwise an 
            // exception will be triggered
            switch(newMode)
            {
                case CameraMode.ModeMarker:
                case CameraMode.ModeMarkerIntensity:
                    // Handle first slider -EXPOSURE
                    pageViewModel.FirstSliderString = Constants.MARKER_EXPOSURE_SLIDER_NAME;                    
                    pageViewModel.FirstSliderMaxValue = Constants.MARKER_MAX_EXPOSURE;
                    pageViewModel.FirstSliderMinValue = Constants.MARKER_MIN_EXPOSURE;
                    pageViewModel.FirstSliderValue = Constants.MARKER_MAX_EXPOSURE / 2;

                    // Handle second slider -THRESHOLD
                    pageViewModel.SecondSliderString = Constants.MARKER_THRESHOLD_SLIDER_NAME;
                    pageViewModel.SecondSliderMaxValue = Constants.MARKER_MAX_THRESHOLD;
                    pageViewModel.SecondSliderMinValue = Constants.MARKER_MIN_THRESHOLD;                    
                    pageViewModel.SecondSliderValue = Constants.MARKER_MAX_THRESHOLD / 2;

                    break;

                case CameraMode.ModeVideo:
                    // Handle first slider -EXPOSURE
                    pageViewModel.FirstSliderString = Constants.VIDEO_EXPOSURE_SLIDER_NAME;                    
                    pageViewModel.FirstSliderMaxValue = Constants.VIDEO_MAX_EXPOSURE;
                    pageViewModel.FirstSliderMinValue = Constants.VIDEO_MIN_EXPOSURE;
                    pageViewModel.FirstSliderValue = Constants.VIDEO_MAX_EXPOSURE / 2;

                    // Handle second slider -FLASH_TIME
                    pageViewModel.SecondSliderString = Constants.VIDEO_FLASH_SLIDER_NAME;                    
                    pageViewModel.SecondSliderMaxValue = Constants.VIDEO_MAX_FLASH;
                    pageViewModel.SecondSliderMinValue = Constants.VIDEO_MIN_FLASH;
                    pageViewModel.SecondSliderValue = Constants.VIDEO_MAX_FLASH / 2;

                    break;

                default:
                    // Add Exception handling
                    break;
            }
        }
    }
}
