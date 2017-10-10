using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Arqus;

namespace Arqus.Helpers
{

    public static class Packet
    {
        public enum Type
        {
            LensControl,
            AutoExposure,
            Default
        }

        public static string CameraImage(int id, bool enabled)
        {
            string packet = @"<QTM_Settings>
                <Image>
                    <Camera>
                        <ID>{0}</ID>
                        <Enabled>False</Enabled>
                    </Camera>
                </Image>
            </QTM_Settings>";

            return FormatStringToXML(string.Format(packet, id));
        }

        public static string CameraImage(int id, bool enabled, int width, int height, string format = "JPG")
        {
            string packet = @"<QTM_Settings>
                <Image>
                    <Camera>
                        <ID>{0}</ID>
                        <Enabled>{1}</Enabled>
                        <Format>{2}</Format>
                        <Width>{3}</Width>
                        <Height>{4}</Height>
                    </Camera>
                </Image>
            </QTM_Settings>";
            
            return FormatStringToXML(string.Format(packet, id, enabled, format, width, height));
        }

        public static string CameraImage(int id, int width, int height)
        {
            string packet = @"<QTM_Settings>
                <Image>
                    <Camera>
                        <ID>{0}</ID>
                        <Width>{1}</Width>
                        <Height>{2}</Height>
                    </Camera>
                </Image>
            </QTM_Settings>";

            return FormatStringToXML(string.Format(packet, id, width, height));

        }

        public static string Camera(int id, string mode)
        {
            string packet = @"<QTM_Settings>
                <General>
                    <Camera>
                        <ID>{0}</ID>
                        <Mode>{1}</Mode> 
                    </Camera>
                </General>
            </QTM_Settings>";

            return FormatStringToXML(string.Format(packet, id, mode));
        }

        public static string SettingsParameter(int id, string parameter, string value)
        {
            string packet = @"<QTM_Settings>
                <General>
                    <Camera>
                        <ID>{0}</ID>
                        <"+parameter+">{1}</"+parameter+">" +
                    "</Camera>" + 
                "</General>" +
            "</QTM_Settings>";
            
            return FormatStringToXML(string.Format(packet, id, value));
        }

        // Sends XML packet specifically for LensControl camera settings
        public static string LensControlParameter(int id, string parameter, string value)
        {
            string packet = @"<QTM_Settings>
                <General>
                    <Camera>
                        <ID>{0}</ID>
                            <LensControl>                                
                                <" + parameter + " Value=\"{1}\"/>" +                                
                            "</LensControl >" +
                    "</Camera>" +
                "</General>" +
            "</QTM_Settings>";

            return FormatStringToXML(string.Format(packet, id, value));
        }

        // Auto exposure-specific packet 
        public static string AutoExposureParameter(int id, string parameter, string value)
        {
            // If parameter is "Enabled", gotta convert from float to string
            if (parameter == "Enabled")
            {
                if (value == "1")
                    value = "true";
                else
                    value = "false";
            }

            string packet = @"<QTM_Settings>
                <General>
                    <Camera>
                        <ID>"+id+"</ID>" +
                            "<AutoExposure " + parameter + "=\""+value+"\"/>" +
                    "</Camera>" +
                "</General>" +
            "</QTM_Settings>";

            return FormatStringToXML(string.Format(packet, id, value));
        }

        public static string CropImage(int id, float left, float right, float top, float bottom)
        {

            string packet = @"<QTM_Settings>
                <Image>
                    <Camera>
                        <ID>{0}</ID>
                        <Left_Crop>{1}</Left_Crop>
                        <Right_Crop>{2}</Right_Crop>
                        <Top_Crop>{3}</Top_Crop>
                        <Bottom_Crop>{4}</Bottom_Crop>
                    </Camera>
                </Image>
            </QTM_Settings>";

            return FormatStringToXML(string.Format(packet, id, left, right, top, bottom));
        }

        private static string FormatStringToXML(string value)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(value);

            return document.OuterXml;
        }

    }
}
