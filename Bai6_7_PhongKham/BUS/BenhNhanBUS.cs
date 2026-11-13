using System;
using System.Data;
using Bai6_7_PhongKham.DAL;
using Bai6_7_PhongKham.DTO;

namespace Bai6_7_PhongKham.BUS
{
    /// <summary>BUS: nghiệp vụ tiếp nhận bệnh nhân.</summary>
    public class BenhNhanBUS
    {
        private readonly BenhNhanDAL dal = new BenhNhanDAL();

        public DataTable LayDanhSach()
        {
            return dal.GetAll();
        }

        public BenhNhanDTO TraCuu(string soDienThoai)
        {
            if (string.IsNullOrWhiteSpace(soDienThoai))
            {
                return null;
            }

            return dal.TimTheoSoDienThoai(soDienThoai.Trim());
        }

        public BenhNhanDTO LayTheoMa(int maBN)
        {
            return dal.GetById(maBN);
        }

        public string KiemTra(BenhNhanDTO bn)
        {
            if (string.IsNullOrWhiteSpace(bn.HoTen))
            {
                return "Họ tên bệnh nhân không được để trống.";
            }

            if (!string.IsNullOrWhiteSpace(bn.SoDienThoai))
            {
                string sdt = bn.SoDienThoai.Trim();
                if (sdt.Length < 10 || sdt.Length > 11)
                {
                    return "Số điện thoại phải có 10 - 11 chữ số.";
                }

                foreach (char c in sdt)
                {
                    if (!char.IsDigit(c))
                    {
                        return "Số điện thoại chỉ được chứa chữ số.";
                    }
                }

                if (dal.SoDienThoaiDaTonTai(sdt, bn.MaBN))
                {
                    return "Số điện thoại này đã thuộc về bệnh nhân khác.";
                }
            }

            int namHienTai = DateTime.Today.Year;
            if (bn.NamSinh.HasValue && (bn.NamSinh < 1900 || bn.NamSinh > namHienTai))
            {
                return "Năm sinh phải nằm trong khoảng 1900 - " + namHienTai + ".";
            }

            return string.Empty;
        }

        /// <summary>Lưu bệnh nhân: nếu MaBN = 0 thì thêm mới, ngược lại cập nhật.</summary>
        public int Luu(BenhNhanDTO bn)
        {
            string loi = KiemTra(bn);
            if (loi.Length > 0)
            {
                throw new InvalidOperationException(loi);
            }

            bn.HoTen = bn.HoTen.Trim();
            bn.SoDienThoai = string.IsNullOrWhiteSpace(bn.SoDienThoai) ? null : bn.SoDienThoai.Trim();

            if (bn.MaBN <= 0)
            {
                return dal.Insert(bn);
            }

            dal.Update(bn);
            return bn.MaBN;
        }
    }
}
