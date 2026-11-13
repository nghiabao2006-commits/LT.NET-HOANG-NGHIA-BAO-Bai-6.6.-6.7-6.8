using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Bai6_6_ThuVien.DAL
{
    /// <summary>
    /// Lớp tiện ích dùng chung cho tầng DAL: mở kết nối, chạy câu lệnh tham số hoá,
    /// trả về DataTable (SqlDataAdapter) hoặc giá trị đơn (ExecuteScalar).
    /// </summary>
    public static class DbHelper
    {
        public static string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["LibraryDB"].ConnectionString; }
        }

        public static SqlConnection OpenConnection()
        {
            var conn = new SqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        /// <summary>Tạo tham số, tự đổi null thành DBNull.Value.</summary>
        public static SqlParameter P(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        /// <summary>SELECT trả về DataTable (mô hình ngắt kết nối).</summary>
        public static DataTable Query(string sql, params SqlParameter[] pars)
        {
            using (var conn = OpenConnection())
            using (var da = new SqlDataAdapter(sql, conn))
            {
                if (pars != null && pars.Length > 0)
                {
                    da.SelectCommand.Parameters.AddRange(pars);
                }

                var table = new DataTable();
                da.Fill(table);
                return table;
            }
        }

        /// <summary>INSERT / UPDATE / DELETE, trả về số dòng bị tác động.</summary>
        public static int Execute(string sql, params SqlParameter[] pars)
        {
            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (pars != null && pars.Length > 0)
                {
                    cmd.Parameters.AddRange(pars);
                }

                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Trả về ô đầu tiên của kết quả (COUNT, SUM, ...).</summary>
        public static object Scalar(string sql, params SqlParameter[] pars)
        {
            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (pars != null && pars.Length > 0)
                {
                    cmd.Parameters.AddRange(pars);
                }

                return cmd.ExecuteScalar();
            }
        }
    }
}
