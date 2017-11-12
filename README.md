## Facebook Client Plugin for Xamarin iOS and Android
Simple cross platform plugin for handling facebook login and sharing.

### Setup
* Available on NuGet: http://www.nuget.org/packages/Plugin.FacebookClient [![NuGet](https://img.shields.io/nuget/v/Plugin.FacebookClient.svg?label=NuGet)](https://www.nuget.org/packages/Plugin.FacebookClient/)
* Install into your PCL project and Client projects.

**Platform Support**

|Platform|Version|
| ------------------- | :------------------: |
|Xamarin.iOS|iOS 8+|
|Xamarin.Android|API 15+|

### API Usage

Call **CrossFacebookClient.Current** from any project or PCL to gain access to APIs.

## Features

- Login
- Share

### Android Setup

On MainActivity.cs

- On OnCreate just after calling base.OnCreate:
```cs
     FacebookSdk.SdkInitialize(this.ApplicationContext);
	  CallbackManager = CallbackManagerFactory.Create();
```

- Override OnActivityResult:
```cs
  protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
  {
		base.OnActivityResult(requestCode, resultCode, intent);
		CallbackManager.OnActivityResult(requestCode, (int)resultCode, intent);
  }
```

### iOS Setup

On AppDelegate FinishedLaunching just before **return base.FinishedLaunching(app, options)**:

```cs
Facebook.CoreKit.ApplicationDelegate.SharedInstance.FinishedLaunching(app, options);
```

On AppDelegate OnActivated add:
```cs
AppEvents.ActivateApp();
```

Need to whitelist Facebook domains in your app by adding the following to your application's Info.plist:

```xml
<key>NSAppTransportSecurity</key>
<dict>
    <key>NSExceptionDomains</key>
    <dict>
        <key>facebook.com</key>
        <dict>
            <key>NSIncludesSubdomains</key>
            <true/>                
            <key>NSThirdPartyExceptionRequiresForwardSecrecy</key>
            <false/>
        </dict>
        <key>fbcdn.net</key>
        <dict>
            <key>NSIncludesSubdomains</key>
            <true/>
            <key>NSThirdPartyExceptionRequiresForwardSecrecy</key>
            <false/>
        </dict>
        <key>akamaihd.net</key>
        <dict>
            <key>NSIncludesSubdomains</key>
            <true/>
            <key>NSThirdPartyExceptionRequiresForwardSecrecy</key>
            <false/>
        </dict>
    </dict>
</dict>
```

Also add:


```xml
<key>CFBundleURLTypes</key>
<array>
  <dict>
    <key>CFBundleURLSchemes</key>
    <array>
      <string>fb{your-fb-app-id}</string>
    </array>
  </dict>
</array>
<key>FacebookAppID</key>
<string>{your-fb-app-id}</string>
<key>FacebookDisplayName</key>
<string>{your-fb-app-name}</string>
<key>LSApplicationQueriesSchemes</key>
<array>
  <string>fbapi</string>
  <string>fb-messenger-api</string>
  <string>fbauth2</string>
  <string>fbshareextension</string>
</array>
<key>NSPhotoLibraryUsageDescription</key>
  <string>{human-readable reason for photo access}</string>
```

On your Entitlement.plist add:


```xml
<key>keychain-access-groups</key>
  <array>
    <string>{your-apple-app-id-prefix}.{your-apple-app-id-bundle-identifier}</string>
  </array>
```
### Plugin Methods
```cs
   Task<FBEventArgs<bool>> LoginAsync(string[] permissions, FacebookPermissionType permissionType = FacebookPermissionType.Read);
   
   Task<FBEventArgs<Dictionary<string, object>>> SharePhotoAsync(byte[] imgBytes, string caption = "");
  
  Task<FBEventArgs<Dictionary<string, object>>> RequestUserDataAsync(string[] fields, string[] permissions, FacebookPermissionType permissionType = FacebookPermissionType.Read);

```
