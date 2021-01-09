using sdmkaryawan.Services;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing;

namespace sdmkaryawan
{
    public partial class App : Application
    {
        public static int screenHeight, screenWidth;
        bool hasKey;

        public App()
        {

            InitializeComponent();
            Preferences.Set("versi", "22109");

            //MainPage = new asdasdLoginPage();

            hasKey = Preferences.ContainsKey("nik");
            if (!hasKey)
            {
                MainPage = new NavigationPage(new LoginPage());
            }
            else
            {
                //MainPage = new Page1();
                MainPage = new NavigationPage(new MainPage());
            }




            //cekVersi();
        }

        //public async void cekVersi()
        //{
        //    String versiServer = await karService.GetVersiPresensi();
        //    //String versiServer = "1.3";

        //    if (versiServer == versi)
        //    {
        //        bool hasKey = Preferences.ContainsKey("nik");
        //        if (!hasKey)
        //        {
        //            MainPage = new NavigationPage(new LoginPage());
        //        }
        //        else
        //        {
        //            //MainPage = new Page1();
        //            MainPage = new NavigationPage(new MainPage());
        //        }
        //    }
        //    else
        //    {
        //        bool hasKey = Preferences.ContainsKey("nik");
        //        if (!hasKey)
        //        {
        //            MainPage = new NavigationPage(new LoginPage());
        //        }
        //        else
        //        {
        //            //MainPage = new Page1();
        //            MainPage = new NavigationPage(new MainPage());

        //        }
        //    }
        //}


        protected override void OnStart()
        {

        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
            //Preferences.Set("onresume", "1");
            //if (!hasKey)
            //{
            //    MainPage = new NavigationPage(new LoginPage());
            //}
            //else
            //{
            //    //MainPage = new Page1();
            //    MainPage = new NavigationPage(new MainPage());
            //}
        }
    }
}
