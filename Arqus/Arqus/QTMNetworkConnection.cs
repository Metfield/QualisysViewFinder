using System;
using System.Collections.Generic;
using QTMRealTimeSDK;
using System.Linq;
using Arqus.Connection;
using System.Diagnostics;
using System.Threading;

namespace Arqus
{
    public class QTMNetworkConnection
    {
        public string IPAddress { private set; get; }
        static RTProtocol rtProtocol = new RTProtocol();

        public QTMNetworkConnection(string ipAddress = "127.0.0.1")
        {
            IPAddress = ipAddress;
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
       
        public List<RTProtocol.DiscoveryResponse> DiscoverQTMServers(ushort port = 4547)
        {
            if (rtProtocol.DiscoverRTServers(port))
            {
                Debug.WriteLine("Found RT servers");
                return rtProtocol.DiscoveryResponses
                    .ToList();
            }

            return null;
        }
    }
}
