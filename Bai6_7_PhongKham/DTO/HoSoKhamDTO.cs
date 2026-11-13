using System;

namespace Bai6_7_PhongKham.DTO
{
    /// <summary>DTO: một dòng của bảng HoSoKham (kèm thông tin bệnh nhân để hiển thị).</summary>
    public class HoSoKhamDTO
    {
        public int MaHoSo { get; set; }
        public int MaBN { get; set; }
        public string TenBenhNhan { get; set; }
        public string SoDienThoai { get; set; }
        public DateTime NgayKham { get; set; }
        public string TrieuChung { get; set; }
        public string ChanDoan { get; set; }

        public override string ToString()
        {
            return "#" + MaHoSo + " - " + TenBenhNhan + " (" + NgayKham.ToString("dd/MM/yyyy") + ")";
        }
    }
}
