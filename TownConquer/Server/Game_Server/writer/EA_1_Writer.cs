﻿using CsvHelper;
using Game_Server.EA.Models.Simple;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Game_Server.writer {
    class EA_1_Writer: StatsWriter<Individual_Simple> {

        public EA_1_Writer(string filename) : base(filename) {
            PrepareFile();
            AddGeneColumns(Enum.GetNames(typeof(PropertyNames_Simple)));
        }

        public override void WriteStats(List<Individual_Simple> records) {
            Individual_Simple longestRecord = PrepareStats(records);
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
                    csv.WriteField(record.timestamp.Last());
                    csv.WriteField(record.score);
                    foreach (int Values in record.gene.properties.Values.ToList()) {
                        csv.WriteField(Values);
                    }
                    //_csv.WriteField(record.townDevelopment);
                }
                writer.Flush();
            }
        }

        private Individual_Simple PrepareStats(List<Individual_Simple> records) {
            Individual_Simple longestRecord;
            longestRecord = GetLongestRecord(records, 0);
            return longestRecord;
        }

        /// <summary>
        /// Gets the longest record
        /// </summary>
        /// <param name="records">All records</param>
        /// <param name="length">Min length of the records</param>
        /// <returns>The longest record</returns>
        private Individual_Simple GetLongestRecord(List<Individual_Simple> records, int length) {
            Individual_Simple rec = null;
            foreach (var record in records) {
                if (record.timestamp.Count > length) {
                    length = record.timestamp.Count;
                    rec = record;
                }
            }
            return rec;
        }


    }
}