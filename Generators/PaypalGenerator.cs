using System;
using Starcounter;
using System.IO;

/*
PayPal-like transactions, with an description, amount, currency, 2 participants, 3 status changes
There are X PayPal transactions a day
 */
namespace VolumeChecker {
    [Database]
    public class Account {
        public DateTime CreatedAt;
        public long AccountNumber;
    }

    [Database]
    public class Currency {
        public string Code;
    }

    [Database]
    public class Transfer {
        public Account FromAccount;
        public Account ToAccount;
        public Int64 Amount;
        public Currency Currency;
        public string Description;
    }

    [Database]
    public class Status {
        public string Name;
    }

    [Database]
    public class TransferStatus {
        public DateTime CreatedAt;
        public Transfer Transfer;
        public Status Status;
    }

    public class PaypalGenerator {
        Int32 accountsCount;
        private static Random rand = new Random();

        private Status TransactionCreatedStatus;
        private Status TransactionStartedStatus;
        private Status TransactionFinishedStatus;

        public PaypalGenerator() {
            var status = Db.SQL<Status>("SELECT c FROM VolumeChecker.Status c FETCH ?", 1).First;
            if (status == null) {
                CreateStatuses();
            }
            TransactionCreatedStatus = Db.SQL<Status>("SELECT c FROM VolumeChecker.Status c WHERE Name LIKE ?", "CREATED").First;
            TransactionStartedStatus = Db.SQL<Status>("SELECT c FROM VolumeChecker.Status c WHERE Name LIKE ?", "STARTED").First;
            TransactionFinishedStatus = Db.SQL<Status>("SELECT c FROM VolumeChecker.Status c WHERE Name LIKE ?", "FINISHED").First;

            var currency = Db.SQL<Currency>("SELECT c FROM VolumeChecker.Currency c FETCH ?", 1).First;
            if (currency == null) {
                CreateCurrencies();
            }

            var account = Db.SQL<Account>("SELECT c FROM VolumeChecker.Account c FETCH ?", 1).First;
            if (account == null) {
                CreateAccounts();
            }
            accountsCount = (Int32)Db.SlowSQL<Int64>("SELECT COUNT(*) FROM VolumeChecker.Account c").First;
        }

        public void CreateStatuses() {
            Db.Transact(() => {
                new Status() {
                    Name = "CREATED"
                };
                new Status() {
                    Name = "STARTED"
                };
                new Status() {
                    Name = "FINISHED"
                };
            });
        }

        public void CreateCurrencies() {
            Db.Transact(() => {
                new Currency() {
                    Code = "SEK"
                };
                });
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
            Int32 index = rand.Next(0, accountsCount - 1);
            var account = Db.SQL<Account>("SELECT c FROM VolumeChecker.Account c FETCH ? OFFSET ?", 1, index).First;
            return account;
        }

        public Int64 CountObjects() {
            Int64 obj = Db.SlowSQL<Int64>("SELECT COUNT(*) FROM VolumeChecker.Transfer c").First;
            return Convert.ToInt64(obj);
        }

        public void CreateData() {
            var max = 10000;
            Currency currency = Db.SQL<Currency>("SELECT c FROM VolumeChecker.Currency c FETCH ?", 1).First;
            for (var i = 0; i < max; i++) {
                Db.Transact(() => {
                    var transfer = new Transfer() {
                        FromAccount = GetRandomAccount(),
                        ToAccount = GetRandomAccount(),
                        Amount = GetRandomUint32(),
                        Currency = currency,
                        Description = GetRandomString(10) + " " + GetRandomString(10) + " " + GetRandomString(10)
                    };

                    new TransferStatus() {
                        CreatedAt = DateTime.Now,
                        Transfer = transfer,
                        Status = TransactionCreatedStatus
                    };

                    new TransferStatus() {
                        CreatedAt = DateTime.Now,
                        Transfer = transfer,
                        Status = TransactionStartedStatus
                    };

                    new TransferStatus() {
                        CreatedAt = DateTime.Now,
                        Transfer = transfer,
                        Status = TransactionFinishedStatus
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
