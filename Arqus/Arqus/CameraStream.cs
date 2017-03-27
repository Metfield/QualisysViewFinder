using System;
using System.Collections.Generic;
using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;

namespace Arqus
{
    public class CameraStream
    {

        private static readonly CameraStream instance = new CameraStream();

        public bool Streaming { private set; get; }

        PacketType packetType;
        ComponentType currentStreamType;
        
        static CameraStream() { }
        private CameraStream(){ }

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
            if(!Streaming && QTMNetworkConnection.Instance.protocol.IsConnected())
            {
                Console.WriteLine("Starting stream!");
                currentStreamType = type;
                QTMNetworkConnection.Instance.protocol.StreamFrames(StreamRate.RateFrequency, streamFrequency, type);

                Streaming = true;
            }
            else if(!QTMNetworkConnection.Instance.protocol.IsConnected())
            {
                Console.WriteLine("Unable to start stream: Not Connected");
                Streaming = false;
            }

            return Streaming;
        }

        private List<Camera> markerData2D;

        public List<Camera> MarkerData2D
        {
            get
            {
                QTMNetworkConnection.Instance.protocol.ReceiveRTPacket(out packetType);
                return QTMNetworkConnection.Instance.protocol.GetRTPacket().Get2DMarkerData();
            }
        }
    }
}
