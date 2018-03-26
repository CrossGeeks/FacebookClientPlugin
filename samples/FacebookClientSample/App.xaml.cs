using Xamarin.Forms;

namespace FacebookClientSample
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            var navPage = new NavigationPage(new LoginPage());
            MainPage = navPage;
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
