using System;
using System.Collections.Generic;
using System.Data;
using Bai6_6_ThuVien.DAL;
using Bai6_6_ThuVien.DTO;

namespace Bai6_6_ThuVien.BUS
{
    /// <summary>BUS: nghiệp vụ mượn / trả sách.</summary>
    public class PhieuMuonBUS
    {
        private readonly PhieuMuonDAL dal = new PhieuMuonDAL();
        private readonly DocGiaBUS docGiaBus = new DocGiaBUS();

        public DataTable LayDanhSach(int? maDocGia, string trangThai)
        {
            return dal.GetPhieu(maDocGia, trangThai);
        }

        public DataTable LayChiTiet(int maPhieu)
        {
            return dal.GetChiTiet(maPhieu);
        }

        /// <summary>
        /// Lập phiếu mượn. Trước khi ghi dữ liệu phải qua hai cửa kiểm tra:
        ///   1. Thẻ độc giả còn hạn (NgayHetHanThe >= hôm nay).
        ///   2. Hạn trả phải sau ngày mượn.
        /// </summary>
        public int LapPhieu(int maDocGia, DateTime hanTra, List<ChiTietMuonDTO> chiTiet)
        {
            if (maDocGia <= 0)
            {
                throw new InvalidOperationException("Chưa chọn độc giả.");
            }

            string thongBao;
            if (!docGiaBus.TheConHan(maDocGia, out thongBao))
            {
                throw new InvalidOperationException(thongBao);
            }

            if (chiTiet == null || chiTiet.Count == 0)
            {
                throw new InvalidOperationException("Chưa chọn sách nào cho phiếu mượn.");
            }

            if (hanTra.Date <= DateTime.Today)
            {
                throw new InvalidOperationException("Hạn trả phải sau ngày mượn (hôm nay).");
            }

            return dal.LapPhieu(maDocGia, hanTra, chiTiet);
        }

        public void TraSach(int maPhieu)
        {
            if (maPhieu <= 0)
            {
                throw new InvalidOperationException("Chưa chọn phiếu cần trả.");
            }

            dal.TraSach(maPhieu);
        }

        /// <summary>Đồng bộ trạng thái quá hạn rồi lấy danh sách cảnh báo.</summary>
        public DataTable LayDanhSachQuaHan()
        {
            dal.CapNhatTrangThaiQuaHan();
            return dal.GetQuaHan();
        }

        public DataTable ThongKeTheoTheLoai()
        {
            return dal.ThongKeTheoTheLoai();
        }

        public int DemTongLuotMuon()
        {
            return dal.DemTongLuotMuon();
        }
    }
}
