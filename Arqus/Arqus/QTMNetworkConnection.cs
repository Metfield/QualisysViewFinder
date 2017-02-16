using System;
using System.Collections.Generic;
using System.Text;

using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;
using System.Linq;

namespace Arqus
{
    public class QTMNetworkConnection
    {
        public string IPAddress { private set; get; }
        RTProtocol rtProtocol;

        public QTMNetworkConnection(ref RTProtocol rtp, string ipAddress = "127.0.0.1")
        {
            this.IPAddress = ipAddress;

            // Set real-time protocol reference
            rtProtocol = new RTProtocol();
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
                if(!rtProtocol.Connect(IPAddress))
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
            IPAddress = ipAddress;
            return Connect();
        }
       
        public List<DiscoveryResponse> DiscoverQTMServers(ushort port)
        {

            if (rtProtocol.DiscoverRTServers(port))
            {
                return rtProtocol.DiscoveryResponses.ToList();
            }
                return null;
        }
    }
}
