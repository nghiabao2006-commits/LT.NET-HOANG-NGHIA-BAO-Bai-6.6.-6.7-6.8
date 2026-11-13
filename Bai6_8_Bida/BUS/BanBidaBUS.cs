using System;
using System.Collections.Generic;
using System.Data;
using Bai6_8_Bida.DAL;
using Bai6_8_Bida.DTO;

namespace Bai6_8_Bida.BUS
{
    /// <summary>BUS: nghiệp vụ quản lý bàn bida (sơ đồ bàn + danh mục bàn).</summary>
    public class BanBidaBUS
    {
        public static readonly string[] LoaiBanHopLe =
            { "Pool (Lỗ)", "Carom (Phăng)", "Snooker" };

        private readonly BanBidaDAL dal = new BanBidaDAL();

        public DataTable LayBang()
        {
            return dal.GetAll();
        }

        /// <summary>Đổ danh sách bàn sang DTO để giao diện vẽ sơ đồ bàn.</summary>
        public List<BanBidaDTO> LaySoDo()
        {
            var ds = new List<BanBidaDTO>();
            DataTable t = dal.GetAll();
            foreach (DataRow r in t.Rows)
            {
                ds.Add(new BanBidaDTO
                {
                    MaBan = Convert.ToInt32(r["MaBan"]),
                    TenBan = Convert.ToString(r["TenBan"]),
                    LoaiBan = Convert.ToString(r["LoaiBan"]),
                    GiaTheoGio = Convert.ToDecimal(r["GiaTheoGio"]),
                    TrangThai = Convert.ToString(r["TrangThai"])
                });
            }

            return ds;
        }

        public BanBidaDTO LayTheoMa(int maBan)
        {
            return dal.GetById(maBan);
        }

        public void Them(BanBidaDTO ban)
        {
            KiemTra(ban);
            dal.Insert(ban);
        }

        public void Sua(BanBidaDTO ban)
        {
            if (ban.MaBan <= 0)
            {
                throw new InvalidOperationException("Chưa chọn bàn cần sửa.");
            }

            KiemTra(ban);
            dal.Update(ban);
        }

        public void Xoa(int maBan)
        {
            if (maBan <= 0)
            {
                throw new InvalidOperationException("Chưa chọn bàn cần xoá.");
            }

            BanBidaDTO ban = dal.GetById(maBan);
            if (ban != null && ban.DangChoi)
            {
                throw new InvalidOperationException("Bàn đang có khách chơi nên không xoá được.");
            }

            if (dal.DemHoaDon(maBan) > 0)
            {
                throw new InvalidOperationException(
                    "Bàn này đã có hóa đơn nên không xoá được. Chỉ nên sửa thông tin bàn.");
            }

            dal.Delete(maBan);
        }

        private static void KiemTra(BanBidaDTO ban)
        {
            if (string.IsNullOrWhiteSpace(ban.TenBan))
            {
                throw new InvalidOperationException("Tên bàn không được để trống.");
            }

            if (ban.TenBan.Trim().Length > 50)
            {
                throw new InvalidOperationException("Tên bàn tối đa 50 ký tự.");
            }

            if (Array.IndexOf(LoaiBanHopLe, ban.LoaiBan) < 0)
            {
                throw new InvalidOperationException("Loại bàn phải là Pool (Lỗ), Carom (Phăng) hoặc Snooker.");
            }

            if (ban.GiaTheoGio <= 0)
            {
                throw new InvalidOperationException("Giá theo giờ phải lớn hơn 0.");
            }

            ban.TenBan = ban.TenBan.Trim();
        }
    }
}
