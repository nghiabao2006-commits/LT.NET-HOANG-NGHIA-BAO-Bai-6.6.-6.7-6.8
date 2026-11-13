namespace Bai6_7_PhongKham.DTO
{
    /// <summary>DTO: một dòng của bảng Thuoc.</summary>
    public class ThuocDTO
    {
        public int MaThuoc { get; set; }
        public string TenThuoc { get; set; }
        public string DonViTinh { get; set; }
        public decimal DonGia { get; set; }
        public int SoLuongTon { get; set; }

        public override string ToString()
        {
            return TenThuoc + " (" + DonViTinh + ") - tồn " + SoLuongTon;
        }
    }
}
