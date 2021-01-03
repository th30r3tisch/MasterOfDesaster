using System.Globalization;
using System.IO;
using CsvHelper;
using Game_Server.writer;

namespace Game_Server {
    class StatsWriter {
        StreamWriter writer;
        CsvWriter csv;

        public StatsWriter(string filename) {
            string path = $"..\\..\\..\\..\\Statistics\\venv\\Scripts\\Stats\\{filename}.csv";
            writer = new StreamWriter(path);
            csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.Configuration.Delimiter = ";";
        }

        public void WriteStats(EA_1_Stat[] records) {
            EA_1_Stat longestRecord = PrepareStats(records);
            using (writer)
            using (csv) {
                csv.WriteField("Name");
                csv.WriteField("Coord");
                csv.WriteField(longestRecord.timeStamps);
                csv.NextRecord();
                foreach (var record in records) {
                    csv.WriteField(record.name);
                    csv.WriteField(record.startPos);
                    csv.WriteField(record.townDevelopment);
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

        #region KnapsackRegion
        public void WriteStats(KnapsackStat[] records) {
            using (writer)
            using (csv) {
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
