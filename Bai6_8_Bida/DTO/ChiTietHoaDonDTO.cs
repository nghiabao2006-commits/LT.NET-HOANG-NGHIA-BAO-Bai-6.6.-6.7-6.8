using System;

namespace Bai6_8_Bida.DTO
{
    /// <summary>DTO: một dòng chi tiết hóa đơn (bảng ChiTietHoaDon).</summary>
    public class ChiTietHoaDonDTO
    {
        public int MaHD { get; set; }
        public int MaDV { get; set; }
        public string TenDV { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public bool TruKho { get; set; }

        public decimal ThanhTien
        {
            get { return SoLuong * DonGia; }
        }
    }
}
