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
    }
}
