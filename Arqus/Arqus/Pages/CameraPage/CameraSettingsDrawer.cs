using QTMRealTimeSDK;
using Arqus.Helpers;
using System.Collections.Generic;
using QTMRealTimeSDK.Settings;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.Threading;

namespace Arqus
{
    // ATTENTION!!!! It is very important to set Max value first, otherwise an
    // exception will be triggered
    // -------------------------------------------------------------------------
    // -------------------------------------------------------------------------
    /// <summary>
    /// This class is in charge of handling the layout for the settings drawer
    /// in the cameraPage according the the currently selected mode
    /// </summary>
    public class CameraSettingsDrawer
    {
        // Holds reference to CameraPageViewModel so that bindings
        // can be changed here
        CameraPageViewModel pageViewModel;
    //    CameraState currentState, previousState;

        CameraMode currentMode, previousMode;

        // Create sliders
        // We're now using four different sliders (two for each mode)
        SettingsSlider /*firstSlider, secondSlider,*/

                       markerExposureSlider, markerThresholdSlider,
                       videoExposureSlider, videoFlashSlider;

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
        public CameraSettingsDrawer(CameraPageViewModel cpvm, CameraState cs, CameraSettings generalSettings)
        {
            pageViewModel = cpvm;            

            // Create slider objects 
            CreateSettingSliders();

            // Set drawer mode
            SetCamera(generalSettings);                  
        }

        /// <summary>
        /// Creates settings sliders objects
        /// </summary>
        void CreateSettingSliders()
        {
            markerExposureSlider  = new SettingsSlider();
            markerThresholdSlider = new SettingsSlider();
            videoExposureSlider   = new SettingsSlider();
            videoFlashSlider      = new SettingsSlider();
        }

        void SetSettings(CameraSettings generalSettings)
        {            
            markerExposureSlider.Max = generalSettings.MarkerExposure.Max;
            markerExposureSlider.Min = generalSettings.MarkerExposure.Min;
            markerExposureSlider.Value = generalSettings.MarkerExposure.Current;

            markerThresholdSlider.Max = generalSettings.MarkerThreshold.Max;
            markerThresholdSlider.Min = generalSettings.MarkerThreshold.Min;
            markerThresholdSlider.Value = generalSettings.MarkerThreshold.Current;

            videoExposureSlider.Max = generalSettings.VideoExposure.Max;
            videoExposureSlider.Min = generalSettings.VideoExposure.Min;
            videoExposureSlider.Value = generalSettings.VideoExposure.Current;

            videoFlashSlider.Max = generalSettings.VideoFlashTime.Max;
            videoFlashSlider.Min = generalSettings.VideoFlashTime.Min;
            videoFlashSlider.Value = generalSettings.VideoFlashTime.Current;
        }

        public void SetCamera(CameraSettings generalSettings)
        {   
            SetSettings(generalSettings);

            UpdateDrawer();
        }

        private void UpdateDrawer()
        {
            // Marker Exposure slider
            pageViewModel.MarkerExposureSliderMax = markerExposureSlider.Max;
            pageViewModel.MarkerExposureSliderMin = markerExposureSlider.Min;
            pageViewModel.MarkerExposureSliderValue = markerExposureSlider.Value;            

            // Marker Threshold slider
            pageViewModel.MarkerThresholdSliderMax = markerThresholdSlider.Max;
            pageViewModel.MarkerThresholdSliderMin = markerThresholdSlider.Min;
            pageViewModel.MarkerThresholdSliderValue = markerThresholdSlider.Value;

            // Video Exposure slider
            pageViewModel.VideoExposureSliderMax = videoExposureSlider.Max;
            pageViewModel.VideoExposureSliderMin = videoExposureSlider.Min;
            pageViewModel.VideoExposureSliderValue = videoExposureSlider.Value;

            // Video Flash slider
            pageViewModel.VideoFlashSliderMax = videoFlashSlider.Max;
            pageViewModel.VideoFlashSliderMin = videoFlashSlider.Min;
            pageViewModel.VideoFlashSliderValue = videoFlashSlider.Value;
        }
         

        /// <summary>
        /// Changes current value on slider and sliders' text
        /// according to the new stream mode
        /// </summary>
        /// <param name="newMode"></param>
        /// <param name="newCamSettings"></param>
       /* public void SetDrawerMode(CameraMode newMode)
        {
            // Return if it's same mode
            if (currentMode == newMode)
                return;

            previousMode = currentMode;
            currentMode = newMode;
            

            //pageViewModel.qtmUpdatedSettingValue = true;

            // Assign current sliders based on mode
            switch (currentMode)
            {
                case CameraMode.ModeMarker:
                case CameraMode.ModeMarkerIntensity:

                    // Return if we were already in similar mode
                    if (previousMode == CameraMode.ModeMarker ||
                        previousMode == CameraMode.ModeMarkerIntensity)
                        return;

                    
                    // Store previous values
                    videoExposureSlider.Value = firstSlider.Value;
                    videoFlashSlider.Value = secondSlider.Value;                    

                    // Set mew sliders and strings for marker mode
                    pageViewModel.FirstSliderString = Constants.MARKER_EXPOSURE_SLIDER_NAME;
                    firstSlider = markerExposureSlider;

                    pageViewModel.SecondSliderString = Constants.MARKER_THRESHOLD_SLIDER_NAME;
                    secondSlider = markerThresholdSlider;

                    break;

                case CameraMode.ModeVideo:

                   
                    // Store previous values
                    markerExposureSlider.Value = firstSlider.Value;
                    markerThresholdSlider.Value = secondSlider.Value;
                    
                    // Set mew sliders and strings for video mode
                    pageViewModel.FirstSliderString = Constants.VIDEO_EXPOSURE_SLIDER_NAME;
                    firstSlider = videoExposureSlider;

                    pageViewModel.SecondSliderString = Constants.VIDEO_FLASH_SLIDER_NAME;
                    secondSlider = videoFlashSlider;

                    break;
            }

            UpdateDrawer();

            //pageViewModel.qtmUpdatedSettingValue = false;
        }*/
       

        //public async void OnFirstSliderValueChangedFromUI(object sender, ValueChangedEventArgs args)
        public async void OnFirstSliderValueChangedFromUI(double value)
        {
          /*  firstSlider.Value = value;
                        
            await SettingsService.SetCameraSettings(currentState.ID, IsCurrentModeVideo() ? 
                                                                        Constants.VIDEO_EXPOSURE_PACKET_STRING :
                                                                        Constants.MARKER_EXPOSURE_PACKET_STRING, (float)value);*/
        }

        //public async void OnSecondSliderValueChangedFromUI(object sender, ValueChangedEventArgs args)
        public async void OnSecondSliderValueChangedFromUI(double value)
        {
            /*secondSlider.Value = value;

            await SettingsService.SetCameraSettings(currentState.ID, IsCurrentModeVideo() ?
                                                                        Constants.VIDEO_FLASH_PACKET_STRING :
                                                                        Constants.MARKER_THRESHOLD_PACKET_STRING, (float)value);*/
        }

        private bool IsCurrentModeVideo()
        {
            return currentMode == CameraMode.ModeVideo;
        }
    }
}
