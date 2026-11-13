using System;

namespace Bai6_6_ThuVien.DTO
{
    /// <summary>DTO: một dòng của bảng PhieuMuon (kèm tên độc giả để hiển thị).</summary>
    public class PhieuMuonDTO
    {
        public int MaPhieu { get; set; }
        public int MaDocGia { get; set; }
        public string TenDocGia { get; set; }
        public DateTime NgayMuon { get; set; }
        public DateTime HanTra { get; set; }
        public string TrangThai { get; set; }

        public bool QuaHan
        {
            get { return HanTra.Date < DateTime.Today && TrangThai != "Đã trả"; }
        }
    }
}
