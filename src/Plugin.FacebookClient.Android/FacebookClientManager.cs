using Plugin.FacebookClient.Abstractions;
using System;
using Xamarin.Facebook.Login;
using Xamarin.Facebook;
using System.Threading.Tasks;
using System.Linq;
using Android.App;
using Xamarin.Facebook.Share.Model;
using Android.Graphics;
using Xamarin.Facebook.Share;
using Android.OS;
using System.Collections.Generic;
using Xamarin.Facebook.AppEvents;
using Java.Interop;
using Org.Json;

namespace Plugin.FacebookClient
{
  /// <summary>
  /// Implementation for Feature
  /// </summary>
  public class FacebookClientManager : Java.Lang.Object, IFacebookClient, GraphRequest.IGraphJSONObjectCallback
    {
        static TaskCompletionSource<FBEventArgs<Dictionary<string, object>>> _userDataTcs;
        static TaskCompletionSource<FBEventArgs<Dictionary<string, object>>> _shareTcs;
        static TaskCompletionSource<FBEventArgs<bool>> _loginTcs;

        static ICallbackManager mCallbackManager;

        //Activity mActivity;
        static FacebookCallback<SharerResult> shareCallback;
        static FacebookCallback<LoginResult> loginCallback;
        public static Activity CurrentActivity { get; set; }

        static FacebookPendingAction<Dictionary<string, object>> pendingAction;

        static EventHandler<FBEventArgs<Dictionary<string, object>>> _onUserData;
        public event EventHandler<FBEventArgs<Dictionary<string, object>>> OnUserData
        {
            add
            {
                _onUserData += value;
            }
            remove
            {
                _onUserData -= value;
            }
        }

        static EventHandler<FBEventArgs<bool>> _onLogin;
        public event EventHandler<FBEventArgs<bool>> OnLogin
        {
            add
            {
                _onLogin += value;
            }
            remove
            {
                _onLogin -= value;
            }
        }
        static EventHandler<FBEventArgs<bool>> _onLogout;
        public event EventHandler<FBEventArgs<bool>> OnLogout
        {
            add
            {
                _onLogout += value;
            }
            remove
            {
                _onLogout -= value;
            }
        }

        static EventHandler<FBEventArgs<Dictionary<string, object>>> _onSharing;
        public event EventHandler<FBEventArgs<Dictionary<string, object>>> OnSharing
        {
            add
            {
                _onSharing += value;
            }
            remove
            {
                _onSharing -= value;
            }
        }

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
                return AccessToken.CurrentAccessToken != null;
            }
        }

        public string ActiveToken
        {
            get
            {
                return AccessToken.CurrentAccessToken?.Token ?? string.Empty;
            }
        }


        public string ActiveUserId
        {
            get
            {
                return AccessToken.CurrentAccessToken?.UserId ?? string.Empty;
            }
        }

        public static void Initialize(ICallbackManager callbackManager, Activity activity)
        {
            mCallbackManager = callbackManager;
            CurrentActivity = activity;

            loginCallback = new FacebookCallback<LoginResult>
            {
                HandleSuccess = loginResult =>
                {

                    if (loginResult.AccessToken != null)
                    {
                        var fbArgs = new FBEventArgs<bool>(true, FacebookActionStatus.Completed);
                        _onLogin?.Invoke(CrossFacebookClient.Current, fbArgs);
                        _loginTcs?.TrySetResult(fbArgs);

                        pendingAction?.Execute();

                        pendingAction = null;
                    }
                },
                HandleCancel = () =>
                {
                    var fbArgs = new FBEventArgs<bool>(false, FacebookActionStatus.Canceled);
                    _onLogin?.Invoke(CrossFacebookClient.Current, fbArgs);
                    _loginTcs?.TrySetResult(fbArgs);
                    //Handle any cancel the user has perform
                    Console.WriteLine("User cancel de login operation");

                    pendingAction?.Execute();

                    pendingAction = null;
                },
                HandleError = loginError =>
                {
                    var fbArgs = new FBEventArgs<bool>(false, FacebookActionStatus.Error, loginError.Cause.Message);
                    _onLogin?.Invoke(CrossFacebookClient.Current, fbArgs);
                    _loginTcs?.TrySetResult(fbArgs);

                    //Handle any error happends here
                    Console.WriteLine("Operation throws an error: " + loginError.Cause.Message);

                    pendingAction?.Execute();
                    pendingAction = null;
                }
            };


            shareCallback = new FacebookCallback<SharerResult>
            {
                HandleSuccess = shareResult =>
                {
                    Console.WriteLine("HelloFacebook: Success!");

                    if (shareResult.PostId != null)
                    {
                        Dictionary<string, object> parameters = new Dictionary<string, object>();

                        parameters.Add("postId", shareResult.PostId);
                        var fbArgs = new FBEventArgs<Dictionary<string, object>>(parameters, FacebookActionStatus.Completed);
                        _onSharing?.Invoke(CrossFacebookClient.Current, fbArgs);
                        _shareTcs?.TrySetResult(fbArgs);
                    }
                },
                HandleCancel = () =>
                {
                    Console.WriteLine("HelloFacebook: Canceled");
                    var fbArgs = new FBEventArgs<Dictionary<string, object>>(null, FacebookActionStatus.Canceled);
                    _onSharing?.Invoke(CrossFacebookClient.Current,fbArgs);
                    _shareTcs?.TrySetResult(fbArgs);
                },
                HandleError = shareError =>
                {
                    Console.WriteLine("HelloFacebook: Error: {0}", shareError);
                    var fbArgs = new FBEventArgs<Dictionary<string, object>>(null, FacebookActionStatus.Error, shareError.Message);
                    _onSharing?.Invoke(CrossFacebookClient.Current, fbArgs);
                    _shareTcs?.TrySetResult(fbArgs);
                }
            };

            LoginManager.Instance.RegisterCallback(mCallbackManager, loginCallback);
        }
        public void Logout()
        {
            _shareTcs = null;
            _userDataTcs = null;
            LoginManager.Instance.LogOut();
            AccessToken.CurrentAccessToken = null;
            Profile.CurrentProfile = null;
            _onLogout?.Invoke(CrossFacebookClient.Current, new FBEventArgs<bool>(true, FacebookActionStatus.Completed));
        }


        public bool VerifyPermission(string permission)
        {

            return (AccessToken.CurrentAccessToken != null) && (AccessToken.CurrentAccessToken.Permissions.Contains(permission));

        }



        async Task<FBEventArgs<Dictionary<string, object>>> PerformAction(Action<Dictionary<string, object>> action, Dictionary<string, object> parameters, Task<FBEventArgs<Dictionary<string, object>>> task, FacebookPermissionType permissionType, string[] permissions)
        {
            pendingAction = null;

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
            return await task;

        }
        public async Task<FBEventArgs<Dictionary<string, object>>> ShareAsync(FacebookShareContent shareContent)
        {
            _shareTcs = new TaskCompletionSource<FBEventArgs<Dictionary<string, object>>>();
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                {"content",shareContent}

            };

            return await PerformAction(RequestShare, parameters, _shareTcs.Task, FacebookPermissionType.Publish, new string[] { "publish_actions" });

        }

        public async Task<FBEventArgs<Dictionary<string, object>>> SharePhotoAsync(byte[] photoBytes, string caption = "")
        {
            _shareTcs = new TaskCompletionSource<FBEventArgs<Dictionary<string, object>>>();
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                {"photo",photoBytes}

            };

            if (!string.IsNullOrEmpty(caption))
            {
                parameters.Add("caption", caption);
            }

           return await PerformAction(RequestSharePhoto, parameters, _shareTcs.Task,FacebookPermissionType.Publish, new string[] { "publish_actions" });

        }

        public bool HasPermissions(string[] permissions)
        {
            if (!IsLoggedIn)
                return false;

            var currentPermissions = AccessToken.CurrentAccessToken.Permissions;

            return permissions.All(p => !AccessToken.CurrentAccessToken.DeclinedPermissions.Contains(p));
        }

        public async Task<FBEventArgs<Dictionary<string, object>>> RequestUserDataAsync(string[] fields, string[] permissions, FacebookPermissionType permissionType = FacebookPermissionType.Read)
        {
            _userDataTcs = new TaskCompletionSource<FBEventArgs<Dictionary<string, object>>>();
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                {"fields",fields}
            };

           return await PerformAction(RequestUserData, parameters, _userDataTcs.Task, FacebookPermissionType.Read, permissions);
        }

        async void RequestUserData(Dictionary<string, object> fieldsDictionary)
        {
            string[] fields = new string[] { };
            var extraParams = string.Empty;

            if (fieldsDictionary != null && fieldsDictionary.ContainsKey("fields"))
            {
                fields = fieldsDictionary["fields"] as string[];
                if (fields.Length > 0)
                    extraParams = $"?fields={string.Join(",", fields)}";
            }

            if (AccessToken.CurrentAccessToken != null)
            {
                
               // Facebook.FacebookClient fb = new Facebook.FacebookClient(AccessToken.CurrentAccessToken.Token);

                //var userInfo = (await fb.GetTaskAsync($"/me{extraParams}")) as IDictionary<string, object>;

                GraphRequest request = GraphRequest.NewMeRequest(AccessToken.CurrentAccessToken, this);

                Bundle parameters = new Bundle();
                parameters.PutString("fields", string.Join(",", fields));
                request.Parameters = parameters;
                request.ExecuteAsync();

              
            }
            else
            {
                _onUserData.Invoke(CrossFacebookClient.Current, new FBEventArgs<Dictionary<string, object>>(null, FacebookActionStatus.Canceled));
                _userDataTcs?.TrySetResult(null);
            }

        }
        async void RequestShare(Dictionary<string, object> paramsDictionary)
        {
            if (paramsDictionary.TryGetValue("content",out object shareContent) && shareContent is FacebookShareContent)
            {
                ShareContent content = null;
                if (shareContent is FacebookShareLinkContent)
                {
                    FacebookShareLinkContent linkContent = shareContent as FacebookShareLinkContent;
                    ShareLinkContent.Builder linkContentBuilder = new ShareLinkContent.Builder();


                    if (linkContent.Quote != null)
                    {
                        linkContentBuilder.SetQuote(linkContent.Quote);
                    }

                    if (linkContent.ContentLink != null)
                    {
                        linkContentBuilder.SetContentUrl(Android.Net.Uri.Parse(linkContent.ContentLink.AbsoluteUri));
                    }

                    if (linkContent.Hashtag != null)
                    {
                        var shareHashTagBuilder = new ShareHashtag.Builder();
                        shareHashTagBuilder.SetHashtag(linkContent.Hashtag);
                        linkContentBuilder.SetShareHashtag(shareHashTagBuilder.Build().JavaCast<ShareHashtag>());
                    }

                    if (linkContent.PeopleIds != null && linkContent.PeopleIds.Length > 0)
                    {
                        linkContentBuilder.SetPeopleIds(linkContent.PeopleIds);
                    }

                    if (!string.IsNullOrEmpty(linkContent.PlaceId))
                    {
                        linkContentBuilder.SetPlaceId(linkContent.PlaceId);
                    }

                    if (!string.IsNullOrEmpty(linkContent.Ref))
                    {
                        linkContentBuilder.SetRef(linkContent.Ref);
                    }

                    content = linkContentBuilder.Build();

                }
                else if (shareContent is FacebookSharePhotoContent)
                {

                    FacebookSharePhotoContent photoContent = shareContent as FacebookSharePhotoContent;

                    SharePhotoContent.Builder photoContentBuilder = new SharePhotoContent.Builder();

                    if (photoContent.Photos != null && photoContent.Photos.Length > 0)
                    {
                        foreach (var p in photoContent.Photos)
                        {
                            SharePhoto.Builder photoBuilder = new SharePhoto.Builder();

                            if (!string.IsNullOrEmpty(p.Caption))
                            {
                                photoBuilder.SetCaption(p.Caption);
                            }

                            if (p.ImageUrl != null && !string.IsNullOrEmpty(p.ImageUrl.AbsoluteUri))
                            {
                                photoBuilder.SetImageUrl(Android.Net.Uri.Parse(p.ImageUrl.AbsoluteUri));
                            }

                            if (p.Image != null)
                            {
                                Bitmap bmp = BitmapFactory.DecodeByteArray(p.Image, 0, p.Image.Length);

                                photoBuilder.SetBitmap(bmp);
                            }
                            photoContentBuilder.AddPhoto(photoBuilder.Build().JavaCast<SharePhoto>());

                        }



                    }

                    if (photoContent.ContentLink != null)
                    {
                        photoContentBuilder.SetContentUrl(Android.Net.Uri.Parse(photoContent.ContentLink.AbsoluteUri));
                    }

                    if (photoContent.Hashtag != null)
                    {
                        var shareHashTagBuilder = new ShareHashtag.Builder();
                        shareHashTagBuilder.SetHashtag(photoContent.Hashtag);
                        photoContentBuilder.SetShareHashtag(shareHashTagBuilder.Build().JavaCast<ShareHashtag>());
                    }

                    if (photoContent.PeopleIds != null && photoContent.PeopleIds.Length > 0)
                    {
                        photoContentBuilder.SetPeopleIds(photoContent.PeopleIds);
                    }

                    if (!string.IsNullOrEmpty(photoContent.PlaceId))
                    {
                        photoContentBuilder.SetPlaceId(photoContent.PlaceId);
                    }

                    if (!string.IsNullOrEmpty(photoContent.Ref))
                    {
                        photoContentBuilder.SetRef(photoContent.Ref);
                    }

                    content = photoContentBuilder.Build();
                }
                else if (shareContent is FacebookShareVideoContent)
                {
                    FacebookShareVideoContent videoContent = shareContent as FacebookShareVideoContent;
                    ShareVideoContent.Builder videoContentBuilder = new ShareVideoContent.Builder();


                    if (videoContent.PreviewPhoto != null)
                    {
                        SharePhoto.Builder photoBuilder = new SharePhoto.Builder();

                        if (!string.IsNullOrEmpty(videoContent.PreviewPhoto.Caption))
                        {
                            photoBuilder.SetCaption(videoContent.PreviewPhoto.Caption);
                        }

                        if (videoContent.PreviewPhoto.ImageUrl != null && !string.IsNullOrEmpty(videoContent.PreviewPhoto.ImageUrl.AbsoluteUri))
                        {
                            photoBuilder.SetImageUrl(Android.Net.Uri.Parse(videoContent.PreviewPhoto.ImageUrl.AbsoluteUri));
                        }

                        if (videoContent.PreviewPhoto.Image != null)
                        {
                            Bitmap bmp = BitmapFactory.DecodeByteArray(videoContent.PreviewPhoto.Image, 0, videoContent.PreviewPhoto.Image.Length);

                            photoBuilder.SetBitmap(bmp);
                        }
                        videoContentBuilder.SetPreviewPhoto(photoBuilder.Build().JavaCast<SharePhoto>());
                    }

                    if (videoContent.Video != null)
                    {
                        ShareVideo.Builder videoBuilder = new ShareVideo.Builder();

                        if (videoContent.Video.LocalUrl != null)
                        {
                            videoBuilder.SetLocalUrl(Android.Net.Uri.Parse(videoContent.PreviewPhoto.ImageUrl.AbsoluteUri));

                        }

                        videoContentBuilder.SetVideo(videoBuilder.Build().JavaCast<ShareVideo>());
                    }


                    if (videoContent.ContentLink != null)
                    {
                        videoContentBuilder.SetContentUrl(Android.Net.Uri.Parse(videoContent.ContentLink.AbsoluteUri));
                    }

                    if (videoContent.Hashtag != null)
                    {
                        var shareHashTagBuilder = new ShareHashtag.Builder();
                        shareHashTagBuilder.SetHashtag(videoContent.Hashtag);
                        videoContentBuilder.SetShareHashtag(shareHashTagBuilder.Build().JavaCast<ShareHashtag>());
                    }

                    if (videoContent.PeopleIds != null && videoContent.PeopleIds.Length > 0)
                    {
                        videoContentBuilder.SetPeopleIds(videoContent.PeopleIds);
                    }

                    if (!string.IsNullOrEmpty(videoContent.PlaceId))
                    {
                        videoContentBuilder.SetPlaceId(videoContent.PlaceId);
                    }

                    if (!string.IsNullOrEmpty(videoContent.Ref))
                    {
                        videoContentBuilder.SetRef(videoContent.Ref);
                    }

                    content = videoContentBuilder.Build();
                }

                if (content != null)
                {
                    ShareApi.Share(content, shareCallback);
                }
            }
           
        }
        async void RequestSharePhoto(Dictionary<string, object> paramsDictionary)
        {
            if (paramsDictionary != null && paramsDictionary.ContainsKey("photo"))
            {
                var imageBytes = paramsDictionary["photo"] as byte[];
                SharePhoto.Builder builder = new SharePhoto.Builder();
                if (paramsDictionary.ContainsKey("caption"))
                {
                    builder.SetCaption($"{paramsDictionary["caption"]}");
                }

                Bitmap bmp = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                //Bitmap mutableBitmap = bmp.copy(Bitmap.Config.ARGB_8888, true);

                SharePhoto sharePhoto = builder.SetBitmap(bmp).Build().JavaCast<SharePhoto>();


                var photos = new List<SharePhoto>();
                photos.Add(sharePhoto);

                var sharePhotoContent = new SharePhotoContent.Builder()
                    .SetPhotos(photos).Build();

                ShareApi.Share(sharePhotoContent, shareCallback);

            }

        }

        public async Task<FBEventArgs<bool>> LoginAsync(string[] permissions, FacebookPermissionType permissionType = FacebookPermissionType.Read)
        {
            _loginTcs = new TaskCompletionSource<FBEventArgs<bool>>();
            var retVal = IsLoggedIn;
            if (!retVal || !HasPermissions(permissions))
            {
                var activity = CurrentActivity;

                if (!activity.IsFinishing)
                {
                    activity.RunOnUiThread(() =>
                    {
                        if (permissionType == FacebookPermissionType.Read)
                        {
                            LoginManager.Instance.LogInWithReadPermissions(activity, permissions.ToList());
                        }
                        else
                        {
                            LoginManager.Instance.LogInWithPublishPermissions(activity, permissions.ToList());
                        }
                    });

                }

            }
            else
            {
                var fbArgs = new FBEventArgs<bool>(retVal, FacebookActionStatus.Completed);
                _onLogin?.Invoke(CrossFacebookClient.Current, fbArgs);
                _loginTcs.TrySetResult(fbArgs);

                pendingAction?.Execute();

                pendingAction = null;

            }
            return await _loginTcs.Task;
        }

        public void ActivateApp()
        {
            AppEventsLogger.ActivateApp(Android.App.Application.Context as Android.App.Application);
        }

        public void LogEvent(string name)
        {
            AppEventsLogger.NewLogger(Android.App.Application.Context as Android.App.Application).LogEvent(name);
        }


        public void OnCompleted(JSONObject @object, GraphResponse response)
        {
            try
            {
                if(response.Error!=null)
                {
                    System.Diagnostics.Debug.WriteLine(response.Error.ErrorMessage.ToString());
                    var fbArgs = new FBEventArgs<Dictionary<string, object>>(null, FacebookActionStatus.Error);
                    _onUserData.Invoke(CrossFacebookClient.Current, fbArgs);
                    _userDataTcs?.TrySetResult(fbArgs);
                }
               else{
                    var fieldsString = response.Request.Parameters.GetString("fields");
                    var fields = fieldsString.Split(',');

                    Dictionary<string, object> userData = new Dictionary<string, object>();

                    for (int i = 0; i < fields.Length; i++)
                    {
                        if (@object.Has(fields[i]))
                        {
                            userData.Add(fields[i], @object.GetString(fields[i]));
                        }

                    }
                    userData.Add("user_id", AccessToken.CurrentAccessToken.UserId);
                    userData.Add("token", AccessToken.CurrentAccessToken.Token);
                    var fbArgs = new FBEventArgs<Dictionary<string, object>>(userData, FacebookActionStatus.Completed);
                    _onUserData.Invoke(CrossFacebookClient.Current, fbArgs);
                    _userDataTcs?.TrySetResult(fbArgs);
                }
              
            }
            catch(JSONException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                var fbArgs = new FBEventArgs<Dictionary<string, object>>(null, FacebookActionStatus.Error);
                _onUserData.Invoke(CrossFacebookClient.Current, fbArgs);
                _userDataTcs?.TrySetResult(fbArgs);
            }
           
        }

      
    }

    /// <summary>
    /// FacebookCallback<TResult> class which will handle any result the FacebookActivity returns.
    /// </summary>
    /// <typeparam name="TResult">The callback result's type you will handle</typeparam>
    public class FacebookCallback<TResult> : Java.Lang.Object, IFacebookCallback where TResult : Java.Lang.Object
    {
        public Action HandleCancel { get; set; }
        public Action<FacebookException> HandleError { get; set; }
        public Action<TResult> HandleSuccess { get; set; }

        public void OnCancel()
        {
            var c = HandleCancel;
            if (c != null)
                c();
        }

        public void OnError(FacebookException error)
        {
            var c = HandleError;
            if (c != null)
                c(error);
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            var c = HandleSuccess;
            if (c != null)
                c(result.JavaCast<TResult>());
        }
    }

}