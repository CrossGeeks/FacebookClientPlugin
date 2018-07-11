using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plugin.FacebookClient.Abstractions
{
    public enum FacebookHttpMethod
    {
        Get,
        Post,
        Delete
    }

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

    public class FacebookResponse<T> 
    {
        public T Data { get; set; }
        public FacebookActionStatus Status { get; set; }
        public string Message { get; set; }

        public FacebookResponse(FBEventArgs<T> evtArgs)
        {
            Data = evtArgs.Data;
            Status = evtArgs.Status;
            Message = evtArgs.Message;
        }

        public FacebookResponse(T data, FacebookActionStatus status, string msg = "")
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
            event EventHandler<FBEventArgs<string>> OnRequestData;
            event EventHandler<FBEventArgs<string>> OnPostData;
            event EventHandler<FBEventArgs<string>> OnDeleteData;

            event EventHandler<FBEventArgs<string>> OnUserData;

            event EventHandler<FBEventArgs<bool>> OnLogin;

            event EventHandler<FBEventArgs<bool>> OnLogout;

            event EventHandler<FBEventArgs<Dictionary<string, object>>> OnSharing;

            string ActiveToken { get; }

            string ActiveUserId { get; }

            DateTime TokenExpirationDate { get; }

            string[] ActivePermissions { get; }

            string[] DeclinedPermissions { get; }

            bool IsLoggedIn { get; }

            Task<FacebookResponse<bool>> LoginAsync(string[] permissions, FacebookPermissionType permissionType = FacebookPermissionType.Read);
            Task<FacebookResponse<Dictionary<string, object>>> SharePhotoAsync(byte[] imgBytes, string caption = "");
            Task<FacebookResponse<Dictionary<string, object>>> ShareAsync(FacebookShareContent shareContent);
            Task<FacebookResponse<string>> RequestUserDataAsync(string[] fields, string[] permissions, FacebookPermissionType permissionType = FacebookPermissionType.Read);
            Task<FacebookResponse<string>> QueryDataAsync(string path, string[] permissions = null, IDictionary<string, string> parameters = null, string version = null);
            Task<FacebookResponse<string>> PostDataAsync(string path, string[] permissions, IDictionary<string, string> parameters = null, string version = null);
            Task<FacebookResponse<string>> DeleteDataAsync(string path, string[] permissions, IDictionary<string, string> parameters = null, string version = null);

            void Logout();

            void LogEvent(string name);
            bool VerifyPermission(string permission);

            bool HasPermissions(string[] permission);

         
    }
}
