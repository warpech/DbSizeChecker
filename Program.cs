using System;
using Starcounter;
using System.Threading;
using System.Diagnostics;

namespace VolumeChecker {

    public class Program {
        TransferGenerator gen;
        DataHistory datahistory;

        public static void Main() {
            var p = new Program();
            p.datahistory = new DataHistory();
            //p.gen = new Generator();
            p.gen = new TransferGenerator();
            p.ObservePerfmon();

            Handle.GET("/benchmark/datahistory", () => {
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

            var firstCheck = true;

            string output;
            while ((output = process.StandardOutput.ReadLine()) != null) {
                if (output.StartsWith(" storage_memory.used_pages")) {
                    Int64 dataSizeKB = Convert.ToInt64(output.Split(delimiterChars)[1]);

                    if(firstCheck) {
                        if (dataSizeKB > 50000) {
                            throw new Exception("Database is not empty. Create a new empty database");
                        }
                        firstCheck = false;
                    }


                    DbSession dbs = new DbSession();
                    dbs.RunAsync(() => {
                        Int64 countObjects = gen.CountObjects();
                        datahistory.Add(countObjects, dataSizeKB);

                        gen.CreateData();

                        if (dataSizeKB > 1000000) { //don't grow above 1 GB
                            ObservePerfmon();
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









