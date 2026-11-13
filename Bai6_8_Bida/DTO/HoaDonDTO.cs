using System;

namespace Bai6_8_Bida.DTO
{
    /// <summary>
    /// DTO: một hóa đơn (bảng HoaDon) kèm thông tin bàn để hiển thị / in biên lai.
    /// </summary>
    public class HoaDonDTO
    {
        public int MaHD { get; set; }
        public int MaBan { get; set; }
        public string TenBan { get; set; }
        public string LoaiBan { get; set; }
        public decimal GiaTheoGio { get; set; }
        public DateTime GioBatDau { get; set; }
        public DateTime? GioKetThuc { get; set; }
        public decimal TienGioChoi { get; set; }
        public decimal TienDichVu { get; set; }
        public bool TrangThai { get; set; }

        /// <summary>Số khối 15 phút đã tính tiền.</summary>
        public int SoKhoi15 { get; set; }

        public decimal SoGio
        {
            get { return SoKhoi15 * 15m / 60m; }
        }

        public decimal TongCong
        {
            get { return TienGioChoi + TienDichVu; }
        }

        public bool DaThanhToan
        {
            get { return TrangThai; }
        }
    }
}
