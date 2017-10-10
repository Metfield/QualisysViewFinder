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
        public const string AUTO_EXPOSURE_ENABLE_PACKET_STRING = "Enabled";
        public const string AUTO_EXPOSURE_COMPENSATION_PACKET_STRING = "Compensation";

        // Mode toolbar icon constants
        public const string MODEBAR_ICON_MARKER_NORMAL = "ic_markers_white_24dp";
        public const string MODEBAR_ICON_MARKER_GREY = "ic_markers_grey_800_24dp";
        public const string MODEBAR_ICON_VIDEO_NORMAL = "ic_videocam_white_24dp";
        public const string MODEBAR_ICON_VIDEO_DEMO = "ic_videocam_grey_800_24dp";
        public const string MODEBAR_ICON_INTENSITY_NORMAL = "ic_intensity_white_24dp";
        public const string MODEBAR_ICON_INTENSITY_DEMO = "ic_intensity_grey_800_24dp";

        /// <summary>
        /// Navigation strings
        /// </summary>
        public const string NAVIGATION_DEMO_MODE_STRING = "demoMode";

        // Page titles
        public const string TITLE_GRIDVIEW = "Select a camera";

        // Texture size
        public const int URHO_TEXTURE_SIZE = 512;
    }
}
