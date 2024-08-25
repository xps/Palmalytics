using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Palmalytics.Extensions;

namespace Palmalytics.SqlServer
{
    public static class ISqlCommandExtensions
    {
        public static SqlParameter AddParameter(this SqlCommand command, string name, long? value) =>
            command.AddParameter(name, value, SqlDbType.BigInt);

        public static SqlParameter AddParameter(this SqlCommand command, string name, int? value) =>
            command.AddParameter(name, value, SqlDbType.Int);

        public static SqlParameter AddParameter(this SqlCommand command, string name, bool? value) =>
            command.AddParameter(name, value, SqlDbType.Bit);

        public static SqlParameter AddParameter(this SqlCommand command, string name, string value) =>
            command.AddParameter(name, value, SqlDbType.NVarChar);

        public static SqlParameter AddParameter(this SqlCommand command, string name, string value, int maxLength) =>
            command.AddParameter(name, value?.Left(maxLength), SqlDbType.NVarChar);

        public static SqlParameter AddParameter(this SqlCommand command, string name, DateTime? value) =>
            command.AddParameter(name, value, SqlDbType.DateTime);

        public static SqlParameter AddParameter(this SqlCommand command, string name, object value, SqlDbType type)
        {
            var parameter = command.Parameters.Add(name, type);
            parameter.Value = value ?? DBNull.Value;
            return parameter;
        }
    }
}
