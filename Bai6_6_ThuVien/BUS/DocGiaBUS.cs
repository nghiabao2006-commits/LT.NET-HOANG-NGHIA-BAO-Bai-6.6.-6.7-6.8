using System;
using System.Data;
using System.Text.RegularExpressions;
using Bai6_6_ThuVien.DAL;
using Bai6_6_ThuVien.DTO;

namespace Bai6_6_ThuVien.BUS
{
    /// <summary>BUS: nghiệp vụ độc giả - kiểm tra dữ liệu trước khi gọi DAL.</summary>
    public class DocGiaBUS
    {
        private readonly DocGiaDAL dal = new DocGiaDAL();

        public DataTable LayDanhSach()
        {
            return dal.GetAll();
        }

        public DataTable TimKiem(string keyword)
        {
            return string.IsNullOrWhiteSpace(keyword) ? dal.GetAll() : dal.Search(keyword.Trim());
        }

        /// <summary>Kiểm tra hợp lệ, trả về chuỗi lỗi (rỗng = hợp lệ).</summary>
        public string KiemTra(DocGiaDTO dg)
        {
            if (string.IsNullOrWhiteSpace(dg.HoTen))
            {
                return "Họ tên không được để trống.";
            }

            if (dg.HoTen.Trim().Length > 100)
            {
                return "Họ tên tối đa 100 ký tự.";
            }

            if (!string.IsNullOrWhiteSpace(dg.Email))
            {
                if (!Regex.IsMatch(dg.Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    return "Email không đúng định dạng.";
                }

                if (dal.EmailDaTonTai(dg.Email.Trim(), dg.MaDocGia))
                {
                    return "Email này đã được dùng cho độc giả khác.";
                }
            }

            return string.Empty;
        }

        public void Them(DocGiaDTO dg)
        {
            string loi = KiemTra(dg);
            if (loi.Length > 0)
            {
                throw new InvalidOperationException(loi);
            }

            dg.HoTen = dg.HoTen.Trim();
            dg.Email = string.IsNullOrWhiteSpace(dg.Email) ? null : dg.Email.Trim();
            dal.Insert(dg);
        }

        public void Sua(DocGiaDTO dg)
        {
            if (dg.MaDocGia <= 0)
            {
                throw new InvalidOperationException("Chưa chọn độc giả cần sửa.");
            }

            string loi = KiemTra(dg);
            if (loi.Length > 0)
            {
                throw new InvalidOperationException(loi);
            }

            dg.HoTen = dg.HoTen.Trim();
            dg.Email = string.IsNullOrWhiteSpace(dg.Email) ? null : dg.Email.Trim();
            dal.Update(dg);
        }

        public void Xoa(int maDocGia)
        {
            if (dal.DemPhieuMuon(maDocGia) > 0)
            {
                throw new InvalidOperationException(
                    "Độc giả này đã có phiếu mượn nên không xoá được. Hãy xoá phiếu mượn trước.");
            }

            dal.Delete(maDocGia);
        }

        public DocGiaDTO LayTheoMa(int maDocGia)
        {
            return dal.GetById(maDocGia);
        }

        /// <summary>Thẻ còn hạn khi NgayHetHanThe >= hôm nay.</summary>
        public bool TheConHan(int maDocGia, out string thongBao)
        {
            DocGiaDTO dg = dal.GetById(maDocGia);
            if (dg == null)
            {
                thongBao = "Không tìm thấy độc giả.";
                return false;
            }

            if (!dg.TheConHan)
            {
                thongBao = "Thẻ độc giả đã hết hạn ngày " + dg.NgayHetHanThe.ToString("dd/MM/yyyy") +
                           ". Không thể lập phiếu mượn mới.";
                return false;
            }

            thongBao = "Thẻ còn hạn đến " + dg.NgayHetHanThe.ToString("dd/MM/yyyy") + ".";
            return true;
        }
    }
}
