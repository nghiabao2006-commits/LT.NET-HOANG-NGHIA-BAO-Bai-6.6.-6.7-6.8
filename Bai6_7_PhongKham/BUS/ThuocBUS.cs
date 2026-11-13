using System;
using System.Data;
using Bai6_7_PhongKham.DAL;
using Bai6_7_PhongKham.DTO;

namespace Bai6_7_PhongKham.BUS
{
    /// <summary>BUS: nghiệp vụ danh mục thuốc và nhập kho.</summary>
    public class ThuocBUS
    {
        private readonly ThuocDAL dal = new ThuocDAL();

        public DataTable LayDanhSach()
        {
            return dal.GetAll();
        }

        public DataTable TimKiem(string keyword)
        {
            return string.IsNullOrWhiteSpace(keyword) ? dal.GetAll() : dal.Search(keyword.Trim());
        }

        public ThuocDTO LayTheoMa(int maThuoc)
        {
            return dal.GetById(maThuoc);
        }

        public string KiemTra(ThuocDTO t)
        {
            if (string.IsNullOrWhiteSpace(t.TenThuoc))
            {
                return "Tên thuốc không được để trống.";
            }

            if (t.TenThuoc.Trim().Length > 150)
            {
                return "Tên thuốc tối đa 150 ký tự.";
            }

            if (string.IsNullOrWhiteSpace(t.DonViTinh))
            {
                return "Chưa chọn đơn vị tính.";
            }

            if (t.DonGia < 0)
            {
                return "Đơn giá không được âm.";
            }

            if (t.SoLuongTon < 0)
            {
                return "Số lượng tồn không được âm.";
            }

            return string.Empty;
        }

        public void Them(ThuocDTO t)
        {
            string loi = KiemTra(t);
            if (loi.Length > 0)
            {
                throw new InvalidOperationException(loi);
            }

            t.TenThuoc = t.TenThuoc.Trim();
            t.DonViTinh = t.DonViTinh.Trim();
            dal.Insert(t);
        }

        public void Sua(ThuocDTO t)
        {
            if (t.MaThuoc <= 0)
            {
                throw new InvalidOperationException("Chưa chọn thuốc cần sửa.");
            }

            string loi = KiemTra(t);
            if (loi.Length > 0)
            {
                throw new InvalidOperationException(loi);
            }

            t.TenThuoc = t.TenThuoc.Trim();
            t.DonViTinh = t.DonViTinh.Trim();
            dal.Update(t);
        }

        public void Xoa(int maThuoc)
        {
            if (dal.DemChiTietDon(maThuoc) > 0)
            {
                throw new InvalidOperationException(
                    "Thuốc này đã có trong đơn thuốc nên không xoá được. Chỉ nên ngừng sử dụng.");
            }

            dal.Delete(maThuoc);
        }

        /// <summary>Nhập thêm hàng vào kho (cộng dồn tồn kho).</summary>
        public void NhapThemTon(int maThuoc, int soLuong)
        {
            if (maThuoc <= 0)
            {
                throw new InvalidOperationException("Chưa chọn thuốc cần nhập.");
            }

            if (soLuong <= 0)
            {
                throw new InvalidOperationException("Số lượng nhập phải lớn hơn 0.");
            }

            dal.NhapThemTon(maThuoc, soLuong);
        }
    }
}
