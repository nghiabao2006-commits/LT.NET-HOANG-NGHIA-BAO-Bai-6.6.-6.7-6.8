using System;
using System.Data;
using Bai6_8_Bida.DTO;

namespace Bai6_8_Bida.DAL
{
    /// <summary>DAL: mọi câu lệnh SQL liên quan bảng BanBida.</summary>
    public class BanBidaDAL
    {
        public DataTable GetAll()
        {
            return DbHelper.Query(
                "SELECT MaBan, TenBan, LoaiBan, GiaTheoGio, TrangThai " +
                "FROM dbo.BanBida ORDER BY MaBan");
        }

        public BanBidaDTO GetById(int maBan)
        {
            DataTable t = DbHelper.Query(
                "SELECT MaBan, TenBan, LoaiBan, GiaTheoGio, TrangThai " +
                "FROM dbo.BanBida WHERE MaBan = @ma",
                DbHelper.P("@ma", maBan));

            if (t.Rows.Count == 0)
            {
                return null;
            }

            DataRow r = t.Rows[0];
            return new BanBidaDTO
            {
                MaBan = Convert.ToInt32(r["MaBan"]),
                TenBan = Convert.ToString(r["TenBan"]),
                LoaiBan = Convert.ToString(r["LoaiBan"]),
                GiaTheoGio = Convert.ToDecimal(r["GiaTheoGio"]),
                TrangThai = Convert.ToString(r["TrangThai"])
            };
        }

        public int Insert(BanBidaDTO ban)
        {
            return DbHelper.Execute(
                "INSERT INTO dbo.BanBida (TenBan, LoaiBan, GiaTheoGio) VALUES (@ten, @loai, @gia)",
                DbHelper.P("@ten", ban.TenBan),
                DbHelper.P("@loai", ban.LoaiBan),
                DbHelper.P("@gia", ban.GiaTheoGio));
        }

        public int Update(BanBidaDTO ban)
        {
            return DbHelper.Execute(
                "UPDATE dbo.BanBida SET TenBan = @ten, LoaiBan = @loai, GiaTheoGio = @gia WHERE MaBan = @ma",
                DbHelper.P("@ten", ban.TenBan),
                DbHelper.P("@loai", ban.LoaiBan),
                DbHelper.P("@gia", ban.GiaTheoGio),
                DbHelper.P("@ma", ban.MaBan));
        }

        public int Delete(int maBan)
        {
            return DbHelper.Execute("DELETE FROM dbo.BanBida WHERE MaBan = @ma", DbHelper.P("@ma", maBan));
        }

        /// <summary>Đếm số hóa đơn của bàn (để chặn xoá khi còn dữ liệu liên quan).</summary>
        public int DemHoaDon(int maBan)
        {
            object v = DbHelper.Scalar(
                "SELECT COUNT(1) FROM dbo.HoaDon WHERE MaBan = @ma",
                DbHelper.P("@ma", maBan));
            return Convert.ToInt32(v);
        }
    }
}
