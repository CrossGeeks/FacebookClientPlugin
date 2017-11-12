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

```cs
   Task<FBEventArgs<bool>> LoginAsync(string[] permissions, FacebookPermissionType permissionType = FacebookPermissionType.Read);
   
   Task<FBEventArgs<Dictionary<string, object>>> SharePhotoAsync(byte[] imgBytes, string caption = "");
  
  Task<FBEventArgs<Dictionary<string, object>>> RequestUserDataAsync(string[] fields, string[] permissions, FacebookPermissionType permissionType = FacebookPermissionType.Read);

```
