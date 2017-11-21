## Facebook Portal Setup

1. Login to https://developers.facebook.com and create a new application

![Creating Application 1](https://github.com/CrossGeeks/FacebookClientPlugin/blob/develop/images/create-fb-app.jpg?raw=true)

![Creating Application 2](https://github.com/CrossGeeks/FacebookClientPlugin/blob/develop/images/create-fb-app-2.jpg?raw=true)

![Creating Application 3](https://github.com/CrossGeeks/FacebookClientPlugin/blob/develop/images/fb-app-start.png?raw=true)

2. Go to settings section to add your android and ios platforms.

![Facebook Settings](https://github.com/CrossGeeks/FacebookClientPlugin/blob/develop/images/fb-app-settings.png)


### Android Setup

3. Click on add platform and then select Android


![Add Android](https://github.com/CrossGeeks/FacebookClientPlugin/blob/develop/images/create-android-app.png?raw=true)

![Add Android 2](https://github.com/CrossGeeks/FacebookClientPlugin/blob/develop/images/create-android-app-2.png?raw=true)

4. Generate a hash for the keystore you will sign the apk with and add the generated hash to android platform config.

```bash
Mac: keytool -exportcert -alias <KEY_STORE_ALIAS> -keystore <KEY_STORE_PATH> | openssl sha1 -binary | openssl base64
 
Windows: keytool -exportcert -alias <KEY_STORE_ALIAS> -keystore <KEY_STORE_PATH> | openssl sha1 -binary | openssl base64
```

Make sure to use the password that you set when you first created the keystore.

5. Add your package name and full class name for your Activity.

![Add Android  Details](https://github.com/CrossGeeks/FacebookClientPlugin/blob/develop/images/create-android-app-details.png?raw=true)


### iOS Setup

6. Click on add platform then select iOS

![Add iOS](https://github.com/CrossGeeks/FacebookClientPlugin/blob/develop/images/create-ios-app.png?raw=true)

7. Enter the bundle identifier for your iOS application on iOs platform config

![Add iOS 2](https://github.com/CrossGeeks/FacebookClientPlugin/blob/develop/images/create-ios-app-2.png?raw=true)

### Enable sharing content setup (Optional)

8. Go to App Review and make public your application so that you are able to test sharing posts.

![Make Public](https://github.com/CrossGeeks/FacebookClientPlugin/blob/develop/images/fb-app-make-public.png)

![Make Public 2](https://github.com/CrossGeeks/FacebookClientPlugin/blob/develop/images/fb-app-make-ublic-2.png?raw=true)

![Make Public 3](https://github.com/CrossGeeks/FacebookClientPlugin/blob/develop/images/fb-app-make-ublic-3.png?raw=true)

9. Click on start submission to add **publish_actions** permission, then click on Add Item (You can add any other permissions you might need in your application)

![Enable publish permission](https://github.com/CrossGeeks/FacebookClientPlugin/blob/develop/images/publish_actions_permission.png?raw=true)

![Enable publish permission](https://github.com/CrossGeeks/FacebookClientPlugin/blob/develop/images/publish_actions_permission_2.png?raw=true)

10. Add your testers

![Testers](https://github.com/CrossGeeks/FacebookClientPlugin/blob/develop/images/testers.png?raw=true)

**Note: This will allow you to test sharing just within your testers group since is not yet approved for public use, so will just work for people added on Roles section, until you submit for approval by clicking on Edit Details and following the steps described there. Until permissions are not on green state are not yet approved (This applies for all permissions that are not approved by default).**

**There are few permissions approved by default. Which are: "email" , "public_profile", "user_friends"**







<= Back to [Table of Contents](../README.md)
