using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using sdmkaryawan.Models;
using sdmkaryawan.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Mobile;

namespace sdmkaryawan
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        private KaryawanService karService;

        public LoginPage()
        {
            InitializeComponent();
            txtVersi.Text = "Versi 1." + Preferences.Get("versi", "");
            karService = new KaryawanService();
            cekVersi();
            CekPermission();
        }


        private async void cekVersi()
        {
            try
            {
                var current = Connectivity.NetworkAccess;
                if (current != NetworkAccess.Internet ||
                    current == NetworkAccess.ConstrainedInternet)
                {
                    await DisplayAlert("Keterangan", "Tidak ada koneksi internet", "OK");
                }
                else
                {
                    popupLoadingView.IsVisible = true;
                    activityIndicator.IsRunning = true;

                    if (await karService.GetVersiPresensi())
                    {
                        popupLoadingView.IsVisible = false;
                        activityIndicator.IsRunning = false;
                    }
                    else
                    {
                        popupLoadingView.IsVisible = false;
                        activityIndicator.IsRunning = false;

                        await DisplayAlert("Perhatian", "Harap Perbarui Versi Aplikasi Anda", "Ok");
                        //System.Diagnostics.Process.GetCurrentProcess().Kill();

                        var supportsUri = await Launcher.CanOpenAsync("market://details?id=com.bethesda.sdmbethesdamobile");
                        await Launcher.TryOpenAsync("market://details?id=com.bethesda.sdmbethesdamobile");
                        System.Environment.Exit(0);

                    }
                }   
            }
            catch (Exception ex)
            {
                await DisplayAlert("Keterangan", "Kesalahan saat cek versi presensi. " + ex.Message, "Ok");
                popupLoadingView.IsVisible = false;
                activityIndicator.IsRunning = false;
            }
            
        }

        

        private async void btnLogin_Clicked(object sender, EventArgs e)
        {
            popupLoadingView.IsVisible = true;
            activityIndicator.IsRunning = true;

            if (txtUsername.Text != (null))
            {
                var current = Connectivity.NetworkAccess;
                if (current != NetworkAccess.Internet ||
                    current == NetworkAccess.ConstrainedInternet)
                {
                    await DisplayAlert("Keterangan", "Tidak ada koneksi internet", "OK");
                }
                else
                {
                    Karyawan results = await karService.GetLoginByNik(txtUsername.Text);
                    popupLoadingView.IsVisible = false;
                    activityIndicator.IsRunning = false;

                    if (results.password == txtPassword.Text)
                    {
 
                        string[] namaAwalan = (results.Nama).Split(' ');

                        bool answer = await DisplayAlert("Perhatian", "Setelah anda login, anda tidak diperkenankan untuk keluar/logout. Apakah anda yakin akan masuk sebagai " + namaAwalan[0] + " (" + results.Gugus + ") ?", "Ya, saya yakin", "Tidak");
                        if (answer)
                        {
                            

                            Preferences.Set("nik", txtUsername.Text);
                            Preferences.Set("name", namaAwalan[0]);
                            Preferences.Set("gugus", results.Gugus);
                            Navigation.InsertPageBefore(new MainPage(), this);
                            await Navigation.PopToRootAsync();
                            //App.Current.MainPage = new NavigationPage(new HalamanMenu());
                        }

                    }
                    else
                    {
                        await DisplayAlert("Peringatan", "Nik / Password Salah", "Ok");
                        popupLoadingView.IsVisible = false;
                        activityIndicator.IsRunning = false;
                    }
                }
            }
            else
            {
                await DisplayAlert("Peringatan", "NIK tidak boleh kosong", "Ok");
                popupLoadingView.IsVisible = false;
                activityIndicator.IsRunning = false;
            }



        }

        private void btnTanpaLogin_Clicked(object sender, EventArgs e)
        {

        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            txtUsername.Focused += InputFocused;
            txtPassword.Focused += InputFocused;
            txtUsername.Unfocused += InputUnfocused;
            txtPassword.Unfocused += InputUnfocused;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            txtUsername.Focused -= InputFocused;
            txtPassword.Focused -= InputFocused;
            txtUsername.Unfocused -= InputUnfocused;
            txtPassword.Unfocused -= InputUnfocused;
        }
        void InputFocused(object sender, EventArgs args)
        {
            Content.LayoutTo(new Rectangle(0, -160, Content.Bounds.Width, Content.Bounds.Height));
        }

        void InputUnfocused(object sender, EventArgs args)
        {
            Content.LayoutTo(new Rectangle(0, 0, Content.Bounds.Width, Content.Bounds.Height));
        }

        async void CekPermission()
        {
            var permissionStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Camera);
            var response = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Camera);
        }
    }


}