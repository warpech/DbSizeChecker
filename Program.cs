using System;
using Starcounter;
using System.Threading;
using System.Diagnostics;

namespace VolumeChecker {

    public class Program {
        PaypalGenerator gen;
        DataHistory datahistory;

        public static void Main() {
            Console.WriteLine("Starting...");

            var p = new Program();
            p.datahistory = new DataHistory();
            //p.gen = new LoyaltyGenerator();
            p.gen = new PaypalGenerator();
            p.ObservePerfmon();

            Handle.GET("/DbSizeChecker/datahistory", () => {
                var str = p.datahistory.ToCSV();
                return str;
            });
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

            process.Start();

            string output;
            while ((output = process.StandardOutput.ReadLine()) != null) {
                if (output.StartsWith(" storage_memory.used_pages")) {
                    Int64 dataSizeKB = Convert.ToInt64(output.Split(delimiterChars)[1]);

                    DbSession dbs = new DbSession();
                    dbs.RunAsync(() => {
                        Int64 countObjects = gen.CountObjects();
                        datahistory.Add(countObjects, dataSizeKB);

                        if (dataSizeKB < 1000000) { //don't grow above 1 GB
                            Console.WriteLine("Continuing...");
                            gen.CreateData();
                            ObservePerfmon();
                        }
                        else {
                            Console.WriteLine("Top size achieved");
                        }
                    });

                    break;

                }
            }

            process.Kill();

            return;
        }

        char[] delimiterChars = { '=' };
    }
}









