## Getting Started

### Setup
* [Facebook Setup](docs/FacebookPortalSetup.md)
* [Android Setup](docs/AndroidSetup.md)
* [iOS Setup](docs/iOSSetup.md)

### Login


### Plugin Methods
```cs
            Task<FacebookResponse<bool>> LoginAsync(string[] permissions, FacebookPermissionType permissionType = FacebookPermissionType.Read);
            Task<FacebookResponse<Dictionary<string, object>>> SharePhotoAsync(byte[] imgBytes, string caption = "");
            Task<FacebookResponse<Dictionary<string, object>>> ShareAsync(FacebookShareContent shareContent);
            Task<FacebookResponse<Dictionary<string, object>>> RequestUserDataAsync(string[] fields, string[] permissions, FacebookPermissionType permissionType = FacebookPermissionType.Read);
            Task<FacebookResponse<string>> QueryDataAsync(string path, string[] permissions, IDictionary<string, string> parameters = null, string version = null);
            Task<FacebookResponse<string>> PostDataAsync(string path, IDictionary<string, string> parameters = null, string version = null);
            Task<FacebookResponse<string>> DeleteDataAsync(string path, IDictionary<string, string> parameters = null, string version = null);
			void Logout();

            void LogEvent(string name);
            bool VerifyPermission(string permission);

            bool HasPermissions(string[] permission);

```

### Permissions

More information about available permissions here
https://developers.facebook.com/docs/facebook-login/permissions/?locale=en_EN

```cs
      string[] ActivePermissions { get; }

            string[] DeclinedPermissions { get; }

			  bool VerifyPermission(string permission);

            bool HasPermissions(string[] permission);

```

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

Login & Get User Data

```cs
 await CrossFacebookClient.Current.RequestUserDataAsync(new string[] { "email", "first_name", "gender", "last_name", "birthday" }, new string[] { "email", "user_birthday" });
```

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
