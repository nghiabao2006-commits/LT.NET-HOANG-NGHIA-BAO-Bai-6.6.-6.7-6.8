using System;
using System.Data;
using Bai6_6_ThuVien.DAL;
using Bai6_6_ThuVien.DTO;

namespace Bai6_6_ThuVien.BUS
{
    /// <summary>BUS: nghiệp vụ đầu sách.</summary>
    public class DauSachBUS
    {
        private readonly DauSachDAL dal = new DauSachDAL();

        public DataTable LayDanhSach()
        {
            return dal.GetAll();
        }

        public DataTable TimKiem(string keyword)
        {
            return string.IsNullOrWhiteSpace(keyword) ? dal.GetAll() : dal.Search(keyword.Trim());
        }

        public DataTable LayTheLoai()
        {
            return dal.GetTheLoai();
        }

        public string KiemTra(DauSachDTO s)
        {
            if (string.IsNullOrWhiteSpace(s.TenSach))
            {
                return "Tên sách không được để trống.";
            }

            if (s.TenSach.Trim().Length > 200)
            {
                return "Tên sách tối đa 200 ký tự.";
            }

            if (string.IsNullOrWhiteSpace(s.TheLoai))
            {
                return "Chưa chọn thể loại.";
            }

            if (s.SoLuongCon < 0)
            {
                return "Số lượng còn phải lớn hơn hoặc bằng 0.";
            }

            return string.Empty;
        }

        public void Them(DauSachDTO s)
        {
            string loi = KiemTra(s);
            if (loi.Length > 0)
            {
                throw new InvalidOperationException(loi);
            }

            s.TenSach = s.TenSach.Trim();
            s.TheLoai = s.TheLoai.Trim();
            dal.Insert(s);
        }

        public void Sua(DauSachDTO s)
        {
            if (s.MaSach <= 0)
            {
                throw new InvalidOperationException("Chưa chọn đầu sách cần sửa.");
            }

            string loi = KiemTra(s);
            if (loi.Length > 0)
            {
                throw new InvalidOperationException(loi);
            }

            s.TenSach = s.TenSach.Trim();
            s.TheLoai = s.TheLoai.Trim();
            dal.Update(s);
        }

        public void Xoa(int maSach)
        {
            if (dal.DemChiTietMuon(maSach) > 0)
            {
                throw new InvalidOperationException(
                    "Đầu sách này đang có trong phiếu mượn nên không xoá được.");
            }

            dal.Delete(maSach);
        }
    }
}
