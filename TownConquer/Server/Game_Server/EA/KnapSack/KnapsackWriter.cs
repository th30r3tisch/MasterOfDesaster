using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Game_Server.EA.KnapSack {
    class KnapsackWriter {

        protected CsvConfiguration _config;
        protected string _path;

        /// <summary>
        /// responsible for writing stats into a csv file
        /// </summary>
        /// <param name="filename">name of the csv file where the data is stored</param>
        public KnapsackWriter(string filename) {
            _path = $"..\\..\\..\\..\\Statistics\\venv\\Scripts\\Stats\\Data\\{filename}.csv";
            _config = new CsvConfiguration(CultureInfo.InvariantCulture) {
                Delimiter = ";"
            };
        }

        /// <summary>
        /// writes the stats into the csv file
        /// </summary>
        /// <param name="records">logged data</param>
        public void WriteStats(List<double>[] records) {
            using (var writer = new StreamWriter(_path))
            using (var csv = new CsvWriter(writer, _config)) {
                foreach (List<double> record in records) {
                    csv.WriteField(record);
                    csv.NextRecord();
                }
                writer.Flush();
            }
        }
    }
}
