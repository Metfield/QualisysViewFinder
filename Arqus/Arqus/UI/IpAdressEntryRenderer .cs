using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Arqus.UI
{
    class IpAdressEntryRenderer : Entry
    {
        public IpAdressEntryRenderer()
        {
            // Specific for iOS, as usual...
#if __IOS__
            Margin = new Thickness(20, 20, 20, 10);
#endif
        }
    }
}
