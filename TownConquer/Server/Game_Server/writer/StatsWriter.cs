using CsvHelper;
using Game_Server.writer;
using System.Globalization;
using System.IO;

namespace Game_Server {
    class StatsWriter {
        private StreamWriter _writer;
        private CsvWriter _csv;

        public StatsWriter(string filename) {
            string path = $"..\\..\\..\\..\\Statistics\\venv\\Scripts\\Stats\\Data\\{filename}.csv";
            _writer = new StreamWriter(path);
            _csv = new CsvWriter(_writer, CultureInfo.InvariantCulture);
            _csv.Configuration.Delimiter = ";";
        }

        public void WriteStats(EA_1_Stat[] records) {
            EA_1_Stat longestRecord = PrepareStats(records);
            using (_writer)
            using (_csv) {
                _csv.WriteField("Name");
                _csv.WriteField("Coord");
                _csv.WriteField("Winner");
                _csv.WriteField(longestRecord.timeStamps);
                _csv.NextRecord();
                foreach (var record in records) {
                    _csv.WriteField(record.name);
                    _csv.WriteField(record.startPos);
                    _csv.WriteField(record.won);
                    _csv.WriteField(record.townDevelopment);
                    _csv.NextRecord();
                }
                _writer.Flush();
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
            using (_writer)
            using (_csv) {
                foreach (var record in records) {
                    _csv.WriteField(record.name);
                    _csv.WriteField(record.entries);
                    _csv.NextRecord();
                }
                _writer.Flush();
            }
        }
        #endregion
    }
}
