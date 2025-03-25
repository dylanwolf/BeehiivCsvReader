using BeehiivPostReader.DTO;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace BeehiivPostReader
{
    public static class BeehiivCsvReader
    {
        public static IEnumerable<BeehiivPost> ReadCsv(TextReader reader)
        {
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.ReadHeader();
                return csv.GetRecords<BeehiivPost>().ToArray();
            }
        }
    }
}
