using System;
using Starcounter;
using System.IO;

namespace VolumeChecker {
    [Database]
    public class Country {
        public string Name;
    }

    [Database]
    public class WhatsAppUser {
        public DateTime CreatedAt;
        public string UserName;
        public Country Country;
        public Int64 PhoneNumber;
    }

    public class Generator {
        Int32 countriesCount;

        public Generator() {
            var country = Db.SQL<Country>("SELECT c FROM VolumeChecker.Country c FETCH ?", 1).First;
            if (country == null) {
                CreateCountries();
            }
            countriesCount = (Int32)Db.SlowSQL<Int64>("SELECT COUNT(*) FROM VolumeChecker.Country c").First;
        }

        public void CreateCountries() {
            string line;
            string path = "C:\\PolyjuiceProjects\\warpech\\Benchmark\\countries.txt";
            System.IO.StreamReader file = new System.IO.StreamReader(path);
            while ((line = file.ReadLine()) != null) {
                Db.Transact(() => {
                    new Country() {
                        Name = line
                    };
                });
            }
            file.Close();
        }

        public Country GetRandomCountry() {
            var r = new Random();
            Int32 index = r.Next(0, countriesCount - 1);
            var country = Db.SQL<Country>("SELECT c FROM VolumeChecker.Country c FETCH ? OFFSET ?", 1, index).First;
            return country;
        }

        public Int64 CountObjects() {
            UInt64 obj = Db.SQL<UInt64>("SELECT ObjectNo FROM VolumeChecker.WhatsAppUser c ORDER BY ObjectNo DESC FETCH ?", 1).First;
            return Convert.ToInt64(obj);
        }

        public void CreateData() {
            var max = 10000;
            for (var i = 0; i < max; i++) {
                Db.Transact(() => {
                    new WhatsAppUser() {
                        Country = GetRandomCountry(),
                        CreatedAt = DateTime.Now,
                        UserName = GetRandomString(8),
                        PhoneNumber = GetRandomNumber()
                    };
                });
            }
        }

        string GetRandomString(int len) {
            string path = Path.GetRandomFileName();
            path = path.Replace(".", ""); // Remove period.
            return path.Substring(0, len);  // Return x character string
        }

        Int64 GetRandomNumber() {
            Random random = new Random();
            string r = "";
            int i;
            for (i = 1; i < 11; i++) {
                r += random.Next(0, 9).ToString();
            }
            return Convert.ToInt64(r);
        }
    }
}
