using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using Xamarin.Forms;
using Arqus.Connection;

namespace Arqus
{
	public partial class HostList : ListView
	{
		public HostList ()
		{
			InitializeComponent ();

            ItemsSource = new List<Host>
            {
                new Host("mac.local", "192.123.123.12"),
                new Host("not.mac.loca", "197.123.12.3")
            };
		}
	}
}
