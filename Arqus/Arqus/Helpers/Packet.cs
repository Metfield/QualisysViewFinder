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

        public static string CameraImage(uint id, bool enabled, string width, string height, string format = "JPG")
        {
            XmlDocument document = new XmlDocument();
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

            document.LoadXml(string.Format(packet, id, enabled, format, width, height));

            return document.OuterXml;
        }

        public static string Camera(uint id, string mode)
        {
            XmlDocument document = new XmlDocument();
            string packet = @"<QTM_Settings>
                <General>
                    <Camera>
                        <ID>{0}</ID>
                        <Mode>{1}</Mode>
                    </Camera>
                </General>
            </QTM_Settings>";

            document.LoadXml(string.Format(packet, id, mode));

            return document.OuterXml;
        }

    }
}
