using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Arqus;
using static Arqus.Helpers.PacketObjects;

namespace Arqus.Helpers
{

    public static class PacketConverter
    {

        public static string ImageModeEnabledPacket(uint id, bool enabled)
        {
            XmlDocument document = new XmlDocument();
            string packet = @"<QTM_Settings>
                <Image>
                    <Camera>
                        <ID>1</ID>
                        <Enabled>{0}</Enabled>
                        <Format>JPG</Format>
                    </Camera>
                </Image>
            </QTM_Settings>";

            document.LoadXml(string.Format(packet, enabled));
            return document.OuterXml;
        }
    }
}
