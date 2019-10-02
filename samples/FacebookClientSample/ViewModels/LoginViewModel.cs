using Newtonsoft.Json.Linq;
using Plugin.FacebookClient;
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
    public class LoginViewModel : INotifyPropertyChanged
    {
        string[] permisions = new string[] { "email", "public_profile" , "user_posts" };

		public bool IsBusy { get; set; } = false;
        public bool IsNotBusy { get { return !IsBusy; } }
        public FacebookProfile Profile { get; set; }

		public Command OnLoginCommand { get; set; }
        public Command OnShareDataCommand { get; set; }
		public Command OnLoadDataCommand { get; set; }
		public Command OnLogoutCommand { get; set; }
        public bool IsLoggedIn { get; set; }

		//public ObservableCollection<FacebookPost> Posts { get; set; }

        //public Command<string> PostMessageCommand { get; set; }
        //public string PostMessage { get; set; } = string.Empty;

        public event PropertyChangedEventHandler PropertyChanged;



        public LoginViewModel()         {

			Profile = new FacebookProfile();

            OnLoginCommand = new Command(async()=> await LoginAsync());
            OnShareDataCommand = new Command(async () => await ShareDataAsync());
			OnLoadDataCommand = new Command(async () => await LoadData());
			OnLogoutCommand = new Command( () =>
			{
				if (CrossFacebookClient.Current.IsLoggedIn)
				{
					CrossFacebookClient.Current.Logout();
                    IsLoggedIn = false;
				}
			});

			//Posts = new ObservableCollection<FacebookPost>();
            //LoadDataCommand.Execute(null);
        }

        public async Task LoginAsync()         {
            FacebookResponse<bool> response = await CrossFacebookClient.Current.LoginAsync(permisions);             switch(response.Status)
            {
                case FacebookActionStatus.Completed:
					IsLoggedIn = true;
					OnLoadDataCommand.Execute(null);
                    break;
                case FacebookActionStatus.Canceled:
                
                    break;
                case FacebookActionStatus.Unauthorized:
                    await App.Current.MainPage.DisplayAlert("Unauthorized", response.Message, "Ok");
                    break;
                case FacebookActionStatus.Error:
                    await App.Current.MainPage.DisplayAlert("Error", response.Message,"Ok");
                    break;
            }          }

		async Task ShareDataAsync()
        {
            FacebookShareLinkContent linkContent = new FacebookShareLinkContent("Awesome team of developers, making the world a better place one project or plugin at the time!",
                                                                                new Uri("http://www.github.com/crossgeeks"));
            var ret = await CrossFacebookClient.Current.ShareAsync(linkContent);
        }

        public async Task LoadData()
        {

            var jsonData = await CrossFacebookClient.Current.RequestUserDataAsync
            (
                  new string[] { "id", "name", "email", "picture", "cover", "friends" }, new string[] { }
            );

            var data = JObject.Parse(jsonData.Data);
            Profile = new FacebookProfile()
            {
                FullName = data["name"].ToString(),
                Picture = new UriImageSource { Uri = new Uri($"{data["picture"]["data"]["url"]}") },
				Email = data["email"].ToString()
            };



            // await LoadPosts();
        }


        /* Under Testing */

        //public async Task LoadPosts()
        //{
        //    if (IsBusy)
        //        return;

        //    IsBusy = true;

        //    Posts.Clear();
        //    FacebookResponse<string> post = await CrossFacebookClient.Current.QueryDataAsync("me/feed", new string[] { "user_posts" });
        //    var jo = JObject.Parse(post.Data);

        //    if(jo.ContainsKey("data"))
        //    {
        //        var array = ((JArray)jo["data"]);
        //        foreach(var item in array)
        //        {
        //            var postData = new FacebookPost();

        //            if (item["message"] != null)
        //            {
        //                postData.Message = $"{item["message"]}";
        //            }

        //            if (item["story"] != null)
        //            {
        //                postData.Story = $"{item["story"]}";
        //            }

        //            Posts.Add(postData);
        //        }
        //    }

        //    IsBusy = false;
        //}

    }
}
