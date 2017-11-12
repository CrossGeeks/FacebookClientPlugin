using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plugin.FacebookClient.Abstractions
{

    public enum FacebookPermissionType
    {
        Read,
        Publish
    }

    public enum FacebookPendingActionType
    {
        None,
        UserData,
        PhotoPost
    }

    public enum FacebookActionStatus
    {
        Canceled,
        Unauthorized,
        Completed,
        Error
    }

    public class FBEventArgs<T> : EventArgs
        where T : new()
    {
        public T Data { get; set; }
        public FacebookActionStatus Status { get; set; }
        public string Message { get; set; }

        public FBEventArgs(T data, FacebookActionStatus status, string msg = "")
        {
            Data = data;
            Status = status;
            Message = msg;
        }

    }

    public class FacebookPendingAction<T>
    {
        Action<T> pAction;
        T pActionParameters;
        //public Action<T> PendingAction { get { return pAction;} }
        //public T Parameters { get { return pActionParameters;} }

        public FacebookPendingAction(Action<T> pendingAction, T parameters)
        {
            pAction = pendingAction;
            pActionParameters = parameters;
        }

        public void Execute()
        {
            pAction?.Invoke(pActionParameters);
        }
    }


    /// <summary>
    /// Interface for FacebookClient
    /// </summary>
    public interface IFacebookClient
    {
            event EventHandler<FBEventArgs<Dictionary<string, object>>> OnUserData;

            event EventHandler<FBEventArgs<bool>> OnLogin;

            event EventHandler<FBEventArgs<bool>> OnLogout;

            event EventHandler<FBEventArgs<Dictionary<string, object>>> OnSharing;

            string ActiveToken { get; }

            string ActiveUserId { get; }

            string[] ActivePermissions { get; }

            string[] DeclinedPermissions { get; }

            bool IsLoggedIn { get; }

            Task<FBEventArgs<bool>> LoginAsync(string[] permissions, FacebookPermissionType permissionType = FacebookPermissionType.Read);
            Task<FBEventArgs<Dictionary<string, object>>> SharePhotoAsync(byte[] imgBytes, string caption = "");
            Task<FBEventArgs<Dictionary<string, object>>> ShareAsync(FacebookShareContent shareContent);
            Task<FBEventArgs<Dictionary<string, object>>> RequestUserDataAsync(string[] fields, string[] permissions, FacebookPermissionType permissionType = FacebookPermissionType.Read);

            void Logout();

            void ActivateApp();

            void LogEvent(string name);
            bool VerifyPermission(string permission);

            bool HasPermissions(string[] permission);

         
    }
}
