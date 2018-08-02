## Facebook Graph Requests

Information on graph api here:

https://developers.facebook.com/docs/graph-api/reference

The following method will allow you to make facebook graph requests:

* **QueryDataAsync** - Request information from facebook

#### Parameters

* **path** : The facebook Graph path for the requested information
* **permissions** : The facebook needed permissions for the requested action. (Should always specify the needed permissions for the graph request, if permission is not granted yet it will automatically request the permission to the user and then complete de request operation if granted)
* **parameters (Optional)** : Dictionary for the facebook graph request parameters.
* **version (Optional)**: Specify api version if need to do the request on an specific Graph Api version.

#### Response

Type: ```FacebookResponse<string>```

Properties:

* **Data**: Raw facebook response string

* **Status**: Response status

     *FacebookActionStatus.Canceled* - If Request was canceled
     
     *FacebookActionStatus.Unauthorized* - If Request was not authorized due to lack of permissions required
     
     *FacebookActionStatus.Completed* - If Request was completed succesfully
     
     *FacebookActionStatus.Error* - If Request failed
        
* **Message**: Error message string

**Note: If response *Status* isn't FacebookActionStatus.Completed, *Data* property will be null and should have a value on *Message* property with the error.**


### QueryDataAsync


Allows you to get information using a Facebook Graph Request Query. 

```cs
 Task<FacebookResponse<string>> QueryDataAsync(string path, string[] permissions, IDictionary<string, string> parameters = null, string version = null);
```

#### Usage Sample:


Get all friends that are authorized on your facebook app

```cs

   await CrossFacebookClient.Current.QueryDataAsync("me/friends", new string[]{ "user_friends"}, new Dictionary<string, string>()
   {
      {"fields", "id, first_name, last_name, middle_name, name, email, picture"}
   });
  
```

Get all friends in facebook

```cs

   await CrossFacebookClient.Current.QueryDataAsync("me/taggable_friends", new string[]{ "user_friends"}, new Dictionary<string, string>()
   {
      {"fields", "id, first_name, last_name, middle_name, name, email, picture"}
   });
  
```

Get all my likes

```cs

  await CrossFacebookClient.Current.QueryDataAsync("me/likes", new string[]{ "user_likes"});
  
```

<= Back to [Table of Contents](../README.md)
