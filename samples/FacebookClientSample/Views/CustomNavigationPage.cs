using System;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace FacebookClientSample.Views
{
	public partial class CustomNavigationPage : Xamarin.Forms.NavigationPage
    {
        public CustomNavigationPage()
        {
			On<iOS>().SetPrefersLargeTitles(true);
        }
    }
}
