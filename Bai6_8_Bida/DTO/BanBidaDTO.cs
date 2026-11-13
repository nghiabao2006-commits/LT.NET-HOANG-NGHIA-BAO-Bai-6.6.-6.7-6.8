namespace Bai6_8_Bida.DTO
{
    /// <summary>DTO: một bàn bida (bảng BanBida).</summary>
    public class BanBidaDTO
    {
        public int MaBan { get; set; }
        public string TenBan { get; set; }
        public string LoaiBan { get; set; }
        public decimal GiaTheoGio { get; set; }
        public string TrangThai { get; set; }

        public bool DangChoi
        {
            get { return TrangThai == "Đang chơi"; }
        }
    }
}
