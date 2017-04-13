using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;
using QTMRealTimeSDK.Settings;
using System.Collections.Generic;

namespace Arqus
{
    public interface ICameraStream
    {
        void UpdateStream(RTPacket framePacket);
    }

    public class RTStream
    {
        private RTProtocol rtProtocol;
        private ICameraStream cameraStreamInterface;
        private int cameraCount;

        // Holds every cameraStream 
        private List<CameraStream> cameraStreams;

        protected int streamFrequency;
        //protected ComponentType currentStreamType;
        protected RTPacket framePacket;

        private List<ComponentType> activeStreams;

        public RTStream(RTProtocol protocol, string versionCommand)
        {
            // Set RTProtocol object reference
            rtProtocol = protocol;

            // Set protocol version and get return command
            string returnCommnand;
            rtProtocol.SendCommandExpectCommandResponse(versionCommand, out returnCommnand);

            // Initialize activeStreams list
            activeStreams = new List<ComponentType>();

            // TODO: Handle possible version rejection
            //
            //
            //
        }

        /// <summary>
        /// Starts the streaming 
        /// </summary>
        /// <param name="frequency"></param>
        /// <param name="type"></param>
        public void StartStream(int frequency)
        {
            // Set stream frequency
            streamFrequency = frequency;

            // Start streaming frames
            // 2D Marker information by default
            rtProtocol.StreamFrames(StreamRate.RateFrequency, streamFrequency, ComponentType.Component2d);
            activeStreams.Add(ComponentType.Component2d);

            // Create 'cameraCount' number of cameras in list
            //CreateCameraStreams();

            // Get information for first frame
            UpdateRTStream();
        }

        /*private void CreateCameraStreams()
        {            
            // Loop through camera list to get ids
            for(int id = 1; id <= QTMNetworkConnection.Instance.GetImageSettings().Count; id++)
            {
                // Create new camera stream with respective ID
                cameraStreams.Add(new CameraStream(id, rtProtocol));
            }
        }*/

        /// <summary>
        /// Updates the stream and data structures for the camera streams that are active
        /// </summary>
        public void UpdateRTStream()
        {
            PacketType packetType;
            rtProtocol.ReceiveRTPacket(out packetType, false);

            if (packetType == PacketType.PacketData)
            {
                // Get new frame packet
                framePacket = rtProtocol.GetRTPacket();
                cameraCount = framePacket.CameraCount;
            }

            // Update camera count
            // TODO: Maybe handling an event in case the count changes?


            // Update every camera stream object
            /*foreach(CameraStream cameraStream in cameraStreams)
                cameraStream.UpdateStream(framePacket);*/
        }

        /// <summary>
        /// Restarts streaming with new frequency
        /// </summary>
        /// <param name="newFrequency"></param>
        /*public void ChangeStreamFrequency(int newFrequency)
        {
            // Set new frequency
            streamFrequency = newFrequency;

            // Stop streaming
            rtProtocol.StreamFramesStop();

            // Restart frame stremaing with new frequency
            rtProtocol.StreamFrames(StreamRate.RateFrequency, streamFrequency, currentStreamType);
        }*/

        /// <summary>
        /// Return camera count
        /// </summary>
        /// <returns></returns>
        public int GetCameraCount()
        {
            return cameraCount;
        }

        public Camera GetMarker2DDataFrom(int id)
        {
            CheckStreamType(ComponentType.Component2d);
            return framePacket.Get2DMarkerData(id - 1);
        }

        public CameraImage GetImageDataFrom(int id)
        {
            CheckStreamType(ComponentType.ComponentImage);
            return framePacket.GetImageData(id - 1);
        }

        /// <summary>
        /// If we're not streaming this data, add it to active streams,
        /// start the stream and update it.
        /// </summary>
        /// <param name="type"></param>
        private void CheckStreamType(ComponentType type)
        {
            if (!activeStreams.Contains(type))
            {
                activeStreams.Add(type);

                // Start the stream if we're adding something else

                UpdateRTStream();
            }
        }
    }
}
