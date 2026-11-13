namespace Bai6_6_ThuVien.DTO
{
    /// <summary>DTO: một dòng của bảng DauSach.</summary>
    public class DauSachDTO
    {
        public int MaSach { get; set; }
        public string TenSach { get; set; }
        public string TheLoai { get; set; }
        public int SoLuongCon { get; set; }

        public override string ToString()
        {
            return TenSach + " (còn " + SoLuongCon + ")";
        }
    }
}
