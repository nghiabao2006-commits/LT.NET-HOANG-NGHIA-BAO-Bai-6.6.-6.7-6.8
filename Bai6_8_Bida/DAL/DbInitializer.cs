using System;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Bai6_8_Bida.DAL
{
    /// <summary>
    /// Tự khởi tạo cơ sở dữ liệu lúc chạy:
    ///   1. Nếu CSDL BilliardsDB chưa có -> CREATE DATABASE.
    ///   2. Chạy script Database/ScriptDB_Bida.sql (nhúng trong .exe) để tạo bảng
    ///      và chèn dữ liệu mẫu. Script viết idempotent nên chạy lại không sao.
    /// </summary>
    public static class DbInitializer
    {
        private const string ScriptResource = "Bai6_8_Bida.ScriptDB.sql";

        public static void EnsureDatabase()
        {
            var builder = new SqlConnectionStringBuilder(DbHelper.ConnectionString);
            string dbName = builder.InitialCatalog;
            if (string.IsNullOrWhiteSpace(dbName))
            {
                throw new InvalidOperationException("Chuỗi kết nối thiếu Initial Catalog.");
            }

            TaoDatabaseNeuChuaCo(builder, dbName);

            using (var conn = DbHelper.OpenConnection())
            {
                ChayScript(conn, DocScript());
            }
        }

        private static void TaoDatabaseNeuChuaCo(SqlConnectionStringBuilder builder, string dbName)
        {
            builder.InitialCatalog = "master";
            using (var master = new SqlConnection(builder.ConnectionString))
            {
                master.Open();
                const string sql =
                    "IF DB_ID(@name) IS NULL " +
                    "BEGIN " +
                    "    DECLARE @s NVARCHAR(MAX) = N'CREATE DATABASE ' + QUOTENAME(@name); " +
                    "    EXEC sp_executesql @s; " +
                    "END";
                using (var cmd = new SqlCommand(sql, master))
                {
                    cmd.Parameters.AddWithValue("@name", dbName);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static string DocScript()
        {
            var asm = Assembly.GetExecutingAssembly();
            using (var stream = asm.GetManifestResourceStream(ScriptResource))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException("Không tìm thấy script CSDL nhúng: " + ScriptResource);
                }

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>Tách script theo mốc GO rồi chạy từng batch.</summary>
        private static void ChayScript(SqlConnection conn, string script)
        {
            string[] batches = Regex.Split(script, @"^\s*GO\s*$",
                RegexOptions.Multiline | RegexOptions.IgnoreCase);

            foreach (string batch in batches)
            {
                if (string.IsNullOrWhiteSpace(batch))
                {
                    continue;
                }

                using (var cmd = new SqlCommand(batch, conn))
                {
                    cmd.CommandTimeout = 60;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
