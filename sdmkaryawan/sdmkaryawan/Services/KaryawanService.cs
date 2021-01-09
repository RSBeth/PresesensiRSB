using Newtonsoft.Json;
using sdmkaryawan.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Essentials;
using ZXing;

namespace sdmkaryawan.Services
{
    public class KaryawanService
    {
        private string restUrl = "https://rs.bethesda.or.id:449/api";
        //private string restUrl = "http://180.214.244.190:8090/api";

        private HttpClient _client;

        public KaryawanService()
        {
            var usernamePassword = String.Format("RSByyk:Rsb2020", "username", "password");
            var convertKeBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(usernamePassword));

            _client = new HttpClient();
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", convertKeBase64);
        }

        public async Task<Karyawan> GetLoginByNik(String nik)
        {
            Karyawan lstEmp = new Karyawan();
            var uri = new Uri($"{restUrl}/GetNik/{nik}");
            try
            {
                var response = await _client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var content2 = JsonConvert.DeserializeObject<Karyawan>(content);
                    lstEmp = content2;
                    //lstEmp = JsonConvert.DeserializeObject<List<Karyawan>>(content);
                }
                else
                {
                    throw new Exception("Gagal mengakses api");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return lstEmp;
        }

        public async Task AbsenMasuk(String nik)
        {
            var uriPost = new Uri($"{restUrl}/presensiMasuk");
            try
            {
                //var jsonObj = JsonConvert.SerializeObject(kar);
                //var jsonObj = JsonConvert.SerializeObject(kar, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings
                //{
                //    NullValueHandling = NullValueHandling.Ignore
                //});

                //var jsonObj = new Dictionary<string, string>();
                //jsonObj.Add("nik", "2906");

      
                //var content = new StringContent(jsonObj,
                //    Encoding.UTF8, "application/json");

                //var response = await _client.PostAsync(uriPost, content);
                //if (!response.IsSuccessStatusCode)
                //    throw new Exception("Data gagal untuk ditambahkan");




                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("nik", nik));
                var req = new HttpRequestMessage(HttpMethod.Post, uriPost) { Content = new FormUrlEncodedContent(nvc) };
                var response = await _client.SendAsync(req);
                if (!response.IsSuccessStatusCode)
                    throw new Exception("Data gagal untuk ditambahkan");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task AbsenKeluar(String nik)
        {
            var uriPost = new Uri($"{restUrl}/presensiKeluar");
            try
            {
                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("nik", nik));
                var req = new HttpRequestMessage(HttpMethod.Post, uriPost) { Content = new FormUrlEncodedContent(nvc) };
                var response = await _client.SendAsync(req);
                if (!response.IsSuccessStatusCode)
                    throw new Exception("Data gagal untuk ditambahkan");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task ResetAbsen(String nik)
        {
            var uriPost = new Uri($"{restUrl}/resetpresensi");
            try
            {
                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("nik", nik));
                var req = new HttpRequestMessage(HttpMethod.Post, uriPost) { Content = new FormUrlEncodedContent(nvc) };
                var response = await _client.SendAsync(req);
                if (!response.IsSuccessStatusCode)
                    throw new Exception("Data gagal untuk ditambahkan");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Karyawan>> Get5TerakhirByNik(String nik)
        {
            List<Karyawan> lstEmp = new List<Karyawan>();
            var uri = new Uri($"{restUrl}/presensiNik/{nik}");
            try
            {
                var response = await _client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    lstEmp = JsonConvert.DeserializeObject<List<Karyawan>>(content);

                    foreach (var kary in lstEmp)
                    {
                        var formatLama = kary.Tgl_masuk;
                        var formatIndo = DateTime.ParseExact(formatLama, "M/dd/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                        kary.Tgl_masuk = formatIndo.ToString("dd/MM/yyyy HH:mm:ss");

                        if (kary.Tgl_keluar != "-")
                        {
                            formatLama = kary.Tgl_keluar;
                            formatIndo = DateTime.ParseExact(formatLama, "M/dd/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                            kary.Tgl_keluar = formatIndo.ToString("dd/MM/yyyy HH:mm:ss");
                        }
                        

                    };

                }
                else
                {
                    throw new Exception("Gagal mengakses api");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return lstEmp;
        }

        public async Task<Boolean> GetVersiPresensi()
        {
            Boolean kembalian = false;
            Karyawan lstEmp = new Karyawan();
            var uri = new Uri($"{restUrl}/GetVersiPresensi");
            try
            {
                var response = await _client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (int.Parse(Regex.Match(content, @"\d+").Value) <= int.Parse(Preferences.Get("versi", "")))
                    {
                        kembalian = true;
                    }
                    else
                    {
                        kembalian = false;
                    }
                   
                }
                else
                {
                    throw new Exception("Gagal mengakses api GetVersiPresensi");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return kembalian;
        }

        public async Task<String> GetCurrentdatetime()
        {
            String kembalian = "";
            Karyawan lstEmp = new Karyawan();
            var uri = new Uri($"{restUrl}/currentdatetime");
            try
            {
                var response = await _client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    content = content.Replace("\"", string.Empty);

                    kembalian = content;
                }
                else
                {
                    throw new Exception("Gagal mengakses api GetCurrentdatetime");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return kembalian;
        }

    }
}
