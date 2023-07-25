using Plugin.FirebasePushNotification;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PlayVideo
{
    public partial class App : Application
    {
        //public App()
        //{
        //    InitializeComponent();
        //    MainPage = new NavigationPage();
        //}
        public static string Data { get; set; }
        public App(bool hasNotification = false, IDictionary<string, object> notificationData = null)
        {
            InitializeComponent();

            if (!hasNotification)
                MainPage = new NavigationPage(new Page1());
            else
            {
                foreach (var data in notificationData)
                {
                    if (data.Key == "LoginPage")
                    {
                        MainPage = new ProgressHeader();
                        return;
                    }
                }
            }
            CrossFirebasePushNotification.Current.OnTokenRefresh += (s, p) =>
            {
                System.Diagnostics.Debug.WriteLine($"TOKEN : {p.Token}");
                Console.WriteLine("Token Is : " + p.Token);
            };
            CrossFirebasePushNotification.Current.OnNotificationReceived += (s, p) =>
            {

                System.Diagnostics.Debug.WriteLine("Received");
                foreach (var data in p.Data)
                {
                    System.Diagnostics.Debug.WriteLine($"{data.Key} : {data.Value}");
                }

            };
            CrossFirebasePushNotification.Current.OnNotificationOpened += (s, p) =>
            {
                System.Diagnostics.Debug.WriteLine("Opened");
                foreach (var data in p.Data)
                {
                    System.Diagnostics.Debug.WriteLine($"{data.Key} : {data.Value}");
                }


            };
        }
        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
