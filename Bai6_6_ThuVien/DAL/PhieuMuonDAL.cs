using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Bai6_6_ThuVien.DTO;

namespace Bai6_6_ThuVien.DAL
{
    /// <summary>DAL: phiếu mượn, chi tiết mượn, cảnh báo quá hạn và thống kê.</summary>
    public class PhieuMuonDAL
    {
        /// <summary>
        /// Lọc phiếu mượn theo độc giả và/hoặc trạng thái.
        /// Tham số truyền null nghĩa là "không lọc theo tiêu chí đó".
        /// </summary>
        public DataTable GetPhieu(int? maDocGia, string trangThai)
        {
            return DbHelper.Query(
                "SELECT p.MaPhieu, p.MaDocGia, d.HoTen AS TenDocGia, p.NgayMuon, p.HanTra, p.TrangThai " +
                "FROM dbo.PhieuMuon p JOIN dbo.DocGia d ON d.MaDocGia = p.MaDocGia " +
                "WHERE (@maDocGia IS NULL OR p.MaDocGia = @maDocGia) " +
                "  AND (@trangThai IS NULL OR p.TrangThai = @trangThai) " +
                "ORDER BY p.NgayMuon DESC",
                DbHelper.P("@maDocGia", maDocGia.HasValue ? (object)maDocGia.Value : null),
                DbHelper.P("@trangThai", string.IsNullOrEmpty(trangThai) ? null : trangThai));
        }

        public DataTable GetChiTiet(int maPhieu)
        {
            return DbHelper.Query(
                "SELECT ct.MaPhieu, ct.MaSach, ds.TenSach, ct.SoLuong " +
                "FROM dbo.ChiTietMuon ct JOIN dbo.DauSach ds ON ds.MaSach = ct.MaSach " +
                "WHERE ct.MaPhieu = @ma",
                DbHelper.P("@ma", maPhieu));
        }

        /// <summary>
        /// Lập phiếu mượn trong một transaction: chèn PhieuMuon, chèn từng ChiTietMuon
        /// và trừ SoLuongCon. Nếu bất kỳ đầu sách nào không đủ số lượng thì rollback.
        /// </summary>
        public int LapPhieu(int maDocGia, DateTime hanTra, List<ChiTietMuonDTO> chiTiet)
        {
            if (chiTiet == null || chiTiet.Count == 0)
            {
                throw new ArgumentException("Phiếu mượn phải có ít nhất một đầu sách.");
            }

            using (var conn = DbHelper.OpenConnection())
            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    int maPhieu;
                    using (var cmd = new SqlCommand(
                        "INSERT INTO dbo.PhieuMuon (MaDocGia, NgayMuon, HanTra, TrangThai) " +
                        "VALUES (@docGia, GETDATE(), @hanTra, N'Đang mượn'); " +
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@docGia", maDocGia);
                        cmd.Parameters.AddWithValue("@hanTra", hanTra);
                        maPhieu = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    foreach (ChiTietMuonDTO ct in chiTiet)
                    {
                        int conLai;
                        using (var cmd = new SqlCommand(
                            "SELECT SoLuongCon FROM dbo.DauSach WITH (UPDLOCK, HOLDLOCK) WHERE MaSach = @ma",
                            conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@ma", ct.MaSach);
                            object v = cmd.ExecuteScalar();
                            if (v == null)
                            {
                                throw new InvalidOperationException("Đầu sách không còn tồn tại (mã " + ct.MaSach + ").");
                            }

                            conLai = Convert.ToInt32(v);
                        }

                        if (conLai < ct.SoLuong)
                        {
                            throw new InvalidOperationException(
                                "Sách \"" + ct.TenSach + "\" chỉ còn " + conLai + " cuốn, không đủ " + ct.SoLuong + ".");
                        }

                        using (var cmd = new SqlCommand(
                            "INSERT INTO dbo.ChiTietMuon (MaPhieu, MaSach, SoLuong) VALUES (@phieu, @sach, @sl)",
                            conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@phieu", maPhieu);
                            cmd.Parameters.AddWithValue("@sach", ct.MaSach);
                            cmd.Parameters.AddWithValue("@sl", ct.SoLuong);
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = new SqlCommand(
                            "UPDATE dbo.DauSach SET SoLuongCon = SoLuongCon - @sl WHERE MaSach = @ma", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@sl", ct.SoLuong);
                            cmd.Parameters.AddWithValue("@ma", ct.MaSach);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    trans.Commit();
                    return maPhieu;
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Trả sách: UPDATE tham số hoá trạng thái phiếu sang N'Đã trả'
        /// và cộng lại SoLuongCon cho từng đầu sách trong cùng transaction.
        /// </summary>
        public void TraSach(int maPhieu)
        {
            using (var conn = DbHelper.OpenConnection())
            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    using (var cmd = new SqlCommand(
                        "UPDATE dbo.PhieuMuon SET TrangThai = N'Đã trả' " +
                        "WHERE MaPhieu = @ma AND TrangThai <> N'Đã trả'", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@ma", maPhieu);
                        if (cmd.ExecuteNonQuery() == 0)
                        {
                            throw new InvalidOperationException("Phiếu này đã được trả trước đó.");
                        }
                    }

                    var dsSach = new List<KeyValuePair<int, int>>();
                    using (var cmd = new SqlCommand(
                        "SELECT MaSach, SoLuong FROM dbo.ChiTietMuon WHERE MaPhieu = @ma", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@ma", maPhieu);
                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                dsSach.Add(new KeyValuePair<int, int>(rd.GetInt32(0), rd.GetInt32(1)));
                            }
                        }
                    }

                    foreach (var item in dsSach)
                    {
                        using (var cmd = new SqlCommand(
                            "UPDATE dbo.DauSach SET SoLuongCon = SoLuongCon + @sl WHERE MaSach = @ma",
                            conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@sl", item.Value);
                            cmd.Parameters.AddWithValue("@ma", item.Key);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Cảnh báo quá hạn: dùng SqlDataReader đọc trực tiếp từng dòng
        /// (yêu cầu của đề bài) rồi đổ vào DataTable để giao diện hiển thị.
        /// </summary>
        public DataTable GetQuaHan()
        {
            var table = new DataTable();
            table.Columns.Add("MaPhieu", typeof(int));
            table.Columns.Add("TenDocGia", typeof(string));
            table.Columns.Add("Email", typeof(string));
            table.Columns.Add("NgayMuon", typeof(DateTime));
            table.Columns.Add("HanTra", typeof(DateTime));
            table.Columns.Add("SoNgayQuaHan", typeof(int));

            const string sql =
                "SELECT p.MaPhieu, d.HoTen, d.Email, p.NgayMuon, p.HanTra, " +
                "       DATEDIFF(DAY, p.HanTra, GETDATE()) AS SoNgayQuaHan " +
                "FROM dbo.PhieuMuon p JOIN dbo.DocGia d ON d.MaDocGia = p.MaDocGia " +
                "WHERE p.HanTra < CAST(GETDATE() AS DATE) AND p.TrangThai <> N'Đã trả' " +
                "ORDER BY SoNgayQuaHan DESC";

            using (var conn = DbHelper.OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            using (SqlDataReader rd = cmd.ExecuteReader())
            {
                while (rd.Read())
                {
                    table.Rows.Add(
                        rd.GetInt32(0),
                        rd.IsDBNull(1) ? "" : rd.GetString(1),
                        rd.IsDBNull(2) ? "" : rd.GetString(2),
                        rd.GetDateTime(3),
                        rd.GetDateTime(4),
                        rd.GetInt32(5));
                }
            }

            return table;
        }

        /// <summary>Đồng bộ trạng thái: phiếu đang mượn mà quá hạn thì chuyển sang N'Quá hạn'.</summary>
        public int CapNhatTrangThaiQuaHan()
        {
            return DbHelper.Execute(
                "UPDATE dbo.PhieuMuon SET TrangThai = N'Quá hạn' " +
                "WHERE HanTra < CAST(GETDATE() AS DATE) AND TrangThai = N'Đang mượn'");
        }

        /// <summary>Thống kê số lượt mượn theo thể loại bằng GROUP BY.</summary>
        public DataTable ThongKeTheoTheLoai()
        {
            return DbHelper.Query(
                "SELECT ds.TheLoai, " +
                "       COUNT(*) AS SoLuotMuon, " +
                "       SUM(ct.SoLuong) AS TongSoCuon, " +
                "       COUNT(DISTINCT pm.MaDocGia) AS SoDocGia " +
                "FROM dbo.ChiTietMuon ct " +
                "JOIN dbo.DauSach ds ON ds.MaSach = ct.MaSach " +
                "JOIN dbo.PhieuMuon pm ON pm.MaPhieu = ct.MaPhieu " +
                "GROUP BY ds.TheLoai " +
                "ORDER BY SoLuotMuon DESC");
        }

        /// <summary>Tổng số lượt mượn (mỗi dòng chi tiết = 1 lượt) bằng ExecuteScalar.</summary>
        public int DemTongLuotMuon()
        {
            object v = DbHelper.Scalar("SELECT COUNT(*) FROM dbo.ChiTietMuon");
            return Convert.ToInt32(v);
        }
    }
}
