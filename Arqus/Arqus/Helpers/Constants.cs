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
    }
}
