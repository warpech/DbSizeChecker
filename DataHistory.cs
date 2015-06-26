using System;
using System.Collections.Generic;

namespace VolumeChecker {
    public class DataHistory {
        public List<DataMeasurement> list = new List<DataMeasurement> { };

        public void Add(Int64 count, Int64 size) {
            list.Add(new DataMeasurement() {
                ObjectCount = count,
                DataSize = size
            });
        }

        public string ToCSV() {
            var str = "";
            foreach (DataMeasurement datapoint in list) {
                str += datapoint.ObjectCount + "," + datapoint.DataSize + "\n";
            }
            return str;
        }
    }
}
