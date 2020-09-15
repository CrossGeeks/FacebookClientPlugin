## Sharing Content in Facebook

### Android Considerations

If you're sharing links, images or video via the Facebook for Android app, you also need to declare the FacebookContentProvider in the manifest.

Append your app id to the end of the authorities value. For example if your Facebook app id is 1234, the declaration looks like:
```xml
<provider android:authorities="com.facebook.app.FacebookContentProvider1234"
          android:name="com.facebook.FacebookContentProvider"
          android:exported="true" />
```

## Testing Sharing Content

If your app is public on Facebook developer portal, you should be able to test sharing with any users. In case it isn't yet public then you need to add the users you want to be able to test sharing:

Add your testers

![Testers](https://github.com/CrossGeeks/FacebookClientPlugin/blob/develop/images/testers.png?raw=true)

## Share content types

You can share 3 types of content:

* **FacebookSharePhotoContent** - Photos
* **FacebookShareLinkContent** - Link
* **FacebookShareVideoContent** - Video

### Simple photo share
```cs
 await CrossFacebookClient.Current.SharePhotoAsync(myPhotoBytes, captionText);
```

### Share Photo using bytes
```cs
FacebookSharePhoto photo = new FacebookSharePhoto(text, image);
FacebookSharePhoto[] photos = new FacebookSharePhoto[] { photo };                    
FacebookSharePhotoContent photoContent = new FacebookSharePhotoContent(photos);
await CrossFacebookClient.Current.ShareAsync(photoContent);
```

### Share Photo using url
```cs
FacebookSharePhoto photo = new FacebookSharePhoto(text, imageUrl);
FacebookSharePhoto[] photos = new FacebookSharePhoto[] { photo };                    
FacebookSharePhotoContent photoContent = new FacebookSharePhotoContent(photos);
 await CrossFacebookClient.Current.ShareAsync(photoContent);
```

### Share Link
```cs               
FacebookShareLinkContent linkContent = new FacebookShareLinkContent("awesome plugins",new Uri( "http://www.github.com/crossgeeks"));
var ret = await CrossFacebookClient.Current.ShareAsync(linkContent);
```

### Share Video
```cs               
FacebookShareVideo video = new FacebookShareVideo(videoPath);
FacebookShareVideoContent videoContent = new FacebookShareVideoContent(video);
await CrossFacebookClient.Current.ShareAsync(videoContent);
```

<= Back to [Table of Contents](../README.md)
