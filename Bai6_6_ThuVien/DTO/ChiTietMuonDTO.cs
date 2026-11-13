namespace Bai6_6_ThuVien.DTO
{
    /// <summary>DTO: một dòng của bảng ChiTietMuon (kèm tên sách để hiển thị).</summary>
    public class ChiTietMuonDTO
    {
        public int MaPhieu { get; set; }
        public int MaSach { get; set; }
        public string TenSach { get; set; }
        public int SoLuong { get; set; }

        public ChiTietMuonDTO() { }

        public ChiTietMuonDTO(int maSach, string tenSach, int soLuong)
        {
            MaSach = maSach;
            TenSach = tenSach;
            SoLuong = soLuong;
        }
    }
}
