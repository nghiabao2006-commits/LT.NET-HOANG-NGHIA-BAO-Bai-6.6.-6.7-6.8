using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Bai6_8_Bida.DTO;

namespace Bai6_8_Bida.DAL
{
    /// <summary>
    /// DAL: hóa đơn, chi tiết hóa đơn và ba transaction nghiệp vụ
    /// (mở bàn, lưu dịch vụ, thanh toán).
    /// </summary>
    public class HoaDonDAL
    {
        // ------------------------------------------------------------------
        //  Truy vấn
        // ------------------------------------------------------------------
        public DataTable GetHoaDonChuaThanhToan()
        {
            return DbHelper.Query(
                "SELECT h.MaHD, h.MaBan, b.TenBan, b.LoaiBan, b.GiaTheoGio, h.GioBatDau, " +
                "       ISNULL((SELECT SUM(ct.SoLuong * ct.DonGia) " +
                "               FROM dbo.ChiTietHoaDon ct WHERE ct.MaHD = h.MaHD), 0) AS TienDichVu " +
                "FROM dbo.HoaDon h JOIN dbo.BanBida b ON b.MaBan = h.MaBan " +
                "WHERE h.TrangThai = 0 ORDER BY h.GioBatDau");
        }

        public HoaDonDTO GetHoaDon(int maHD)
        {
            DataTable t = DbHelper.Query(
                "SELECT h.MaHD, h.MaBan, b.TenBan, b.LoaiBan, b.GiaTheoGio, h.GioBatDau, " +
                "       h.GioKetThuc, h.TienGioChoi, h.TienDichVu, h.TrangThai " +
                "FROM dbo.HoaDon h JOIN dbo.BanBida b ON b.MaBan = h.MaBan " +
                "WHERE h.MaHD = @ma",
                DbHelper.P("@ma", maHD));

            if (t.Rows.Count == 0)
            {
                return null;
            }

            DataRow r = t.Rows[0];
            return new HoaDonDTO
            {
                MaHD = Convert.ToInt32(r["MaHD"]),
                MaBan = Convert.ToInt32(r["MaBan"]),
                TenBan = Convert.ToString(r["TenBan"]),
                LoaiBan = Convert.ToString(r["LoaiBan"]),
                GiaTheoGio = Convert.ToDecimal(r["GiaTheoGio"]),
                GioBatDau = Convert.ToDateTime(r["GioBatDau"]),
                GioKetThuc = r["GioKetThuc"] == DBNull.Value
                    ? (DateTime?)null
                    : Convert.ToDateTime(r["GioKetThuc"]),
                TienGioChoi = Convert.ToDecimal(r["TienGioChoi"]),
                TienDichVu = Convert.ToDecimal(r["TienDichVu"]),
                TrangThai = Convert.ToBoolean(r["TrangThai"])
            };
        }

        /// <summary>Hóa đơn chưa thanh toán của một bàn (0 = bàn chưa mở).</summary>
        public int GetHoaDonMoCuaBan(int maBan)
        {
            object v = DbHelper.Scalar(
                "SELECT TOP 1 MaHD FROM dbo.HoaDon WHERE MaBan = @ban AND TrangThai = 0 ORDER BY MaHD DESC",
                DbHelper.P("@ban", maBan));
            return v == null ? 0 : Convert.ToInt32(v);
        }

        /// <summary>
        /// Nạp chi tiết hóa đơn bằng SqlDataAdapter (mô hình ngắt kết nối).
        /// Cột ThanhTien là cột biểu thức nên tự tính lại khi SoLuong thay đổi.
        /// </summary>
        public DataTable GetChiTiet(int maHD)
        {
            DataTable t = DbHelper.Query(
                "SELECT ct.MaHD, ct.MaDV, dv.TenDV, ct.SoLuong, ct.DonGia, dv.TruKho " +
                "FROM dbo.ChiTietHoaDon ct JOIN dbo.MenuDichVu dv ON dv.MaDV = ct.MaDV " +
                "WHERE ct.MaHD = @ma ORDER BY dv.TenDV",
                DbHelper.P("@ma", maHD));

            t.Columns.Add("ThanhTien", typeof(decimal), "SoLuong * DonGia");
            return t;
        }

        public DataTable GetLichSu()
        {
            return DbHelper.Query(
                "SELECT h.MaHD, b.TenBan, h.GioBatDau, h.GioKetThuc, h.TienGioChoi, h.TienDichVu, " +
                "       h.TienGioChoi + h.TienDichVu AS TongCong " +
                "FROM dbo.HoaDon h JOIN dbo.BanBida b ON b.MaBan = h.MaBan " +
                "WHERE h.TrangThai = 1 ORDER BY h.GioKetThuc DESC");
        }

        public DataTable ThongKeTheoBan()
        {
            return DbHelper.Query(
                "SELECT b.TenBan, b.LoaiBan, COUNT(*) AS SoHoaDon, " +
                "       SUM(h.TienGioChoi) AS TienGio, SUM(h.TienDichVu) AS TienDichVu, " +
                "       SUM(h.TienGioChoi + h.TienDichVu) AS TongTien " +
                "FROM dbo.HoaDon h JOIN dbo.BanBida b ON b.MaBan = h.MaBan " +
                "WHERE h.TrangThai = 1 " +
                "GROUP BY b.TenBan, b.LoaiBan ORDER BY TongTien DESC");
        }

        public decimal TongDoanhThu()
        {
            object v = DbHelper.Scalar(
                "SELECT ISNULL(SUM(TienGioChoi + TienDichVu), 0) FROM dbo.HoaDon WHERE TrangThai = 1");
            return Convert.ToDecimal(v);
        }

        // ------------------------------------------------------------------
        //  Nghiệp vụ
        // ------------------------------------------------------------------
        /// <summary>
        /// Mở bàn: tạo hóa đơn mới (GioBatDau = GETDATE()) và đổi trạng thái bàn
        /// sang N'Đang chơi' trong CÙNG một transaction.
        /// </summary>
        public int MoBan(int maBan)
        {
            using (var conn = DbHelper.OpenConnection())
            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    // Bước 1: khoá dòng bàn và kiểm tra trạng thái hiện tại
                    string trangThai = null;
                    using (var cmd = new SqlCommand(
                        "SELECT TrangThai FROM dbo.BanBida WITH (UPDLOCK, HOLDLOCK) WHERE MaBan = @ban",
                        conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@ban", maBan);
                        object v = cmd.ExecuteScalar();
                        if (v == null || v == DBNull.Value)
                        {
                            throw new InvalidOperationException("Không tìm thấy bàn có mã " + maBan + ".");
                        }

                        trangThai = Convert.ToString(v);
                    }

                    if (trangThai == "Đang chơi")
                    {
                        throw new InvalidOperationException("Bàn này đang có khách chơi, không mở lại được.");
                    }

                    // Bước 2: chặn trường hợp bàn còn hóa đơn chưa thanh toán
                    using (var cmd = new SqlCommand(
                        "SELECT COUNT(1) FROM dbo.HoaDon WHERE MaBan = @ban AND TrangThai = 0", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@ban", maBan);
                        if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                        {
                            throw new InvalidOperationException(
                                "Bàn này còn hóa đơn chưa thanh toán. Hãy thanh toán trước khi mở bàn mới.");
                        }
                    }

                    // Bước 3: tạo hóa đơn mới
                    int maHD;
                    using (var cmd = new SqlCommand(
                        "INSERT INTO dbo.HoaDon (MaBan, GioBatDau) VALUES (@ban, GETDATE()); " +
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@ban", maBan);
                        maHD = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // Bước 4: đánh dấu bàn đang chơi
                    using (var cmd = new SqlCommand(
                        "UPDATE dbo.BanBida SET TrangThai = N'Đang chơi' WHERE MaBan = @ban", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@ban", maBan);
                        cmd.ExecuteNonQuery();
                    }

                    trans.Commit();
                    return maHD;
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Lưu dịch vụ: ghi đè toàn bộ dòng chi tiết của hóa đơn bằng dữ liệu
        /// người dùng đã sửa trong DataTable (mô hình ngắt kết nối) trong một transaction.
        /// </summary>
        public void LuuDichVu(int maHD, List<ChiTietHoaDonDTO> chiTiet)
        {
            using (var conn = DbHelper.OpenConnection())
            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    // Bước 1: hóa đơn phải tồn tại và còn mở
                    using (var cmd = new SqlCommand(
                        "SELECT TrangThai FROM dbo.HoaDon WITH (UPDLOCK, HOLDLOCK) WHERE MaHD = @ma",
                        conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@ma", maHD);
                        object v = cmd.ExecuteScalar();
                        if (v == null || v == DBNull.Value)
                        {
                            throw new InvalidOperationException("Không tìm thấy hóa đơn " + maHD + ".");
                        }

                        if (Convert.ToBoolean(v))
                        {
                            throw new InvalidOperationException(
                                "Hóa đơn này đã thanh toán nên không sửa được dịch vụ.");
                        }
                    }

                    // Bước 2: xoá các dòng cũ rồi chèn lại theo bảng trong bộ nhớ
                    using (var cmd = new SqlCommand(
                        "DELETE FROM dbo.ChiTietHoaDon WHERE MaHD = @ma", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@ma", maHD);
                        cmd.ExecuteNonQuery();
                    }

                    foreach (ChiTietHoaDonDTO ct in chiTiet)
                    {
                        using (var cmd = new SqlCommand(
                            "INSERT INTO dbo.ChiTietHoaDon (MaHD, MaDV, SoLuong, DonGia) " +
                            "VALUES (@ma, @dv, @sl, @gia)", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@ma", maHD);
                            cmd.Parameters.AddWithValue("@dv", ct.MaDV);
                            cmd.Parameters.AddWithValue("@sl", ct.SoLuong);
                            cmd.Parameters.AddWithValue("@gia", ct.DonGia);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Bước 3: cập nhật tiền dịch vụ tạm tính trên hóa đơn
                    using (var cmd = new SqlCommand(
                        "UPDATE dbo.HoaDon SET TienDichVu = ISNULL((SELECT SUM(SoLuong * DonGia) " +
                        "FROM dbo.ChiTietHoaDon WHERE MaHD = @ma), 0) WHERE MaHD = @ma", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@ma", maHD);
                        cmd.ExecuteNonQuery();
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
        /// Thanh toán trong một transaction:
        ///   1. Khoá hóa đơn, đọc giờ bắt đầu + giá bàn + giờ server.
        ///   2. Cộng tiền dịch vụ từ chi tiết, kiểm tra tồn kho.
        ///   3. Làm tròn giờ chơi theo khối 15 phút, tính tiền giờ.
        ///   4. UPDATE hóa đơn (GioKetThuc, tiền, TrangThai = 1).
        ///   5. Trừ tồn kho CHỈ với dịch vụ có TruKho = 1.
        ///   6. Trả bàn về N'Trống'.
        /// </summary>
        public HoaDonDTO ThanhToan(int maHD)
        {
            using (var conn = DbHelper.OpenConnection())
            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    var hd = new HoaDonDTO { MaHD = maHD };
                    DateTime gioKetThuc;

                    // Bước 1: khoá hóa đơn + đọc dữ liệu cần cho việc tính tiền
                    using (var cmd = new SqlCommand(
                        "SELECT h.MaBan, b.TenBan, b.LoaiBan, b.GiaTheoGio, h.GioBatDau, h.TrangThai, GETDATE() " +
                        "FROM dbo.HoaDon h WITH (UPDLOCK, HOLDLOCK) " +
                        "JOIN dbo.BanBida b ON b.MaBan = h.MaBan WHERE h.MaHD = @ma", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@ma", maHD);
                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            if (!rd.Read())
                            {
                                throw new InvalidOperationException("Không tìm thấy hóa đơn " + maHD + ".");
                            }

                            hd.MaBan = rd.GetInt32(0);
                            hd.TenBan = rd.GetString(1);
                            hd.LoaiBan = rd.GetString(2);
                            hd.GiaTheoGio = rd.GetDecimal(3);
                            hd.GioBatDau = rd.GetDateTime(4);
                            if (rd.GetBoolean(5))
                            {
                                throw new InvalidOperationException("Hóa đơn này đã được thanh toán trước đó.");
                            }

                            gioKetThuc = rd.GetDateTime(6);
                        }
                    }

                    // Bước 2: tiền dịch vụ = tổng các dòng chi tiết
                    using (var cmd = new SqlCommand(
                        "SELECT ISNULL(SUM(SoLuong * DonGia), 0) FROM dbo.ChiTietHoaDon WHERE MaHD = @ma",
                        conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@ma", maHD);
                        hd.TienDichVu = Convert.ToDecimal(cmd.ExecuteScalar());
                    }

                    // Bước 3: kiểm tra tồn kho cho các dịch vụ có trừ kho
                    using (var cmd = new SqlCommand(
                        "SELECT TOP 1 dv.TenDV, dv.TonKho, ct.SoLuong " +
                        "FROM dbo.ChiTietHoaDon ct JOIN dbo.MenuDichVu dv ON dv.MaDV = ct.MaDV " +
                        "WHERE ct.MaHD = @ma AND dv.TruKho = 1 AND dv.TonKho < ct.SoLuong", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@ma", maHD);
                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            if (rd.Read())
                            {
                                throw new InvalidOperationException(
                                    "Dịch vụ \"" + rd.GetString(0) + "\" chỉ còn " + rd.GetInt32(1) +
                                    " trong kho, không đủ " + rd.GetInt32(2) + " để trừ.");
                            }
                        }
                    }

                    // Bước 4: chốt giờ chơi (làm tròn lên theo khối 15 phút) và tiền giờ
                    hd.GioKetThuc = gioKetThuc;
                    hd.SoKhoi15 = TinhTien.SoKhoi(hd.GioBatDau, gioKetThuc);
                    hd.TienGioChoi = TinhTien.TienGio(hd.GiaTheoGio, hd.SoKhoi15);

                    using (var cmd = new SqlCommand(
                        "UPDATE dbo.HoaDon SET GioKetThuc = @kt, TienGioChoi = @gio, " +
                        "       TienDichVu = @dv, TrangThai = 1 " +
                        "WHERE MaHD = @ma AND TrangThai = 0", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@kt", gioKetThuc);
                        cmd.Parameters.AddWithValue("@gio", hd.TienGioChoi);
                        cmd.Parameters.AddWithValue("@dv", hd.TienDichVu);
                        cmd.Parameters.AddWithValue("@ma", maHD);
                        if (cmd.ExecuteNonQuery() == 0)
                        {
                            throw new InvalidOperationException(
                                "Hóa đơn đã được thanh toán bởi thao tác khác. Hãy tải lại danh sách.");
                        }
                    }

                    // Bước 5: chỉ trừ kho với dịch vụ có TruKho = 1
                    using (var cmd = new SqlCommand(
                        "UPDATE dv SET TonKho = TonKho - ct.SoLuong " +
                        "FROM dbo.MenuDichVu dv JOIN dbo.ChiTietHoaDon ct ON ct.MaDV = dv.MaDV " +
                        "WHERE ct.MaHD = @ma AND dv.TruKho = 1", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@ma", maHD);
                        cmd.ExecuteNonQuery();
                    }

                    // Bước 6: trả bàn về trạng thái trống
                    using (var cmd = new SqlCommand(
                        "UPDATE dbo.BanBida SET TrangThai = N'Trống' WHERE MaBan = @ban", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@ban", hd.MaBan);
                        cmd.ExecuteNonQuery();
                    }

                    trans.Commit();
                    hd.TrangThai = true;
                    return hd;
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }
    }
}
