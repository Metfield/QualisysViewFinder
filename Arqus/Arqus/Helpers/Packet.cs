﻿using System;
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

        public static string CameraImage(int id, bool enabled, string format = "JPG")
        {
            string packet = @"<QTM_Settings>
                <Image>
                    <Camera>
                        <ID>{0}</ID>
                        <Enabled>{1}</Enabled>
                        <Format>{2}</Format>
                        <Width>640</Width>
                        <Height>400</Height>
                        <Left_Crop>0.0</Left_Crop>
                        <Top_Crop>0.0</Top_Crop>
                        <Right_Crop>1.0</Right_Crop>
                        <Bottom_Crop>1.0</Bottom_Crop>
                    </Camera>
                </Image>
            </QTM_Settings>";

            return FormatStringToXML(string.Format(packet, id, enabled, format));
        }

        public static string CameraImage(int id, bool enabled, string width, string height, string format = "JPG")
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

        public static string SettingsParameter(int id, string parameter, float value)
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

        private static string FormatStringToXML(string value)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(value);

            return document.OuterXml;
        }

    }
}
