using CsvHelper;
using Game_Server.EA.Models.Advanced;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Game_Server.writer.EA_2 {
    class EA_2_Writer: StatsWriter<Individual_Advanced> {

        public EA_2_Writer(string filename) : base(filename) {
            PrepareFile();
            AddEASpecificColumns(Enum.GetNames(typeof(PropertyNames_Advanced)));
            AddGeneColumns(Enum.GetNames(typeof(PropertyNames_General)));
        }

        private void AddEASpecificColumns(string[] propertynames) {
            using (var stream = File.Open(_path, FileMode.Append))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, _config)) {
                csv.WriteField("deffScore");
                csv.WriteField("suppScore");
                csv.WriteField("townLifeDeviation");
                csv.WriteField("supportActions");
                csv.WriteField("attackActions");
                csv.WriteField("dominanceLevel");
                foreach (string property in propertynames) {
                    csv.WriteField("deff-" + property);
                }
                foreach (string property in propertynames) {
                    csv.WriteField("off-" + property);
                }
                foreach (string property in propertynames) {
                    csv.WriteField("supp-" + property);
                }
                csv.WriteField(""); //seperator at the end of the line
            }
        }

        public override void WriteStats(List<Individual_Advanced> records) {
            int counter = 0;
            using (var stream = File.Open(_path, FileMode.Append))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, _config)) {
                foreach (var record in records) {
                    csv.NextRecord();
                    csv.WriteField("KI_" + counter);
                    csv.WriteField(record.startPos);
                    csv.WriteField(record.won);
                    csv.WriteField(record.fitness);
                    csv.WriteField(record.timestamp.Last());
                    csv.WriteField(record.atkScore);
                    csv.WriteField(record.deffScore);
                    csv.WriteField(record.suppScore);
                    csv.WriteField(record.townLifeDeviation);
                    csv.WriteField(record.supportActions);
                    csv.WriteField(record.attackActions);
                    csv.WriteField(record.dominanceLevel);
                    foreach (int value in record.gene.defensiveProperties.Values.ToList()) {
                        csv.WriteField(value);
                    }
                    foreach (int value in record.gene.attackProperties.Values.ToList()) {
                        csv.WriteField(value);
                    }
                    foreach (int value in record.gene.supportProperties.Values.ToList()) {
                        csv.WriteField(value);
                    }
                    foreach (int value in record.gene.generalProperties.Values.ToList()) {
                        csv.WriteField(value);
                    }
                    counter++;
                }
                writer.Flush();
            }
        }
    }
}
