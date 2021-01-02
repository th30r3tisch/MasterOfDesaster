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

        public void WriteStats(StatEntry[] records) {
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

    }
}
