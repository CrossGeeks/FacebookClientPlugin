## Facebook Client Plugin for Xamarin iOS and Android


1. Login to https://developer.facebook.com and create a new project

![Creating Application](https://github.com/CrossGeeks/FirebasePushNotificationPlugin/blob/master/images/firebase-portal-create-project.png?raw=true)


2. After project is created you should see options to add Firebase to your iOS and Android apps

![Firebase Overview](https://github.com/CrossGeeks/FirebasePushNotificationPlugin/blob/master/images/firebase-overview.png?raw=true)

Simple cross platform plugin for handling facebook login, sharing, facebook graph requests and permissions handling.

### Setup
* Available on NuGet: http://www.nuget.org/packages/Plugin.FacebookClient [![NuGet](https://img.shields.io/nuget/v/Plugin.FacebookClient.svg?label=NuGet)](https://www.nuget.org/packages/Plugin.FacebookClient/)
* Install into your PCL project and Client projects.
* Create your [Facebook](docs/FacebookPortalSetup.md) application.
* Follow the [Android](docs/AndroidSetup.md) and [iOS](docs/iOSSetup.md) Setup guides
* Follow [Getting Started](docs/GettingStarted.md)

**Platform Support**

|Platform|Version|
| ------------------- | :------------------: |
|Xamarin.iOS|iOS 8+|
|Xamarin.Android|API 15+|

### API Usage

Call **CrossFacebookClient.Current** from any project or PCL to gain access to APIs.

## Features

- Login
- Permissions Handling
- Share (Photos, Link, Video)
- Facebook Graph Requests

## Documentation

Here you will find detailed documentation on setting up and using the Firebase Push Notification Plugin for Xamarin

* [Facebook Setup](docs/FacebookPortalSetup.md)
* [Android Setup](docs/AndroidSetup.md)
* [iOS Setup](docs/iOSSetup.md)
* [Getting Started](docs/GettingStarted.md)
* [Sharing Content](docs/SharingContent.md)
* [Facebook Graph Requests](docs/FacebookGraphRequests.md)

#### Contributors

* [Rendy Del Rosario](https://github.com/rdelrosario)
* [Charlin Agramonte](https://github.com/char0394)
* [Sayed Seliman](https://github.com/sayed-seliman)
