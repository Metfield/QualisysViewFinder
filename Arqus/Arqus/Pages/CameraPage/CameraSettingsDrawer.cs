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

        public int  MARKER_MAX_EXPOSURE, MARKER_MIN_EXPOSURE,
                    MARKER_MAX_THRESHOLD, MARKER_MIN_THRESHOLD,

                    VIDEO_MAX_EXPOSURE, VIDEO_MIN_EXPOSURE,
                    VIDEO_MAX_FLASH, VIDEO_MIN_FLASH;

        private float   SLIDER_MIN,
                        SLIDER_MAX;

        /// <summary>
        /// Creates settings drawer object that will hold values
        /// for sliders. This class also normalizes camera settings
        /// values.
        /// </summary>
        /// <param name="cpvm">Current Camera Page ViewModel</param>        
        /// <param name="camID">Current camera in view</param>
        /// <param name="generalSettings">RTSDK's general settings to get key values from</param>
        /// <param name="currCamSettings"></param>
        /// <param name="min">Min value for slider</param>
        /// <param name="max">Max value for slider</param>
        public CameraSettingsDrawer(CameraPageViewModel cpvm, int camID, SettingsGeneralCameraSystem generalSettings, float sliderMin = 0.0f, float sliderMax = 1.0f)
        {
            pageViewModel = cpvm;            
            currentCamera = camID;

            // Set current camera min and max settings
            MARKER_MAX_EXPOSURE = generalSettings.MarkerExposure.Max;
            MARKER_MIN_EXPOSURE = generalSettings.MarkerExposure.Min;

            MARKER_MAX_THRESHOLD = generalSettings.MarkerThreshold.Max;
            MARKER_MIN_THRESHOLD = generalSettings.MarkerThreshold.Min;

            VIDEO_MAX_EXPOSURE = generalSettings.VideoExposure.Max;
            VIDEO_MIN_EXPOSURE = generalSettings.VideoExposure.Min;

            VIDEO_MAX_FLASH = generalSettings.VideoFlashTime.Max;
            VIDEO_MIN_FLASH = generalSettings.VideoFlashTime.Min;

            // Set local min and max values [0,1]
            SLIDER_MIN = sliderMin;
            SLIDER_MAX = sliderMax;

            // Set these min and max values to actual sliders
            pageViewModel.FirstSliderMaxValue = SLIDER_MAX;
            pageViewModel.FirstSliderMinValue = SLIDER_MIN;

            pageViewModel.SecondSliderMaxValue = SLIDER_MAX;
            pageViewModel.SecondSliderMinValue = SLIDER_MIN;                        
        }             

        /// <summary>
        /// Changes current value on slider and sliders' text
        /// according to the new stream mode
        /// </summary>
        /// <param name="newMode"></param>
        /// <param name="newCamSettings"></param>
        public void SetDrawerMode(CameraMode newMode, CameraSettings newCamSettings)
        {
            // ATTENTION! It is very important to set Max value first, otherwise an 
            // exception will be triggered
            switch(newMode)
            {
                case CameraMode.ModeMarker:
                case CameraMode.ModeMarkerIntensity:
                    // Handle first slider -EXPOSURE
                    pageViewModel.FirstSliderString = Constants.MARKER_EXPOSURE_SLIDER_NAME;
                    pageViewModel.FirstSliderValue = DataOperations.ConvertRange(MARKER_MIN_EXPOSURE, MARKER_MAX_EXPOSURE,
                                                                                         SLIDER_MIN, SLIDER_MAX, newCamSettings.MarkerExposure);                        

                    // Handle second slider -THRESHOLD
                    pageViewModel.SecondSliderString = Constants.MARKER_THRESHOLD_SLIDER_NAME;                                  
                    pageViewModel.SecondSliderValue = DataOperations.ConvertRange(MARKER_MIN_THRESHOLD, MARKER_MAX_THRESHOLD,
                                                                                         SLIDER_MIN, SLIDER_MAX, newCamSettings.MarkerThreshold);                                        
                    break;

                case CameraMode.ModeVideo:
                    // Handle first slider -EXPOSURE
                    pageViewModel.FirstSliderString = Constants.VIDEO_EXPOSURE_SLIDER_NAME;
                    pageViewModel.FirstSliderValue = DataOperations.ConvertRange(VIDEO_MIN_EXPOSURE, VIDEO_MAX_EXPOSURE,
                                                                                         SLIDER_MIN, SLIDER_MAX, newCamSettings.VideoExposure);                    

                    // Handle second slider -FLASH_TIME
                    pageViewModel.SecondSliderString = Constants.VIDEO_FLASH_SLIDER_NAME;
                    pageViewModel.SecondSliderValue = DataOperations.ConvertRange(VIDEO_MIN_FLASH, VIDEO_MAX_FLASH,
                                                                                         SLIDER_MIN, SLIDER_MAX, newCamSettings.VideoFlash);
                    break;

                default:
                    // Add Exception handling
                    break;
            }
        }

        /////////////////////////////////////////////////////
        /// Good old fasioned getters from here and out    
        /////////////////////////////////////////////////////
        public int GetMarkerExposureMin()
        {
            return MARKER_MIN_EXPOSURE;
        }
        public int GetMarkerExposureMax()
        {
            return MARKER_MAX_EXPOSURE;
        }

        public int GetMarkerThresholdMin()
        {
            return MARKER_MIN_THRESHOLD;
        }

        public int GetMarkerThresholdMax()
        {
            return MARKER_MAX_THRESHOLD;
        }

        public int GetVideoExposureMin()
        {
            return VIDEO_MIN_EXPOSURE;
        }

        public int GetVideoExposureMax()
        {
            return VIDEO_MAX_EXPOSURE;
        }

        public int GetVideoFlashMin()
        {
            return VIDEO_MIN_FLASH;
        }

        public int GetVideoFlashMax()
        {
            return VIDEO_MAX_FLASH;
        }
    }
}
