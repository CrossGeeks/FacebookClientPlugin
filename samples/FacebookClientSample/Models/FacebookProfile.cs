using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace FacebookClientSample
{
    public class FacebookProfile 
    {
		public string FullName { get; set; }
		public UriImageSource Cover { get; set; }
		public UriImageSource PictureUrl { get; set; }
    }
}
