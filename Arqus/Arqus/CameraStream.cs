using System;
using System.Collections.Generic;
using System.Text;

using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace Arqus
{
    public class CameraStream
    {
        QTMNetworkConnection qtmConnection;
        string qtmVersion;
        ComponentType currentStreamType;

        private static CameraStream instance;

        private CameraStream() { }

        // Temporary singleton stuff
        public static CameraStream Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CameraStream();
                }

                return instance;
            }
        }

        public bool ConnectToIP(string ipAddress)
        {
            // Create network connection with given IP
            qtmConnection = QTMNetworkConnection.Instance;

            // Attempt to connect 
            bool success = qtmConnection.Connect(ipAddress);

            if(success)
            {
                // Get qtm version (because why not?)
                qtmConnection.protocol.GetQTMVersion(out qtmVersion);                
            }

            return success;
        }
        
        /// <summary>
        /// Gets connected QTM version
        /// </summary>
        /// <returns></returns>
        public string GetQTMVersion()
        {
            return qtmVersion;
        }

        /// <summary>
        /// Starts frame streaming of "type"
        /// </summary>
        /// <param name="type">Specifies the component type to stream</param>
        public void StartStream(int streamFrequency, ComponentType type)
        {
            Console.WriteLine("Starting stream!");

            currentStreamType = type;
            qtmConnection.protocol.StreamFrames(StreamRate.RateFrequency, streamFrequency, type);

            GetStreamMarkerData();
        }

        /// <summary>
        /// Gets new stream marker data structure.
        /// The returned structure depends on the way the streaming was initialized.
        /// 
        /// E.g. If streaming was initialized as 2DMarkerData, then this method will return 
        /// a 2DMarkerData structure.
        /// </summary>
        /// <returns></returns>
        public dynamic GetStreamMarkerData()
        {
            PacketType packetType;
            qtmConnection.protocol.ReceiveRTPacket(out packetType, false);

            // Beautiful switch depending on data type
            if (packetType == PacketType.PacketData)
            {
                switch (currentStreamType)
                {
                    case ComponentType.Component2d:
                        return qtmConnection.protocol.GetRTPacket().Get2DMarkerData();

                    case ComponentType.Component2dLinearized:
                        return qtmConnection.protocol.GetRTPacket().Get2DLinearizedMarkerData();
                        
                    case ComponentType.Component3d:
                        return qtmConnection.protocol.GetRTPacket().Get3DMarkerData();

                    case ComponentType.Component3dNoLabels:
                        return qtmConnection.protocol.GetRTPacket().Get3DMarkerNoLabelsData();

                    case ComponentType.Component3dNoLabelsResidual:
                        return qtmConnection.protocol.GetRTPacket().Get3DMarkerNoLabelsResidualData();

                    case ComponentType.Component3dResidual:
                        return qtmConnection.protocol.GetRTPacket().Get3DMarkerResidualData();

                    case ComponentType.Component6d:
                        return qtmConnection.protocol.GetRTPacket().Get6DOFData();

                    case ComponentType.Component6dEuler:
                        return qtmConnection.protocol.GetRTPacket().Get6DOFEulerData();

                    case ComponentType.Component6dEulerResidual:
                        return qtmConnection.protocol.GetRTPacket().Get6DOFEulerResidualData();

                    case ComponentType.Component6dResidual:
                        return qtmConnection.protocol.GetRTPacket().Get6DOFResidualData();
                }
            }

            // TODO: Handle special streaming events
            //
            // ..
            // ..
            //
                
            return null;
        }
    }
}
