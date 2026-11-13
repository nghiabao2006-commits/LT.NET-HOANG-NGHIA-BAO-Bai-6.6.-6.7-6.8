using System;

namespace Bai6_8_Bida.DTO
{
    /// <summary>
    /// Quy tắc tính tiền giờ chơi dùng chung cho tầng DAL (lúc ghi hóa đơn)
    /// và tầng BUS/GUI (lúc xem trước). Thời gian chơi được làm tròn lên
    /// theo từng khối 15 phút: số khối = ceil(số phút / 15).
    /// </summary>
    public static class TinhTien
    {
        /// <summary>Độ dài một khối tính tiền (phút).</summary>
        public const int PhutMoiKhoi = 15;

        /// <summary>
        /// Số khối 15 phút phải trả. Chơi chưa đủ một khối vẫn tính 1 khối
        /// (quy ước tối thiểu của quán bida), sau đó làm tròn lên.
        /// </summary>
        public static int SoKhoi(DateTime gioBatDau, DateTime gioKetThuc)
        {
            double soPhut = (gioKetThuc - gioBatDau).TotalMinutes;
            if (soPhut <= 0)
            {
                return 1;
            }

            return (int)Math.Ceiling(soPhut / PhutMoiKhoi);
        }

        /// <summary>Tiền giờ = giá mỗi giờ * (số khối * 15 / 60) giờ.</summary>
        public static decimal TienGio(decimal giaTheoGio, int soKhoi)
        {
            decimal soGio = soKhoi * (decimal)PhutMoiKhoi / 60m;
            return Math.Round(giaTheoGio * soGio, 2, MidpointRounding.AwayFromZero);
        }
    }
}
