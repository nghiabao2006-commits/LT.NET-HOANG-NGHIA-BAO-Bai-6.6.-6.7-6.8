using System;
using System.Data;
using Bai6_8_Bida.DAL;
using Bai6_8_Bida.DTO;

namespace Bai6_8_Bida.BUS
{
    /// <summary>BUS: nghiệp vụ menu dịch vụ (nước uống, mì, thuê gậy...).</summary>
    public class DichVuBUS
    {
        private readonly DichVuDAL dal = new DichVuDAL();

        public DataTable LayDanhSach()
        {
            return dal.GetAll();
        }

        public DichVuDTO LayTheoMa(int maDV)
        {
            return dal.GetById(maDV);
        }

        public void Them(DichVuDTO dv)
        {
            KiemTra(dv);
            dal.Insert(dv);
        }

        public void Sua(DichVuDTO dv)
        {
            if (dv.MaDV <= 0)
            {
                throw new InvalidOperationException("Chưa chọn dịch vụ cần sửa.");
            }

            KiemTra(dv);
            dal.Update(dv);
        }

        public void Xoa(int maDV)
        {
            if (maDV <= 0)
            {
                throw new InvalidOperationException("Chưa chọn dịch vụ cần xoá.");
            }

            if (dal.DemChiTiet(maDV) > 0)
            {
                throw new InvalidOperationException(
                    "Dịch vụ này đã xuất hiện trong hóa đơn nên không xoá được.");
            }

            dal.Delete(maDV);
        }

        private static void KiemTra(DichVuDTO dv)
        {
            if (string.IsNullOrWhiteSpace(dv.TenDV))
            {
                throw new InvalidOperationException("Tên dịch vụ không được để trống.");
            }

            if (dv.TenDV.Trim().Length > 100)
            {
                throw new InvalidOperationException("Tên dịch vụ tối đa 100 ký tự.");
            }

            if (dv.DonGia < 0)
            {
                throw new InvalidOperationException("Đơn giá không được âm.");
            }

            if (dv.TonKho < 0)
            {
                throw new InvalidOperationException("Tồn kho không được âm.");
            }

            if (dv.TruKho && dv.TonKho == 0)
            {
                throw new InvalidOperationException(
                    "Dịch vụ có trừ kho phải có tồn kho lớn hơn 0 (dịch vụ thuê gậy hãy bỏ chọn Trừ kho).");
            }

            dv.TenDV = dv.TenDV.Trim();
        }
    }
}
