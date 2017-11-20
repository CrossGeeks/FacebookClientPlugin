## Facebook Portal Setup

1. Login to https://developers.facebook.com and create a new application

![Creating Application 1](https://github.com/CrossGeeks/FacebookClientPlugin/blob/develop/images/create-fb-app.jpg?raw=true)

![Creating Application 2](https://github.com/CrossGeeks/FacebookClientPlugin/blob/develop/images/create-fb-app-2.jpg?raw=true)

![Creating Application 3](https://github.com/CrossGeeks/FacebookClientPlugin/blob/develop/images/fb-app-start.png?raw=true)

2. Go to settings section to add your android and ios platforms.

![Facebook Settings](https://github.com/CrossGeeks/FacebookClientPlugin/blob/develop/images/fb-app-settings.png)

### Android Setup

1. Click on add platform and then select Android


![Add Android](https://github.com/CrossGeeks/FacebookClientPlugin/blob/develop/images/create-android-app.png?raw=true)

![Add Android 2](https://github.com/CrossGeeks/FacebookClientPlugin/blob/develop/images/create-android-app-2.png?raw=true)

2. Generate a hash for the keychain

```bash
Mac: keytool -exportcert -alias androiddebugkey -keystore /Users/[USERNAME]]/.local/share/Xamarin/Mono\ for\ Android/debug.keystore | openssl sha1 -binary | openssl base64
 
Windows: keytool -exportcert -alias androiddebugkey -keystore "C:\Users\[USERNAME]\AppData\Local\Xamarin\Mono for Android\debug.keystore" | openssl sha1 -binary | openssl base64
```


<= Back to [Table of Contents](../README.md)
