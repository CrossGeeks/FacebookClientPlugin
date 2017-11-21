## Getting Started

If developing an application that supports iOS and Android, make sure you installed the NuGet package into your PCL project and Client projects.

### Setup
* [Facebook Setup](docs/FacebookPortalSetup.md)
* [Android Setup](docs/AndroidSetup.md)
* [iOS Setup](docs/iOSSetup.md)

### Login

Here some examples on how to login depending on your use case:

Login with read permissions requesting email permission.

```cs
 await CrossFacebookClient.Current.LoginAsync( new string[] {"email"});
```

Login with publish permissions

```cs
  await CrossFacebookClient.Current.LoginAsync( new string[] {"publish_actions"},FacebookPermissionType.Publish);
```

The above login methods trigger **OnLogin** event.

```cs
  CrossFacebookClient.Current.OnLogin += (s,a)=> 
  {
  };
```

Login with read permissions & get user data

```cs
 await CrossFacebookClient.Current.RequestUserDataAsync(new string[] { "email", "first_name", "gender", "last_name", "birthday" }, new string[] { "email", "user_birthday" });
```
Note: This method will check if has the requested permissions granted, if so will just get the user data, if not will prompt login requesting the missing permissions and after granted will get the user data.

The above method trigger **OnUserData** event.

```cs
  CrossFacebookClient.Current.OnUserData += (s,a)=> 
  {
      
  };
```

### Logout

```cs
 CrossFacebookClient.Current.Logout();
```

Triggers **OnLogout** event.

```cs
  CrossFacebookClient.Current.OnLogout += (s,a)=> 
  {
      
  };
```

### Permissions

There are some methods & properties to verify the current state of facebook permissions:

**CrossFacebookClient.Current.ActivePermissions** : List of granted permissions.

**CrossFacebookClient.Current.DeclinedPermissions** : List of declined permissions.

**CrossFacebookClient.Current.VerifyPermission** : Verify if a specific permission has been granted.

Usage:

```cs
CrossFacebookClient.Current.VerifyPermission("publish_actions");
```

**CrossFacebookClient.Current.HasPermissions** : Verify if all the permissions specified have been granted.

Usage:

```cs
CrossFacebookClient.Current.HasPermissions(new string[]{"user_friends","user_likes"});
```

More information about available permissions here:

https://developers.facebook.com/docs/facebook-login/permissions/?locale=en_EN


### Events

```cs
            event EventHandler<FBEventArgs<string>> OnRequestData;
            
            event EventHandler<FBEventArgs<string>> OnPostData;
            
            event EventHandler<FBEventArgs<string>> OnDeleteData;

            event EventHandler<FBEventArgs<Dictionary<string, object>>> OnSharing;

```

### Sharing

By default sharing methods request the **publish_actions** permission if not granted.

Simple Share
```cs
 await CrossFacebookClient.Current.SharePhotoAsync(myPhotoBytes, captionText);
```

Share multiple photos
```cs
FacebookSharePhoto photo = new FacebookSharePhoto(text, imageBytes);
FacebookSharePhoto photo2 = new FacebookSharePhoto(text2, imageBytes2);
FacebookSharePhoto[] photos = new FacebookSharePhoto[] { photo, photo2 };                    
FacebookSharePhotoContent photoContent = new FacebookSharePhotoContent(photos);
 var ret = await CrossFacebookClient.Current.ShareAsync(photoContent);
```

More information on [Sharing Content](../docs/SharingContent.md) section.

<= Back to [Table of Contents](../README.md)
