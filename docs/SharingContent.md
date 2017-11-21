## Sharing Content in Facebook

By default sharing methods request the publish_actions permission if not granted.

Make sure your app is public on the App Review section of Facebook developer portal and also have added **publish_actions** permission item, so that you are able to test sharing. 

## Testing Sharing Content

If you have **publish_actions** permission approved you can ignore this part.

Add your testers

![Testers](https://github.com/CrossGeeks/FacebookClientPlugin/blob/develop/images/testers.png?raw=true)

**Note: This will allow you to test sharing just within your testers group since if not yet approved by Facebook for public use, so will just work for people added on Roles section, until you submit for approval by clicking on Edit Details and following the steps described there. Until permissions are not on green state are not yet approved**

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
 var ret = await CrossFacebookClient.Current.ShareAsync(photoContent);
```

### Share Photo using url
```cs
FacebookSharePhoto photo = new FacebookSharePhoto(text, imageUrl);
FacebookSharePhoto[] photos = new FacebookSharePhoto[] { photo };                    
FacebookSharePhotoContent photoContent = new FacebookSharePhotoContent(photos);
 var ret = await CrossFacebookClient.Current.ShareAsync(photoContent);
```

### Share Link
```cs               
FacebookShareLinkContent linkContent = new FacebookShareLinkContent("http://www.github.com/crossgeeks");
 var ret = await CrossFacebookClient.Current.ShareAsync(linkContent);
```

### Share Video
```cs               
FacebookShareVideo video = new FacebookShareVideo(videoPath);
FacebookShareVideoContent videoContent = new FacebookShareVideoContent(video);
 var ret = await CrossFacebookClient.Current.ShareAsync(videoContent);
```

<= Back to [Table of Contents](../README.md)
