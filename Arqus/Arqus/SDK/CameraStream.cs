using System;
using System.Collections.Generic;
using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;
using System.Linq;
using System.Diagnostics;
using Arqus.Helpers;
using static Arqus.Helpers.PacketConverter;
using System.Threading;
using System.Threading.Tasks;
using ImageSharp.Formats;
using System.IO;
using ImageSharp;

namespace Arqus
{

    public class CameraStream
    {

        private static readonly CameraStream instance = new CameraStream();

        private static JpegDecoder decoder = new JpegDecoder();
        List<ImageSharp.Color[]> imageData = new List<ImageSharp.Color[]>();
        private List<QTMRealTimeSDK.Data.CameraImage> cameras;

        public bool Streaming { private set; get; }

        PacketType packetType;
        ComponentType currentStreamType;
        

        static CameraStream() { }
        private CameraStream() { }

        public static CameraStream Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Starts frame streaming of "type"
        /// </summary>
        /// <param name="type">Specifies the component type to stream</param>
        public bool StartStream(int streamFrequency, ComponentType type)
        {
            if (!Streaming && QTMNetworkConnection.Instance.protocol.IsConnected())
            {
                Console.WriteLine("Starting stream!");
                currentStreamType = type;
                QTMNetworkConnection.Instance.protocol.StreamFrames(StreamRate.RateFrequency, streamFrequency, type); 
                Streaming = true;
            }
            else if (!QTMNetworkConnection.Instance.protocol.IsConnected())
            {
                Console.WriteLine("Unable to start stream: Not Connected");
                Streaming = false;
            }

            return Streaming;
        }

        public async Task<List<QTMRealTimeSDK.Data.CameraImage>> GetMarkerData2D()
        {
            QTMNetworkConnection.Instance.protocol.ReceiveRTPacket(out packetType);
            return await Task.Run(() => (QTMNetworkConnection.Instance.protocol.GetRTPacket().GetImageData()));
        }

        public async Task<int> GetCurrentFrame()
        {
            QTMNetworkConnection.Instance.protocol.ReceiveRTPacket(out packetType);
            return await Task.Run(() => (QTMNetworkConnection.Instance.protocol.GetRTPacket().GetFrameNumber()));
        }
        
        public async Task<List<ImageSharp.Color[]>> GetImageData()
        {
            imageData.Clear();
            cameras = await GetMarkerData2D();

            foreach(QTMRealTimeSDK.Data.CameraImage camera in cameras)
            {
                using (Image image = await Task.Run(() => Image.Load(camera.ImageData)))
                {
                    imageData.Add(image.Pixels);
                }
            }

            return imageData;
        }

        public void UpdateStream(RTPacket framePacket)
        {
            throw new NotImplementedException();
        }
    }
}
