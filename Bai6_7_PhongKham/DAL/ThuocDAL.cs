using System;
using System.Data;
using Bai6_7_PhongKham.DTO;

namespace Bai6_7_PhongKham.DAL
{
    /// <summary>DAL: danh mục thuốc và tồn kho.</summary>
    public class ThuocDAL
    {
        public DataTable GetAll()
        {
            return DbHelper.Query(
                "SELECT MaThuoc, TenThuoc, DonViTinh, DonGia, SoLuongTon FROM dbo.Thuoc ORDER BY TenThuoc");
        }

        public DataTable Search(string keyword)
        {
            return DbHelper.Query(
                "SELECT MaThuoc, TenThuoc, DonViTinh, DonGia, SoLuongTon FROM dbo.Thuoc " +
                "WHERE TenThuoc LIKE @kw ORDER BY TenThuoc",
                DbHelper.P("@kw", "%" + keyword + "%"));
        }

        public ThuocDTO GetById(int maThuoc)
        {
            DataTable t = DbHelper.Query(
                "SELECT MaThuoc, TenThuoc, DonViTinh, DonGia, SoLuongTon FROM dbo.Thuoc WHERE MaThuoc = @ma",
                DbHelper.P("@ma", maThuoc));

            if (t.Rows.Count == 0)
            {
                return null;
            }

            DataRow r = t.Rows[0];
            return new ThuocDTO
            {
                MaThuoc = Convert.ToInt32(r["MaThuoc"]),
                TenThuoc = Convert.ToString(r["TenThuoc"]),
                DonViTinh = Convert.ToString(r["DonViTinh"]),
                DonGia = Convert.ToDecimal(r["DonGia"]),
                SoLuongTon = Convert.ToInt32(r["SoLuongTon"])
            };
        }

        public int Insert(ThuocDTO t)
        {
            return DbHelper.Execute(
                "INSERT INTO dbo.Thuoc (TenThuoc, DonViTinh, DonGia, SoLuongTon) VALUES (@ten, @dvt, @gia, @ton)",
                DbHelper.P("@ten", t.TenThuoc),
                DbHelper.P("@dvt", t.DonViTinh),
                DbHelper.P("@gia", t.DonGia),
                DbHelper.P("@ton", t.SoLuongTon));
        }

        /// <summary>Cập nhật cả đơn giá và tồn kho (dùng cho sửa danh mục).</summary>
        public int Update(ThuocDTO t)
        {
            return DbHelper.Execute(
                "UPDATE dbo.Thuoc SET TenThuoc = @ten, DonViTinh = @dvt, DonGia = @gia, SoLuongTon = @ton " +
                "WHERE MaThuoc = @ma",
                DbHelper.P("@ten", t.TenThuoc),
                DbHelper.P("@dvt", t.DonViTinh),
                DbHelper.P("@gia", t.DonGia),
                DbHelper.P("@ton", t.SoLuongTon),
                DbHelper.P("@ma", t.MaThuoc));
        }

        /// <summary>Nhập thêm hàng: chỉ cộng tồn, không sửa đơn giá.</summary>
        public int NhapThemTon(int maThuoc, int soLuong)
        {
            return DbHelper.Execute(
                "UPDATE dbo.Thuoc SET SoLuongTon = SoLuongTon + @sl WHERE MaThuoc = @ma",
                DbHelper.P("@sl", soLuong),
                DbHelper.P("@ma", maThuoc));
        }

        public int Delete(int maThuoc)
        {
            return DbHelper.Execute(
                "DELETE FROM dbo.Thuoc WHERE MaThuoc = @ma",
                DbHelper.P("@ma", maThuoc));
        }

        public int DemChiTietDon(int maThuoc)
        {
            object v = DbHelper.Scalar(
                "SELECT COUNT(1) FROM dbo.ChiTietDonThuoc WHERE MaThuoc = @ma",
                DbHelper.P("@ma", maThuoc));
            return Convert.ToInt32(v);
        }
    }
}
