using System;
using System.Collections.Generic;
using System.Text;

using QTMRealTimeSDK;

namespace Arqus
{
    public class CameraStream
    {
        QTMNetworkConnection qtmConnection;
        RTProtocol rtProtocol;
        string qtmVersion;

        public CameraStream()
        {
            // Instantiate rt protocol
            rtProtocol = new RTProtocol();
        }

        public bool ConnectToIP(string ipAddress)
        {
            // Create network connection with given IP
            qtmConnection = new QTMNetworkConnection(ref rtProtocol, ipAddress);

            // Attempt to connect 
            bool success = qtmConnection.Connect();

            if(success)
            {
                // Get qtm version (because why not?)
                rtProtocol.GetQTMVersion(out qtmVersion);                
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
    }
}
