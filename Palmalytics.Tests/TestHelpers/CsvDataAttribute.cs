using System.Globalization;
using System.Reflection;
using CsvHelper;
using CsvHelper.Configuration;
using Xunit.Sdk;

namespace Palmalytics.Tests.TestHelpers
{
    public class CsvDataAttribute(string path) : DataAttribute
    {
        private readonly string path = path;

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            using var reader = new StreamReader("Files/" + path);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));

            var records = new List<object[]>();
            csv.Read();
            csv.ReadHeader();

            if (csv.HeaderRecord.Length != testMethod.GetParameters().Length)
            {
                throw new InvalidOperationException(
                    $"The number of columns in {path} ({csv.ColumnCount}) does not match " +
                    $"the number of parameters in {testMethod.Name} ({testMethod.GetParameters().Count()}).");
            }

            while (csv.Read())
            {
                var record = new object[csv.HeaderRecord.Length];
                for (var i = 0; i < csv.HeaderRecord.Length; i++)
                {
                    record[i] = csv.GetField(i);
                }
                records.Add(record);
            }
            return records;
        }
    }
}
