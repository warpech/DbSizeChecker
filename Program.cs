
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

    public class Program {
        Generator gen;

        public static void Main() {
            var p = new Program();
            p.gen = new Generator();
            p.ObservePerfmon();
        }

        public void ObservePerfmon() {
            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "C:\\StarcounterProjects\\perfmon.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    Arguments = "PERSONAL_DEFAULT"
                }
            };

            process.OutputDataReceived += (sender, args) => ReadStats(args.Data);
            process.ErrorDataReceived += (sender, args) => ReadStats(args.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit(); //you need this in order to flush the output buffer
        }

        char[] delimiterChars = { '=' };

        Int64 startCountObjects = 0;
        Int64 lastUsedPages = 0;
        Int64 startUsedPages = 0;

        void SendReport(string category, Int64 objectsCount, Int64 dbSize) {
            Http.POST("http://localhost:8282/datapoint/" + category + "/" + objectsCount + "/" + dbSize, (string) null, null);
        }

        void ReadStats(string output) {
            if (output.StartsWith(" storage_memory.used_pages")) {
                Int64 usedPages = Convert.ToInt64(output.Split(delimiterChars)[1]);
                if (usedPages != lastUsedPages) {
                    lastUsedPages = usedPages;
                    DbSession dbs = new DbSession();
                    dbs.RunAsync(() => {
                        Int64 countObjects = gen.CountObjects();
                        if (startUsedPages == 0) {
                            startUsedPages = usedPages;
                            startCountObjects = countObjects;
                        } else {
                            Console.WriteLine(usedPages);
                            SendReport("whatsapp", countObjects, usedPages);
                            //SendReport("whatsapp2", countObjects - startCountObjects, usedPages - startUsedPages);
                        }
                        gen.CreateData();
                    });
                }
            }
            Console.WriteLine(output);

        }

    }
}
