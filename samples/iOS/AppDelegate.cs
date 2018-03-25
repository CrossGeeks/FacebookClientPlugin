using System;
using System.Collections.Generic;
using System.Linq;
using Facebook.LoginKit;
using Foundation;
using Plugin.FacebookClient;
using Plugin.FacebookClient.Abstractions;
using UIKit;

namespace FacebookClientSample.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();

            LoadApplication(new App());
			FacebookClientManager.Initialize(app, options);
			return base.FinishedLaunching(app, options);
        }

		public override void OnActivated(UIApplication uiApplication)
		{
			FacebookClientManager.OnActivated();
		}

		public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
		{
			return FacebookClientManager.OpenUrl(app, url, options);
		}

		public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
		{
			return FacebookClientManager.OpenUrl(application, url, sourceApplication, annotation);
		}
    }
}
