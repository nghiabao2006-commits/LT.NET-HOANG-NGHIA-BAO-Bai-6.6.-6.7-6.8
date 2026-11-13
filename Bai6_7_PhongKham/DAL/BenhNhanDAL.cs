using System;
using System.Data;
using System.Data.SqlClient;
using Bai6_7_PhongKham.DTO;

namespace Bai6_7_PhongKham.DAL
{
    /// <summary>DAL: bệnh nhân - tra cứu theo số điện thoại bằng SqlParameter.</summary>
    public class BenhNhanDAL
    {
        public DataTable GetAll()
        {
            return DbHelper.Query(
                "SELECT MaBN, HoTen, SoDienThoai, NamSinh FROM dbo.BenhNhan ORDER BY HoTen");
        }

        /// <summary>
        /// Tra cứu bệnh nhân theo số điện thoại. Câu lệnh dùng tham số @sdt
        /// nên không bị SQL Injection.
        /// </summary>
        public BenhNhanDTO TimTheoSoDienThoai(string soDienThoai)
        {
            const string sql =
                "SELECT MaBN, HoTen, SoDienThoai, NamSinh FROM dbo.BenhNhan WHERE SoDienThoai = @sdt";

            using (var conn = DbHelper.OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add(new SqlParameter("@sdt", SqlDbType.VarChar, 15) { Value = soDienThoai });
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    if (!rd.Read())
                    {
                        return null;
                    }

                    return new BenhNhanDTO
                    {
                        MaBN = rd.GetInt32(0),
                        HoTen = rd.GetString(1),
                        SoDienThoai = rd.IsDBNull(2) ? "" : rd.GetString(2),
                        NamSinh = rd.IsDBNull(3) ? (int?)null : rd.GetInt32(3)
                    };
                }
            }
        }

        public BenhNhanDTO GetById(int maBN)
        {
            DataTable t = DbHelper.Query(
                "SELECT MaBN, HoTen, SoDienThoai, NamSinh FROM dbo.BenhNhan WHERE MaBN = @ma",
                DbHelper.P("@ma", maBN));

            if (t.Rows.Count == 0)
            {
                return null;
            }

            DataRow r = t.Rows[0];
            return new BenhNhanDTO
            {
                MaBN = Convert.ToInt32(r["MaBN"]),
                HoTen = Convert.ToString(r["HoTen"]),
                SoDienThoai = Convert.ToString(r["SoDienThoai"]),
                NamSinh = r["NamSinh"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["NamSinh"])
            };
        }

        /// <summary>Thêm bệnh nhân mới, trả về MaBN vừa sinh.</summary>
        public int Insert(BenhNhanDTO bn)
        {
            object v = DbHelper.Scalar(
                "INSERT INTO dbo.BenhNhan (HoTen, SoDienThoai, NamSinh) VALUES (@ten, @sdt, @nam); " +
                "SELECT CAST(SCOPE_IDENTITY() AS INT);",
                DbHelper.P("@ten", bn.HoTen),
                DbHelper.P("@sdt", bn.SoDienThoai),
                DbHelper.P("@nam", bn.NamSinh));
            return Convert.ToInt32(v);
        }

        public int Update(BenhNhanDTO bn)
        {
            return DbHelper.Execute(
                "UPDATE dbo.BenhNhan SET HoTen = @ten, SoDienThoai = @sdt, NamSinh = @nam WHERE MaBN = @ma",
                DbHelper.P("@ten", bn.HoTen),
                DbHelper.P("@sdt", bn.SoDienThoai),
                DbHelper.P("@nam", bn.NamSinh),
                DbHelper.P("@ma", bn.MaBN));
        }

        /// <summary>Số điện thoại đã thuộc bệnh nhân khác chưa.</summary>
        public bool SoDienThoaiDaTonTai(string soDienThoai, int boQuaMaBN)
        {
            object v = DbHelper.Scalar(
                "SELECT COUNT(1) FROM dbo.BenhNhan WHERE SoDienThoai = @sdt AND MaBN <> @boQua",
                DbHelper.P("@sdt", soDienThoai),
                DbHelper.P("@boQua", boQuaMaBN));
            return Convert.ToInt32(v) > 0;
        }

        public int DemHoSo(int maBN)
        {
            object v = DbHelper.Scalar(
                "SELECT COUNT(1) FROM dbo.HoSoKham WHERE MaBN = @ma",
                DbHelper.P("@ma", maBN));
            return Convert.ToInt32(v);
        }
    }
}
