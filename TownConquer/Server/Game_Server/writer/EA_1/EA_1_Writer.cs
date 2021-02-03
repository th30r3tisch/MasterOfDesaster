using CsvHelper;
using System.IO;

namespace Game_Server.writer.EA_1 {
    class EA_1_Writer: StatsWriter<EA_1_Stat> {

        public EA_1_Writer(string filename) : base(filename) {
            PrepareFile();
        }

        public override void WriteStats(EA_1_Stat[] records) {
            EA_1_Stat longestRecord = PrepareStats(records);
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
                    csv.WriteField(record.gameTime);
                    csv.WriteField(record.properties["initialConquerRadius"]);
                    csv.WriteField(record.properties["maxConquerRadius"]);
                    csv.WriteField(record.properties["radiusExpansionStep"]);
                    csv.WriteField(record.properties["attackMinLife"]);
                    //_csv.WriteField(record.townDevelopment);
                }
                writer.Flush();
            }
        }

        private EA_1_Stat PrepareStats(EA_1_Stat[] records) {
            EA_1_Stat longestRecord;
            longestRecord = GetLongestRecord(records, 0);
            return longestRecord;
        }

        /// <summary>
        /// Gets the longest record
        /// </summary>
        /// <param name="records">All records</param>
        /// <param name="length">Min length of the records</param>
        /// <returns>The longest record</returns>
        private EA_1_Stat GetLongestRecord(EA_1_Stat[] records, int length) {
            EA_1_Stat rec = null;
            foreach (var record in records) {
                if (record.timeStamps.Count > length) {
                    length = record.timeStamps.Count;
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
