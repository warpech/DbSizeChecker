using System;
using Starcounter;
using System.IO;

namespace Benchmark {
    [Database]
    public class Account {
        public DateTime CreatedAt;
        public long AccountNumber;
    }

    [Database]
    public class Transfer {
        public DateTime CreatedAt;
        public DateTime FinishedAt;
        public Account FromAccount;
        public Account ToAccount;
        public Int64 Amount;
        public string Description;
    }

    public class TransferGenerator {
        Int32 accountsCount;
        private static Random rand = new Random();

        public TransferGenerator() {
            var account = Db.SQL<Account>("SELECT c FROM Benchmark.Account c FETCH ?", 1).First;
            if (account == null) {
                CreateAccounts();
            }
            accountsCount = (Int32)Db.SlowSQL<Int64>("SELECT COUNT(*) FROM Benchmark.Account c").First;
        }

        public void CreateAccounts() {
            var max = 1000;
            for (var i = 0; i < max; i++) {
                Db.Transact(() => {
                    new Account() {
                        CreatedAt = DateTime.Now,
                        AccountNumber = GetRandomNumber()
                    };
                });
            }
        }

        public Account GetRandomAccount() {
            var r = new Random();
            Int32 index = r.Next(0, accountsCount - 1);
            var account = Db.SQL<Account>("SELECT c FROM Benchmark.Account c FETCH ? OFFSET ?", 1, index).First;
            return account;
        }

        public Int64 CountObjects() {
            Int64 obj = Db.SlowSQL<Int64>("SELECT COUNT(*) FROM Benchmark.Transfer c").First;
            return Convert.ToInt64(obj);
        }

        public void CreateData() {
            var max = 10000;
            for (var i = 0; i < max; i++) {
                Db.Transact(() => {
                    new Transfer() {
                        CreatedAt = DateTime.Now,
                        FromAccount = GetRandomAccount(),
                        ToAccount = GetRandomAccount(),
                        Amount = GetRandomUint32(),
                        Description = GetRandomString(10) + " " + GetRandomString(10) + " " + GetRandomString(10)
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
            string r = "";
            int i;
            for (i = 1; i < 11; i++) {
                r += rand.Next(0, 9).ToString();
            }
            return Convert.ToInt64(r);
        }

        UInt32 GetRandomUint32() {
            return (UInt32)(rand.Next(1 << 30)) << 2 | (uint)(rand.Next(1 << 2));
        }
    }
}
