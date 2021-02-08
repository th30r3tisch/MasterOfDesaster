using CsvHelper;
using Game_Server.EA.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Game_Server.writer.EA_1 {
    class EA_1_Writer: StatsWriter<Individual_Simple> {

        public EA_1_Writer(string filename) : base(filename) {
            PrepareFile();
        }

        public override void WriteStats(List<Individual_Simple> records) {
            Individual_Simple longestRecord = PrepareStats(records);
            using (var stream = File.Open(_path, FileMode.Append))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, _config)) {
                //_csv.WriteField(longestRecord.timeStamps);
                foreach (var record in records) {
                    csv.NextRecord();
                    csv.WriteField(record.name);
                    csv.WriteField(record.startPos);
                    csv.WriteField(record.won);
                    csv.WriteField(record.fitness);
                    csv.WriteField(record.townLifeSum);
                    csv.WriteField(record.score);
                    csv.WriteField(record.timestamp.Last());
                    csv.WriteField(record.gene.properties["initialConquerRadius"]);
                    csv.WriteField(record.gene.properties["maxConquerRadius"]);
                    csv.WriteField(record.gene.properties["radiusExpansionStep"]);
                    csv.WriteField(record.gene.properties["attackMinLife"]);
                    //_csv.WriteField(record.townDevelopment);
                }
                writer.Flush();
            }
        }

        private Individual_Simple PrepareStats(List<Individual_Simple> records) {
            Individual_Simple longestRecord;
            longestRecord = GetLongestRecord(records, 0);
            return longestRecord;
        }

        /// <summary>
        /// Gets the longest record
        /// </summary>
        /// <param name="records">All records</param>
        /// <param name="length">Min length of the records</param>
        /// <returns>The longest record</returns>
        private Individual_Simple GetLongestRecord(List<Individual_Simple> records, int length) {
            Individual_Simple rec = null;
            foreach (var record in records) {
                if (record.timestamp.Count > length) {
                    length = record.timestamp.Count;
                    rec = record;
                }
            }
            return rec;
        }

        /// <summary>
        /// Writes the headlines to the file. Needs the same sequence than the WriteStats method
        /// </summary>
        private void PrepareFile() {
            using (var writer = new StreamWriter(_path))
            using (var csv = new CsvWriter(writer, _config)) {
                csv.WriteField("Name");
                csv.WriteField("Coord");
                csv.WriteField("Winner");
                csv.WriteField("Fitness");
                csv.WriteField("TownLifeSum");
                csv.WriteField("Score");
                csv.WriteField("GameTime");
                csv.WriteField("InitialConquerRadius");
                csv.WriteField("MaxConquerRadius");
                csv.WriteField("RadiusExpansionStep");
                csv.WriteField("AttackMinLife");
                writer.Flush();
            }
        }
    }
}
