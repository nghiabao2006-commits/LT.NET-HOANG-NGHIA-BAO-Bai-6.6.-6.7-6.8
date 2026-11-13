using System.Data;
using Bai6_6_ThuVien.DTO;

namespace Bai6_6_ThuVien.DAL
{
    /// <summary>DAL: mọi câu lệnh SQL liên quan bảng DauSach.</summary>
    public class DauSachDAL
    {
        public DataTable GetAll()
        {
            return DbHelper.Query(
                "SELECT MaSach, TenSach, TheLoai, SoLuongCon FROM dbo.DauSach ORDER BY TheLoai, TenSach");
        }

        public DataTable Search(string keyword)
        {
            return DbHelper.Query(
                "SELECT MaSach, TenSach, TheLoai, SoLuongCon FROM dbo.DauSach " +
                "WHERE TenSach LIKE @kw OR TheLoai LIKE @kw ORDER BY TheLoai, TenSach",
                DbHelper.P("@kw", "%" + keyword + "%"));
        }

        public int Insert(DauSachDTO s)
        {
            return DbHelper.Execute(
                "INSERT INTO dbo.DauSach (TenSach, TheLoai, SoLuongCon) VALUES (@ten, @loai, @sl)",
                DbHelper.P("@ten", s.TenSach),
                DbHelper.P("@loai", s.TheLoai),
                DbHelper.P("@sl", s.SoLuongCon));
        }

        public int Update(DauSachDTO s)
        {
            return DbHelper.Execute(
                "UPDATE dbo.DauSach SET TenSach = @ten, TheLoai = @loai, SoLuongCon = @sl WHERE MaSach = @ma",
                DbHelper.P("@ten", s.TenSach),
                DbHelper.P("@loai", s.TheLoai),
                DbHelper.P("@sl", s.SoLuongCon),
                DbHelper.P("@ma", s.MaSach));
        }

        public int Delete(int maSach)
        {
            return DbHelper.Execute(
                "DELETE FROM dbo.DauSach WHERE MaSach = @ma",
                DbHelper.P("@ma", maSach));
        }

        /// <summary>Đếm số dòng chi tiết mượn đang tham chiếu đầu sách.</summary>
        public int DemChiTietMuon(int maSach)
        {
            object v = DbHelper.Scalar(
                "SELECT COUNT(1) FROM dbo.ChiTietMuon WHERE MaSach = @ma",
                DbHelper.P("@ma", maSach));
            return System.Convert.ToInt32(v);
        }

        /// <summary>Các thể loại đang có trong CSDL (gợi ý cho ComboBox).</summary>
        public DataTable GetTheLoai()
        {
            return DbHelper.Query(
                "SELECT DISTINCT TheLoai FROM dbo.DauSach WHERE TheLoai IS NOT NULL ORDER BY TheLoai");
        }
    }
}
