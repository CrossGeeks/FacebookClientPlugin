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
  public class FacebookClientManager : NSObject,IFacebookClient, ISharingDelegate
  {
        TaskCompletionSource<FBEventArgs<Dictionary<string, object>>> _userDataTcs;
        TaskCompletionSource<FBEventArgs<Dictionary<string, object>>> _shareTcs;

        public event EventHandler<FBEventArgs<Dictionary<string, object>>> OnUserData = delegate { };

        public event EventHandler<FBEventArgs<bool>> OnLogin = delegate { };

        public event EventHandler<FBEventArgs<bool>> OnLogout = delegate { };

        public event EventHandler<FBEventArgs<Dictionary<string, object>>> OnSharing = delegate { };

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
                return AccessToken.CurrentAccessToken != null;
            }
        }

        public string ActiveToken
        {
            get
            {
                return AccessToken.CurrentAccessToken?.TokenString ?? string.Empty;
            }
        }

        public string ActiveUserId
        {
            get
            {
                return AccessToken.CurrentAccessToken?.UserID ?? string.Empty;
            }
        }

        public bool HasPermissions(string[] permissions)
        {
            if (!IsLoggedIn)
                return false;

            var currentPermissions = AccessToken.CurrentAccessToken.Permissions;

            return permissions.All(p => AccessToken.CurrentAccessToken.HasGranted(p));
        }

        public bool VerifyPermission(string permission)
        {

            return IsLoggedIn && (AccessToken.CurrentAccessToken.HasGranted(permission));

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
            _shareTcs?.TrySetResult(fbArgs);
        }

        public void DidFail(ISharing sharer, NSError error)
        {
            var fbArgs = new FBEventArgs<Dictionary<string, object>>(null, FacebookActionStatus.Error, error.Description);
            OnSharing(this, fbArgs);
            _shareTcs?.TrySetResult(fbArgs);
        }

        public void DidCancel(ISharing sharer)
        {
            var fbArgs = new FBEventArgs<Dictionary<string, object>>(null, FacebookActionStatus.Canceled);

            OnSharing(this, fbArgs);
            _shareTcs?.TrySetResult(fbArgs);
        }

        public async Task<FBEventArgs<bool>> LoginAsync(string[] permissions, FacebookPermissionType permissionType = FacebookPermissionType.Read)
        {
            var retVal = IsLoggedIn;
            FacebookActionStatus status = FacebookActionStatus.Error;
            if (!retVal || !HasPermissions(permissions))
            {
                var window = UIApplication.SharedApplication.KeyWindow;
                var vc = window.RootViewController;
                while (vc.PresentedViewController != null)
                {
                    vc = vc.PresentedViewController;
                }

                LoginManagerLoginResult result = await (permissionType == FacebookPermissionType.Read ? loginManager.LogInWithReadPermissionsAsync(permissions, vc) : loginManager.LogInWithPublishPermissionsAsync(permissions, vc));

                if (result.IsCancelled)
                {
                    retVal = false;
                    status = FacebookActionStatus.Canceled;
                }
                else
                {
                    //result.GrantedPermissions.h
                    retVal = HasPermissions(result.GrantedPermissions.Select(p => $"{p}").ToArray());

                    status = retVal ? FacebookActionStatus.Completed : FacebookActionStatus.Unauthorized;
                }

            }
            else
            {

                status = FacebookActionStatus.Completed;

            }
            var fbArgs = new FBEventArgs<bool>(retVal, status);
            OnLogin(this, fbArgs);

            pendingAction?.Execute();

            pendingAction = null;


            return fbArgs;
        }



        public async Task<FBEventArgs<Dictionary<string, object>>> SharePhotoAsync(byte[] photoBytes, string caption = "")
        {
            _shareTcs = new TaskCompletionSource<FBEventArgs<Dictionary<string, object>>>();
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                {"photo",photoBytes},

            };

            if (!string.IsNullOrEmpty(caption))
            {
                parameters.Add("caption", caption);
            }

          return await PerformAction(RequestSharePhoto, parameters, _shareTcs.Task, FacebookPermissionType.Publish, new string[] { "publish_actions" });
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

                ShareAPI.Share(photoContent, this);
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
                        for (int i = 0; i < fields.Length; i++)
                        {
                            userData.Add(fields[i], $"{result.ValueForKey(new NSString(fields[i]))}");
                        }
                        userData.Add("user_id", AccessToken.CurrentAccessToken.UserID);
                        userData.Add("token", AccessToken.CurrentAccessToken.TokenString);
                        var fbArgs = new FBEventArgs<Dictionary<string, object>>(userData, FacebookActionStatus.Completed);
                        OnUserData(this,fbArgs);
                        _userDataTcs?.TrySetResult(fbArgs);
                        
                    }
                    else
                    {
                        var fbArgs = new FBEventArgs<Dictionary<string, object>>(null, FacebookActionStatus.Error, $"Facebook User Data Request Failed - {error.Code} - {error.Description}");
                        OnUserData(this,fbArgs );
                        _userDataTcs?.TrySetResult(fbArgs);
                       
                    }


                });
                requestConnection.Start();
            }
            else
            {
                var fbArgs = new FBEventArgs<Dictionary<string, object>>(null, FacebookActionStatus.Canceled);
                OnUserData(this, null);
                _userDataTcs?.TrySetResult(fbArgs);
            }


        }


        async Task<FBEventArgs<Dictionary<string, object>>> PerformAction(Action<Dictionary<string, object>> action, Dictionary<string, object> parameters,Task<FBEventArgs<Dictionary<string, object>>> task, FacebookPermissionType permissionType, string[] permissions)
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

        public void Logout()
        {
            if (IsLoggedIn)
                loginManager.LogOut();

            OnLogout(this, new FBEventArgs<bool>(true, FacebookActionStatus.Completed));
        }

        public async Task<FBEventArgs<Dictionary<string, object>>> RequestUserDataAsync(string[] fields, string[] permissions, FacebookPermissionType permissionType = FacebookPermissionType.Read)
        {
            _userDataTcs = new TaskCompletionSource<FBEventArgs<Dictionary<string, object>>>();
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                {"fields",fields}
            };

           return await PerformAction(RequestUserData, parameters,_userDataTcs.Task, FacebookPermissionType.Read, permissions);
        }

        public void ActivateApp()
        {
            AppEvents.ActivateApp();
        }

        public void LogEvent(string name)
        {
            AppEvents.LogEvent(name);
        }
    }
}