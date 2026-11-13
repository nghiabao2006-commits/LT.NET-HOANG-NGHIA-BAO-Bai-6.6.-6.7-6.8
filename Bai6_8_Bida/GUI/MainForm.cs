using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Bai6_8_Bida.BUS;
using Bai6_8_Bida.DTO;

namespace Bai6_8_Bida.GUI
{
    /// <summary>
    /// Giao diện chính (Presentation Layer). Form chỉ gọi xuống BUS,
    /// không viết câu SQL nào và không mở kết nối trực tiếp.
    /// </summary>
    public class MainForm : Form
    {
        // ---------- BUS ----------
        private readonly BanBidaBUS banBus = new BanBidaBUS();
        private readonly DichVuBUS dichVuBus = new DichVuBUS();
        private readonly HoaDonBUS hoaDonBus = new HoaDonBUS();

        // ---------- Trạng thái nội bộ ----------
        private readonly List<BanBidaDTO> dsBan = new List<BanBidaDTO>();
        private DataTable bangChiTiet;      // bảng chi tiết ngắt kết nối của tab Gọi dịch vụ
        private int maBanDangChon;
        private int maHDDangChon;           // hóa đơn mở của bàn đang chọn (tab 1)
        private int maHDGoiDichVu;          // hóa đơn đang gọi dịch vụ (tab 2)
        private int maHDThanhToan;          // hóa đơn đang chọn thanh toán (tab 3)
        private int maBanSua;
        private int maDVSua;
        private bool dangTai;               // chặn sự kiện khi đang nạp dữ liệu

        // ---------- Điều khiển chung ----------
        private TabControl tabs;
        private TabPage tabSoDo, tabGoiDichVu, tabThanhToan, tabQuanLy, tabLichSu;
        private ToolStripStatusLabel lblTrangThai;

        // Tab 1 - Sơ đồ bàn
        private Panel pnlBan;
        private Label lblBanTen, lblBanLoai, lblBanGia, lblBanTrangThai, lblBanHoaDon, lblBanTien;
        private Button btnMoBan, btnThanhToanBan, btnLamMoiSoDo, btnGoiDichVu;
        private DataGridView dgvHoaDonMo;

        // Tab 2 - Gọi dịch vụ
        private ComboBox cboBanGoi, cboDichVu;
        private Label lblGoiGio, lblTongDichVu;
        private NumericUpDown numSoLuong;
        private DataGridView dgvChiTiet, dgvMenu;

        // Tab 3 - Thanh toán
        private DataGridView dgvChoThanhToan;
        private Label lblXemGioChoi, lblXemTienGio, lblXemTienDv, lblXemTong, lblKetQua;
        private Button btnThanhToan, btnLamMoiTt;

        // Tab 4 - Quản lý bàn & menu
        private TextBox txtTenBan, txtTenDv;
        private ComboBox cboLoaiBan;
        private NumericUpDown numGiaBan, numDonGiaDv, numTonKho;
        private CheckBox chkTruKho;
        private Label lblTrangThaiBan;
        private DataGridView dgvBan, dgvDichVu;

        // Tab 5 - Lịch sử & doanh thu
        private DataGridView dgvLichSu, dgvThongKe;
        private Label lblDoanhThu;

        public MainForm()
        {
            KhoiTaoGiaoDien();
        }

        // =====================================================================
        //  Dựng giao diện
        // =====================================================================
        private void KhoiTaoGiaoDien()
        {
            Text = "Bài 6.8 - Hệ thống quản lý bàn bida & dịch vụ giải trí";
            StartPosition = FormStartPosition.CenterScreen;
            // Giao diện dựng bằng code nên tắt auto-scale để toạ độ tuyệt đối giữ nguyên
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1180, 720);
            MinimumSize = new Size(1024, 660);
            Font = new Font("Segoe UI", 9.5F);

            tabs = new TabControl { Dock = DockStyle.Fill };
            tabSoDo = TaoTabSoDo();
            tabGoiDichVu = TaoTabGoiDichVu();
            tabThanhToan = TaoTabThanhToan();
            tabQuanLy = TaoTabQuanLy();
            tabLichSu = TaoTabLichSu();
            tabs.TabPages.Add(tabSoDo);
            tabs.TabPages.Add(tabGoiDichVu);
            tabs.TabPages.Add(tabThanhToan);
            tabs.TabPages.Add(tabQuanLy);
            tabs.TabPages.Add(tabLichSu);

            var status = new StatusStrip { SizingGrip = false };
            lblTrangThai = new ToolStripStatusLabel("Sẵn sàng.");
            status.Items.Add(lblTrangThai);

            Controls.Add(tabs);
            Controls.Add(status);

            Load += MainForm_Load;
        }

        /// <summary>
        /// TabPage mới tạo mặc định chỉ 200x100; nếu gắn Anchor cho control con ngay
        /// thì khi TabControl phóng trang ra, control sẽ bị kéo giãn sai.
        /// Vì vậy cấp sẵn kích thước vùng hiển thị ước tính của trang.
        /// </summary>
        private Size KichThuocTab()
        {
            return new Size(1172, 664);
        }

        // ------------------------------------------------------------------
        //  Tab 1: sơ đồ bàn
        // ------------------------------------------------------------------
        private TabPage TaoTabSoDo()
        {
            var tab = new TabPage("1. Sơ đồ bàn") { Padding = new Padding(6), Size = KichThuocTab() };

            var gbSoDo = new GroupBox
            {
                Text = "Sơ đồ bàn (xanh = trống, cam = đang chơi)",
                Location = new Point(4, 4),
                Size = new Size(796, 648)
            };

            pnlBan = new Panel
            {
                Location = new Point(10, 24),
                Size = new Size(776, 614),
                AutoScroll = true,
                BackColor = Color.White
            };
            gbSoDo.Controls.Add(pnlBan);

            var gbChon = new GroupBox
            {
                Text = "Bàn đang chọn",
                Location = new Point(808, 4),
                Size = new Size(358, 296)
            };

            lblBanTen = new Label
            {
                Location = new Point(12, 24),
                Size = new Size(332, 28),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 56, 100),
                Text = "—"
            };
            lblBanLoai = TaoNhan("Loại bàn: —", 12, 56);
            lblBanGia = TaoNhan("Giá: —", 12, 80);
            lblBanTrangThai = TaoNhan("Trạng thái: —", 12, 104);
            lblBanHoaDon = TaoNhan("Hóa đơn mở: —", 12, 128);
            lblBanTien = new Label
            {
                Location = new Point(12, 152),
                Size = new Size(332, 24),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(160, 40, 40),
                Text = "Tạm tính: 0 đ"
            };

            btnMoBan = TaoNut("Mở bàn", 12, 186, 160);
            btnThanhToanBan = TaoNut("Thanh toán", 184, 186, 160);
            btnGoiDichVu = TaoNut("Gọi dịch vụ", 12, 226, 160);
            btnLamMoiSoDo = TaoNut("Làm mới sơ đồ", 184, 226, 160);
            btnMoBan.Click += BtnMoBan_Click;
            btnThanhToanBan.Click += BtnThanhToanBan_Click;
            btnGoiDichVu.Click += BtnGoiDichVu_Click;
            btnLamMoiSoDo.Click += (s, e) => { TaiTatCa(); Bao("Đã làm mới sơ đồ bàn."); };

            gbChon.Controls.AddRange(new Control[]
            {
                lblBanTen, lblBanLoai, lblBanGia, lblBanTrangThai, lblBanHoaDon, lblBanTien,
                btnMoBan, btnThanhToanBan, btnGoiDichVu, btnLamMoiSoDo
            });

            var gbHoaDon = new GroupBox
            {
                Text = "Hóa đơn chưa thanh toán",
                Location = new Point(808, 308),
                Size = new Size(358, 344)
            };
            dgvHoaDonMo = TaoLuoi(10, 24, 338, 308);
            gbHoaDon.Controls.Add(dgvHoaDonMo);

            tab.Controls.Add(gbSoDo);
            tab.Controls.Add(gbChon);
            tab.Controls.Add(gbHoaDon);
            return tab;
        }

        // ------------------------------------------------------------------
        //  Tab 2: gọi dịch vụ (mô hình ngắt kết nối)
        // ------------------------------------------------------------------
        private TabPage TaoTabGoiDichVu()
        {
            var tab = new TabPage("2. Gọi dịch vụ") { Padding = new Padding(6), Size = KichThuocTab() };

            var gbChiTiet = new GroupBox
            {
                Text = "Chi tiết hóa đơn của bàn đang chơi",
                Location = new Point(4, 4),
                Size = new Size(716, 648)
            };

            gbChiTiet.Controls.Add(TaoNhan("Bàn đang chơi", 12, 24));
            cboBanGoi = new ComboBox
            {
                Location = new Point(12, 44),
                Size = new Size(310, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboBanGoi.SelectedIndexChanged += CboBanGoi_SelectedIndexChanged;
            gbChiTiet.Controls.Add(cboBanGoi);

            lblGoiGio = new Label
            {
                Location = new Point(334, 48),
                Size = new Size(370, 22),
                ForeColor = Color.DimGray,
                Text = "Chưa chọn hóa đơn."
            };
            gbChiTiet.Controls.Add(lblGoiGio);

            gbChiTiet.Controls.Add(TaoNhan("Dịch vụ", 12, 84));
            cboDichVu = new ComboBox
            {
                Location = new Point(12, 104),
                Size = new Size(330, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            gbChiTiet.Controls.Add(cboDichVu);

            gbChiTiet.Controls.Add(TaoNhan("SL", 352, 84));
            numSoLuong = new NumericUpDown
            {
                Location = new Point(352, 106),
                Size = new Size(66, 26),
                Minimum = 1,
                Maximum = 999,
                Value = 1
            };
            gbChiTiet.Controls.Add(numSoLuong);

            var btnThem = TaoNut("Thêm / Tăng", 430, 104, 140);
            var btnBot = TaoNut("Giảm SL", 582, 104, 122);
            btnThem.Click += BtnThemDichVu_Click;
            btnBot.Click += BtnBotDichVu_Click;
            gbChiTiet.Controls.AddRange(new Control[] { btnThem, btnBot });

            dgvChiTiet = TaoLuoi(12, 148, 692, 400);
            gbChiTiet.Controls.Add(dgvChiTiet);

            var btnBo = TaoNut("Bỏ dòng", 12, 558, 120);
            btnBo.Click += BtnBoDong_Click;
            gbChiTiet.Controls.Add(btnBo);

            lblTongDichVu = new Label
            {
                Location = new Point(146, 564),
                Size = new Size(404, 24),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 56, 100),
                Text = "Tổng dịch vụ: 0 đ"
            };
            gbChiTiet.Controls.Add(lblTongDichVu);

            var btnLuu = TaoNut("Lưu dịch vụ", 560, 556, 144);
            btnLuu.Click += BtnLuuDichVu_Click;
            gbChiTiet.Controls.Add(btnLuu);

            var gbMenu = new GroupBox
            {
                Text = "Menu dịch vụ",
                Location = new Point(728, 4),
                Size = new Size(438, 648)
            };

            var lblGhiChu = new Label
            {
                Location = new Point(12, 22),
                Size = new Size(414, 34),
                ForeColor = Color.DimGray,
                Text = "Chọn dịch vụ rồi bấm Thêm / Tăng để sửa hóa đơn trong bộ nhớ, sau đó bấm \"Lưu dịch vụ\" để ghi xuống CSDL."
            };
            dgvMenu = TaoLuoi(12, 60, 414, 574);
            gbMenu.Controls.Add(lblGhiChu);
            gbMenu.Controls.Add(dgvMenu);

            tab.Controls.Add(gbChiTiet);
            tab.Controls.Add(gbMenu);
            return tab;
        }

        // ------------------------------------------------------------------
        //  Tab 3: thanh toán
        // ------------------------------------------------------------------
        private TabPage TaoTabThanhToan()
        {
            var tab = new TabPage("3. Thanh toán") { Padding = new Padding(6), Size = KichThuocTab() };

            var lblTieuDe = new Label
            {
                Location = new Point(10, 10),
                Size = new Size(600, 24),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 56, 100),
                Text = "Danh sách hóa đơn chưa thanh toán"
            };

            dgvChoThanhToan = TaoLuoi(10, 38, 1140, 300);
            dgvChoThanhToan.SelectionChanged += DgvChoThanhToan_SelectionChanged;

            var gbXem = new GroupBox
            {
                Text = "Xem trước hóa đơn (làm tròn 15 phút)",
                Location = new Point(10, 348),
                Size = new Size(1140, 200)
            };

            lblXemGioChoi = new Label
            {
                Location = new Point(12, 26),
                Size = new Size(1116, 24),
                Text = "Chọn một hóa đơn chưa thanh toán để xem trước."
            };
            lblXemTienGio = new Label { Location = new Point(12, 54), Size = new Size(1116, 24), Text = "" };
            lblXemTienDv = new Label { Location = new Point(12, 80), Size = new Size(1116, 24), Text = "" };
            lblXemTong = new Label
            {
                Location = new Point(12, 106),
                Size = new Size(700, 26),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(160, 40, 40),
                Text = ""
            };

            btnThanhToan = TaoNut("Thanh toán & in hóa đơn", 700, 148, 250);
            btnThanhToan.Click += BtnThanhToan_Click;
            btnLamMoiTt = TaoNut("Làm mới", 964, 148, 164);
            btnLamMoiTt.Click += (s, e) => { TaiTatCa(); Bao("Đã làm mới danh sách hóa đơn."); };
            gbXem.Controls.AddRange(new Control[]
            {
                lblXemGioChoi, lblXemTienGio, lblXemTienDv, lblXemTong, btnThanhToan, btnLamMoiTt
            });

            var gbKetQua = new GroupBox
            {
                Text = "Biên lai gần nhất",
                Location = new Point(10, 556),
                Size = new Size(1140, 92)
            };
            lblKetQua = new Label
            {
                Location = new Point(12, 24),
                Size = new Size(1116, 58),
                ForeColor = Color.FromArgb(20, 110, 60),
                Text = "Chưa có giao dịch nào trong phiên làm việc này."
            };
            gbKetQua.Controls.Add(lblKetQua);

            tab.Controls.Add(lblTieuDe);
            tab.Controls.Add(dgvChoThanhToan);
            tab.Controls.Add(gbXem);
            tab.Controls.Add(gbKetQua);
            return tab;
        }

        // ------------------------------------------------------------------
        //  Tab 4: quản lý bàn & menu dịch vụ
        // ------------------------------------------------------------------
        private TabPage TaoTabQuanLy()
        {
            var tab = new TabPage("4. Quản lý bàn & menu") { Padding = new Padding(6), Size = KichThuocTab() };

            var gbBan = new GroupBox
            {
                Text = "Thông tin bàn",
                Location = new Point(4, 4),
                Size = new Size(574, 200)
            };
            gbBan.Controls.Add(TaoNhan("Tên bàn", 12, 24));
            txtTenBan = TaoOChu(12, 44, 250);
            gbBan.Controls.Add(txtTenBan);

            gbBan.Controls.Add(TaoNhan("Loại bàn", 274, 24));
            cboLoaiBan = new ComboBox
            {
                Location = new Point(274, 44),
                Size = new Size(288, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboLoaiBan.Items.AddRange(BanBidaBUS.LoaiBanHopLe);
            gbBan.Controls.Add(cboLoaiBan);

            gbBan.Controls.Add(TaoNhan("Giá theo giờ (đ)", 12, 78));
            numGiaBan = new NumericUpDown
            {
                Location = new Point(12, 100),
                Size = new Size(150, 26),
                Minimum = 0,
                Maximum = 10000000,
                Increment = 5000
            };
            gbBan.Controls.Add(numGiaBan);

            gbBan.Controls.Add(TaoNhan("Trạng thái", 274, 78));
            lblTrangThaiBan = new Label
            {
                Location = new Point(274, 102),
                Size = new Size(288, 22),
                ForeColor = Color.DimGray,
                Text = "—"
            };
            gbBan.Controls.Add(lblTrangThaiBan);

            var btnThemBan = TaoNut("Thêm", 12, 148, 90);
            var btnSuaBan = TaoNut("Sửa", 110, 148, 90);
            var btnXoaBan = TaoNut("Xoá", 208, 148, 90);
            var btnMoiBan = TaoNut("Làm mới", 306, 148, 110);
            btnThemBan.Click += BtnThemBan_Click;
            btnSuaBan.Click += BtnSuaBan_Click;
            btnXoaBan.Click += BtnXoaBan_Click;
            btnMoiBan.Click += (s, e) => LamMoiBan();
            gbBan.Controls.AddRange(new Control[] { btnThemBan, btnSuaBan, btnXoaBan, btnMoiBan });

            var gbDv = new GroupBox
            {
                Text = "Thông tin dịch vụ",
                Location = new Point(584, 4),
                Size = new Size(584, 200)
            };
            gbDv.Controls.Add(TaoNhan("Tên dịch vụ", 12, 24));
            txtTenDv = TaoOChu(12, 44, 270);
            gbDv.Controls.Add(txtTenDv);

            gbDv.Controls.Add(TaoNhan("Đơn giá (đ)", 294, 24));
            numDonGiaDv = new NumericUpDown
            {
                Location = new Point(294, 44),
                Size = new Size(120, 26),
                Minimum = 0,
                Maximum = 10000000,
                Increment = 1000
            };
            gbDv.Controls.Add(numDonGiaDv);

            gbDv.Controls.Add(TaoNhan("Tồn kho", 426, 24));
            numTonKho = new NumericUpDown
            {
                Location = new Point(426, 44),
                Size = new Size(90, 26),
                Minimum = 0,
                Maximum = 100000
            };
            gbDv.Controls.Add(numTonKho);

            chkTruKho = new CheckBox
            {
                Location = new Point(12, 84),
                Size = new Size(340, 24),
                Checked = true,
                Text = "Trừ kho khi thanh toán (nước uống, mì...)"
            };
            gbDv.Controls.Add(chkTruKho);

            var btnThemDv = TaoNut("Thêm", 12, 130, 90);
            var btnSuaDv = TaoNut("Sửa", 110, 130, 90);
            var btnXoaDv = TaoNut("Xoá", 208, 130, 90);
            var btnMoiDv = TaoNut("Làm mới", 306, 130, 110);
            btnThemDv.Click += BtnThemDv_Click;
            btnSuaDv.Click += BtnSuaDv_Click;
            btnXoaDv.Click += BtnXoaDv_Click;
            btnMoiDv.Click += (s, e) => LamMoiDv();
            gbDv.Controls.AddRange(new Control[] { btnThemDv, btnSuaDv, btnXoaDv, btnMoiDv });

            var gbDsBan = new GroupBox
            {
                Text = "Danh sách bàn",
                Location = new Point(4, 210),
                Size = new Size(574, 436)
            };
            dgvBan = TaoLuoi(10, 24, 554, 402);
            dgvBan.SelectionChanged += DgvBan_SelectionChanged;
            gbDsBan.Controls.Add(dgvBan);

            var gbDsDv = new GroupBox
            {
                Text = "Danh sách dịch vụ",
                Location = new Point(584, 210),
                Size = new Size(584, 436)
            };
            dgvDichVu = TaoLuoi(10, 24, 564, 402);
            dgvDichVu.SelectionChanged += DgvDichVu_SelectionChanged;
            gbDsDv.Controls.Add(dgvDichVu);

            tab.Controls.Add(gbBan);
            tab.Controls.Add(gbDv);
            tab.Controls.Add(gbDsBan);
            tab.Controls.Add(gbDsDv);
            return tab;
        }

        // ------------------------------------------------------------------
        //  Tab 5: lịch sử & doanh thu
        // ------------------------------------------------------------------
        private TabPage TaoTabLichSu()
        {
            var tab = new TabPage("5. Lịch sử & doanh thu") { Padding = new Padding(6), Size = KichThuocTab() };

            var btnXem = TaoNut("Xem lịch sử", 10, 10, 150);
            btnXem.Click += (s, e) => { TaiLichSu(); Bao("Đã tải lịch sử hóa đơn đã thanh toán."); };

            lblDoanhThu = new Label
            {
                Location = new Point(176, 16),
                Size = new Size(960, 24),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 56, 100),
                Text = "Tổng doanh thu: —"
            };

            dgvLichSu = TaoLuoi(10, 50, 560, 532);
            dgvThongKe = TaoLuoi(580, 50, 570, 532);

            var lblGhiChu = new Label
            {
                Location = new Point(10, 590),
                Size = new Size(1140, 40),
                ForeColor = Color.DimGray,
                Text = "Lịch sử: các hóa đơn TrangThai = 1. Thống kê: GROUP BY theo bàn, " +
                       "tổng doanh thu lấy bằng ExecuteScalar."
            };

            tab.Controls.Add(btnXem);
            tab.Controls.Add(lblDoanhThu);
            tab.Controls.Add(dgvLichSu);
            tab.Controls.Add(dgvThongKe);
            tab.Controls.Add(lblGhiChu);
            return tab;
        }

        // ---------- Hàm phụ trợ dựng control ----------
        private static Label TaoNhan(string text, int x, int y)
        {
            return new Label { Text = text, Location = new Point(x, y), AutoSize = true };
        }

        private static TextBox TaoOChu(int x, int y, int w)
        {
            return new TextBox { Location = new Point(x, y), Size = new Size(w, 26) };
        }

        private static Button TaoNut(string text, int x, int y, int w)
        {
            return new Button { Text = text, Location = new Point(x, y), Size = new Size(w, 30) };
        }

        private static DataGridView TaoLuoi(int x, int y, int w, int h)
        {
            var g = new DataGridView
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                EnableHeadersVisualStyles = false
            };

            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(31, 56, 100);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersHeight = 30;
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 244, 250);
            g.RowTemplate.Height = 26;
            return g;
        }

        private static void DatTieuDe(DataGridView g, params string[] tieuDe)
        {
            for (int i = 0; i < tieuDe.Length && i < g.Columns.Count; i++)
            {
                g.Columns[i].HeaderText = tieuDe[i];
                g.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private static void AnCot(DataGridView g, string tenCot)
        {
            if (g.Columns[tenCot] != null)
            {
                g.Columns[tenCot].Visible = false;
            }
        }

        /// <summary>Đặt tiêu đề theo tên cột, dùng khi bảng có cột bị ẩn.</summary>
        private static void DatTieuDeTheoTen(DataGridView g, params string[] cap)
        {
            foreach (string s in cap)
            {
                string[] p = s.Split('|');
                if (p.Length == 2 && g.Columns[p[0]] != null)
                {
                    g.Columns[p[0]].HeaderText = p[1];
                    g.Columns[p[0]].SortMode = DataGridViewColumnSortMode.NotSortable;
                }
            }
        }

        /// <summary>Tăng tỉ lệ bề rộng cho cột chứa tên (chế độ Fill).</summary>
        private static void DatDoRong(DataGridView g, string tenCot, float trongSo)
        {
            if (g.Columns[tenCot] != null)
            {
                g.Columns[tenCot].FillWeight = trongSo;
            }
        }

        private class MucChon
        {
            public int Ma { get; set; }
            public int MaBan { get; set; }
            public string Ten { get; set; }
            public string MoTa { get; set; }
            public decimal Gia { get; set; }
            public int TonKho { get; set; }
            public bool TruKho { get; set; }

            public override string ToString()
            {
                return string.IsNullOrEmpty(MoTa) ? Ten : MoTa;
            }
        }

        private static string Tien(decimal soTien)
        {
            return soTien.ToString("#,##0") + " đ";
        }

        private void Bao(string thongBao)
        {
            lblTrangThai.Text = thongBao;
        }

        // =====================================================================
        //  Nạp dữ liệu
        // =====================================================================
        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                TaiTatCa();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi tải dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>Nạp lại toàn bộ màn hình sau mỗi nghiệp vụ.</summary>
        private void TaiTatCa()
        {
            dangTai = true;
            try
            {
                TaiSoDo();
                TaiHoaDonChoThanhToan();
                TaiComboBanGoi();
                TaiMenu();
            }
            finally
            {
                dangTai = false;
            }

            TaiChiTietGoi();
            TaiBangQuanLy();
            TaiLichSu();
        }

        private void TaiSoDo()
        {
            dsBan.Clear();
            dsBan.AddRange(banBus.LaySoDo());

            if (maBanDangChon > 0 && dsBan.Find(b => b.MaBan == maBanDangChon) == null)
            {
                maBanDangChon = 0;
            }

            // Chọn sẵn bàn đầu tiên để khung thông tin không bị trống lúc mở form
            if (maBanDangChon == 0 && dsBan.Count > 0)
            {
                maBanDangChon = dsBan[0].MaBan;
            }

            pnlBan.SuspendLayout();
            pnlBan.Controls.Clear();
            for (int i = 0; i < dsBan.Count; i++)
            {
                BanBidaDTO ban = dsBan[i];
                int cot = i % 3;
                int dong = i / 3;

                var btn = new Button
                {
                    Location = new Point(8 + cot * 256, 8 + dong * 132),
                    Size = new Size(244, 120),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = ban.DangChoi ? Color.FromArgb(230, 81, 0) : Color.FromArgb(46, 125, 50),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Tag = ban.MaBan,
                    Text = ban.TenBan + "\n" + ban.LoaiBan + "\n" +
                           ban.GiaTheoGio.ToString("#,##0") + " đ/giờ\n" + ban.TrangThai
                };
                btn.FlatAppearance.BorderSize = ban.MaBan == maBanDangChon ? 4 : 0;
                btn.FlatAppearance.BorderColor = Color.FromArgb(31, 56, 100);
                btn.Click += BanBtn_Click;
                pnlBan.Controls.Add(btn);
            }

            pnlBan.ResumeLayout();

            HienThiBanChon();
            TaiHoaDonMo();
        }

        private void BanBtn_Click(object sender, EventArgs e)
        {
            var btn = sender as Button;
            if (btn == null)
            {
                return;
            }

            maBanDangChon = Convert.ToInt32(btn.Tag);
            TaiSoDo();
            Bao("Đang chọn " + lblBanTen.Text + ".");
        }

        private void HienThiBanChon()
        {
            BanBidaDTO ban = dsBan.Find(b => b.MaBan == maBanDangChon);
            if (ban == null)
            {
                lblBanTen.Text = "—";
                lblBanLoai.Text = "Loại bàn: —";
                lblBanGia.Text = "Giá: —";
                lblBanTrangThai.Text = "Trạng thái: —";
                lblBanHoaDon.Text = "Hóa đơn mở: —";
                lblBanTien.Text = "Tạm tính: 0 đ";
                maHDDangChon = 0;
                btnMoBan.Enabled = false;
                btnThanhToanBan.Enabled = false;
                btnGoiDichVu.Enabled = false;
                return;
            }

            lblBanTen.Text = ban.TenBan;
            lblBanLoai.Text = "Loại bàn: " + ban.LoaiBan;
            lblBanGia.Text = "Giá: " + Tien(ban.GiaTheoGio) + "/giờ";
            lblBanTrangThai.Text = "Trạng thái: " + ban.TrangThai;
            maHDDangChon = hoaDonBus.LayHoaDonMoCuaBan(ban.MaBan);

            if (maHDDangChon > 0)
            {
                try
                {
                    HoaDonDTO hd = hoaDonBus.XemTruoc(maHDDangChon);
                    lblBanHoaDon.Text = "HĐ #" + hd.MaHD + " - bắt đầu " + hd.GioBatDau.ToString("dd/MM HH:mm");
                    lblBanTien.Text = "Tạm tính: " + Tien(hd.TongCong);
                }
                catch (Exception ex)
                {
                    lblBanHoaDon.Text = "Hóa đơn mở: #" + maHDDangChon;
                    lblBanTien.Text = "Tạm tính: " + ex.Message;
                }
            }
            else
            {
                lblBanHoaDon.Text = "Hóa đơn mở: chưa có";
                lblBanTien.Text = "Tạm tính: 0 đ";
            }

            btnMoBan.Enabled = !ban.DangChoi;
            btnThanhToanBan.Enabled = maHDDangChon > 0;
            btnGoiDichVu.Enabled = maHDDangChon > 0;
        }

        private void TaiHoaDonMo()
        {
            DataTable t = hoaDonBus.LayHoaDonChuaThanhToan();
            dgvHoaDonMo.DataSource = t;
            DatTieuDeTheoTen(dgvHoaDonMo,
                "MaHD|Mã HĐ", "TenBan|Bàn", "GioBatDau|Bắt đầu", "TienDichVu|Tiền DV");
            AnCot(dgvHoaDonMo, "MaBan");
            AnCot(dgvHoaDonMo, "LoaiBan");
            AnCot(dgvHoaDonMo, "GiaTheoGio");
            if (dgvHoaDonMo.Columns["GioBatDau"] != null)
            {
                dgvHoaDonMo.Columns["GioBatDau"].DefaultCellStyle.Format = "HH:mm";
            }

            if (dgvHoaDonMo.Columns["TienDichVu"] != null)
            {
                dgvHoaDonMo.Columns["TienDichVu"].DefaultCellStyle.Format = "#,##0";
            }
        }

        private void TaiHoaDonChoThanhToan()
        {
            DataTable t = hoaDonBus.LayHoaDonChuaThanhToan();
            dgvChoThanhToan.DataSource = t;
            DatTieuDe(dgvChoThanhToan, "Mã HĐ", "Mã bàn", "Bàn", "Loại bàn", "Giá/giờ", "Bắt đầu", "Dịch vụ");
            AnCot(dgvChoThanhToan, "MaBan");
            if (dgvChoThanhToan.Columns.Count >= 7)
            {
                dgvChoThanhToan.Columns[4].DefaultCellStyle.Format = "#,##0";
                dgvChoThanhToan.Columns[5].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                dgvChoThanhToan.Columns[6].DefaultCellStyle.Format = "#,##0";
            }

            if (dgvChoThanhToan.Rows.Count > 0)
            {
                dgvChoThanhToan.Rows[0].Selected = true;
                object v = dgvChoThanhToan.Rows[0].Cells[0].Value;
                maHDThanhToan = v == null ? 0 : Convert.ToInt32(v);
            }
            else
            {
                maHDThanhToan = 0;
            }

            HienThiXemTruoc();
        }

        private void DgvChoThanhToan_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvChoThanhToan.CurrentRow == null || dgvChoThanhToan.CurrentRow.Index < 0)
            {
                maHDThanhToan = 0;
            }
            else
            {
                object v = dgvChoThanhToan.CurrentRow.Cells[0].Value;
                maHDThanhToan = v == null ? 0 : Convert.ToInt32(v);
            }

            HienThiXemTruoc();
        }

        private void HienThiXemTruoc()
        {
            if (maHDThanhToan <= 0)
            {
                lblXemGioChoi.Text = "Chưa có hóa đơn nào đang chờ thanh toán.";
                lblXemTienGio.Text = "";
                lblXemTienDv.Text = "";
                lblXemTong.Text = "";
                btnThanhToan.Enabled = false;
                return;
            }

            try
            {
                HoaDonDTO hd = hoaDonBus.XemTruoc(maHDThanhToan);
                int soPhut = (int)Math.Round((DateTime.Now - hd.GioBatDau).TotalMinutes);
                if (soPhut < 0)
                {
                    soPhut = 0;
                }

                lblXemGioChoi.Text = hd.TenBan + " (" + hd.LoaiBan + ") - bắt đầu " +
                                     hd.GioBatDau.ToString("dd/MM/yyyy HH:mm") + " - đã chơi " + soPhut +
                                     " phút => làm tròn " + hd.SoKhoi15 + " khối 15 phút (" +
                                     hd.SoGio.ToString("0.##") + " giờ)";
                lblXemTienGio.Text = "Tiền giờ chơi: " + Tien(hd.TienGioChoi) +
                                     "   (giá bàn " + Tien(hd.GiaTheoGio) + "/giờ)";
                lblXemTienDv.Text = "Tiền dịch vụ: " + Tien(hd.TienDichVu);
                lblXemTong.Text = "TỔNG CỘNG: " + Tien(hd.TongCong);
                btnThanhToan.Enabled = true;
            }
            catch (Exception ex)
            {
                lblXemGioChoi.Text = "Không xem trước được hóa đơn: " + ex.Message;
                lblXemTienGio.Text = "";
                lblXemTienDv.Text = "";
                lblXemTong.Text = "";
                btnThanhToan.Enabled = false;
            }
        }

        private void TaiComboBanGoi()
        {
            int muonChon = maHDGoiDichVu > 0 ? maHDGoiDichVu : maHDDangChon;

            cboBanGoi.Items.Clear();
            DataTable t = hoaDonBus.LayHoaDonChuaThanhToan();
            foreach (DataRow r in t.Rows)
            {
                cboBanGoi.Items.Add(new MucChon
                {
                    Ma = Convert.ToInt32(r["MaHD"]),
                    MaBan = Convert.ToInt32(r["MaBan"]),
                    Ten = Convert.ToString(r["TenBan"]),
                    MoTa = Convert.ToString(r["TenBan"]) + "  (hóa đơn #" + r["MaHD"] + ")",
                    Gia = Convert.ToDecimal(r["GiaTheoGio"])
                });
            }

            int idx = TimViTri(cboBanGoi, muonChon);
            if (idx < 0 && cboBanGoi.Items.Count > 0)
            {
                idx = 0;
            }

            cboBanGoi.SelectedIndex = idx;
            maHDGoiDichVu = idx >= 0 ? ((MucChon)cboBanGoi.SelectedItem).Ma : 0;
        }

        private static int TimViTri(ComboBox cbo, int ma)
        {
            for (int i = 0; i < cbo.Items.Count; i++)
            {
                var m = cbo.Items[i] as MucChon;
                if (m != null && m.Ma == ma)
                {
                    return i;
                }
            }

            return -1;
        }

        private void CboBanGoi_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dangTai)
            {
                return;
            }

            var muc = cboBanGoi.SelectedItem as MucChon;
            maHDGoiDichVu = muc == null ? 0 : muc.Ma;
            TaiChiTietGoi();
        }

        private void TaiChiTietGoi()
        {
            if (maHDGoiDichVu <= 0)
            {
                bangChiTiet = null;
                dgvChiTiet.DataSource = null;
                lblGoiGio.Text = "Chưa chọn hóa đơn.";
                lblTongDichVu.Text = "Tổng dịch vụ: 0 đ";
                return;
            }

            try
            {
                // Nạp lại bảng ngắt kết nối từ CSDL (mỗi lần chọn bàn)
                bangChiTiet = hoaDonBus.LayChiTiet(maHDGoiDichVu);
                dgvChiTiet.DataSource = bangChiTiet;
                DatTieuDe(dgvChiTiet, "Mã HĐ", "Mã DV", "Dịch vụ", "SL", "Đơn giá", "Trừ kho", "Thành tiền");
                AnCot(dgvChiTiet, "MaHD");
                AnCot(dgvChiTiet, "TruKho");
                if (dgvChiTiet.Columns.Count >= 7)
                {
                    dgvChiTiet.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgvChiTiet.Columns[4].DefaultCellStyle.Format = "#,##0";
                    dgvChiTiet.Columns[6].DefaultCellStyle.Format = "#,##0";
                }

                DatDoRong(dgvChiTiet, "MaDV", 55);
                DatDoRong(dgvChiTiet, "TenDV", 200);
                DatDoRong(dgvChiTiet, "SoLuong", 55);
                DatDoRong(dgvChiTiet, "DonGia", 85);
                DatDoRong(dgvChiTiet, "ThanhTien", 95);

                HoaDonDTO hd = hoaDonBus.LayHoaDon(maHDGoiDichVu);
                lblGoiGio.Text = hd == null
                    ? "Chưa chọn hóa đơn."
                    : hd.TenBan + " - bắt đầu " + hd.GioBatDau.ToString("dd/MM/yyyy HH:mm");
                CapNhatTongDichVu();
            }
            catch (Exception ex)
            {
                Bao("Lỗi tải chi tiết: " + ex.Message);
            }
        }

        private void CapNhatTongDichVu()
        {
            if (bangChiTiet == null)
            {
                lblTongDichVu.Text = "Tổng dịch vụ: 0 đ";
                return;
            }

            decimal tong = 0;
            foreach (DataRow r in bangChiTiet.Rows)
            {
                tong += Convert.ToDecimal(r["SoLuong"]) * Convert.ToDecimal(r["DonGia"]);
            }

            lblTongDichVu.Text = "Tổng dịch vụ: " + Tien(tong) + "  (chưa gồm tiền giờ)";
        }

        private void TaiMenu()
        {
            DataTable t = dichVuBus.LayDanhSach();
            dgvMenu.DataSource = t;
            DatTieuDeTheoTen(dgvMenu, "MaDV|Mã", "TenDV|Tên dịch vụ", "DonGia|Đơn giá",
                "TonKho|Tồn kho", "TruKhoText|Trừ kho");
            AnCot(dgvMenu, "TruKho");
            if (dgvMenu.Columns["DonGia"] != null)
            {
                dgvMenu.Columns["DonGia"].DefaultCellStyle.Format = "#,##0";
            }

            DatDoRong(dgvMenu, "MaDV", 45);
            DatDoRong(dgvMenu, "TenDV", 190);
            DatDoRong(dgvMenu, "DonGia", 90);
            DatDoRong(dgvMenu, "TonKho", 75);
            DatDoRong(dgvMenu, "TruKhoText", 75);

            cboDichVu.Items.Clear();
            foreach (DataRow r in t.Rows)
            {
                bool truKho = Convert.ToBoolean(r["TruKho"]);
                int ton = Convert.ToInt32(r["TonKho"]);
                cboDichVu.Items.Add(new MucChon
                {
                    Ma = Convert.ToInt32(r["MaDV"]),
                    Ten = Convert.ToString(r["TenDV"]),
                    Gia = Convert.ToDecimal(r["DonGia"]),
                    TonKho = ton,
                    TruKho = truKho,
                    MoTa = Convert.ToString(r["TenDV"]) + "  -  " + Tien(Convert.ToDecimal(r["DonGia"])) +
                           (truKho ? "  (tồn " + ton + ")" : "  (không trừ kho)")
                });
            }

            if (cboDichVu.Items.Count > 0 && cboDichVu.SelectedIndex < 0)
            {
                cboDichVu.SelectedIndex = 0;
            }
        }

        private void TaiBangQuanLy()
        {
            DataTable tBan = banBus.LayBang();
            dgvBan.DataSource = tBan;
            DatTieuDe(dgvBan, "Mã", "Tên bàn", "Loại bàn", "Giá/giờ", "Trạng thái");
            if (dgvBan.Columns.Count >= 4)
            {
                dgvBan.Columns[3].DefaultCellStyle.Format = "#,##0";
            }

            DataTable tDv = dichVuBus.LayDanhSach();
            dgvDichVu.DataSource = tDv;
            DatTieuDeTheoTen(dgvDichVu, "MaDV|Mã", "TenDV|Tên dịch vụ", "DonGia|Đơn giá",
                "TonKho|Tồn kho", "TruKhoText|Trừ kho");
            AnCot(dgvDichVu, "TruKho");
            if (dgvDichVu.Columns["DonGia"] != null)
            {
                dgvDichVu.Columns["DonGia"].DefaultCellStyle.Format = "#,##0";
            }

            DatDoRong(dgvDichVu, "MaDV", 45);
            DatDoRong(dgvDichVu, "TenDV", 190);
            DatDoRong(dgvDichVu, "DonGia", 90);
            DatDoRong(dgvDichVu, "TonKho", 75);
            DatDoRong(dgvDichVu, "TruKhoText", 75);
            DatDoRong(dgvBan, "TenBan", 110);
            DatDoRong(dgvBan, "LoaiBan", 120);
        }

        private void TaiLichSu()
        {
            DataTable t = hoaDonBus.LayLichSu();
            dgvLichSu.DataSource = t;
            DatTieuDe(dgvLichSu, "Mã HĐ", "Bàn", "Bắt đầu", "Kết thúc", "Tiền giờ", "Tiền DV", "Tổng cộng");
            if (dgvLichSu.Columns.Count >= 7)
            {
                dgvLichSu.Columns[2].DefaultCellStyle.Format = "dd/MM HH:mm";
                dgvLichSu.Columns[3].DefaultCellStyle.Format = "dd/MM HH:mm";
                dgvLichSu.Columns[4].DefaultCellStyle.Format = "#,##0";
                dgvLichSu.Columns[5].DefaultCellStyle.Format = "#,##0";
                dgvLichSu.Columns[6].DefaultCellStyle.Format = "#,##0";
            }

            DataTable tk = hoaDonBus.ThongKeTheoBan();
            dgvThongKe.DataSource = tk;
            DatTieuDe(dgvThongKe, "Bàn", "Loại bàn", "Số hóa đơn", "Tiền giờ", "Tiền DV", "Tổng tiền");
            if (dgvThongKe.Columns.Count >= 6)
            {
                dgvThongKe.Columns[3].DefaultCellStyle.Format = "#,##0";
                dgvThongKe.Columns[4].DefaultCellStyle.Format = "#,##0";
                dgvThongKe.Columns[5].DefaultCellStyle.Format = "#,##0";
            }

            lblDoanhThu.Text = "Tổng doanh thu: " + Tien(hoaDonBus.TongDoanhThu()) +
                               "   -   " + t.Rows.Count + " hóa đơn đã thanh toán";
        }

        // =====================================================================
        //  Tab 1: mở bàn / thanh toán
        // =====================================================================
        private void BtnMoBan_Click(object sender, EventArgs e)
        {
            if (maBanDangChon <= 0)
            {
                MessageBox.Show("Chọn một bàn trên sơ đồ trước khi mở bàn.", "Thông báo");
                return;
            }

            try
            {
                int maHD = hoaDonBus.MoBan(maBanDangChon);
                TaiTatCa();
                Bao("Đã mở bàn. Hóa đơn số " + maHD + " đang chờ thanh toán.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không mở được bàn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnThanhToanBan_Click(object sender, EventArgs e)
        {
            if (maHDDangChon <= 0)
            {
                MessageBox.Show("Bàn này chưa có hóa đơn để thanh toán.", "Thông báo");
                return;
            }

            ThanhToanHoaDon(maHDDangChon);
        }

        private void BtnGoiDichVu_Click(object sender, EventArgs e)
        {
            if (maHDDangChon <= 0)
            {
                MessageBox.Show("Bàn này chưa mở hóa đơn nên chưa gọi dịch vụ được.", "Thông báo");
                return;
            }

            dangTai = true;
            try
            {
                maHDGoiDichVu = maHDDangChon;
                TaiComboBanGoi();
            }
            finally
            {
                dangTai = false;
            }

            TaiChiTietGoi();
            tabs.SelectedTab = tabGoiDichVu;
            Bao("Đang gọi dịch vụ cho hóa đơn #" + maHDGoiDichVu + ".");
        }

        // =====================================================================
        //  Tab 2: sửa chi tiết trong bộ nhớ rồi lưu
        // =====================================================================
        private void BtnThemDichVu_Click(object sender, EventArgs e)
        {
            var muc = cboDichVu.SelectedItem as MucChon;
            if (muc == null)
            {
                MessageBox.Show("Chưa chọn dịch vụ trong menu.", "Thông báo");
                return;
            }

            if (bangChiTiet == null || maHDGoiDichVu <= 0)
            {
                MessageBox.Show("Chọn bàn đang chơi ở ô \"Bàn đang chơi\" trước.", "Thông báo");
                return;
            }

            int soLuong = (int)numSoLuong.Value;
            DataRow daCo = null;
            foreach (DataRow r in bangChiTiet.Rows)
            {
                if (Convert.ToInt32(r["MaDV"]) == muc.Ma)
                {
                    daCo = r;
                    break;
                }
            }

            int soLuongCu = daCo == null ? 0 : Convert.ToInt32(daCo["SoLuong"]);
            if (muc.TruKho && soLuongCu + soLuong > muc.TonKho)
            {
                MessageBox.Show("Dịch vụ \"" + muc.Ten + "\" chỉ còn " + muc.TonKho +
                                " trong kho, hóa đơn đã có " + soLuongCu + ".",
                    "Không đủ tồn kho", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (daCo != null)
            {
                daCo["SoLuong"] = soLuongCu + soLuong;
            }
            else
            {
                DataRow r = bangChiTiet.NewRow();
                r["MaHD"] = maHDGoiDichVu;
                r["MaDV"] = muc.Ma;
                r["TenDV"] = muc.Ten;
                r["SoLuong"] = soLuong;
                r["DonGia"] = muc.Gia;
                r["TruKho"] = muc.TruKho;
                bangChiTiet.Rows.Add(r);
            }

            CapNhatTongDichVu();
            Bao("Đã thêm " + soLuong + " \"" + muc.Ten + "\" vào hóa đơn (chưa lưu).");
        }

        private void BtnBotDichVu_Click(object sender, EventArgs e)
        {
            if (bangChiTiet == null || dgvChiTiet.CurrentRow == null || dgvChiTiet.CurrentRow.Index < 0)
            {
                MessageBox.Show("Chọn một dòng chi tiết trong bảng.", "Thông báo");
                return;
            }

            DataRow r = bangChiTiet.Rows[dgvChiTiet.CurrentRow.Index];
            int soLuong = Convert.ToInt32(r["SoLuong"]);
            if (soLuong <= 1)
            {
                bangChiTiet.Rows.Remove(r);
            }
            else
            {
                r["SoLuong"] = soLuong - 1;
            }

            CapNhatTongDichVu();
            Bao("Đã giảm số lượng dịch vụ (chưa lưu).");
        }

        private void BtnBoDong_Click(object sender, EventArgs e)
        {
            if (bangChiTiet == null || dgvChiTiet.CurrentRow == null || dgvChiTiet.CurrentRow.Index < 0)
            {
                MessageBox.Show("Chọn một dòng chi tiết trong bảng.", "Thông báo");
                return;
            }

            bangChiTiet.Rows.RemoveAt(dgvChiTiet.CurrentRow.Index);
            CapNhatTongDichVu();
            Bao("Đã bỏ dòng dịch vụ (chưa lưu).");
        }

        private void BtnLuuDichVu_Click(object sender, EventArgs e)
        {
            if (maHDGoiDichVu <= 0 || bangChiTiet == null)
            {
                MessageBox.Show("Chưa chọn hóa đơn để lưu dịch vụ.", "Thông báo");
                return;
            }

            try
            {
                hoaDonBus.LuuDichVu(maHDGoiDichVu, bangChiTiet);
                int soDong = bangChiTiet.Rows.Count;
                TaiTatCa();
                Bao("Đã lưu " + soDong + " dòng dịch vụ cho hóa đơn #" + maHDGoiDichVu + ".");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không lưu được dịch vụ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // =====================================================================
        //  Tab 3: thanh toán trong transaction
        // =====================================================================
        private void BtnThanhToan_Click(object sender, EventArgs e)
        {
            if (maHDThanhToan <= 0)
            {
                MessageBox.Show("Chọn một hóa đơn trong danh sách trước.", "Thông báo");
                return;
            }

            ThanhToanHoaDon(maHDThanhToan);
        }

        private void ThanhToanHoaDon(int maHD)
        {
            try
            {
                // Xem trước để hiện hộp thoại xác nhận tiền
                HoaDonDTO hd = hoaDonBus.XemTruoc(maHD);
                string hoi =
                    "Xác nhận thanh toán hóa đơn #" + hd.MaHD + " - " + hd.TenBan + " (" + hd.LoaiBan + ")?\n\n" +
                    "Giờ bắt đầu  : " + hd.GioBatDau.ToString("dd/MM/yyyy HH:mm") + "\n" +
                    "Giờ kết thúc : " + hd.GioKetThuc.Value.ToString("dd/MM/yyyy HH:mm") + "\n" +
                    "Thời gian    : " + hd.SoKhoi15 + " khối 15 phút (" + hd.SoGio.ToString("0.##") + " giờ)\n\n" +
                    "Tiền giờ chơi : " + Tien(hd.TienGioChoi) + "\n" +
                    "Tiền dịch vụ  : " + Tien(hd.TienDichVu) + "\n" +
                    "TỔNG CỘNG     : " + Tien(hd.TongCong);

                if (MessageBox.Show(hoi, "Xác nhận thanh toán",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                // Thanh toán thật: DAL tự chốt giờ, tính tiền, trừ kho, trả bàn trong transaction
                HoaDonDTO kq = hoaDonBus.ThanhToan(maHD);

                string bienLai =
                    "HÓA ĐƠN THANH TOÁN\n" +
                    "------------------------------------------\n" +
                    "Số hóa đơn   : " + kq.MaHD + "\n" +
                    "Bàn          : " + kq.TenBan + " (" + kq.LoaiBan + ")\n" +
                    "Giờ bắt đầu  : " + kq.GioBatDau.ToString("dd/MM/yyyy HH:mm") + "\n" +
                    "Giờ kết thúc : " + kq.GioKetThuc.Value.ToString("dd/MM/yyyy HH:mm") + "\n" +
                    "Giờ chơi     : " + kq.SoKhoi15 + " khối 15 phút (" + kq.SoGio.ToString("0.##") + " giờ)\n" +
                    "------------------------------------------\n" +
                    "Tiền giờ     : " + Tien(kq.TienGioChoi) + "\n" +
                    "Tiền dịch vụ : " + Tien(kq.TienDichVu) + "\n" +
                    "TỔNG CỘNG    : " + Tien(kq.TongCong) + "\n" +
                    "------------------------------------------\n" +
                    "Cảm ơn quý khách!";

                lblKetQua.Text = "HĐ #" + kq.MaHD + " - " + kq.TenBan + " - " + kq.SoKhoi15 +
                                 " khối 15 phút - tiền giờ " + Tien(kq.TienGioChoi) +
                                 " - dịch vụ " + Tien(kq.TienDichVu) +
                                 " - tổng cộng " + Tien(kq.TongCong) + " (đã trả bàn).";

                TaiTatCa();
                Bao("Đã thanh toán hóa đơn #" + kq.MaHD + ".");
                MessageBox.Show(bienLai, "Thanh toán thành công",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không thanh toán được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // =====================================================================
        //  Tab 4: quản lý bàn
        // =====================================================================
        private void DgvBan_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvBan.CurrentRow == null || dgvBan.CurrentRow.Index < 0)
            {
                return;
            }

            DataGridViewRow r = dgvBan.CurrentRow;
            if (r.Cells[0].Value == null)
            {
                return;
            }

            maBanSua = Convert.ToInt32(r.Cells[0].Value);
            txtTenBan.Text = Convert.ToString(r.Cells[1].Value);
            cboLoaiBan.SelectedItem = Convert.ToString(r.Cells[2].Value);
            decimal gia = Convert.ToDecimal(r.Cells[3].Value);
            numGiaBan.Value = gia < numGiaBan.Minimum ? numGiaBan.Minimum
                : (gia > numGiaBan.Maximum ? numGiaBan.Maximum : gia);
            lblTrangThaiBan.Text = Convert.ToString(r.Cells[4].Value);
        }

        private BanBidaDTO BanTuForm()
        {
            return new BanBidaDTO
            {
                MaBan = maBanSua,
                TenBan = txtTenBan.Text,
                LoaiBan = Convert.ToString(cboLoaiBan.SelectedItem),
                GiaTheoGio = numGiaBan.Value
            };
        }

        private void BtnThemBan_Click(object sender, EventArgs e)
        {
            try
            {
                BanBidaDTO ban = BanTuForm();
                ban.MaBan = 0;
                banBus.Them(ban);
                LamMoiBan();
                TaiTatCa();
                Bao("Đã thêm " + ban.TenBan + ".");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không thêm được bàn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnSuaBan_Click(object sender, EventArgs e)
        {
            if (maBanSua <= 0)
            {
                MessageBox.Show("Chọn một bàn trong danh sách trước.", "Thông báo");
                return;
            }

            try
            {
                BanBidaDTO ban = BanTuForm();
                banBus.Sua(ban);
                TaiTatCa();
                Bao("Đã cập nhật " + ban.TenBan + ".");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không sửa được bàn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnXoaBan_Click(object sender, EventArgs e)
        {
            if (maBanSua <= 0)
            {
                MessageBox.Show("Chọn một bàn trong danh sách trước.", "Thông báo");
                return;
            }

            if (MessageBox.Show("Xoá bàn \"" + txtTenBan.Text + "\"?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                banBus.Xoa(maBanSua);
                LamMoiBan();
                TaiTatCa();
                Bao("Đã xoá bàn mã " + maBanSua + ".");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không xoá được bàn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LamMoiBan()
        {
            maBanSua = 0;
            txtTenBan.Clear();
            cboLoaiBan.SelectedIndex = cboLoaiBan.Items.Count > 0 ? 0 : -1;
            numGiaBan.Value = 60000;
            lblTrangThaiBan.Text = "—";
            txtTenBan.Focus();
        }

        // =====================================================================
        //  Tab 4: quản lý menu dịch vụ
        // =====================================================================
        private void DgvDichVu_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvDichVu.CurrentRow == null || dgvDichVu.CurrentRow.Index < 0)
            {
                return;
            }

            DataGridViewRow r = dgvDichVu.CurrentRow;
            if (r.Cells[0].Value == null)
            {
                return;
            }

            maDVSua = Convert.ToInt32(r.Cells[0].Value);
            txtTenDv.Text = Convert.ToString(r.Cells[1].Value);
            decimal gia = Convert.ToDecimal(r.Cells[2].Value);
            numDonGiaDv.Value = gia < numDonGiaDv.Minimum ? numDonGiaDv.Minimum
                : (gia > numDonGiaDv.Maximum ? numDonGiaDv.Maximum : gia);
            int ton = Convert.ToInt32(r.Cells[3].Value);
            numTonKho.Value = ton < numTonKho.Minimum ? numTonKho.Minimum
                : (ton > numTonKho.Maximum ? numTonKho.Maximum : ton);
            chkTruKho.Checked = Convert.ToBoolean(r.Cells[4].Value);
        }

        private DichVuDTO DvTuForm()
        {
            return new DichVuDTO
            {
                MaDV = maDVSua,
                TenDV = txtTenDv.Text,
                DonGia = numDonGiaDv.Value,
                TonKho = (int)numTonKho.Value,
                TruKho = chkTruKho.Checked
            };
        }

        private void BtnThemDv_Click(object sender, EventArgs e)
        {
            try
            {
                DichVuDTO dv = DvTuForm();
                dv.MaDV = 0;
                dichVuBus.Them(dv);
                LamMoiDv();
                TaiTatCa();
                Bao("Đã thêm dịch vụ \"" + dv.TenDV + "\".");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không thêm được dịch vụ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnSuaDv_Click(object sender, EventArgs e)
        {
            if (maDVSua <= 0)
            {
                MessageBox.Show("Chọn một dịch vụ trong danh sách trước.", "Thông báo");
                return;
            }

            try
            {
                DichVuDTO dv = DvTuForm();
                dichVuBus.Sua(dv);
                TaiTatCa();
                Bao("Đã cập nhật dịch vụ \"" + dv.TenDV + "\".");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không sửa được dịch vụ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnXoaDv_Click(object sender, EventArgs e)
        {
            if (maDVSua <= 0)
            {
                MessageBox.Show("Chọn một dịch vụ trong danh sách trước.", "Thông báo");
                return;
            }

            if (MessageBox.Show("Xoá dịch vụ \"" + txtTenDv.Text + "\"?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                dichVuBus.Xoa(maDVSua);
                LamMoiDv();
                TaiTatCa();
                Bao("Đã xoá dịch vụ mã " + maDVSua + ".");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không xoá được dịch vụ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LamMoiDv()
        {
            maDVSua = 0;
            txtTenDv.Clear();
            numDonGiaDv.Value = 0;
            numTonKho.Value = 0;
            chkTruKho.Checked = true;
            txtTenDv.Focus();
        }
    }
}
