using System;
using System.Collections.Generic;
using System.Text;

namespace Arqus.Helpers
{
    class PacketObjects
    {

        public class ImageCameraPacket
        {
            public Camera Camera { get; set; }
        }

        public class Camera
        {
            public Image Image { get; set; }

        }

        public class Image
        {
            //<summary></summary>
            public uint ID { get; set; }
            //<summary></summary>
            public bool Enabled { get; set; }
            //<summary></summary>
            public uint Width { get; set; }
            //<summary></summary>
            public uint Height { get; set; }
            //<summary></summary>
            public float Left_Crop { get; set; }
            //<summary></summary>
            public float Top_Crop { get; set; }
            //<summary></summary>
            public float Right_Crop { get; set; }
            //<summary></summary>
            public float Bottom_Crop { get; set; }
        }

    }
}
