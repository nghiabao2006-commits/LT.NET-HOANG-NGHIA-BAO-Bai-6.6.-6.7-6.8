using System;
using System.Data;
using Bai6_7_PhongKham.DAL;
using Bai6_7_PhongKham.DTO;

namespace Bai6_7_PhongKham.BUS
{
    /// <summary>BUS: nghiệp vụ hồ sơ khám.</summary>
    public class HoSoKhamBUS
    {
        private readonly HoSoKhamDAL dal = new HoSoKhamDAL();
        private readonly BenhNhanBUS benhNhanBus = new BenhNhanBUS();

        public DataTable LayDanhSach()
        {
            return dal.GetDanhSach();
        }

        public HoSoKhamDTO LayTheoMa(int maHoSo)
        {
            return dal.GetById(maHoSo);
        }

        /// <summary>
        /// Tạo hồ sơ khám mới. Triệu chứng là bắt buộc, chẩn đoán có thể bổ sung sau.
        /// </summary>
        public int TaoHoSo(int maBN, string trieuChung, string chanDoan)
        {
            if (maBN <= 0)
            {
                throw new InvalidOperationException("Chưa có bệnh nhân. Hãy lưu thông tin bệnh nhân trước.");
            }

            if (string.IsNullOrWhiteSpace(trieuChung))
            {
                throw new InvalidOperationException("Chưa nhập triệu chứng.");
            }

            if (trieuChung.Trim().Length > 2000)
            {
                throw new InvalidOperationException("Triệu chứng quá dài (tối đa 2000 ký tự).");
            }

            if (!string.IsNullOrWhiteSpace(chanDoan) && chanDoan.Trim().Length > 250)
            {
                throw new InvalidOperationException("Chẩn đoán tối đa 250 ký tự.");
            }

            return dal.Insert(maBN, trieuChung.Trim(), chanDoan == null ? null : chanDoan.Trim());
        }

        public void CapNhatChanDoan(int maHoSo, string chanDoan)
        {
            if (maHoSo <= 0)
            {
                throw new InvalidOperationException("Chưa chọn hồ sơ khám.");
            }

            if (!string.IsNullOrWhiteSpace(chanDoan) && chanDoan.Trim().Length > 250)
            {
                throw new InvalidOperationException("Chẩn đoán tối đa 250 ký tự.");
            }

            dal.UpdateChanDoan(maHoSo, chanDoan == null ? null : chanDoan.Trim());
        }

        public void KiemTraXoaHoSo(int maHoSo)
        {
            if (dal.DemDonThuoc(maHoSo) > 0)
            {
                throw new InvalidOperationException("Hồ sơ này đã có đơn thuốc nên không xoá được.");
            }
        }
    }
}
