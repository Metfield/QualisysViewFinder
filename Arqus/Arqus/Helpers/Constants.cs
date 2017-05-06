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
        public static string MARKER_EXPOSURE_SLIDER_NAME = "Marker Exposure";
        public static string MARKER_THRESHOLD_SLIDER_NAME = "Marker Threshold";

        // Video mode constants
        public static string VIDEO_EXPOSURE_SLIDER_NAME = "Video Exposure";
        public static string VIDEO_FLASH_SLIDER_NAME = "Video Flash";

        // Packet constants
        public static string MARKER_EXPOSURE_PACKET_STRING = "Marker_Exposure";
        public static string MARKER_THRESHOLD_PACKET_STRING = "Marker_Threshold";
        public static string VIDEO_EXPOSURE_PACKET_STRING = "Video_Exposure";
        public static string VIDEO_FLASH_PACKET_STRING = "Video_Flash_Time";
    }
}
