using System;
using System.Data;
using Bai6_7_PhongKham.DTO;

namespace Bai6_7_PhongKham.DAL
{
    /// <summary>DAL: hồ sơ tiếp nhận khám.</summary>
    public class HoSoKhamDAL
    {
        public DataTable GetDanhSach()
        {
            return DbHelper.Query(
                "SELECT h.MaHoSo, h.MaBN, b.HoTen AS TenBenhNhan, b.SoDienThoai, h.NgayKham, " +
                "       h.TrieuChung, h.ChanDoan " +
                "FROM dbo.HoSoKham h JOIN dbo.BenhNhan b ON b.MaBN = h.MaBN " +
                "ORDER BY h.NgayKham DESC, h.MaHoSo DESC");
        }

        public HoSoKhamDTO GetById(int maHoSo)
        {
            DataTable t = DbHelper.Query(
                "SELECT h.MaHoSo, h.MaBN, b.HoTen, b.SoDienThoai, h.NgayKham, h.TrieuChung, h.ChanDoan " +
                "FROM dbo.HoSoKham h JOIN dbo.BenhNhan b ON b.MaBN = h.MaBN WHERE h.MaHoSo = @ma",
                DbHelper.P("@ma", maHoSo));

            if (t.Rows.Count == 0)
            {
                return null;
            }

            DataRow r = t.Rows[0];
            return new HoSoKhamDTO
            {
                MaHoSo = Convert.ToInt32(r["MaHoSo"]),
                MaBN = Convert.ToInt32(r["MaBN"]),
                TenBenhNhan = Convert.ToString(r["HoTen"]),
                SoDienThoai = Convert.ToString(r["SoDienThoai"]),
                NgayKham = Convert.ToDateTime(r["NgayKham"]),
                TrieuChung = Convert.ToString(r["TrieuChung"]),
                ChanDoan = Convert.ToString(r["ChanDoan"])
            };
        }

        /// <summary>Tạo hồ sơ khám, trả về MaHoSo vừa sinh.</summary>
        public int Insert(int maBN, string trieuChung, string chanDoan)
        {
            object v = DbHelper.Scalar(
                "INSERT INTO dbo.HoSoKham (MaBN, NgayKham, TrieuChung, ChanDoan) " +
                "VALUES (@bn, GETDATE(), @tc, @cd); " +
                "SELECT CAST(SCOPE_IDENTITY() AS INT);",
                DbHelper.P("@bn", maBN),
                DbHelper.P("@tc", trieuChung),
                DbHelper.P("@cd", chanDoan));
            return Convert.ToInt32(v);
        }

        public int UpdateChanDoan(int maHoSo, string chanDoan)
        {
            return DbHelper.Execute(
                "UPDATE dbo.HoSoKham SET ChanDoan = @cd WHERE MaHoSo = @ma",
                DbHelper.P("@cd", chanDoan),
                DbHelper.P("@ma", maHoSo));
        }

        public int DemDonThuoc(int maHoSo)
        {
            object v = DbHelper.Scalar(
                "SELECT COUNT(1) FROM dbo.DonThuoc WHERE MaHoSo = @ma",
                DbHelper.P("@ma", maHoSo));
            return Convert.ToInt32(v);
        }
    }
}
