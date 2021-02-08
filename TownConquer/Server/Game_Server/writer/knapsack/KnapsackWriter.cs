using CsvHelper;
using System.IO;

namespace Game_Server.writer.knapsack {
    class KnapsackWriter: StatsWriter {

        public KnapsackWriter(string filename) : base(filename) { }

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
    }
}
