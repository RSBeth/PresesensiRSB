using sdmkaryawan.Models;
using sdmkaryawan.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace sdmkaryawan
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        Boolean statusMasuk = Preferences.Get("statusMasuk", false);
        private KaryawanService karService;
        private Boolean jamServerBerjalan = false;
        DateTime tglServer;
        private int refreshGps = 0;

        public MainPage()
        {
            InitializeComponent();
            karService = new KaryawanService();

            cekVersi();

            txtNama.Text = HurufPertamaBesar(Preferences.Get("name", "")) + " / " + Preferences.Get("gugus", "");
            txtJamKerjaEfektif.Text = Preferences.Get("jamKerjaEfektif", "-");

            DateTime ucapanSelamat = DateTime.Now;
            if (ucapanSelamat.Hour >= 5 && ucapanSelamat.Hour < 11)
            {
                txtUcapanSelamat.Text = "Selamat Pagi,";
            }
            else if (ucapanSelamat.Hour >= 11 && ucapanSelamat.Hour < 15)
            {
                txtUcapanSelamat.Text = "Selamat Siang,";
            }
            else if (ucapanSelamat.Hour >= 15 && ucapanSelamat.Hour < 18)
            {
                txtUcapanSelamat.Text = "Selamat Sore,";
            }
            else
            {
                txtUcapanSelamat.Text = "Selamat Malam,";
            }


            txtTanggalKerja.Text = DateTime.Now.ToString("ddd, dd MMM yyyy");


            btnDisable();
            getLocation();

            tampilkanJam();

            refreshView.IsRefreshing = false;

        }


        public async void tampilkanJam()
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
                    var results = await karService.GetCurrentdatetime();
                    tglServer = DateTime.ParseExact(results, "M/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

                    if (jamServerBerjalan == false)
                    {
                        jamServerBerjalan = true;
                        Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                        {
                            tglServer = tglServer.AddSeconds(1);
                            txtJamServer.Text = tglServer.ToString("T");
                            return true;
                        });
                    }
                    
                }   
            }
            catch
            {

            }
            
        }

        private void tambahJam()
        {
            
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
                popupLoadingView.IsVisible = false;
                activityIndicator.IsRunning = false;
                await DisplayAlert("Keterangan", "Kesalahan saat cek versi presensi. " + ex.Message, "Ok");
            }

        }



        private void btnCek_Clicked(object sender, EventArgs e)
        {
            getLocation();
        }

        public string Derypt(string text)
        {
            return Encoding.Unicode.GetString(Convert.FromBase64String(text));
        }

        private async Task scan(Boolean masuk)
        {
            try
            {
                var scanner = DependencyService.Get<IQrScanningService>();
                var result = await scanner.ScanAsync();
                result = Derypt(result);
                if (result != null)
                {
                    if (result != "")
                    {
                        string[] getTgl = result.Split('|');
                        if (getTgl[0] == DateTime.Now.ToString("dd-MM-yyyy"))
                        {
                            var results = await karService.GetCurrentdatetime();
                            DateTime tglServer = DateTime.ParseExact(results, "M/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                            //DateTime tglHp = Convert.ToDateTime(DateTime.Now);
                            DateTime tglBarcode = Convert.ToDateTime(getTgl[1]);

                            var resultss = tglServer.Subtract(tglBarcode).TotalMinutes;
                            //await DisplayAlert("Keterangan", resultss.ToString(), "Ok");

                            if (resultss < 5)
                            {
                                if (masuk)
                                {
                                    absenMasuk();
                                }
                                else
                                {
                                    absenKeluar();
                                }
                            }
                            else
                            {
                                await DisplayAlert("Keterangan", "Barcode tidak valid", "Ok");
                            }
                        }
                        else
                        {
                            await DisplayAlert("Keterangan123", "Barcode tidak valid", "Ok");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Keterangan", "Barcode tidak valid", "Ok");
                    }

                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Keterangan", "Error scan : " + ex.Message, "Ok");
            }
        }

        private void btnLogout_Clicked(object sender, EventArgs e)
        {
            Preferences.Remove("username");
            App.Current.MainPage = new NavigationPage(new LoginPage());
        }

        private async void getLocation()
        {

            btnDisable();
            activity.IsEnabled = true;
            activity.IsRunning = true;
            activity.IsVisible = true;
            imgGps.IsVisible = false;
            txtLokasi.Text = "Sedang mencari lokasi anda";

            try
            {
                var Latitude = 0.0;
                var Longitude = 0.0;
                var request = new GeolocationRequest(GeolocationAccuracy.Medium);

                refreshGps += 1;
                Location location = new Location();

                if (refreshGps % 2 == 0)
                {
                    location = await Geolocation.GetLastKnownLocationAsync();
                    if (location == null)
                    {
                        location = await Geolocation.GetLocationAsync(request);
                    }
                }
                else
                {
                    location = await Geolocation.GetLocationAsync(request);
                }

                //if (location == null)
                //{
                //    location = await Geolocation.GetLocationAsync(request);
                //}

                //if (refreshGps >= 2)
                //{
                //    location = await Geolocation.GetLocationAsync(request);
                //    refreshGps = 0;
                //}

                if (location != null)
                {
                    if (location.IsFromMockProvider)
                    {
                        txtLokasi.Text = "Anda menggunakan aplikasi fake GPS";
                        cariNamaAlamat(Latitude, Longitude);
                        txtLokasi.TextColor = Color.FromHex("#ff4c4c");
                        await DisplayAlert("Keterangan", "Anda menggunakan aplikasi fake GPS", "Ok");
                        System.Environment.Exit(0);
                        //btnDisable();
                    }
                    else
                    {
                        Latitude = location.Latitude;
                        Longitude = location.Longitude;

                        if (Latitude < -7.782246 && Latitude > -7.785398)
                        {
                            if (Longitude < 110.379714 && Longitude > 110.375284)
                            {
                                txtLokasi.Text = "Anda berada di RS. Bethesda";
                                //cariNamaAlamat(Latitude, Longitude);
                                txtAlamatLengkap.Text = "Jalan Jendral Sudirman, No. 70, Kecamatan Gondokusuman, Daerah Istimewa Yogyakarta";
                                txtLokasi.TextColor = Color.White;
                                //await DisplayAlert("Keterangan", "Anda berada di lingkungan RS. Bethesda", "Ok");

                                await GetPresensi5Terakhir(true);
                                


                            }
                            else
                            {
                                txtLokasi.Text = "Anda tidak berada di RS. Bethesda";
                                cariNamaAlamat(Latitude, Longitude);
                                txtLokasi.TextColor = Color.FromHex("#ff4c4c");
                                await DisplayAlert("Keterangan", "Anda tidak berada di lingkungan RS. Bethesda", "Ok");

                                await GetPresensi5Terakhir(false);
                                
                            }
                        }
                        else
                        {
                            txtLokasi.Text = "Anda tidak berada di RS. Bethesda";
                            cariNamaAlamat(Latitude, Longitude);
                            txtLokasi.TextColor = Color.FromHex("#ff4c4c");
                            await DisplayAlert("Keterangan", "Anda tidak berada di lingkungan RS. Bethesda", "Ok");

                            await GetPresensi5Terakhir(false);
                            
                        }
                        //await DisplayAlert("Keterangan", "Latitude = " + location.Latitude + " /// Longitude = " + location.Longitude, "Ok");
                    }
                }

                
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "GPS anda tidak menyala atau terdapat kesalahan pada GPS. Error : " + ex.Message , "Ok");
            }

            activity.IsEnabled = false;
            activity.IsRunning = false;
            activity.IsVisible = false;
            imgGps.IsVisible = true;
        }

        private async void cariNamaAlamat(Double Latitude, Double Longitude)
        {
            var alamat = txtLokasi.Text;
            try
            {
               
                var placemarks = await Geocoding.GetPlacemarksAsync(Latitude, Longitude);
                var placemark = placemarks?.FirstOrDefault();
                if (placemark != null)
                {
                    var geocodeAddress =
                        $"AdminArea:       {placemark.AdminArea}\n" +
                        $"CountryCode:     {placemark.CountryCode}\n" +
                        $"CountryName:     {placemark.CountryName}\n" +
                        $"FeatureName:     {placemark.FeatureName}\n" +
                        $"Locality:        {placemark.Locality}\n" +
                        $"PostalCode:      {placemark.PostalCode}\n" +
                        $"SubAdminArea:    {placemark.SubAdminArea}\n" +
                        $"SubLocality:     {placemark.SubLocality}\n" +
                        $"SubThoroughfare: {placemark.SubThoroughfare}\n" +
                        $"Thoroughfare:    {placemark.Thoroughfare}\n";

                    //Console.WriteLine(geocodeAddress);

                    alamat = $" { placemark.Thoroughfare }, No.{placemark.SubThoroughfare}, {placemark.Locality}, {placemark.AdminArea}";
                    txtAlamatLengkap.Text = alamat;


                    //await DisplayAlert("Keterangan", geocodeAddress, "Ok");

                }
            }
            catch (Exception ex)
            {
                //await DisplayAlert("test", ex.Message, "oke");
                txtAlamatLengkap.Text = alamat;
            }
            
        }

        private void btnMasukEnable()
        {
            btnMasuk.IsEnabled = true;
            btnKeluar.IsEnabled = false;
            btnReset.IsEnabled = false;
            btnMasuk.BackgroundColor = Color.FromHex("#24a0ed");
            btnKeluar.BackgroundColor = Color.FromHex("#cccccc");
            btnReset.BackgroundColor = Color.FromHex("#cccccc");
            btnMasuk.TextColor = Color.White;
            btnKeluar.TextColor = Color.Black;
            btnReset.TextColor = Color.Black;
        }

        private void btnKeluarEnable()
        {
            btnMasuk.IsEnabled = false;
            btnKeluar.IsEnabled = true;
            btnReset.IsEnabled = true;
            btnMasuk.BackgroundColor = Color.FromHex("#cccccc");
            btnKeluar.BackgroundColor = Color.FromHex("#24a0ed");
            btnReset.BackgroundColor = Color.FromHex("#CD5C5C");
            btnMasuk.TextColor = Color.Black;
            btnKeluar.TextColor = Color.White;
            btnReset.TextColor = Color.White;
        }
        
        private void btnDisable()
        {
            btnMasuk.IsEnabled = false;
            btnKeluar.IsEnabled = false;
            btnReset.IsEnabled = false;
            btnMasuk.BackgroundColor = Color.FromHex("#cccccc");
            btnKeluar.BackgroundColor = Color.FromHex("#cccccc");
            btnReset.BackgroundColor = Color.FromHex("#cccccc");
            btnMasuk.TextColor = Color.Black;
            btnKeluar.TextColor = Color.Black;
            btnReset.TextColor = Color.Black;
        }

        private void btnResetSajaEnable()
        {
            btnMasuk.IsEnabled = false;
            btnKeluar.IsEnabled = false;
            btnReset.IsEnabled = true;
            btnMasuk.BackgroundColor = Color.FromHex("#cccccc");
            btnKeluar.BackgroundColor = Color.FromHex("#cccccc");
            btnReset.BackgroundColor = Color.FromHex("#CD5C5C");
            btnMasuk.TextColor = Color.Black;
            btnKeluar.TextColor = Color.Black;
            btnReset.TextColor = Color.White;
        }

        private async void absenMasuk()
        {
            popupLoadingView.IsVisible = true;
            activityIndicator.IsRunning = true;

            string jamPilih = "-";
            //jamPilih = await DisplayActionSheet("Pilih Jam Masuk Anda", "Cancel", null, "07:30 - 14:30", "14:30 - 21:00", "21:00 - 07:00");
            //Preferences.Set("jamKerjaEfektif", jamPilih);
            //txtJamKerjaEfektif.Text = jamPilih;
            ////Console.WriteLine("Action: " + action);


            //DateTime jamMasuk = DateTime.Now;
            //if (jamMasuk.Hour >= 4 && jamMasuk.Hour <= 10)
            //{
            //    jamPilih = await DisplayActionSheet("Pilih Jam Masuk Anda", null, null, "05:00 - 12:00", "07:30 - 14:30", "08:00 - 15:00", "09:00 - 16:00", "10:00 - 17:00");
            //    Preferences.Set("jamKerjaEfektif", jamPilih);
            //    txtJamKerjaEfektif.Text = jamPilih;


            //    //Preferences.Set("jamKerjaEfektif", "07:30 - 14:30");
            //    //txtJamKerjaEfektif.Text = "07:30 - 14:30";
            //}
            //else if (jamMasuk.Hour >= 11 && jamMasuk.Hour <= 16)
            //{
            //    jamPilih = await DisplayActionSheet("Pilih Jam Masuk Anda", null, null, "10:00 - 17:00", "12:00 - 19:00", "14:00 - 21:00", "14:00 - 22:00");
            //    Preferences.Set("jamKerjaEfektif", jamPilih);
            //    txtJamKerjaEfektif.Text = jamPilih;
            //}
            //else if (jamMasuk.Hour >= 19 && jamMasuk.Hour < 23)
            //{
            //    jamPilih = await DisplayActionSheet("Pilih Jam Masuk Anda", null, null, "21:00 - 07:00", "22:00 - 07:00");
            //    Preferences.Set("jamKerjaEfektif", jamPilih);
            //    txtJamKerjaEfektif.Text = jamPilih;
            //}
            //else
            //{
            //    jamPilih = await DisplayActionSheet("Pilih Jam Masuk Anda", null, null, "05:00 - 12:00", "07:30 - 14:30", "08:00 - 15:00", "09:00 - 16:00", "10:00 - 17:00", "12:00 - 19:00", "14:00 - 21:00", "14:00 - 22:00", "21:00 - 07:00", "22:00 - 07:00");
            //    Preferences.Set("jamKerjaEfektif", jamPilih);
            //    txtJamKerjaEfektif.Text = jamPilih;
            //}




            var current = Connectivity.NetworkAccess;
            if (current != NetworkAccess.Internet ||
                current == NetworkAccess.ConstrainedInternet)
            {
                await DisplayAlert("Keterangan", "Tidak ada koneksi internet", "OK");
            }
            else
            {
                try
                {
                    await karService.AbsenMasuk(Preferences.Get("nik", ""));

                    popupLoadingView.IsVisible = false;
                    activityIndicator.IsRunning = false;

                    await DisplayAlert("Keterangan", "Anda telah berhasil Presensi Masuk", "Ok");
                    statusMasuk = true;
                    Preferences.Set("statusMasuk", true);
                    btnKeluarEnable();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", ex.Message, "OK");
                }
            }

            await GetPresensi5Terakhir(true);

            



            //btnMasuk.Text = "MASUK (" + getTgl[1] + ")";

        }

        private async void absenKeluar()
        {
            popupLoadingView.IsVisible = true;
            activityIndicator.IsRunning = true;

            var current = Connectivity.NetworkAccess;
            if (current != NetworkAccess.Internet ||
                current == NetworkAccess.ConstrainedInternet)
            {
                await DisplayAlert("Keterangan", "Tidak ada koneksi internet", "OK");
            }
            else
            {
                try
                {
                    await karService.AbsenKeluar(Preferences.Get("nik", ""));

                    popupLoadingView.IsVisible = false;
                    activityIndicator.IsRunning = false;

                    await DisplayAlert("Keterangan", "Anda telah berhasil Presensi Keluar", "Ok");
                    statusMasuk = false;
                    Preferences.Set("statusMasuk", false);
                    Preferences.Set("jamKerjaEfektif", "-");
                    txtJamKerjaEfektif.Text = "-";
                    btnMasukEnable();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error Presensi Keluar : ", ex.Message, "OK");
                }
            }
            await GetPresensi5Terakhir(true); 
        }

        private async void btnMasuk_Clicked(object sender, EventArgs e)
        {
            await scan(true);
            
        }

        private async void btnKeluar_Clicked(object sender, EventArgs e)
        {
            await scan(false);
        }

        private async void btnReset_Clicked(object sender, EventArgs e)
        {
            
            bool answer = await DisplayAlert("Apakah anda yakin ?", "Fungsi reset presensi digunakan jika anda lupa menekan tombol keluar pada jadwal sebelumnya.", "Ya, saya yakin", "Tidak");
            if (answer)
            {
                popupLoadingView.IsVisible = true;
                activityIndicator.IsRunning = true;

                try
                {
                    await karService.ResetAbsen(Preferences.Get("nik", ""));

                    popupLoadingView.IsVisible = false;
                    activityIndicator.IsRunning = false;

                    await DisplayAlert("Keterangan", "Anda telah berhasil Reset Presensi", "Ok");
                    statusMasuk = false;
                    Preferences.Set("statusMasuk", false);
                    Preferences.Set("jamKerjaEfektif", "-");
                    txtJamKerjaEfektif.Text = "-";
                    //btnMasukEnable();
                    getLocation();

                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", ex.Message, "OK");
                }

                btnMasukEnable();
                txtJamKerjaEfektif.Text = "-";
                statusMasuk = false;
            }
            //string action = await DisplayActionSheet("ActionSheet: Send to?", "Cancel", null, "Email", "Twitter", "Facebook");
        }

        private void refreshView_Refreshing(object sender, EventArgs e)
        {
            try
            {
                btnDisable();
                getLocation();
                tampilkanJam();
            }
            catch
            {
                DisplayAlert("Keterangan", "Error saat refresh halaman", "OK");
            }
            refreshView.IsRefreshing = false;   
        }

        public string HurufPertamaBesar(string str)
        {
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }


        private async Task GetPresensi5Terakhir(Boolean posisiDiBethesda)
        {
            try
            {
                popupLoadingView.IsVisible = true;
                activityIndicator.IsRunning = true;

                var current = Connectivity.NetworkAccess;
                if (current != NetworkAccess.Internet ||
                    current == NetworkAccess.ConstrainedInternet)
                {
                    await DisplayAlert("Keterangan", "Tidak ada koneksi internet", "OK");
                }
                else
                {
                    var results = await karService.Get5TerakhirByNik(Preferences.Get("nik", ""));
                    lvData.ItemsSource = results;

                    if (results.Count > 0)
                    {
                        if (results[0].Tgl_keluar == "-")
                        {
                            if (results[0].keterangan != "Reset")
                            {
                                statusMasuk = true;
                                Preferences.Set("statusMasuk", true);
                            }
                            else
                            {
                                statusMasuk = false;
                                Preferences.Set("statusMasuk", false);
                            }
                        };
                    }
                    else
                    {
                        statusMasuk = false;
                        Preferences.Set("statusMasuk", false);
                    }


                }

                popupLoadingView.IsVisible = false;
                activityIndicator.IsRunning = false;

                if (posisiDiBethesda)
                {
                    if (statusMasuk)
                    {
                        btnKeluarEnable();
                    }
                    else
                    {
                        btnMasukEnable();
                    }
                }
                else
                {
                    if (statusMasuk)
                    {
                        btnResetSajaEnable();
                    }
                    else
                    {
                        btnDisable();
                    }
                }
            }
            catch (Exception ex)
            {
                popupLoadingView.IsVisible = false;
                activityIndicator.IsRunning = false;
                await DisplayAlert("Keterangan", "Get data presensi error. Error : " + ex.Message, "Ok");
            }

        }


    }
}
