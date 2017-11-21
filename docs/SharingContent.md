## Sharing Content  in Facebook

* Permissions (authorization, portal, add testers)
* Plugin auto request permission

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
FacebookShareVideoContent videoContent = new FacebookShareVideoContent(filePath);
 var ret = await CrossFacebookClient.Current.ShareAsync(videoContent);
```

<= Back to [Table of Contents](../README.md)
