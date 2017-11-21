## Facebook Graph Requests

Information on graph api:

https://developers.facebook.com/docs/graph-api/reference

The following method will allow you to make facebook graph requests:

* **QueryDataAsync** - Request information from facebook
* **DeleteDataAsync** - Delete a previously posted information from facebook
* **PostDataAsync** - Post information to facebook

#### Parameters

* **path** : The facebook Graph path for the requested information
* **permissions** : The facebook needed permissions for the requested action.
* **parameters (Optional)** : Dictionary for the facebook graph request parameters.
* **version (Optional)**: Specify api version if need to do the request on an specific Graph Api version.

#### Response

Type: ```FacebookResponse<string>```

Properties:

* **Data**: Raw facebook response string

* **Status**: Response status

        - Canceled - If Request was canceled
        - Unauthorized - If Request was not authorized due to lack of permissions required
        - Completed - If Request was completed succesfully
        - Error - If Request failed
        
* **Message**: Error message string

Note: If response status is not **Completed**, *Data* property will be null and should have a value on *Message* property with the error.


### QueryDataAsync


Allows you to get information using a Facebook Graph Request Query. 

```cs
 Task<FacebookResponse<string>> QueryDataAsync(string path, string[] permissions, IDictionary<string, string> parameters = null, string version = null);
```

#### Usage Sample:


Get all friends that are authorized on your facebook app

```cs

   CrossFacebookClient.Current.QueryDataAsync("me/friends", new string[]{ "user_friends"}, new Dictionary<string, string>()
   {
      {"fields", "id, first_name, last_name, middle_name, name, email, picture"}
   });
  
```

Get all friends in facebook

```cs

   CrossFacebookClient.Current.QueryDataAsync("me/taggable_friends", new string[]{ "user_friends"}, new Dictionary<string, string>()
   {
      {"fields", "id, first_name, last_name, middle_name, name, email, picture"}
   });
  
```

Get all my likes

```cs

   CrossFacebookClient.Current.QueryDataAsync("me/likes", new string[]{ "user_likes"});
  
```

### DeleteDataAsync

Allows you to delete information within the specified facebook graph path.

```cs
Task<FacebookResponse<string>> DeleteDataAsync(string path, string[] permissions, IDictionary<string, string> parameters = null, string version = null);
```


#### Usage Sample

```cs

   CrossFacebookClient.Current.DeleteDataAsync("1234",new string[]{"publish_actions");
  
```

### PostDataAsync

Allows you to post information within the specified facebook graph path. 

```cs
Task<FacebookResponse<string>> PostDataAsync(string path, string[] permissions, IDictionary<string, string> parameters = null, string version = null);
```

#### Usage Sample

```cs

   CrossFacebookClient.Current.PostDataAsync("me/feed", new Dictionary<string, string>()
   {
      {"message" , "hello world"}
   },new string[]{"publish_actions");
  
```


<= Back to [Table of Contents](../README.md)
