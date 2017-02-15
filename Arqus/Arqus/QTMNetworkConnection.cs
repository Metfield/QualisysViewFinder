using System;
using System.Collections.Generic;
using System.Text;

using QTMRealTimeSDK;

namespace Arqus
{
    public class QTMNetworkConnection
    {
        string qtmIPAddress;
        RTProtocol rtProtocol;

        public QTMNetworkConnection(string ipAddress, ref RTProtocol rtp)
        {
            // Set this connection's IP address
            setIPAddress(ipAddress);

            // Set real-time protocol reference
            rtProtocol = rtp;
        }

        /// <summary>
        /// Sets connection's ip address
        /// </summary>
        /// <param name="ipAddress"></param>
        public void setIPAddress(string ipAddress)
        {
            qtmIPAddress = ipAddress;
        }

        /// <summary>
        /// Connect to previously set IP
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            // Check if we're already connected
            if(!rtProtocol.IsConnected())
            {
                // Return false if connection was not successfull
                if(!rtProtocol.Connect(qtmIPAddress))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Connect to this new IP
        /// </summary>
        /// <param name="ipAddress">New IP to connect to</param>
        /// <returns></returns>
        public bool Connect(string ipAddress)
        {
            // Set IP and try to connect
            setIPAddress(ipAddress);
            return Connect();
        }
    }
}
