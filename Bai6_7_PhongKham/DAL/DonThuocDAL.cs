using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Bai6_7_PhongKham.DTO;

namespace Bai6_7_PhongKham.DAL
{
    /// <summary>DAL: đơn thuốc, chi tiết đơn và thanh toán.</summary>
    public class DonThuocDAL
    {
        public DataTable GetDanhSach(string trangThai)
        {
            return DbHelper.Query(
                "SELECT d.MaDon, d.MaHoSo, b.HoTen AS TenBenhNhan, h.NgayKham, " +
                "       d.TienKham, d.TongTien, d.TrangThai " +
                "FROM dbo.DonThuoc d " +
                "JOIN dbo.HoSoKham h ON h.MaHoSo = d.MaHoSo " +
                "JOIN dbo.BenhNhan b ON b.MaBN = h.MaBN " +
                "WHERE (@tt IS NULL OR d.TrangThai = @tt) " +
                "ORDER BY d.MaDon DESC",
                DbHelper.P("@tt", string.IsNullOrEmpty(trangThai) ? null : trangThai));
        }

        public DataTable GetChiTiet(int maDon)
        {
            return DbHelper.Query(
                "SELECT ct.MaDon, ct.MaThuoc, t.TenThuoc, t.DonViTinh, t.DonGia, ct.SoLuong, ct.LieuDung, " +
                "       CAST(ct.SoLuong * t.DonGia AS DECIMAL(18,2)) AS ThanhTien " +
                "FROM dbo.ChiTietDonThuoc ct JOIN dbo.Thuoc t ON t.MaThuoc = ct.MaThuoc " +
                "WHERE ct.MaDon = @ma ORDER BY t.TenThuoc",
                DbHelper.P("@ma", maDon));
        }

        public DonThuocDTO GetById(int maDon)
        {
            DataTable t = DbHelper.Query(
                "SELECT d.MaDon, d.MaHoSo, b.HoTen, h.NgayKham, d.TienKham, d.TongTien, d.TrangThai " +
                "FROM dbo.DonThuoc d " +
                "JOIN dbo.HoSoKham h ON h.MaHoSo = d.MaHoSo " +
                "JOIN dbo.BenhNhan b ON b.MaBN = h.MaBN " +
                "WHERE d.MaDon = @ma",
                DbHelper.P("@ma", maDon));

            if (t.Rows.Count == 0)
            {
                return null;
            }

            DataRow r = t.Rows[0];
            return new DonThuocDTO
            {
                MaDon = Convert.ToInt32(r["MaDon"]),
                MaHoSo = Convert.ToInt32(r["MaHoSo"]),
                TenBenhNhan = Convert.ToString(r["HoTen"]),
                NgayKham = Convert.ToDateTime(r["NgayKham"]),
                TienKham = Convert.ToDecimal(r["TienKham"]),
                TongTien = Convert.ToDecimal(r["TongTien"]),
                TrangThai = Convert.ToString(r["TrangThai"])
            };
        }

        /// <summary>
        /// Lưu đơn thuốc và xuất kho trong MỘT SqlTransaction:
        ///   1. Tạo bản ghi DonThuoc.
        ///   2. Tạo các bản ghi ChiTietDonThuoc.
        ///   3. Trừ SoLuongTon tương ứng trong bảng Thuoc.
        /// Nếu bất kỳ thuốc nào không đủ tồn -> Rollback và ném lỗi cho BUS/GUI.
        /// </summary>
        public int LuuDon(int maHoSo, decimal tienKham, List<ChiTietDonThuocDTO> chiTiet)
        {
            if (chiTiet == null || chiTiet.Count == 0)
            {
                throw new ArgumentException("Đơn thuốc phải có ít nhất một loại thuốc.");
            }

            using (var conn = DbHelper.OpenConnection())
            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    int maDon;
                    using (var cmd = new SqlCommand(
                        "INSERT INTO dbo.DonThuoc (MaHoSo, TienKham, TongTien, TrangThai) " +
                        "VALUES (@hoSo, @tienKham, 0, N'Chờ lấy thuốc'); " +
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@hoSo", maHoSo);
                        cmd.Parameters.AddWithValue("@tienKham", tienKham);
                        maDon = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    foreach (ChiTietDonThuocDTO ct in chiTiet)
                    {
                        int ton;
                        using (var cmd = new SqlCommand(
                            "SELECT SoLuongTon FROM dbo.Thuoc WITH (UPDLOCK, HOLDLOCK) WHERE MaThuoc = @ma",
                            conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@ma", ct.MaThuoc);
                            object v = cmd.ExecuteScalar();
                            if (v == null)
                            {
                                throw new InvalidOperationException("Thuốc không còn trong danh mục (mã " + ct.MaThuoc + ").");
                            }

                            ton = Convert.ToInt32(v);
                        }

                        if (ton < ct.SoLuong)
                        {
                            throw new InvalidOperationException(
                                "Thuốc \"" + ct.TenThuoc + "\" chỉ còn " + ton + " " + ct.DonViTinh +
                                ", không đủ " + ct.SoLuong + ".");
                        }

                        using (var cmd = new SqlCommand(
                            "INSERT INTO dbo.ChiTietDonThuoc (MaDon, MaThuoc, SoLuong, LieuDung) " +
                            "VALUES (@don, @thuoc, @sl, @lieu)", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@don", maDon);
                            cmd.Parameters.AddWithValue("@thuoc", ct.MaThuoc);
                            cmd.Parameters.AddWithValue("@sl", ct.SoLuong);
                            cmd.Parameters.AddWithValue("@lieu", (object)ct.LieuDung ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = new SqlCommand(
                            "UPDATE dbo.Thuoc SET SoLuongTon = SoLuongTon - @sl WHERE MaThuoc = @ma", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@sl", ct.SoLuong);
                            cmd.Parameters.AddWithValue("@ma", ct.MaThuoc);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Tạm tính tổng tiền ngay khi lưu đơn (chốt lại lần nữa khi thanh toán)
                    using (var cmd = new SqlCommand(
                        "UPDATE d SET TongTien = d.TienKham + ISNULL((" +
                        "    SELECT SUM(ct.SoLuong * t.DonGia) FROM dbo.ChiTietDonThuoc ct " +
                        "    JOIN dbo.Thuoc t ON t.MaThuoc = ct.MaThuoc WHERE ct.MaDon = d.MaDon), 0) " +
                        "FROM dbo.DonThuoc d WHERE d.MaDon = @don", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@don", maDon);
                        cmd.ExecuteNonQuery();
                    }

                    trans.Commit();
                    return maDon;
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Thanh toán: tính TongTien = TienKham + SUM(SoLuong * DonGia),
        /// chuyển TrangThai sang N'Đã thanh toán' (chỉ khi đang chờ lấy thuốc).
        /// </summary>
        public decimal ThanhToan(int maDon, out decimal tienThuoc)
        {
            using (var conn = DbHelper.OpenConnection())
            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    decimal tienKham;
                    using (var cmd = new SqlCommand(
                        "SELECT TienKham FROM dbo.DonThuoc WHERE MaDon = @don AND TrangThai = N'Chờ lấy thuốc'",
                        conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@don", maDon);
                        object v = cmd.ExecuteScalar();
                        if (v == null)
                        {
                            throw new InvalidOperationException("Đơn này không tồn tại hoặc đã được thanh toán.");
                        }

                        tienKham = Convert.ToDecimal(v);
                    }

                    object tongThuoc = null;
                    using (var cmd = new SqlCommand(
                        "SELECT SUM(ct.SoLuong * t.DonGia) FROM dbo.ChiTietDonThuoc ct " +
                        "JOIN dbo.Thuoc t ON t.MaThuoc = ct.MaThuoc WHERE ct.MaDon = @don", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@don", maDon);
                        tongThuoc = cmd.ExecuteScalar();
                    }

                    tienThuoc = tongThuoc == null || tongThuoc == DBNull.Value ? 0m : Convert.ToDecimal(tongThuoc);
                    decimal tongTien = tienKham + tienThuoc;

                    using (var cmd = new SqlCommand(
                        "UPDATE dbo.DonThuoc SET TongTien = @tong, TrangThai = N'Đã thanh toán' " +
                        "WHERE MaDon = @don AND TrangThai = N'Chờ lấy thuốc'", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@tong", tongTien);
                        cmd.Parameters.AddWithValue("@don", maDon);
                        if (cmd.ExecuteNonQuery() == 0)
                        {
                            throw new InvalidOperationException("Đơn này đã được thanh toán trước đó.");
                        }
                    }

                    trans.Commit();
                    return tongTien;
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
