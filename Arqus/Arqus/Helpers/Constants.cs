using System;
using System.Collections.Generic;
using System.Text;

namespace Arqus.Helpers
{
    public static class Constants
    {        
        /// <summary>
        /// Camera settings DRAWER string constants  
        /// </summary>        
        // Marker mode constants
        public const string MARKER_EXPOSURE_SLIDER_NAME = "Marker Exposure";
        public const string MARKER_THRESHOLD_SLIDER_NAME = "Marker Threshold";

        // Video mode constants
        public const string VIDEO_EXPOSURE_SLIDER_NAME = "Video Exposure";
        public const string VIDEO_FLASH_SLIDER_NAME = "Video Flash";

        // Packet constants
        public const string MARKER_EXPOSURE_PACKET_STRING = "Marker_Exposure";
        public const string MARKER_THRESHOLD_PACKET_STRING = "Marker_Threshold";
        public const string VIDEO_EXPOSURE_PACKET_STRING = "Video_Exposure";
        public const string VIDEO_FLASH_PACKET_STRING = "Video_Flash_Time";
        public const string LENS_FOCUS_PACKET_STRING = "Focus";
        public const string LENS_APERTURE_PACKET_STRING = "Aperture";

        // Mode toolbar icon constants
        public const string MODEBAR_ICON_VIDEO_NORMAL = "drawable-hdpi/ic_camera_white_24dp.png";
        public const string MODEBAR_ICON_VIDEO_DEMO = "drawable-hdpi/ic_camera_grey_800_24dp.png";
        public const string MODEBAR_ICON_INTENSITY_NORMAL = "drawable-hdpi/ic_exposure_white_24dp.png";
        public const string MODEBAR_ICON_INTENSITY_DEMO = "drawable-hdpi/ic_exposure_grey_800_24dp.png";

        /// <summary>
        /// Navigation strings
        /// </summary>
        public const string NAVIGATION_DEMO_MODE_STRING = "demoMode";
    }
}
