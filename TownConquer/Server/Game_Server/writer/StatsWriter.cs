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
            csv.Configuration.HasHeaderRecord = true;
            csv.Configuration.AutoMap<StatEntry>();
        }

        public void WriteStats(StatEntry[] records) {
            using (writer)
            using (csv) {
                csv.WriteHeader<StatEntry>();
                csv.WriteRecords(records);
                writer.Flush();
            }
        }

    }
}
