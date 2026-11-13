using System;

namespace Bai6_6_ThuVien.DTO
{
    /// <summary>DTO: một dòng của bảng DocGia.</summary>
    public class DocGiaDTO
    {
        public int MaDocGia { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public DateTime NgayHetHanThe { get; set; }

        public bool TheConHan
        {
            get { return NgayHetHanThe.Date >= DateTime.Today; }
        }

        public override string ToString()
        {
            return HoTen;
        }
    }
}
