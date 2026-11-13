using System;
using System.Data;
using Bai6_8_Bida.DTO;

namespace Bai6_8_Bida.DAL
{
    /// <summary>DAL: mọi câu lệnh SQL liên quan bảng MenuDichVu.</summary>
    public class DichVuDAL
    {
        public DataTable GetAll()
        {
            return DbHelper.Query(
                "SELECT MaDV, TenDV, DonGia, TonKho, TruKho, " +
                "       CASE WHEN TruKho = 1 THEN N'Có' ELSE N'Không' END AS TruKhoText " +
                "FROM dbo.MenuDichVu ORDER BY TenDV");
        }

        public DichVuDTO GetById(int maDV)
        {
            DataTable t = DbHelper.Query(
                "SELECT MaDV, TenDV, DonGia, TonKho, TruKho FROM dbo.MenuDichVu WHERE MaDV = @ma",
                DbHelper.P("@ma", maDV));

            if (t.Rows.Count == 0)
            {
                return null;
            }

            DataRow r = t.Rows[0];
            return new DichVuDTO
            {
                MaDV = Convert.ToInt32(r["MaDV"]),
                TenDV = Convert.ToString(r["TenDV"]),
                DonGia = Convert.ToDecimal(r["DonGia"]),
                TonKho = Convert.ToInt32(r["TonKho"]),
                TruKho = Convert.ToBoolean(r["TruKho"])
            };
        }

        public int Insert(DichVuDTO dv)
        {
            return DbHelper.Execute(
                "INSERT INTO dbo.MenuDichVu (TenDV, DonGia, TonKho, TruKho) VALUES (@ten, @gia, @kho, @tru)",
                DbHelper.P("@ten", dv.TenDV),
                DbHelper.P("@gia", dv.DonGia),
                DbHelper.P("@kho", dv.TonKho),
                DbHelper.P("@tru", dv.TruKho));
        }

        public int Update(DichVuDTO dv)
        {
            return DbHelper.Execute(
                "UPDATE dbo.MenuDichVu SET TenDV = @ten, DonGia = @gia, TonKho = @kho, TruKho = @tru " +
                "WHERE MaDV = @ma",
                DbHelper.P("@ten", dv.TenDV),
                DbHelper.P("@gia", dv.DonGia),
                DbHelper.P("@kho", dv.TonKho),
                DbHelper.P("@tru", dv.TruKho),
                DbHelper.P("@ma", dv.MaDV));
        }

        public int Delete(int maDV)
        {
            return DbHelper.Execute("DELETE FROM dbo.MenuDichVu WHERE MaDV = @ma", DbHelper.P("@ma", maDV));
        }

        /// <summary>Đếm số dòng chi tiết hóa đơn đang dùng dịch vụ (để chặn xoá).</summary>
        public int DemChiTiet(int maDV)
        {
            object v = DbHelper.Scalar(
                "SELECT COUNT(1) FROM dbo.ChiTietHoaDon WHERE MaDV = @ma",
                DbHelper.P("@ma", maDV));
            return Convert.ToInt32(v);
        }
    }
}
