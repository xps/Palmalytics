using System.Linq;
using System.Text;
using Microsoft.Data.SqlClient;

namespace Palmalytics.SqlServer.Utilities
{
    public static class SqlDataReaderExtensions
    {
        /// <summary>
        /// Exports an SqlDataReader to CSV
        /// </summary>
        public static string ToCsv(this SqlDataReader reader, bool includeHeaders = true)
        {
            static string encode(object value) =>
                "\"" + (value ?? "").ToString().Replace("\"", "\"\"") + "\"";

            // The 'stream' we're writting to
            var builder = new StringBuilder();

            // Get all the columns
            var columns = reader.GetColumnSchema()
                .Where(x => x.ColumnOrdinal != null)
                .OrderBy(x => x.ColumnOrdinal)
                .ToList();

            // Write the headers
            if (includeHeaders)
                builder.AppendLine(string.Join(",", columns.Select(col => encode(col.ColumnName))));

            // Write values
            while (reader.Read())
                builder.AppendLine(string.Join(",", columns.Select(col => encode(reader.GetSqlValue(col.ColumnOrdinal.Value)))));

            // Build the string
            return builder.ToString();
        }
    }
}
