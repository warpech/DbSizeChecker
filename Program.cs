
using System;
using Starcounter;
using System.Threading;
using System.Diagnostics;

namespace Benchmark {

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

    public class Program {
        TransferGenerator gen;

        public static void Main() {
            var p = new Program();
            //p.gen = new Generator();
            p.gen = new TransferGenerator();
            p.ObservePerfmon();
        }

        public void ObservePerfmon() {
            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "C:\\StarcounterProjects\\perfmon.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    Arguments = "PERSONAL_DEFAULT"
                }
            };



            process.OutputDataReceived += (sender, args) => {
                ReadStats(process, args.Data);
            };

            process.Start();
            process.BeginOutputReadLine();

            process.WaitForExit(); //you need this in order to flush the output buffer
        }

        char[] delimiterChars = { '=' };

        Int64 lastUsedPages = 0;

        void SendReport(string category, Int64 objectsCount, Int64 dbSize) {
            Http.POST("http://localhost:8282/datapoint/" + category + "/" + objectsCount + "/" + dbSize, (string)null, null);
        }

        void ReadStats(Process process, string output) {
            if (output != null && output.StartsWith(" storage_memory.used_pages")) {
                Int64 usedPages = Convert.ToInt64(output.Split(delimiterChars)[1]);
                if (usedPages != lastUsedPages) {
                    lastUsedPages = usedPages;
                    DbSession dbs = new DbSession();
                    dbs.RunAsync(() => {
                        process.Kill();

                        Int64 countObjects = gen.CountObjects();
                        SendReport("paypal", countObjects, usedPages);
                         
                        gen.CreateData();
                        ObservePerfmon();
                    });
                }
                Console.WriteLine(output);
            }

        }

    }
}
