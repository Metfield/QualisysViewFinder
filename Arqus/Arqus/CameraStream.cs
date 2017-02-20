using System;
using System.Collections.Generic;
using System.Text;

using QTMRealTimeSDK;
using Xamarin.Forms;

namespace Arqus
{
    public class CameraStream
    {
        QTMNetworkConnection qtmConnection;
        string qtmVersion;      

        public CameraStream()
        {
            // Bind command to method
            StreamCommand = new Command(OnStreamCommand);
        }
      
        public bool ConnectToIP(string ipAddress)
        {
            // Create network connection with given IP
            qtmConnection = new QTMNetworkConnection(ipAddress);

            // Attempt to connect 
            bool success = qtmConnection.Connect();

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
        public void StartStream(QTMRealTimeSDK.Data.ComponentType type)
        {
            qtmConnection.protocol.StreamAllFrames(type);
        }

        // OnlineStreamMenuPage.xaml bindings         
        //// Stream button command
        public Command StreamCommand { get; }
        
        /// GUI stream start callback
        void OnStreamCommand()
        {
            StartStream(QTMRealTimeSDK.Data.ComponentType.Component3d);
        }

        //// QtmVersion string binding to text label
        public string QtmVersion
        {
            set { qtmVersion = value; }
        }
    }
}
