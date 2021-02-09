using CsvHelper;
using Game_Server.EA.Models.Advanced;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Game_Server.writer.EA_2 {
    class EA_2_Writer: StatsWriter<Individual_Advanced> {

        public EA_2_Writer(string filename) : base(filename) {
            PrepareFile(Enum.GetNames(typeof(PropertyNames_Advanced)));
        }

        public override void WriteStats(List<Individual_Advanced> records) {
            using (var stream = File.Open(_path, FileMode.Append))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, _config)) {
                foreach (var record in records) {
                    csv.NextRecord();
                    csv.WriteField(record.name);
                    csv.WriteField(record.startPos);
                    csv.WriteField(record.won);
                    csv.WriteField(record.fitness);
                    csv.WriteField(record.score);
                    csv.WriteField(record.timestamp.Last());
                    //foreach (string key in record.gene.properties.Keys.ToList()) {
                    //    csv.WriteField(key);
                    //}
                }
                writer.Flush();
            }
        }
    }
}
