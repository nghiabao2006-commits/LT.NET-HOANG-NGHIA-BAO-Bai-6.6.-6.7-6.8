namespace Bai6_8_Bida.DTO
{
    /// <summary>
    /// DTO: một dịch vụ trong menu (bảng MenuDichVu).
    /// TruKho = true  -> trừ tồn kho khi thanh toán (nước uống, mì xào...).
    /// TruKho = false -> không trừ kho (thuê gậy, khăn cho thuê...).
    /// </summary>
    public class DichVuDTO
    {
        public int MaDV { get; set; }
        public string TenDV { get; set; }
        public decimal DonGia { get; set; }
        public int TonKho { get; set; }
        public bool TruKho { get; set; }

        public string TruKhoText
        {
            get { return TruKho ? "Có" : "Không"; }
        }
    }
}
