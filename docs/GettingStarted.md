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

Login with read permissions & get user data

```cs
 await CrossFacebookClient.Current.RequestUserDataAsync(new string[] { "email", "first_name", "gender", "last_name", "birthday" }, new string[] { "email", "user_birthday" });
```
Note: This method will check if has the requested permissions granted, if so will just get the user data, if not will prompt login requesting the missing permissions and after granted will get the user data.


### Logout

```cs
 CrossFacebookClient.Current.Logout();
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


### Facebook Response Status

#### Response

Type: ```FacebookResponse<T>```

Properties:

* **Data**:

     On login method returns a bool.
     
     On sharing/user data returns a dictionary with the data response.

* **Status**: Response status

     *FacebookActionStatus.Canceled* - If Request was canceled
     
     *FacebookActionStatus.Unauthorized* - If Request was not authorized due to lack of permissions required
     
     *FacebookActionStatus.Completed* - If Request was completed succesfully
     
     *FacebookActionStatus.Error* - If Request failed
        
* **Message**: Error message string

**Note: If response *Status* isn't FacebookActionStatus.Completed, *Data* property will be null and should have a value on *Message* property with the error.**
```

### Events


Login event:

```cs
  CrossFacebookClient.Current.OnLogin += (s,a)=> 
  {
     switch (a.Status)
      {
         case FacebookActionStatus.Completed:
          //Logged in succesfully
         break;
      }
  };
```

RequestUserData event:

```cs
  CrossFacebookClient.Current.OnUserData += (s,a)=> 
  {
     switch (a.Status)
      {
         case FacebookActionStatus.Completed:
          //Got user data
         break;
      }
      
  };
```

Logout event:

```cs
  CrossFacebookClient.Current.OnLogout += (s,a)=> 
  {
      
  };
```

Sharing event:
  
```cs
  CrossFacebookClient.Current.OnSharing += (s,a)=> 
  {
      switch (a.Status)
      {
         case FacebookActionStatus.Completed:
          //Shared post succesfully
         break;
      }
  };
```

<= Back to [Table of Contents](../README.md)
