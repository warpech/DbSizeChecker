using System;
using Starcounter;
using System.Threading;
using System.Diagnostics;

namespace Benchmark {

    public class Program {
        TransferGenerator gen;
        Datahistory datahistory;

        public static void Main() {
            var p = new Program();
            p.datahistory = new Datahistory();
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

            string output;
            while ((output = process.StandardOutput.ReadLine()) != null) {
                if (output.StartsWith(" storage_memory.used_pages")) {
                    Int64 usedPages = Convert.ToInt64(output.Split(delimiterChars)[1]);



                    DbSession dbs = new DbSession();
                    dbs.RunAsync(() => {
                        Int64 countObjects = gen.CountObjects();
                        datahistory.Add(countObjects, usedPages);

                        gen.CreateData();
                        ObservePerfmon();
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









