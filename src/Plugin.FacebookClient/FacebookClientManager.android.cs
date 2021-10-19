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
using Android.Content;
using Xamarin.Facebook.Share.Widget;

namespace Plugin.FacebookClient
{
    /// <summary>
    /// Implementation for Feature
    /// </summary>
    public class FacebookClientManager : Java.Lang.Object, IFacebookClient, GraphRequest.IGraphJSONObjectCallback, GraphRequest.ICallback
    {
        static TaskCompletionSource<FacebookResponse<string>> _userDataTcs;
        static TaskCompletionSource<FacebookResponse<Dictionary<string, object>>> _shareTcs;
        static TaskCompletionSource<FacebookResponse<Dictionary<string, object>>> _gameRequestTcs;
        static TaskCompletionSource<FacebookResponse<string>> _requestTcs;
        static TaskCompletionSource<FacebookResponse<string>> _postTcs;
        static TaskCompletionSource<FacebookResponse<string>> _deleteTcs;
        static TaskCompletionSource<FacebookResponse<bool>> _loginTcs;

        static AppEventsLogger mLogger;
        static ICallbackManager mCallbackManager;

        //Activity mActivity;
        static FacebookCallback<SharerResult> shareCallback;
        static FacebookCallback<LoginResult> loginCallback;
        static FacebookCallback<GameRequestDialog.Result> gameRequestCallback;

        public static Activity CurrentActivity { get; set; }

        static FacebookPendingAction<Dictionary<string, object>> pendingAction;

        static EventHandler<FBEventArgs<string>> _onUserData;
        public event EventHandler<FBEventArgs<string>> OnUserData
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

        static EventHandler<FBEventArgs<string>> _onRequestData;
        public event EventHandler<FBEventArgs<string>> OnRequestData
        {
            add
            {
                _onRequestData += value;
            }
            remove
            {
                _onRequestData -= value;
            }
        }

        static EventHandler<FBEventArgs<string>> _onPostData;
        public event EventHandler<FBEventArgs<string>> OnPostData
        {
            add
            {
                _onPostData += value;
            }
            remove
            {
                _onPostData -= value;
            }
        }

        static EventHandler<FBEventArgs<string>> _onDeleteData;
        public event EventHandler<FBEventArgs<string>> OnDeleteData
        {
            add
            {
                _onDeleteData += value;
            }
            remove
            {
                _onDeleteData -= value;
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

        static EventHandler<FBEventArgs<Dictionary<string, object>>> _onGameRequest;
        public event EventHandler<FBEventArgs<Dictionary<string, object>>> OnGameRequest
        {
            add
            {
                _onGameRequest += value;
            }
            remove
            {
                _onGameRequest -= value;
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
                return AccessToken.CurrentAccessToken != null && !AccessToken.CurrentAccessToken.IsExpired;
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

        public DateTime TokenExpirationDate
        {
            get
            {

                return AccessToken.CurrentAccessToken == null ? DateTime.MinValue : new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(AccessToken.CurrentAccessToken.Expires.Time);
            }
        }

        public static void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {
            mCallbackManager?.OnActivityResult(requestCode, (int)resultCode, intent);
        }
        public static void Initialize(Activity activity)
        {
            mLogger = AppEventsLogger.NewLogger(Android.App.Application.Context as Android.App.Application);
            mCallbackManager = CallbackManagerFactory.Create();
            CurrentActivity = activity;

            loginCallback = new FacebookCallback<LoginResult>
            {
                HandleSuccess = loginResult =>
                {

                    if (loginResult.AccessToken != null)
                    {
                        var fbArgs = new FBEventArgs<bool>(true, FacebookActionStatus.Completed);
                        _onLogin?.Invoke(CrossFacebookClient.Current, fbArgs);
                        _loginTcs?.TrySetResult(new FacebookResponse<bool>(fbArgs));

                        pendingAction?.Execute();

                        pendingAction = null;
                    }
                },
                HandleCancel = () =>
                {
                    var fbArgs = new FBEventArgs<bool>(false, FacebookActionStatus.Canceled, "User cancelled facebook operation");
                    _onLogin?.Invoke(CrossFacebookClient.Current, fbArgs);
                    _loginTcs?.TrySetResult(new FacebookResponse<bool>(fbArgs));
                    //Handle any cancel the user has perform
                    Console.WriteLine("User cancelled facebook login operation");

                    pendingAction?.Execute();

                    pendingAction = null;
                },
                HandleError = loginError =>
                {
                    var fbArgs = new FBEventArgs<bool>(false, FacebookActionStatus.Error, loginError.ToString());
                    _onLogin?.Invoke(CrossFacebookClient.Current, fbArgs);
                    _loginTcs?.TrySetResult(new FacebookResponse<bool>(fbArgs));

                    //Handle any error happends here
                    Console.WriteLine("Operation throws an error: " + loginError.ToString());

                    pendingAction?.Execute();
                    pendingAction = null;
                }
            };


            shareCallback = new FacebookCallback<SharerResult>
            {
                HandleSuccess = shareResult =>
                {
                    Dictionary<string, object> parameters = null;
                    if (shareResult.PostId != null)
                    {
                        parameters = new Dictionary<string, object>();
                        parameters.Add("postId", shareResult.PostId);
                    }
                    var fbArgs = new FBEventArgs<Dictionary<string, object>>(parameters, FacebookActionStatus.Completed);
                    _onSharing?.Invoke(CrossFacebookClient.Current, fbArgs);
                    _shareTcs?.TrySetResult(new FacebookResponse<Dictionary<string, object>>(fbArgs));
                },
                HandleCancel = () =>
                {
                    var fbArgs = new FBEventArgs<Dictionary<string, object>>(null, FacebookActionStatus.Canceled, "User cancelled facebook operation");
                    _onSharing?.Invoke(CrossFacebookClient.Current, fbArgs);
                    _shareTcs?.TrySetResult(new FacebookResponse<Dictionary<string, object>>(fbArgs));
                },
                HandleError = shareError =>
                {
                    var fbArgs = new FBEventArgs<Dictionary<string, object>>(null, FacebookActionStatus.Error, shareError.Message);
                    _onSharing?.Invoke(CrossFacebookClient.Current, fbArgs);
                    _shareTcs?.TrySetResult(new FacebookResponse<Dictionary<string, object>>(fbArgs));
                }
            };


            gameRequestCallback = new FacebookCallback<GameRequestDialog.Result>
            {
                HandleSuccess = gameRequestResult =>
                {

                    if (!string.IsNullOrEmpty(gameRequestResult.RequestId))
                    {
                        Dictionary<string, object> parameters = new Dictionary<string, object>();

                        parameters.Add("requestId", gameRequestResult.RequestId);
                        var fbArgs = new FBEventArgs<Dictionary<string, object>>(parameters, FacebookActionStatus.Completed);
                        _onGameRequest?.Invoke(CrossFacebookClient.Current, fbArgs);
                        _gameRequestTcs?.TrySetResult(new FacebookResponse<Dictionary<string, object>>(fbArgs));
                    }
                },
                HandleCancel = () =>
                {
                    var fbArgs = new FBEventArgs<Dictionary<string, object>>(null, FacebookActionStatus.Canceled, "User cancelled facebook operation");
                    _onGameRequest?.Invoke(CrossFacebookClient.Current, fbArgs);
                    _gameRequestTcs?.TrySetResult(new FacebookResponse<Dictionary<string, object>>(fbArgs));
                },
                HandleError = gameRequestError =>
                {
                    var fbArgs = new FBEventArgs<Dictionary<string, object>>(null, FacebookActionStatus.Error, gameRequestError.Message);
                    _onGameRequest?.Invoke(CrossFacebookClient.Current, fbArgs);
                    _gameRequestTcs?.TrySetResult(new FacebookResponse<Dictionary<string, object>>(fbArgs));
                }
            };

            LoginManager.Instance.RegisterCallback(mCallbackManager, loginCallback);
        }
        public void Logout()
        {
            //_shareTcs = null;
            //_userDataTcs = null;
            LoginManager.Instance.LogOut();
            AccessToken.CurrentAccessToken = null;
            Profile.CurrentProfile = null;
            _onLogout?.Invoke(CrossFacebookClient.Current, new FBEventArgs<bool>(true, FacebookActionStatus.Completed));
        }


        public bool VerifyPermission(string permission)
        {

            return (AccessToken.CurrentAccessToken != null) && (AccessToken.CurrentAccessToken.Permissions.Contains(permission));

        }


        /*async Task<FBEventArgs<Dictionary<string, object>>> PerformAction(Action<Dictionary<string, object>> action, Dictionary<string, object> parameters, Task<FBEventArgs<Dictionary<string, object>>> task, FacebookPermissionType permissionType, string[] permissions)
        {
            pendingAction = null;
            if(permissions == null)
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

        }*/

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
        public async Task<FacebookResponse<Dictionary<string, object>>> ShareAsync(FacebookShareContent shareContent)
        {
            _shareTcs = new TaskCompletionSource<FacebookResponse<Dictionary<string, object>>>();
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                {"content",shareContent}

            };

            return await PerformAction<FacebookResponse<Dictionary<string, object>>>(RequestShare, parameters, _shareTcs.Task, FacebookPermissionType.Publish, new string[] { });

        }

        public async Task<FacebookResponse<Dictionary<string, object>>> SharePhotoAsync(byte[] photoBytes, string caption = "")
        {
            _shareTcs = new TaskCompletionSource<FacebookResponse<Dictionary<string, object>>>();
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                {"photo",photoBytes}

            };

            if (!string.IsNullOrEmpty(caption))
            {
                parameters.Add("caption", caption);
            }

            return await PerformAction(RequestSharePhoto, parameters, _shareTcs.Task, FacebookPermissionType.Publish, new string[] {  });

        }

        public bool HasPermissions(string[] permissions)
        {
            if (!IsLoggedIn)
                return false;

            var currentPermissions = AccessToken.CurrentAccessToken.Permissions;

            return permissions.All(p => currentPermissions.Contains(p));
        }

        public async Task<FacebookResponse<string>> RequestUserDataAsync(string[] fields, string[] permissions, FacebookPermissionType permissionType = FacebookPermissionType.Read)
        {
            _userDataTcs = new TaskCompletionSource<FacebookResponse<string>>();
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                {"fields",fields}
            };

            return await PerformAction(RequestUserData, parameters, _userDataTcs.Task, FacebookPermissionType.Read, permissions);
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

        public async Task<FacebookResponse<string>> DeleteDataAsync(string path, string[] permissions, IDictionary<string, string> parameters = null, string version = null)
        {
            _deleteTcs = new TaskCompletionSource<FacebookResponse<string>>();
            Dictionary<string, object> paramDict = new Dictionary<string, object>()
            {
                {"path", path},
                {"parameters",parameters},
                {"method", FacebookHttpMethod.Delete },
                {"version", version}
            };
            return await PerformAction<FacebookResponse<string>>(RequestData, paramDict, _deleteTcs.Task, FacebookPermissionType.Publish, permissions);
        }



        void RequestData(Dictionary<string, object> pDictionary)
        {
            //string path,Bundle parameters = null,HttpMethod method = null,string version = null
            string path = $"{pDictionary["path"]}";
            string version = $"{pDictionary["version"]}";
            Dictionary<string, string> parameters = pDictionary["parameters"] as Dictionary<string, string>;


            FacebookHttpMethod? method = pDictionary["method"] as FacebookHttpMethod?;
            var currentTcs = _requestTcs;
            var _onEvent = _onRequestData;
            var httpMethod = HttpMethod.Get;
            if (method != null)
            {
                switch (method)
                {
                    case FacebookHttpMethod.Get:
                        httpMethod = HttpMethod.Get;
                        break;
                    case FacebookHttpMethod.Post:
                        httpMethod = HttpMethod.Post;
                        _onEvent = _onPostData;
                        currentTcs = _postTcs;
                        break;
                    case FacebookHttpMethod.Delete:
                        httpMethod = HttpMethod.Delete;
                        _onEvent = _onDeleteData;
                        currentTcs = _deleteTcs;
                        break;
                }

            }

            if (string.IsNullOrEmpty(path))
            {
                var fbResponse = new FBEventArgs<string>(null, FacebookActionStatus.Error, "Graph query path not specified");
                _onEvent?.Invoke(CrossFacebookClient.Current, fbResponse);
                currentTcs?.TrySetResult(new FacebookResponse<string>(fbResponse));
                return;
            }

            if (AccessToken.CurrentAccessToken != null)
            {


                GraphRequest request = new GraphRequest(AccessToken.CurrentAccessToken, path);

                request.Callback = this;
                request.HttpMethod = httpMethod;

                if (parameters != null)
                {
                    Bundle bundle = new Bundle();
                    foreach (var p in parameters)
                    {
                        if (!string.IsNullOrEmpty(p.Key) && !string.IsNullOrEmpty(p.Value))
                        {
                            bundle.PutString($"{p.Key}", $"{p.Value}");
                        }

                    }
                    request.Parameters = bundle;


                }

                if (!string.IsNullOrEmpty(version))
                {
                    request.Version = version;
                }

                request.ExecuteAsync();


            }
            else
            {

                var fbResponse = new FBEventArgs<string>(null, FacebookActionStatus.Unauthorized, "Facebook operation not authorized, be sure you requested the right permissions");
                _onEvent?.Invoke(CrossFacebookClient.Current, fbResponse);
                currentTcs?.TrySetResult(new FacebookResponse<string>(fbResponse));
            }

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

            if (AccessToken.CurrentAccessToken != null)
            {

                GraphRequest request = GraphRequest.NewMeRequest(AccessToken.CurrentAccessToken, this);

                Bundle parameters = new Bundle();
                parameters.PutString("fields", string.Join(",", fields));
                request.Parameters = parameters;
                request.ExecuteAsync();

            }
            else
            {
                var fbResponse = new FBEventArgs<string>(null, FacebookActionStatus.Canceled, "User cancelled facebook operation");

                _onUserData?.Invoke(CrossFacebookClient.Current, fbResponse);

                _userDataTcs?.TrySetResult(new FacebookResponse<string>(fbResponse));
            }

        }

        void RequestShare(Dictionary<string, object> paramsDictionary)
        {
            if (paramsDictionary.TryGetValue("content", out object shareContent) && shareContent is FacebookShareContent)
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

                    if (!string.IsNullOrEmpty(linkContent.Hashtag))
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

                    if (!string.IsNullOrEmpty(photoContent.Hashtag))
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


                   /*if (videoContent.PreviewPhoto != null)
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
                    }*/

                    if (videoContent.Video != null)
                    {
                        ShareVideo.Builder videoBuilder = new ShareVideo.Builder();

                        if (videoContent.Video.LocalUrl != null)
                        {
                            videoBuilder.SetLocalUrl(Android.Net.Uri.Parse(videoContent.Video.LocalUrl.AbsoluteUri));

                        }

                        videoContentBuilder.SetVideo(videoBuilder.Build().JavaCast<ShareVideo>());
                    }


                    if (videoContent.ContentLink != null)
                    {
                        videoContentBuilder.SetContentUrl(Android.Net.Uri.Parse(videoContent.ContentLink.AbsoluteUri));
                    }

                    if (!string.IsNullOrEmpty(videoContent.Hashtag))
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
                    //ShareApi.Share(content, shareCallback);
                    ShareDialog dialog = new ShareDialog(CurrentActivity);
                    dialog.RegisterCallback(mCallbackManager, shareCallback);
                    dialog.ShouldFailOnDataError = true;
                    dialog.Show(content, ShareDialog.Mode.Automatic);
                }
            }

        }

        void RequestSharePhoto(Dictionary<string, object> paramsDictionary)
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

                //ShareApi.Share(sharePhotoContent, shareCallback);

                ShareDialog dialog = new ShareDialog(CurrentActivity);
                dialog.RegisterCallback(mCallbackManager, shareCallback);
                dialog.ShouldFailOnDataError = true;
                dialog.Show(sharePhotoContent, ShareDialog.Mode.Automatic);

            }

        }

        public async Task<FacebookResponse<bool>> LoginAsync(string[] permissions, FacebookPermissionType permissionType = FacebookPermissionType.Read)
        {
            _loginTcs = new TaskCompletionSource<FacebookResponse<bool>>();
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
                _loginTcs.TrySetResult(new FacebookResponse<bool>(fbArgs));

                pendingAction?.Execute();

                pendingAction = null;

            }
            return await _loginTcs.Task;
        }

        public static void OnActivated()
        {
            AppEventsLogger.ActivateApp(Android.App.Application.Context as Android.App.Application);
        }

        public void LogEvent(string name)
        {
            mLogger?.LogEvent(name);
        }


        public void OnCompleted(JSONObject @object, GraphResponse response)
        {
            try
            {
                if (response.Error != null)
                {
                    System.Diagnostics.Debug.WriteLine(response.Error.ErrorMessage.ToString());
                    var fbArgs = new FBEventArgs<string>(null, FacebookActionStatus.Error, response.Error.ErrorMessage.ToString());
                    _onUserData?.Invoke(CrossFacebookClient.Current, fbArgs);
                    _userDataTcs?.TrySetResult(new FacebookResponse<string>(fbArgs));
                }
                else {
                    var fbArgs = new FBEventArgs<string>(@object?.ToString(), FacebookActionStatus.Completed);
                    _onUserData?.Invoke(CrossFacebookClient.Current, fbArgs);
                    _userDataTcs?.TrySetResult(new FacebookResponse<string>(fbArgs));
                }

            }
            catch (JSONException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                var fbArgs = new FBEventArgs<string>(null, FacebookActionStatus.Error, ex.ToString());
                _onUserData?.Invoke(CrossFacebookClient.Current, fbArgs);
                _userDataTcs?.TrySetResult(new FacebookResponse<string>(fbArgs));
            }

        }

        public void OnCompleted(GraphResponse response)
        {

            var currentTcs = _requestTcs;

            var _onEvent = _onRequestData;
            if (response.Request.HttpMethod == HttpMethod.Post)
            {
                currentTcs = _postTcs;

                _onEvent = _onPostData;
            }
            else if (response.Request.HttpMethod == HttpMethod.Delete)
            {
                currentTcs = _deleteTcs;
                _onEvent = _onDeleteData;
            }

            if (response.Error != null)
            {

                System.Diagnostics.Debug.WriteLine(response.Error.ErrorMessage.ToString());
                var fbResponse = new FBEventArgs<string>(null, FacebookActionStatus.Error, response.Error.ErrorMessage);
                _onEvent?.Invoke(CrossFacebookClient.Current, fbResponse);
                currentTcs?.TrySetResult(new FacebookResponse<string>(fbResponse));
            }
            else
            {
                var fbResponse = new FBEventArgs<string>(response?.JSONObject?.ToString(), FacebookActionStatus.Completed);
                _onEvent?.Invoke(CrossFacebookClient.Current, fbResponse);
                currentTcs?.TrySetResult(new FacebookResponse<string>(fbResponse));
            }
        }

        public async Task<FacebookResponse<Dictionary<string, object>>> RequestGameRequestDialogAsync(FacebookGameRequestContent gameRequestContent)
        {
            _gameRequestTcs = new TaskCompletionSource<FacebookResponse<Dictionary<string, object>>>();
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                {"content",gameRequestContent}

            };

            return await PerformAction<FacebookResponse<Dictionary<string, object>>>(RequestGame, parameters, _gameRequestTcs.Task, FacebookPermissionType.Publish, new string[] { });

        }
        void RequestGame(Dictionary<string, object> paramsDictionary)
        {
            if (paramsDictionary.TryGetValue("content", out object request) && request is FacebookGameRequestContent)
            {
                GameRequestContent.Builder gRequestContent = new GameRequestContent.Builder();
                var gameRequestContent = request as FacebookGameRequestContent;

                if (!string.IsNullOrEmpty(gameRequestContent.ObjectId))
                {
                    gRequestContent.SetObjectId(gameRequestContent.ObjectId);
                }

                if (!string.IsNullOrEmpty(gameRequestContent.Title))
                {
                    gRequestContent.SetTitle(gameRequestContent.Title);
                }

                if (!string.IsNullOrEmpty(gameRequestContent.Message))
                {
                    gRequestContent.SetMessage(gameRequestContent.Message);
                }

                if (!string.IsNullOrEmpty(gameRequestContent.Data))
                {
                    gRequestContent.SetData(gameRequestContent.Data);
                }

                if (gameRequestContent.Recipients != null && gameRequestContent.Recipients.Length > 0)
                {
                    gRequestContent.SetRecipients(gameRequestContent.Recipients);
                }

                if (gameRequestContent.RecipientSuggestions != null && gameRequestContent.RecipientSuggestions.Length > 0)
                {
                    gRequestContent.SetSuggestions(gameRequestContent.RecipientSuggestions);
                }

                switch (gameRequestContent.ActionType)
                {
                    case FacebookGameRequestActionType.None:
                      
                        break;
                    case FacebookGameRequestActionType.Send:
                        gRequestContent.SetActionType(GameRequestContent.ActionType.Send);
                        break;
                    case FacebookGameRequestActionType.AskFor:
                        gRequestContent.SetActionType(GameRequestContent.ActionType.Askfor);
                        break;
                    case FacebookGameRequestActionType.Turn:
                        gRequestContent.SetActionType(GameRequestContent.ActionType.Turn);
                        break;

                }


                switch (gameRequestContent.Filters)
                {
                    case FacebookGameRequestFilter.None:
            
                        break;
                    case FacebookGameRequestFilter.AppUsers:
                        gRequestContent.SetFilters(GameRequestContent.Filters.AppUsers);
                        break;
                    case FacebookGameRequestFilter.AppNonUsers:
                        gRequestContent.SetFilters(GameRequestContent.Filters.AppNonUsers);
                        break;

                }

                GameRequestDialog dialog = new GameRequestDialog(CurrentActivity);
                dialog.RegisterCallback(mCallbackManager, gameRequestCallback);
                dialog.Show(gRequestContent.Build());
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
                var handler = HandleCancel;
                if (handler != null)
                    handler();
            }

            public void OnError(FacebookException error)
            {
                var handler = HandleError;
                if (handler != null)
                    handler(error);
            }

            public void OnSuccess(Java.Lang.Object result)
            {
                var handler = HandleSuccess;
                if (handler != null)
                    handler(result.JavaCast<TResult>());
            }
        }
    }

}
