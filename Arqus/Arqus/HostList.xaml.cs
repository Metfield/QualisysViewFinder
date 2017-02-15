using System.Net;
using System.Collections.Generic;

using Xamarin.Forms;
using Arqus.Connection;

namespace Arqus
{
	public partial class HostList : ListView
	{
		public HostList ()
		{
			InitializeComponent();

            ItemsSource = new List<Host>
            {
                new Host("mac.local", "192.123.123.12"),
                new Host("not.mac.loca", "197.123.12.3")
            };
		}
	}
}
