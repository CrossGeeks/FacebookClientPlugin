using FacebookClientSample.Models;
using Newtonsoft.Json.Linq;
using Plugin.FacebookClient;
using Plugin.FacebookClient.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FacebookClientSample.ViewModels
{
    public class MyProfileViewModel : INotifyPropertyChanged
    {
        public bool IsBusy { get; set; } = false;

        public bool IsNotBusy { get { return !IsBusy; } }
        public ObservableCollection<FacebookPost> Posts { get; set; }
        public FacebookProfile Profile { get; set; }         public Command LoadDataCommand { get; set; }

        public Command<string> PostMessageCommand { get; set; }         public string PostMessage { get; set; } = string.Empty;

        public event PropertyChangedEventHandler PropertyChanged;

        public MyProfileViewModel()
        {
            Profile = new FacebookProfile();
            Posts = new ObservableCollection<FacebookPost>();             LoadDataCommand = new Command(async() => await LoadData());
            PostMessageCommand = new Command<string>(async(msg) => await PostAsync(msg));
            LoadDataCommand.Execute(null);

        }


        public async Task LoadData()         {                          var jsonData = await CrossFacebookClient.Current.RequestUserDataAsync             (
                  new string[] { "id", "name", "picture", "cover", "friends" }, new string[] { }             );

            var data = JObject.Parse(jsonData.Data);
            Profile = new FacebookProfile()
            {
                FullName = data["name"].ToString(),
                Cover = new UriImageSource { Uri = new System.Uri($"{data["cover"]["source"]}") },
                Picture = new UriImageSource { Uri = new System.Uri($"{data["picture"]["data"]["url"]}") }
            };
            LoadPosts();         }

        public async Task PostAsync(string message)         {            var response= await CrossFacebookClient.Current.PostDataAsync("me/feed", new string[] { "publish_actions" },                                                               new Dictionary<string, string>()                                                                {                                                                    {"message" ,message}                                                                });             PostMessage = string.Empty;
            LoadPosts();         }


        public async Task LoadPosts()         {
            if (IsBusy)
                return;

            IsBusy = true;
          
            Posts.Clear();
            FacebookResponse<string> post = await CrossFacebookClient.Current.QueryDataAsync("me/feed", new string[] { "user_posts" });
            var jo = JObject.Parse(post.Data); 
            if(jo.ContainsKey("data"))
            {
                var array = ((JArray)jo["data"]);
                foreach(var item in array)
                {
                    var postData = new FacebookPost();

                    if (item["message"] != null)
                    {
                        postData.Message = $"{item["message"]}";
                    }

                    if (item["story"] != null)
                    {
                        postData.Story = $"{item["story"]}";
                    }

                    Posts.Add(postData);
                }
            }

            IsBusy = false;
        }
    }
}
