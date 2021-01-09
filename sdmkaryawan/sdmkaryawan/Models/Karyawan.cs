using System;
using System.Collections.Generic;
using System.Text;

namespace sdmkaryawan.Models
{
    public class Karyawan
    {
        public string NIK { get; set; }
        public string Nama { get; set; }
        public string password { get; set; }
        public string Gugus { get; set; }
        
        public string dt_jam_masuk { get; set; }
        public string dt_jam_pulang { get; set; }

        public string Tgl_masuk { get; set; }
        public string Tgl_keluar { get; set; }

        public string Id { get; set; }
        public string lstatus { get; set; }
        public string response { get; set; }
        public string deskripsiresponse { get; set; }
        public string lAktif { get; set; }
        public string keterangan { get; set; }
        

    }
}
