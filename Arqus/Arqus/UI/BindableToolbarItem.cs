using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Arqus.UI
{
    class BindableToolbarItem : ToolbarItem
    {

        public BindableToolbarItem()
        {
            Init();
        }

        private void Init()
        {
            OnIsVisibleChanged(this, false, IsVisible);
        }

        
        public static readonly BindableProperty IsVisibleProperty =
            BindableProperty.Create("BindableToolbarItem", typeof(bool), typeof(ToolbarItem),
                true, BindingMode.TwoWay, propertyChanged: OnIsVisibleChanged);
        
        public bool IsVisible
        {
            get { return (bool) GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        private static void OnIsVisibleChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var item = bindable as BindableToolbarItem;

            if (item.Parent == null)
                return;

            if (item != null && item.Parent == null)
                return;

            if (item != null)
            {
                var items = ((ContentPage)item.Parent).ToolbarItems;

                if ((bool)newValue && !items.Contains(item))
                {
                    Device.BeginInvokeOnMainThread(() => { items.Add(item); });
                }
                else if (!(bool)newValue && items.Contains(item))
                {
                    Device.BeginInvokeOnMainThread(() => { items.Remove(item); });
                }
            }

        }

    }
}
