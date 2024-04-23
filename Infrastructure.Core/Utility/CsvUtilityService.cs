using CsvHelper;
using CsvHelper.Configuration;

using DocumentFormat.OpenXml.Spreadsheet;

using System.Globalization;
using System.Text;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility
{
    public class CsvUtilityService : ISingletonDependency
    {
        public TCsvLine[] ReadCsv<TCsvLine, TMap>(byte[] content, Action<CsvConfiguration> configurationModify = null, Encoding encoding = null)
            where TMap : ClassMap
        {
            if (encoding == null)
                encoding = new UTF8Encoding(true); //.UTF8;

            var configuration = DefaultConfiguration(encoding);

            if (configurationModify != null)
               configurationModify(configuration);

            using (var memoryStream = new MemoryStream(content))
            using (var streamReader = new StreamReader(memoryStream, encoding))
            {
                var csvHelper = new CsvHelper.CsvReader(streamReader, configuration);

                csvHelper.Context.RegisterClassMap<TMap>();

                var csvLines = csvHelper.GetRecords<TCsvLine>().ToArray();

                return csvLines;
            }
        }

        public byte[] CreateCsv<TCsvLine, TMap>(IEnumerable<TCsvLine> csvLines, Action<CsvConfiguration> configurationModify = null, Encoding encoding = null)
             where TMap : ClassMap
        {
            if (encoding == null)
                encoding = new UTF8Encoding(true); //.UTF8;

            var configuration = DefaultConfiguration(encoding);

            if (configurationModify != null)
                configurationModify(configuration);

            using (var memoryStream = new MemoryStream())
            {
                using (var streamWritter = new StreamWriter(memoryStream, encoding))
                using (var csvWriter = new CsvWriter(streamWritter, configuration))
                {
                    csvWriter.Context.RegisterClassMap<TMap>();

                    csvWriter.WriteHeader<TCsvLine>();
                    csvWriter.NextRecord();

                    foreach (var csvLine in csvLines)
                    {
                        csvWriter.WriteRecord(csvLine);
                        csvWriter.NextRecord();
                    }
                }

                return memoryStream.ToArray();
            }
        }

        protected virtual CsvConfiguration DefaultConfiguration(Encoding encoding)
        {
            return new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true,
                Encoding = encoding
            };
        }
    }
}
