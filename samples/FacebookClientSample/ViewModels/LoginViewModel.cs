using Plugin.FacebookClient;
using Plugin.FacebookClient.Abstractions;
using System;
using System.Collections.Generic;
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

        public event PropertyChangedEventHandler PropertyChanged;

        public Command LoginCommand { get; set; }

        public LoginViewModel()         {             LoginCommand = new Command(async()=> await LoginAsync());

         
        }

        public async Task LoginAsync()         {
            FacebookResponse<bool> response = await CrossFacebookClient.Current.LoginAsync(permisions);             switch(response.Status)
            {
                case FacebookActionStatus.Completed:
                    await App.Current.MainPage.Navigation.PushAsync(new MyProfilePage());
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


    }
}
