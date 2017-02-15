using System;
using System.Collections.Generic;
using System.Text;

namespace Arqus.Connection
{
    class Host
    {
        public string Name { private set; get; }
        public string Address { private set; get; }

        public Host(string name, string address)
        {
            this.Name = name;
            this.Address = address;
        }
    }
}
