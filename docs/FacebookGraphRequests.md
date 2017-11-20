## Facebook Graph Requests

https://developers.facebook.com/docs/graph-api/reference

```cs
            Task<FacebookResponse<string>> QueryDataAsync(string path, string[] permissions, IDictionary<string, string> parameters = null, string version = null);
            Task<FacebookResponse<string>> PostDataAsync(string path, IDictionary<string, string> parameters = null, string version = null);
            Task<FacebookResponse<string>> DeleteDataAsync(string path, IDictionary<string, string> parameters = null, string version = null);
```

### QueryDataAsync

Allows you to get information using a Facebook Graph Request Query. You should specify at least two things:

* *path* : The facebook Graph path for the requested information
* *permissions* : The facebook needed permissions for the requested information.

Also you can specify the parameters for the facebook graph request.

* **parameters**: Dictionary for the facebook graph request parameters.

Sample:

```cs

   var fbParams =  new Dictionary<string, string>()
   {
      {"fields", "id, first_name, last_name, middle_name, name, email, picture"}
   }
  
```

If need to do the request on an specific Graph Api version, could be specify by setting the **version** parameter.

Usage:


//Get all friends that are authorized on your facebook app

```cs

   CrossFacebookClient.Current.QueryDataAsync("me/friends", new string[]{ "user_friends"}, new Dictionary<string, string>()
   {
      {"fields", "id, first_name, last_name, middle_name, name, email, picture"}
   });
  
```

//Get all friends in facebook

```cs

   CrossFacebookClient.Current.QueryDataAsync("me/taggable_friends", new string[]{ "user_friends"}, new Dictionary<string, string>()
   {
      {"fields", "id, first_name, last_name, middle_name, name, email, picture"}
   });
  
```

//Get all my likes

```cs

   CrossFacebookClient.Current.QueryDataAsync("me/likes", new string[]{ "user_likes"});
  
```


//Delete post

```cs

   CrossFacebookClient.Current.DeleteDataAsync("1234");
  
```


//Post Data

```cs

   CrossFacebookClient.Current.PostDataAsync("me/feed", new Dictionary<string, string>()
   {
      {"message" , "hello world"}
   });
  
```


<= Back to [Table of Contents](../README.md)