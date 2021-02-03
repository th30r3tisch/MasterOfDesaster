using CsvHelper.Configuration;
using Game_Server.writer;
using System.Globalization;

namespace Game_Server {
    abstract class StatsWriter<T> where T : StatEntry {
        protected CsvConfiguration _config;
        protected string _path;

        public StatsWriter(string filename) {
            _path = $"..\\..\\..\\..\\Statistics\\venv\\Scripts\\Stats\\Data\\{filename}.csv";
            _config = new CsvConfiguration(CultureInfo.InvariantCulture) {
                Delimiter = ";"
            };
        }

        public abstract void WriteStats(T[] records);
    }
}
