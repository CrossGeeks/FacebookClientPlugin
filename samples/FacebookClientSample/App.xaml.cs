using Xamarin.Forms;

namespace FacebookClientSample
{
    public partial class App : Application
    {
		public static INavigation Navigation { get; set; }

        public App()
        {
            InitializeComponent();
            var navPage = new NavigationPage(new LoginPage());
            Navigation = navPage.Navigation;
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
