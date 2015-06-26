using System;
using Starcounter;
using System.Collections.Generic;

namespace Benchmark {
    public class Datapoint {
        public Int64 Int1;
        public Int64 Int2;
    }

    public class Datahistory {
        public List<Datapoint> list = new List<Datapoint>{};

        public void Add(Int64 int1, Int64 int2) {
            list.Add(new Datapoint() {
                        Int1 = int1,
                        Int2 = int2
                    });
        }

        public string ToCSV() {
            var str = "";
            foreach (Datapoint datapoint in list) {
                str += datapoint.Int1 + "," + datapoint.Int2 + "\n";
            }
            return str;
        }

        public Datahistory() {
            
        }
    }
}
