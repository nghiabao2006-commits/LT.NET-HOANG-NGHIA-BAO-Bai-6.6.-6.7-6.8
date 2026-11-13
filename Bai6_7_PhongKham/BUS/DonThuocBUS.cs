using System;
using System.Collections.Generic;
using System.Data;
using Bai6_7_PhongKham.DAL;
using Bai6_7_PhongKham.DTO;

namespace Bai6_7_PhongKham.BUS
{
    /// <summary>BUS: nghiệp vụ kê đơn, xuất kho và thanh toán.</summary>
    public class DonThuocBUS
    {
        private readonly DonThuocDAL dal = new DonThuocDAL();
        private readonly HoSoKhamBUS hoSoBus = new HoSoKhamBUS();

        public DataTable LayDanhSach(string trangThai)
        {
            return dal.GetDanhSach(trangThai);
        }

        public DataTable LayChiTiet(int maDon)
        {
            return dal.GetChiTiet(maDon);
        }

        public DonThuocDTO LayTheoMa(int maDon)
        {
            return dal.GetById(maDon);
        }

        /// <summary>
        /// Lưu đơn thuốc. Business kiểm tra dữ liệu đầu vào;
        /// việc trừ tồn kho và rollback do DAL thực hiện trong transaction.
        /// </summary>
        public int LuuDon(int maHoSo, decimal tienKham, List<ChiTietDonThuocDTO> chiTiet)
        {
            if (maHoSo <= 0)
            {
                throw new InvalidOperationException("Chưa chọn hồ sơ khám để kê đơn.");
            }

            if (hoSoBus.LayTheoMa(maHoSo) == null)
            {
                throw new InvalidOperationException("Hồ sơ khám không tồn tại.");
            }

            if (tienKham < 0)
            {
                throw new InvalidOperationException("Tiền khám không được âm.");
            }

            if (chiTiet == null || chiTiet.Count == 0)
            {
                throw new InvalidOperationException("Đơn thuốc chưa có thuốc nào.");
            }

            foreach (ChiTietDonThuocDTO ct in chiTiet)
            {
                if (ct.SoLuong <= 0)
                {
                    throw new InvalidOperationException("Số lượng thuốc \"" + ct.TenThuoc + "\" phải lớn hơn 0.");
                }
            }

            return dal.LuuDon(maHoSo, tienKham, chiTiet);
        }

        public decimal ThanhToan(int maDon, out decimal tienThuoc)
        {
            if (maDon <= 0)
            {
                throw new InvalidOperationException("Chưa chọn đơn cần thanh toán.");
            }

            return dal.ThanhToan(maDon, out tienThuoc);
        }
    }
}
