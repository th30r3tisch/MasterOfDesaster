using CsvHelper;
using CsvHelper.Configuration;
using Game_Server.writer;
using System.Globalization;
using System.IO;

namespace Game_Server {
    class StatsWriter {
        private CsvConfiguration _config;
        private string _path;

        public StatsWriter(string filename) {
            _path = $"..\\..\\..\\..\\Statistics\\venv\\Scripts\\Stats\\Data\\{filename}.csv";
            _config = new CsvConfiguration(CultureInfo.InvariantCulture) {
                Delimiter = ";"
            };
            PrepareFile();
        }

        public void WriteStats(EA_1_Stat[] records) {
            EA_1_Stat longestRecord = PrepareStats(records);
            using (var stream = File.Open(_path, FileMode.Append))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, _config)) {
                //_csv.WriteField(longestRecord.timeStamps);
                foreach (var record in records) {
                    csv.WriteField(record.name);
                    csv.WriteField(record.startPos);
                    csv.WriteField(record.won);
                    csv.WriteField(record.fitness);
                    csv.WriteField(record.townLifeSum);
                    csv.WriteField(record.score);
                    csv.WriteField(record.gameTime);
                    //_csv.WriteField(record.townDevelopment);
                    csv.NextRecord();
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
                writer.Flush();
            }
        }

        #region KnapsackRegion
        public void WriteStats(KnapsackStat[] records) {
            using (var writer = new StreamWriter(_path))
            using (var csv = new CsvWriter(writer, _config)) {
                foreach (var record in records) {
                    csv.WriteField(record.name);
                    csv.WriteField(record.entries);
                    csv.NextRecord();
                }
                writer.Flush();
            }
        }
        #endregion
    }
}
