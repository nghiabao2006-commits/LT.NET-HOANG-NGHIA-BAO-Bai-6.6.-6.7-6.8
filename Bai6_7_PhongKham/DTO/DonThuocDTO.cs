using System;

namespace Bai6_7_PhongKham.DTO
{
    /// <summary>DTO: một dòng của bảng DonThuoc (kèm tên bệnh nhân để hiển thị).</summary>
    public class DonThuocDTO
    {
        public int MaDon { get; set; }
        public int MaHoSo { get; set; }
        public string TenBenhNhan { get; set; }
        public DateTime NgayKham { get; set; }
        public decimal TienKham { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; }
    }
}
