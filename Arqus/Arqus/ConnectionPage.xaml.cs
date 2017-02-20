using Xamarin.Forms;

namespace Arqus
{
	public partial class ConnectionPage : ContentPage
	{
        private ConnectionViewModel viewModel = new ConnectionViewModel();

		public ConnectionPage()
		{
			InitializeComponent();
            BindingContext = viewModel;
		}

        protected override void OnAppearing()
        {
            base.OnAppearing();

            viewModel.RefreshQTMServers();
        }
    }
}
