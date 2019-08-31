using Plugin.FacebookClient.Abstractions;
using System;
using Facebook.LoginKit;
using Facebook;
using UIKit;
using System.Threading.Tasks;
using System.Linq;
using Facebook.ShareKit;
using Foundation;
using Facebook.CoreKit;
using System.Collections.Generic;
using ExternalAccessory;
using System.IO;
using AVFoundation;


namespace Plugin.FacebookClient
{
    /// <summary>
    /// Implementation for FacebookClient
    /// </summary>
    public class FacebookClientManager : NSObject, IFacebookClient, ISharingDelegate
    {
        TaskCompletionSource<FacebookResponse<string>> _userDataTcs;
        TaskCompletionSource<FacebookResponse<Dictionary<string, object>>> _shareTcs;
        TaskCompletionSource<FacebookResponse<string>> _requestTcs;
        TaskCompletionSource<FacebookResponse<string>> _postTcs;
        TaskCompletionSource<FacebookResponse<string>> _deleteTcs;
        TaskCompletionSource<FacebookResponse<bool>> _loginTcs;

        static NSString FBAccessTokenKey = new NSString("FBAccessToken");
        static NSString FBAccessTokenExpirationDateKey = new NSString("FBAccessTokenExpirationDateKey");
        static NSString FBUserIdKey = new NSString("FBUserIdKey");

        public event EventHandler<FBEventArgs<string>> OnUserData = delegate { };

        public event EventHandler<FBEventArgs<bool>> OnLogin = delegate { };

        public event EventHandler<FBEventArgs<bool>> OnLogout = delegate { };

        public event EventHandler<FBEventArgs<Dictionary<string, object>>> OnSharing = delegate { };

        public event EventHandler<FBEventArgs<string>> OnRequestData = delegate { };
        public event EventHandler<FBEventArgs<string>> OnPostData = delegate { };
        public event EventHandler<FBEventArgs<string>> OnDeleteData = delegate { };

        LoginManager loginManager = new LoginManager();
        FacebookPendingAction<Dictionary<string, object>> pendingAction;


        public string[] ActivePermissions
        {
            get
            {
                return AccessToken.CurrentAccessToken == null ? new string[] { } : AccessToken.CurrentAccessToken.Permissions.Select(p => $"{p}").ToArray();
            }
        }

        public string[] DeclinedPermissions
        {
            get
            {
                return AccessToken.CurrentAccessToken == null ? new string[] { } : AccessToken.CurrentAccessToken.DeclinedPermissions.Select(p => $"{p}").ToArray();
            }
        }

        public bool IsLoggedIn
        {
            get
            {
                return !string.IsNullOrEmpty(ActiveToken) && NSUserDefaults.StandardUserDefaults.ValueForKey(FBAccessTokenExpirationDateKey) !=null && NSDate.Now.Compare(NSUserDefaults.StandardUserDefaults.ValueForKey(FBAccessTokenExpirationDateKey) as NSDate) == NSComparisonResult.Ascending;
            }
        }

        public string ActiveToken
        {
            get
            {
                return  AccessToken.CurrentAccessToken?.TokenString ?? NSUserDefaults.StandardUserDefaults.StringForKey(FBAccessTokenKey) ?? string.Empty;
            }
        }

        public string ActiveUserId
        {
            get
            {
                return AccessToken.CurrentAccessToken?.UserID ?? NSUserDefaults.StandardUserDefaults.StringForKey(FBUserIdKey) ?? string.Empty;
            }
        }

        public DateTime TokenExpirationDate
        {
            get
            {
                return AccessToken.CurrentAccessToken.ExpirationDate.ToDateTime();
            }
        }

        bool IsLoginSessionActive
        {
            get
            {
                return AccessToken.CurrentAccessToken != null && NSDate.Now.Compare(AccessToken.CurrentAccessToken.ExpirationDate) == NSComparisonResult.Ascending;
            }
        }

        public bool HasPermissions(string[] permissions)
        {
            if (!IsLoginSessionActive)
                return false;

            var currentPermissions = AccessToken.CurrentAccessToken.Permissions;

            return permissions.All(p => AccessToken.CurrentAccessToken.HasGranted(p));
        }

        public bool VerifyPermission(string permission)
        {

            return IsLoginSessionActive && (AccessToken.CurrentAccessToken.HasGranted(permission));

        }
        public static void Initialize(UIApplication app,NSDictionary options)
        {
            ApplicationDelegate.SharedInstance.FinishedLaunching(app, options);
        }
        public static void OnActivated()
        {
            AppEvents.ActivateApp();
        }
        public static bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
           return ApplicationDelegate.SharedInstance.OpenUrl(app, url, $"{options["UIApplicationOpenURLOptionsSourceApplicationKey"]}", options["UIApplicationOpenURLOptionsAnnotationKey"]);
        }

        public static bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
           return ApplicationDelegate.SharedInstance.OpenUrl(application, url, sourceApplication, annotation);
        }
        
        public void DidComplete(ISharing sharer, NSDictionary results)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            foreach (var r in results)
            {
                parameters.Add($"{r.Key}", $"{r.Value}");
            }
            var fbArgs = new FBEventArgs<Dictionary<string, object>>(parameters, FacebookActionStatus.Completed);
            OnSharing(this, fbArgs);
            _shareTcs?.TrySetResult(new FacebookResponse<Dictionary<string, object>>(fbArgs));
        }

        public void DidFail(ISharing sharer, NSError error)
        {
            var fbArgs = new FBEventArgs<Dictionary<string, object>>(null, FacebookActionStatus.Error, $"Facebook Sharing Failed - {error.Code} - {error.Description}");
            OnSharing(this, fbArgs);
            _shareTcs?.TrySetResult(new FacebookResponse<Dictionary<string, object>>(fbArgs));
        }

        public void DidCancel(ISharing sharer)
        {
            var fbArgs = new FBEventArgs<Dictionary<string, object>>(null, FacebookActionStatus.Canceled, "User cancelled facebook operation");

            OnSharing(this, fbArgs);
            _shareTcs?.TrySetResult(new FacebookResponse<Dictionary<string, object>>(fbArgs));
        }

        public async Task<FacebookResponse<bool>> LoginAsync(string[] permissions, FacebookPermissionType permissionType = FacebookPermissionType.Read)
        {
            _loginTcs = new TaskCompletionSource<FacebookResponse<bool>>();
            var retVal = IsLoginSessionActive;
            FacebookActionStatus status = FacebookActionStatus.Error;
            if (!HasPermissions(permissions))
            {
                var window = UIApplication.SharedApplication.KeyWindow;
                var vc = window.RootViewController;
                while (vc.PresentedViewController != null)
                {
                    vc = vc.PresentedViewController;
                }

                loginManager.LogIn(permissions, vc, (result, error) =>
                {
                    if (error == null)
                    {


                        if (result.IsCancelled)
                        {
                            retVal = false;
                            status = FacebookActionStatus.Canceled;
                        }
                        else
                        {

                            retVal = HasPermissions(result.GrantedPermissions.Select(p => $"{p}").ToArray());

                            NSUserDefaults.StandardUserDefaults.SetString(result.Token.TokenString, FBAccessTokenKey);
                            NSUserDefaults.StandardUserDefaults.SetValueForKey(result.Token.ExpirationDate, FBAccessTokenExpirationDateKey);
                            NSUserDefaults.StandardUserDefaults.SetString(result.Token.UserId, FBUserIdKey);
                            NSUserDefaults.StandardUserDefaults.Synchronize();

                            status = retVal ? FacebookActionStatus.Completed : FacebookActionStatus.Unauthorized;
                        }
                    }
                    else
                    {
                        retVal = false;
                        status = FacebookActionStatus.Error;
                    }



                    var fbArgs = new FBEventArgs<bool>(retVal, status);
                    OnLogin?.Invoke(CrossFacebookClient.Current, fbArgs);
                    _loginTcs?.TrySetResult(new FacebookResponse<bool>(fbArgs));

                    pendingAction?.Execute();

                    pendingAction = null;
                });

              
                
            }
            else
            {
               
                var fbArgs = new FBEventArgs<bool>(retVal, FacebookActionStatus.Completed);
                OnLogin?.Invoke(CrossFacebookClient.Current, fbArgs);
                _loginTcs?.TrySetResult(new FacebookResponse<bool>(fbArgs));

                pendingAction?.Execute();

                pendingAction = null;

            }

            return await _loginTcs.Task;
        }



        public async Task<FacebookResponse<Dictionary<string, object>>> SharePhotoAsync(byte[] photoBytes, string caption = "")
        {
            _shareTcs = new TaskCompletionSource<FacebookResponse<Dictionary<string, object>>>();
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                {"photo",photoBytes},

            };

            if (!string.IsNullOrEmpty(caption))
            {
                parameters.Add("caption", caption);
            }

          return await PerformAction(RequestSharePhoto, parameters, _shareTcs.Task, FacebookPermissionType.Publish, new string[] { });
        }

        public async Task<FacebookResponse<Dictionary<string, object>>> ShareAsync(FacebookShareContent shareContent)
        {
            _shareTcs = new TaskCompletionSource<FacebookResponse<Dictionary<string, object>>>();
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                {"content",shareContent}

            };

            return await PerformAction(RequestShare, parameters, _shareTcs.Task, FacebookPermissionType.Publish, new string[] { });
        }
        void RequestShare(Dictionary<string, object> paramsDictionary)
        {
            if (paramsDictionary.TryGetValue("content", out object shareContent) && shareContent is FacebookShareContent)
            {
                ISharingContent content = null;
                if (shareContent is FacebookShareLinkContent)
                {
                    FacebookShareLinkContent linkContent = shareContent as FacebookShareLinkContent;
                    ShareLinkContent sLinkContent = new ShareLinkContent();


                    if (linkContent.Quote != null)
                    {
                        sLinkContent.Quote=linkContent.Quote;
                    }

                    if (linkContent.ContentLink != null)
                    {
                        sLinkContent.SetContentUrl(linkContent.ContentLink);
                    }

                    if (!string.IsNullOrEmpty(linkContent.Hashtag))
                    {
                        var shareHashTag = new Hashtag();
                        shareHashTag.StringRepresentation = linkContent.Hashtag;
                        sLinkContent.Hashtag = shareHashTag;
                    }

                    if (linkContent.PeopleIds != null && linkContent.PeopleIds.Length > 0)
                    {
                        sLinkContent.SetPeopleIds(linkContent.PeopleIds);
                    }

                    if (!string.IsNullOrEmpty(linkContent.PlaceId))
                    {
                        sLinkContent.SetPlaceId(linkContent.PlaceId);
                    }

                    if (!string.IsNullOrEmpty(linkContent.Ref))
                    {
                        sLinkContent.SetRef(linkContent.Ref);
                    }

                    content = sLinkContent;
                }
                else if (shareContent is FacebookSharePhotoContent)
                {

                    FacebookSharePhotoContent photoContent = shareContent as FacebookSharePhotoContent;

                    SharePhotoContent sharePhotoContent = new SharePhotoContent();

                    if (photoContent.Photos != null && photoContent.Photos.Length > 0)
                    {
                        List<SharePhoto> photos = new List<SharePhoto>();
                        foreach (var p in photoContent.Photos)
                        {

                            if (p.ImageUrl != null && !string.IsNullOrEmpty(p.ImageUrl.AbsoluteUri))
                            {
                                SharePhoto photoFromUrl = SharePhoto.From(p.ImageUrl, true);

                                if (!string.IsNullOrEmpty(p.Caption))
                                {
                                    photoFromUrl.Caption = p.Caption;
                                }

                                photos.Add(photoFromUrl);
                            }

                            if (p.Image != null)
                            {
                                UIImage image = null;

                                var imageBytes = p.Image as byte[];

                                if (imageBytes != null)
                                {
                                    using (var data = NSData.FromArray(imageBytes))
                                        image = UIImage.LoadFromData(data);

                                    SharePhoto sPhoto = Facebook.ShareKit.SharePhoto.From(image, true);

                                    if (!string.IsNullOrEmpty(p.Caption))
                                    {
                                        sPhoto.Caption = p.Caption;
                                    }


                                    photos.Add(sPhoto);
                                }

                            }

                        }

                        if(photos.Count >0)
                        {
                            sharePhotoContent.Photos = photos.ToArray();
                        }

                    }

                    if (photoContent.ContentLink != null)
                    {
                        sharePhotoContent.SetContentUrl(photoContent.ContentLink);
                    }

                    if (!string.IsNullOrEmpty(photoContent.Hashtag))
                    {
                        var shareHashTag = new Hashtag();
                        shareHashTag.StringRepresentation = photoContent.Hashtag;
                        sharePhotoContent.Hashtag = (shareHashTag);
                    }

                    if (photoContent.PeopleIds != null && photoContent.PeopleIds.Length > 0)
                    {
                        sharePhotoContent.SetPeopleIds(photoContent.PeopleIds);
                    }

                    if (!string.IsNullOrEmpty(photoContent.PlaceId))
                    {
                        sharePhotoContent.SetPlaceId(photoContent.PlaceId);
                    }

                    if (!string.IsNullOrEmpty(photoContent.Ref))
                    {
                        sharePhotoContent.SetRef(photoContent.Ref);
                    }

                    content = sharePhotoContent;
                }
                else if (shareContent is FacebookShareVideoContent)
                {
                    FacebookShareVideoContent videoContent = shareContent as FacebookShareVideoContent;
                    ShareVideoContent shareVideoContent = new ShareVideoContent();


                    if (videoContent.Video != null)
                    {
                        
                        if (videoContent.Video.LocalUrl != null)
                        {
                            shareVideoContent.Video = ShareVideo.From(videoContent.Video.LocalUrl);
                        }

                        
                    }

                    if (videoContent.ContentLink != null)
                    {
                        shareVideoContent.SetContentUrl(videoContent.ContentLink);
                    }

                    if (!string.IsNullOrEmpty(videoContent.Hashtag))
                    {
                        var shareHashTag = new Hashtag();
                        shareHashTag.StringRepresentation = videoContent.Hashtag;
                        shareVideoContent.Hashtag = (shareHashTag);
                    }

                    if (videoContent.PeopleIds != null && videoContent.PeopleIds.Length > 0)
                    {
                        shareVideoContent.SetPeopleIds(videoContent.PeopleIds);
                    }

                    if (!string.IsNullOrEmpty(videoContent.PlaceId))
                    {
                        shareVideoContent.SetPlaceId(videoContent.PlaceId);
                    }

                    if (!string.IsNullOrEmpty(videoContent.Ref))
                    {
                        shareVideoContent.SetRef(videoContent.Ref);
                    }

                    content = shareVideoContent;
                }

                if (content != null)
                {

                    //ShareAPI.Share(content, this);

                    var window = UIApplication.SharedApplication.KeyWindow;
                    var vc = window.RootViewController;
                    while (vc.PresentedViewController != null)
                    {
                        vc = vc.PresentedViewController;
                    }

                    ShareDialog.Show(vc,content, this);
                }
            }
        }

        void RequestSharePhoto(Dictionary<string, object> paramsDictionary)
        {
            if (paramsDictionary != null && paramsDictionary.ContainsKey("photo"))
            {
                UIImage image = null;

                var imageBytes = paramsDictionary["photo"] as byte[];

                if (imageBytes != null)
                {
                    using (var data = NSData.FromArray(imageBytes))
                      image = UIImage.LoadFromData(data);
                }
                

                SharePhoto photo = Facebook.ShareKit.SharePhoto.From(image, true);
           
                if (paramsDictionary.ContainsKey("caption"))
                {
                    photo.Caption = $"{paramsDictionary["caption"]}";
                }

         
                var photoContent = new SharePhotoContent()
                {
                    Photos = new SharePhoto[] { photo }

                };

                //ShareAPI.Share(photoContent, this);
                var window = UIApplication.SharedApplication.KeyWindow;
                var vc = window.RootViewController;
                while (vc.PresentedViewController != null)
                {
                    vc = vc.PresentedViewController;
                }

                ShareDialog.Show(vc, photoContent, this);
            }

        }

        public async Task<FacebookResponse<string>> QueryDataAsync(string path, string[] permissions = null, IDictionary<string, string> parameters = null, string version = null)
        {
            _requestTcs = new TaskCompletionSource<FacebookResponse<string>>();
            Dictionary<string, object> paramDict = new Dictionary<string, object>()
            {
                {"path", path},
                {"parameters",parameters},
                {"method", FacebookHttpMethod.Get},
                {"version", version}
            };
            return await PerformAction<FacebookResponse<string>>(RequestData, paramDict, _requestTcs.Task, FacebookPermissionType.Read, permissions);
        }

        public async Task<FacebookResponse<string>> PostDataAsync(string path, string[] permissions, IDictionary<string, string> parameters = null, string version = null)
        {
            _postTcs = new TaskCompletionSource<FacebookResponse<string>>();
            Dictionary<string, object> paramDict = new Dictionary<string, object>()
            {
                {"path", path},
                {"parameters",parameters},
                {"method", FacebookHttpMethod.Post},
                {"version", version}
            };
            return await PerformAction<FacebookResponse<string>>(RequestData, paramDict, _postTcs.Task, FacebookPermissionType.Publish, permissions);
        }

        public async Task<FacebookResponse<string>> DeleteDataAsync(string path, string[] permissions,IDictionary<string, string> parameters = null, string version = null)
        {
            _deleteTcs = new TaskCompletionSource<FacebookResponse<string>>();
            Dictionary<string, object> paramDict = new Dictionary<string, object>()
            {
                {"path", path},
                {"parameters",parameters},
                {"method", FacebookHttpMethod.Delete },
                {"version", version}
            };
            return await PerformAction<FacebookResponse<string>>(RequestData, paramDict, _deleteTcs.Task, FacebookPermissionType.Publish,permissions);
        }



        void RequestData(Dictionary<string, object> pDictionary)
        {
            string path = $"{pDictionary["path"]}";
            string version = $"{pDictionary["version"]}";
            Dictionary<string, string> paramDict = pDictionary["parameters"] as Dictionary<string, string>;
            FacebookHttpMethod? method = pDictionary["method"] as FacebookHttpMethod?;
            var currentTcs = _requestTcs;
            var onEvent = OnRequestData;
            var httpMethod = "GET";
            if (method != null)
            {
                switch (method)
                {
                    case FacebookHttpMethod.Get:
                        httpMethod = "GET";
                        break;
                    case FacebookHttpMethod.Post:
                        httpMethod = "POST";
                        onEvent = OnPostData;
                        currentTcs = _postTcs;
                        break;
                    case FacebookHttpMethod.Delete:
                        httpMethod = "DELETE";
                        onEvent = OnDeleteData;
                        currentTcs = _deleteTcs;
                        break;
                }

            }

            if (string.IsNullOrEmpty(path))
            {
                var fbResponse = new FBEventArgs<string>(null, FacebookActionStatus.Error, "Graph query path not specified");
                onEvent?.Invoke(CrossFacebookClient.Current, fbResponse);
                currentTcs?.TrySetResult(new FacebookResponse<string>(fbResponse));
                return;
            }

         

            

            NSMutableDictionary parameters=null;

            if (paramDict != null)
            {
                parameters = new NSMutableDictionary();
                foreach (var p in paramDict)
                {
                    if(!string.IsNullOrEmpty(p.Key) && !string.IsNullOrEmpty(p.Value))
                    {
                        parameters.Add(new NSString($"{p.Key}"), new NSString($"{p.Value}"));
                    }
                  
                }
            }

            var graphRequest = string.IsNullOrEmpty(version)?new GraphRequest(path, parameters, httpMethod): new GraphRequest(path, parameters, AccessToken.CurrentAccessToken.TokenString,version, httpMethod);
            var requestConnection = new GraphRequestConnection();
            requestConnection.AddRequest(graphRequest, (connection, result, error) =>
            {
                
                if (error == null)
                {
                    
                    NSData responseData= NSJsonSerialization.Serialize(result, NSJsonWritingOptions.PrettyPrinted, out NSError jsonError);
                    if(jsonError == null)
                    {
                        NSString responseString = NSString.FromData(responseData, NSStringEncoding.UTF8);
                        var fbResponse = new FBEventArgs<string>(responseString, FacebookActionStatus.Completed);
                        onEvent?.Invoke(CrossFacebookClient.Current, fbResponse);
                        currentTcs?.TrySetResult(new FacebookResponse<string>(fbResponse));
                    }
                    else
                    {
                        var fbResponse = new FBEventArgs<string>(null, FacebookActionStatus.Error, $" Facebook response parse failed - {jsonError.Code} - {jsonError.Description}");
                        onEvent?.Invoke(CrossFacebookClient.Current, fbResponse);
                        currentTcs?.TrySetResult(new FacebookResponse<string>(fbResponse));
                    }
                    
                }
                else
                {
                    var fbResponse = new FBEventArgs<string>(null, FacebookActionStatus.Error, $" Facebook operation failed - {error.Code} - {error.Description}");
                    onEvent?.Invoke(CrossFacebookClient.Current, fbResponse);
                    currentTcs?.TrySetResult(new FacebookResponse<string>(fbResponse));
                }
            });
            requestConnection.Start();
        }
        void RequestUserData(Dictionary<string, object> fieldsDictionary)
        {
            string[] fields = new string[] { };
            var extraParams = string.Empty;
            if (fieldsDictionary != null && fieldsDictionary.ContainsKey("fields"))
            {
                fields = fieldsDictionary["fields"] as string[];
                if (fields.Length > 0)
                    extraParams = $"?fields={string.Join(",", fields)}";
            }

            //email,first_name,gender,last_name,birthday

            if (AccessToken.CurrentAccessToken != null)
            {
                Dictionary<string, object> userData = new Dictionary<string, object>();
                var graphRequest = new GraphRequest($"/me{extraParams}", null, AccessToken.CurrentAccessToken.TokenString, null, "GET");
                var requestConnection = new GraphRequestConnection();
                requestConnection.AddRequest(graphRequest, (connection, result, error) =>
                {
                    if (error == null)
                    {
                        
                        NSData responseData = NSJsonSerialization.Serialize(result, NSJsonWritingOptions.PrettyPrinted, out NSError jsonError);
                        if (jsonError == null)
                        {
                            NSString responseString = NSString.FromData(responseData, NSStringEncoding.UTF8);
                            var fbResponse = new FBEventArgs<string>(responseString, FacebookActionStatus.Completed);
                            OnUserData?.Invoke(this, fbResponse);
                            _userDataTcs?.TrySetResult(new FacebookResponse<string>(fbResponse));
                        }
                        else
                        {
                            var fbResponse = new FBEventArgs<string>(null, FacebookActionStatus.Error, $" Facebook response parse failed - {jsonError.Code} - {jsonError.Description}");
                            OnUserData?.Invoke(this, fbResponse);
                            _userDataTcs?.TrySetResult(new FacebookResponse<string>(fbResponse));
                        }
                        
                    }
                    else
                    {
                        var fbArgs = new FBEventArgs<string>(null, FacebookActionStatus.Error, $"Facebook User Data Request Failed - {error.Code} - {error.Description}");
                        OnUserData?.Invoke(this, fbArgs);
                        _userDataTcs?.TrySetResult(new FacebookResponse<string>(fbArgs));

                    }


                });
                requestConnection.Start();
            }
            else
            {
                var fbArgs = new FBEventArgs<string>(null, FacebookActionStatus.Canceled, "User cancelled facebook operation");
                OnUserData?.Invoke(this, null);
                _userDataTcs?.TrySetResult(new FacebookResponse<string>(fbArgs));
            }


        }

        async Task<T> PerformAction<T>(Action<Dictionary<string, object>> action, Dictionary<string, object> parameters, Task<T> task, FacebookPermissionType permissionType, string[] permissions) 
        {
            pendingAction = null;
            if (permissions == null)
            {
                action(parameters);
            }
            else
            {
                bool authorized = HasPermissions(permissions);

                if (!authorized)
                {
                    pendingAction = new FacebookPendingAction<Dictionary<string, object>>(action, parameters);
                    await LoginAsync(permissions, permissionType);
                }
                else
                {
                    action(parameters);
                }
            }

            return await task;

        }


        async Task<FBEventArgs<Dictionary<string, object>>> PerformAction(Action<Dictionary<string, object>> action, Dictionary<string, object> parameters,Task<FBEventArgs<Dictionary<string, object>>> task, FacebookPermissionType permissionType, string[] permissions)
        {
            pendingAction = null;
            if (permissions == null)
            {
                action(parameters);
            }
            else
            {
                bool authorized = HasPermissions(permissions);

                if (!authorized)
                {
                    pendingAction = new FacebookPendingAction<Dictionary<string, object>>(action, parameters);
                    await LoginAsync(permissions, permissionType);

                }
                else
                {
                    action(parameters);
                }
            }
           

            return await task;
        }

        public void Logout()
        {
            if (IsLoginSessionActive)
                loginManager.LogOut();

            NSUserDefaults.StandardUserDefaults.SetString(string.Empty, FBAccessTokenKey);
            NSUserDefaults.StandardUserDefaults.SetValueForKey(NSDate.DistantPast, FBAccessTokenExpirationDateKey);
            NSUserDefaults.StandardUserDefaults.SetString(string.Empty, FBUserIdKey);
            NSUserDefaults.StandardUserDefaults.Synchronize();

            OnLogout(this, new FBEventArgs<bool>(true, FacebookActionStatus.Completed));
        }

        public async Task<FacebookResponse<string>> RequestUserDataAsync(string[] fields, string[] permissions, FacebookPermissionType permissionType = FacebookPermissionType.Read)
        {
            _userDataTcs = new TaskCompletionSource<FacebookResponse<string>>();
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                {"fields",fields}
            };

           return await PerformAction(RequestUserData, parameters,_userDataTcs.Task, FacebookPermissionType.Read, permissions);
        }

        public void LogEvent(string name)
        {
            AppEvents.LogEvent(name);
        }
        
    }
}