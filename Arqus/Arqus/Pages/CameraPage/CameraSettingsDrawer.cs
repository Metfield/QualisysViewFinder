using QTMRealTimeSDK;
using Arqus.Helpers;
using System.Collections.Generic;
using QTMRealTimeSDK.Settings;

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

        int currentCamera;

        int MARKER_MAX_EXPOSURE, MARKER_MIN_EXPOSURE,
            MARKER_MAX_THRESHOLD, MARKER_MIN_THRESHOLD,

            VIDEO_MAX_EXPOSURE, VIDEO_MIN_EXPOSURE,
            VIDEO_MAX_FLASH, VIDEO_MIN_FLASH;

        public CameraSettingsDrawer(CameraPageViewModel cpvm, CameraMode mode, int camID, SettingsGeneralCameraSystem generalSettings, CameraSettings currCamSettings)
        {
            pageViewModel = cpvm;            
            currentCamera = camID;

            MARKER_MAX_EXPOSURE = generalSettings.MarkerExposure.Max;
            MARKER_MIN_EXPOSURE = generalSettings.MarkerExposure.Min;

            MARKER_MAX_THRESHOLD = generalSettings.MarkerThreshold.Max;
            MARKER_MIN_THRESHOLD = generalSettings.MarkerThreshold.Min;

            VIDEO_MAX_EXPOSURE = generalSettings.VideoExposure.Max;
            VIDEO_MIN_EXPOSURE = generalSettings.VideoExposure.Min;

            VIDEO_MAX_FLASH = generalSettings.VideoFlashTime.Max;
            VIDEO_MIN_FLASH = generalSettings.VideoFlashTime.Min;

            ChangeDrawerMode(mode, currCamSettings);
        }

        public void ChangeDrawerMode(CameraMode newMode, CameraSettings newCamSettings)
        {
            // ATTENTION! It is very important to set Max value first, otherwise an 
            // exception will be triggered
            switch(newMode)
            {
                case CameraMode.ModeMarker:
                case CameraMode.ModeMarkerIntensity:
                    // Handle first slider -EXPOSURE
                    pageViewModel.FirstSliderString = Constants.MARKER_EXPOSURE_SLIDER_NAME;                    
                    pageViewModel.FirstSliderMaxValue = MARKER_MAX_EXPOSURE;
                    pageViewModel.FirstSliderMinValue = MARKER_MIN_EXPOSURE;
                    pageViewModel.FirstSliderValue = newCamSettings.MarkerExposure;

                    // Handle second slider -THRESHOLD
                    pageViewModel.SecondSliderString = Constants.MARKER_THRESHOLD_SLIDER_NAME;
                    pageViewModel.SecondSliderMaxValue = MARKER_MAX_THRESHOLD;
                    pageViewModel.SecondSliderMinValue = MARKER_MIN_THRESHOLD;                    
                    pageViewModel.SecondSliderValue = newCamSettings.MarkerThreshold;

                    break;

                case CameraMode.ModeVideo:
                    // Handle first slider -EXPOSURE
                    pageViewModel.FirstSliderString = Constants.VIDEO_EXPOSURE_SLIDER_NAME;                    
                    pageViewModel.FirstSliderMaxValue = VIDEO_MAX_EXPOSURE;
                    pageViewModel.FirstSliderMinValue = VIDEO_MIN_EXPOSURE;
                    pageViewModel.FirstSliderValue = newCamSettings.VideoExposure;

                    // Handle second slider -FLASH_TIME
                    pageViewModel.SecondSliderString = Constants.VIDEO_FLASH_SLIDER_NAME;                    
                    pageViewModel.SecondSliderMaxValue = VIDEO_MAX_FLASH;
                    pageViewModel.SecondSliderMinValue = VIDEO_MIN_FLASH;
                    pageViewModel.SecondSliderValue = newCamSettings.VideoFlash;

                    break;

                default:
                    // Add Exception handling
                    break;
            }
        }
    }
}
