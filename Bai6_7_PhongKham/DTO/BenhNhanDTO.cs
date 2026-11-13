namespace Bai6_7_PhongKham.DTO
{
    /// <summary>DTO: một dòng của bảng BenhNhan.</summary>
    public class BenhNhanDTO
    {
        public int MaBN { get; set; }
        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }
        public int? NamSinh { get; set; }

        public override string ToString()
        {
            return HoTen + " - " + SoDienThoai;
        }
    }
}
