using CsvHelper;
using CsvHelper.Configuration;
using Game_Server.EA.Models;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Game_Server {
    abstract class StatsWriter<T> where T : IIndividual {
        protected CsvConfiguration _config;
        protected string _path;

        public StatsWriter(string filename) {
            _path = $"..\\..\\..\\..\\Statistics\\venv\\Scripts\\Stats\\Data\\{filename}.csv";
            _config = new CsvConfiguration(CultureInfo.InvariantCulture) {
                Delimiter = ";"
            };
        }

        public abstract void WriteStats(List<T> records);

        /// <summary>
        /// Writes the headlines to the file. Needs the same sequence than the WriteStats method
        /// </summary>
        protected void PrepareFile() {
            using (var writer = new StreamWriter(_path))
            using (var csv = new CsvWriter(writer, _config)) {
                csv.WriteField("Name");
                csv.WriteField("Coord");
                csv.WriteField("Winner");
                csv.WriteField("Fitness");
                csv.WriteField("GameTime");
                csv.WriteField("Score");
                csv.WriteField(""); //seperator at the end of the line
            }
        }

        protected void AddGeneColumns(string[] propertynames) {
            using (var stream = File.Open(_path, FileMode.Append))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, _config)) {
                foreach (string property in propertynames) {
                    csv.WriteField(property);
                }
            }
        }
    }
}
