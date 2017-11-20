## Android Setup

* Install Plugin.FacebookClient package into your Android project.

### MainActivity.cs

- On OnCreate just after calling base.OnCreate:
```cs
     FacebookClientManager.Initialize(this);
```

Note: Save that CallbackManager in class variable/property since you will need it in the following step.

- Override OnActivityResult:
```cs
  protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
  {
		base.OnActivityResult(requestCode, resultCode, intent);
		FacebookClientManager.OnActivityResult(requestCode, resultCode, intent);
  }
```

### Resources/values/strings.xml

```xml
<string name="facebook_app_name">MyFacebookAppName</string>
<string name="facebook_app_id">xxxxxxxxxxxxx</string>
<string name="fb_login_protocol_scheme">fbxxxxxxxxxxxxx</string>
```
Replace *xxxxxxxxxxxxx* with your facebook app id.

### AndroidManifest.xml

Add this permission.

```xml
<uses-permission android:name="android.permission.INTERNET"/>
```

Also add this inside the **application** node

```xml
    <meta-data android:name="com.facebook.sdk.ApplicationId"  android:value="@string/facebook_app_id"/>
    <activity android:name="com.facebook.FacebookActivity"
        android:configChanges=
                "keyboard|keyboardHidden|screenLayout|screenSize|orientation"
        android:label="@string/facebook_app_name" />
    <activity
        android:name="com.facebook.CustomTabActivity"
        android:exported="true">
        <intent-filter>
            <action android:name="android.intent.action.VIEW" />
            <category android:name="android.intent.category.DEFAULT" />
            <category android:name="android.intent.category.BROWSABLE" />
            <data android:scheme="@string/fb_login_protocol_scheme" />
        </intent-filter>
    </activity>
```

### Keystore Auto Signing

This step is optional, but highly recommended. The reason is because in order to test facebook you have to use have the apk signed with the keystore of the hash that was generated for facebook portal.

Enable keystore auto signing, so that you are able to test within the same keystore on Debug and Release.

Inside:
**<PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">** and  **<PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ">**

```xml
    <AndroidKeyStore>True</AndroidKeyStore>
    <AndroidSigningKeyStore>KeystoreFullPath</AndroidSigningKeyStore>
    <AndroidSigningStorePass>KeystorePassword</AndroidSigningStorePass>
    <AndroidSigningKeyAlias>KeystoreAlias</AndroidSigningKeyAlias>
    <AndroidSigningKeyPass>KeystorePassword</AndroidSigningKeyPass>
```

Replace KeystoreFullPath with your keystore full path. If you save your keystore on the same android project location, then is just the keystore name.
Replace KeystorePassword with your keystore password.
Replace KeystoreAlias with your keystore alias.

If you have different facebook apps for testing on debug and release then put this different on each correspondant to the keystore for that configuration


<= Back to [Table of Contents](../README.md)