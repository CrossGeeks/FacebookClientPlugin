## Getting Started

If developing an application that supports iOS and Android, make sure you installed the NuGet package into your PCL project and Client projects.

### Setup
* [Facebook Setup](docs/FacebookPortalSetup.md)
* [Android Setup](docs/AndroidSetup.md)
* [iOS Setup](docs/iOSSetup.md)

### Login

Login with read permissions requesting email permission.
```cs
 CrossFacebookClient.Current.LoginAsync( new string[] {"email"});
```

Login with publish permissions
```cs
 CrossFacebookClient.Current.LoginAsync( new string[] {"publish_actions"},FacebookPermissionType.Publish);
```


Login with read permissions & Get User Data

```cs
 await CrossFacebookClient.Current.RequestUserDataAsync(new string[] { "email", "first_name", "gender", "last_name", "birthday" }, new string[] { "email", "user_birthday" });
```

### Logout

```cs
 CrossFacebookClient.Current.Logout();
```

### Plugin Methods
```cs
            Task<FacebookResponse<Dictionary<string, object>>> SharePhotoAsync(byte[] imgBytes, string caption = "");
            Task<FacebookResponse<Dictionary<string, object>>> ShareAsync(FacebookShareContent shareContent);
            Task<FacebookResponse<Dictionary<string, object>>> RequestUserDataAsync(string[] fields, string[] permissions, FacebookPermissionType permissionType = FacebookPermissionType.Read);
            Task<FacebookResponse<string>> QueryDataAsync(string path, string[] permissions, IDictionary<string, string> parameters = null, string version = null);
            Task<FacebookResponse<string>> PostDataAsync(string path, IDictionary<string, string> parameters = null, string version = null);
            Task<FacebookResponse<string>> DeleteDataAsync(string path, IDictionary<string, string> parameters = null, string version = null);
            void LogEvent(string name);

```

### Permissions

**CrossFacebookClient.Current.ActivePermissions** : Get the granted permissions

**CrossFacebookClient.Current.DeclinedPermissions** : Get the declined permissions

**CrossFacebookClient.Current.VerifyPermission** : Verify if a specific permission has been granted

```cs
CrossFacebookClient.Current.VerifyPermission("publish_actions");
```

**CrossFacebookClient.Current.HasPermissions** : Verify if all the permissions specified have been granted

```cs
CrossFacebookClient.Current.HasPermissions(new string[]{"user_friends","birthdat"});
```

More information about available permissions here
https://developers.facebook.com/docs/facebook-login/permissions/?locale=en_EN


### Events

```cs
            event EventHandler<FBEventArgs<string>> OnRequestData;
            
            event EventHandler<FBEventArgs<string>> OnPostData;
            
            event EventHandler<FBEventArgs<string>> OnDeleteData;

            event EventHandler<FBEventArgs<Dictionary<string, object>>> OnUserData;

            event EventHandler<FBEventArgs<bool>> OnLogin;

            event EventHandler<FBEventArgs<bool>> OnLogout;

            event EventHandler<FBEventArgs<Dictionary<string, object>>> OnSharing;

```

### Sample use


To Share
```cs
 await CrossFacebookClient.Current.SharePhotoAsync(myPhotoBytes, captionText);
```
### Share Content
```cs
FacebookSharePhoto photo = new FacebookSharePhoto(text, image);
FacebookSharePhoto[] photos = new FacebookSharePhoto[] { photo };                    
FacebookSharePhotoContent photoContent = new FacebookSharePhotoContent(photos);
 var ret = await CrossFacebookClient.Current.ShareAsync(photoContent);
```

<= Back to [Table of Contents](../README.md)
