using CsvHelper;
using CsvHelper.Configuration;

using System.Globalization;
using System.IO;
using System.Text;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility
{
    public static class CsvUtility
    {
        public static byte[] CreateCsv<TCsvLine>(IEnumerable<TCsvLine> csvLines, CsvConfiguration configuration = null, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = new UTF8Encoding(true); //.UTF8;

            if(configuration == null)
            {
                configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    HasHeaderRecord = true,
                    Encoding = encoding
                };
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var streamWritter = new StreamWriter(memoryStream, encoding))
                using (var csv = new CsvWriter(streamWritter, configuration))
                {
                    csv.WriteHeader<TCsvLine>();
                    csv.NextRecord();

                    foreach (var csvLine in csvLines)
                    {
                        csv.WriteRecord(csvLine);
                        csv.NextRecord();
                    }
                }

                return memoryStream.ToArray();
            }
        }
    }
}
