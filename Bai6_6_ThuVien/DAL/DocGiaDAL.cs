using System.Data;
using System.Data.SqlClient;
using Bai6_6_ThuVien.DTO;

namespace Bai6_6_ThuVien.DAL
{
    /// <summary>DAL: mọi câu lệnh SQL liên quan bảng DocGia.</summary>
    public class DocGiaDAL
    {
        public DataTable GetAll()
        {
            return DbHelper.Query(
                "SELECT MaDocGia, HoTen, Email, NgayHetHanThe FROM dbo.DocGia ORDER BY HoTen");
        }

        public DataTable Search(string keyword)
        {
            return DbHelper.Query(
                "SELECT MaDocGia, HoTen, Email, NgayHetHanThe FROM dbo.DocGia " +
                "WHERE HoTen LIKE @kw OR Email LIKE @kw ORDER BY HoTen",
                DbHelper.P("@kw", "%" + keyword + "%"));
        }

        public DocGiaDTO GetById(int maDocGia)
        {
            DataTable t = DbHelper.Query(
                "SELECT MaDocGia, HoTen, Email, NgayHetHanThe FROM dbo.DocGia WHERE MaDocGia = @ma",
                DbHelper.P("@ma", maDocGia));

            if (t.Rows.Count == 0)
            {
                return null;
            }

            DataRow r = t.Rows[0];
            return new DocGiaDTO
            {
                MaDocGia = (int)r["MaDocGia"],
                HoTen = r["HoTen"].ToString(),
                Email = r["Email"].ToString(),
                NgayHetHanThe = (System.DateTime)r["NgayHetHanThe"]
            };
        }

        /// <summary>Kiểm tra email đã dùng chưa (bỏ qua chính độc giả đang sửa).</summary>
        public bool EmailDaTonTai(string email, int boQuaMaDocGia)
        {
            object v = DbHelper.Scalar(
                "SELECT COUNT(1) FROM dbo.DocGia WHERE Email = @email AND MaDocGia <> @boQua",
                DbHelper.P("@email", email),
                DbHelper.P("@boQua", boQuaMaDocGia));
            return System.Convert.ToInt32(v) > 0;
        }

        public int Insert(DocGiaDTO dg)
        {
            return DbHelper.Execute(
                "INSERT INTO dbo.DocGia (HoTen, Email, NgayHetHanThe) VALUES (@ten, @email, @han)",
                DbHelper.P("@ten", dg.HoTen),
                DbHelper.P("@email", dg.Email),
                DbHelper.P("@han", dg.NgayHetHanThe));
        }

        public int Update(DocGiaDTO dg)
        {
            return DbHelper.Execute(
                "UPDATE dbo.DocGia SET HoTen = @ten, Email = @email, NgayHetHanThe = @han WHERE MaDocGia = @ma",
                DbHelper.P("@ten", dg.HoTen),
                DbHelper.P("@email", dg.Email),
                DbHelper.P("@han", dg.NgayHetHanThe),
                DbHelper.P("@ma", dg.MaDocGia));
        }

        public int Delete(int maDocGia)
        {
            return DbHelper.Execute(
                "DELETE FROM dbo.DocGia WHERE MaDocGia = @ma",
                DbHelper.P("@ma", maDocGia));
        }

        /// <summary>Đếm số phiếu mượn của độc giả (để chặn xoá khi còn dữ liệu liên quan).</summary>
        public int DemPhieuMuon(int maDocGia)
        {
            object v = DbHelper.Scalar(
                "SELECT COUNT(1) FROM dbo.PhieuMuon WHERE MaDocGia = @ma",
                DbHelper.P("@ma", maDocGia));
            return System.Convert.ToInt32(v);
        }
    }
}
