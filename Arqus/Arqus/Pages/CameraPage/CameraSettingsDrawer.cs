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
    //    CameraStore.CurrentCamera currentState, previousState;

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
        public CameraSettingsDrawer(CameraPageViewModel cpvm, CameraSettings generalSettings)
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
            /*
            markerExposureSlider  = new SettingsSlider();
            markerThresholdSlider = new SettingsSlider();
            videoExposureSlider   = new SettingsSlider();
            videoFlashSlider      = new SettingsSlider();
            */
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

        }
        
       
    }
}
