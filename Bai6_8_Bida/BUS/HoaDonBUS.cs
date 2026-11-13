using System;
using System.Collections.Generic;
using System.Data;
using Bai6_8_Bida.DAL;
using Bai6_8_Bida.DTO;

namespace Bai6_8_Bida.BUS
{
    /// <summary>BUS: nghiệp vụ hóa đơn - mở bàn, gọi dịch vụ, thanh toán.</summary>
    public class HoaDonBUS
    {
        private readonly HoaDonDAL dal = new HoaDonDAL();

        public DataTable LayHoaDonChuaThanhToan()
        {
            return dal.GetHoaDonChuaThanhToan();
        }

        public int LayHoaDonMoCuaBan(int maBan)
        {
            return maBan <= 0 ? 0 : dal.GetHoaDonMoCuaBan(maBan);
        }

        /// <summary>Chi tiết hóa đơn dạng DataTable để gọi dịch vụ ngắt kết nối.</summary>
        public DataTable LayChiTiet(int maHD)
        {
            return dal.GetChiTiet(maHD);
        }

        public HoaDonDTO LayHoaDon(int maHD)
        {
            return dal.GetHoaDon(maHD);
        }

        public DataTable LayLichSu()
        {
            return dal.GetLichSu();
        }

        public DataTable ThongKeTheoBan()
        {
            return dal.ThongKeTheoBan();
        }

        public decimal TongDoanhThu()
        {
            return dal.TongDoanhThu();
        }

        /// <summary>Mở bàn: tạo hóa đơn và đổi trạng thái bàn (transaction ở tầng DAL).</summary>
        public int MoBan(int maBan)
        {
            if (maBan <= 0)
            {
                throw new InvalidOperationException("Chưa chọn bàn cần mở.");
            }

            return dal.MoBan(maBan);
        }

        /// <summary>
        /// Lưu dịch vụ: kiểm tra dữ liệu trong DataTable rồi mới ghi xuống CSDL.
        /// </summary>
        public void LuuDichVu(int maHD, DataTable chiTiet)
        {
            if (maHD <= 0)
            {
                throw new InvalidOperationException("Chưa chọn bàn đang chơi để gọi dịch vụ.");
            }

            var ds = new List<ChiTietHoaDonDTO>();
            if (chiTiet != null)
            {
                foreach (DataRow r in chiTiet.Rows)
                {
                    int soLuong = Convert.ToInt32(r["SoLuong"]);
                    if (soLuong <= 0)
                    {
                        throw new InvalidOperationException("Số lượng của mỗi dịch vụ phải lớn hơn 0.");
                    }

                    ds.Add(new ChiTietHoaDonDTO
                    {
                        MaHD = maHD,
                        MaDV = Convert.ToInt32(r["MaDV"]),
                        TenDV = Convert.ToString(r["TenDV"]),
                        SoLuong = soLuong,
                        DonGia = Convert.ToDecimal(r["DonGia"]),
                        TruKho = r["TruKho"] != DBNull.Value && Convert.ToBoolean(r["TruKho"])
                    });
                }
            }

            dal.LuuDichVu(maHD, ds);
        }

        /// <summary>Tính tiền dự kiến (dùng cho hộp thoại xác nhận và bảng thanh toán).</summary>
        public HoaDonDTO XemTruoc(int maHD)
        {
            HoaDonDTO hd = dal.GetHoaDon(maHD);
            if (hd == null)
            {
                throw new InvalidOperationException("Không tìm thấy hóa đơn " + maHD + ".");
            }

            if (hd.TrangThai)
            {
                throw new InvalidOperationException("Hóa đơn này đã thanh toán.");
            }

            hd.GioKetThuc = DateTime.Now;
            hd.SoKhoi15 = TinhTien.SoKhoi(hd.GioBatDau, DateTime.Now);
            hd.TienGioChoi = TinhTien.TienGio(hd.GiaTheoGio, hd.SoKhoi15);
            return hd;
        }

        /// <summary>Thanh toán thật: để tầng DAL chốt giờ và tính tiền trong transaction.</summary>
        public HoaDonDTO ThanhToan(int maHD)
        {
            if (maHD <= 0)
            {
                throw new InvalidOperationException("Chưa chọn hóa đơn cần thanh toán.");
            }

            return dal.ThanhToan(maHD);
        }
    }
}
