namespace Bai6_7_PhongKham.DTO
{
    /// <summary>DTO: một dòng của bảng ChiTietDonThuoc (kèm tên thuốc, đơn giá để tính tiền).</summary>
    public class ChiTietDonThuocDTO
    {
        public int MaDon { get; set; }
        public int MaThuoc { get; set; }
        public string TenThuoc { get; set; }
        public string DonViTinh { get; set; }
        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }
        public string LieuDung { get; set; }

        public decimal ThanhTien
        {
            get { return SoLuong * DonGia; }
        }

        public ChiTietDonThuocDTO() { }

        public ChiTietDonThuocDTO(int maThuoc, string tenThuoc, string donViTinh,
            decimal donGia, int soLuong, string lieuDung)
        {
            MaThuoc = maThuoc;
            TenThuoc = tenThuoc;
            DonViTinh = donViTinh;
            DonGia = donGia;
            SoLuong = soLuong;
            LieuDung = lieuDung;
        }
    }
}
