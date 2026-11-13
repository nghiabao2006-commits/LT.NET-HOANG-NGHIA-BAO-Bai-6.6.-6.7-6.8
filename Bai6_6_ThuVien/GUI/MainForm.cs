using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Bai6_6_ThuVien.BUS;
using Bai6_6_ThuVien.DTO;

namespace Bai6_6_ThuVien.GUI
{
    /// <summary>
    /// Giao diện chính (Presentation Layer). Form chỉ gọi xuống BUS,
    /// không viết câu SQL nào và không mở kết nối trực tiếp.
    /// </summary>
    public class MainForm : Form
    {
        // ---------- BUS ----------
        private readonly DocGiaBUS docGiaBus = new DocGiaBUS();
        private readonly DauSachBUS dauSachBus = new DauSachBUS();
        private readonly PhieuMuonBUS phieuMuonBus = new PhieuMuonBUS();

        // ---------- Trạng thái nội bộ ----------
        private readonly DataTable gioMuon = new DataTable();
        private readonly Dictionary<int, int> tonSach = new Dictionary<int, int>();
        private int maDocGiaDangChon;
        private int maSachDangChon;
        private int maPhieuDangChon;

        // ---------- Điều khiển ----------
        private TabControl tabs;
        private ToolStripStatusLabel lblTrangThai;

        // Tab Độc giả
        private TextBox txtHoTen, txtEmail, txtTimDocGia;
        private DateTimePicker dtpHetHan;
        private DataGridView dgvDocGia;

        // Tab Đầu sách
        private TextBox txtTenSach, txtTimSach;
        private ComboBox cboTheLoai;
        private NumericUpDown numSoLuong;
        private DataGridView dgvDauSach;

        // Tab Mượn / Trả
        private ComboBox cboLocDocGia, cboLocTrangThai, cboDocGiaMuon, cboSach;
        private DateTimePicker dtpHanTra;
        private DataGridView dgvPhieu, dgvChiTiet, dgvGioMuon;
        private NumericUpDown numMuon;
        private Label lblTheDocGia;

        // Tab Quá hạn
        private DataGridView dgvQuaHan;
        private Label lblDemQuaHan;

        // Tab Thống kê
        private DataGridView dgvThongKe;
        private Label lblTongLuot;

        public MainForm()
        {
            KhoiTaoGiaoDien();
            KhoiTaoDuLieuTam();
        }

        // =====================================================================
        //  Dựng giao diện
        // =====================================================================
        private void KhoiTaoGiaoDien()
        {
            Text = "Bài 6.6 - Hệ thống quản lý mượn trả sách thư viện";
            StartPosition = FormStartPosition.CenterScreen;
            // Giao diện dựng bằng code nên tắt auto-scale để toạ độ tuyệt đối giữ nguyên
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1180, 720);
            MinimumSize = new Size(1024, 660);
            Font = new Font("Segoe UI", 9.5F);

            tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(TaoTabDocGia());
            tabs.TabPages.Add(TaoTabDauSach());
            tabs.TabPages.Add(TaoTabMuonTra());
            tabs.TabPages.Add(TaoTabQuaHan());
            tabs.TabPages.Add(TaoTabThongKe());

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

        private TabPage TaoTabDocGia()
        {
            var tab = new TabPage("1. Độc giả") { Padding = new Padding(6), Size = KichThuocTab() };

            var gb = new GroupBox
            {
                Text = "Thông tin độc giả",
                Location = new Point(6, 6),
                Size = new Size(340, 300)
            };

            gb.Controls.Add(TaoNhan("Họ tên", 12, 28));
            txtHoTen = TaoOChu(12, 48, 310);
            gb.Controls.Add(txtHoTen);

            gb.Controls.Add(TaoNhan("Email", 12, 86));
            txtEmail = TaoOChu(12, 106, 310);
            gb.Controls.Add(txtEmail);

            gb.Controls.Add(TaoNhan("Ngày hết hạn thẻ", 12, 144));
            dtpHetHan = new DateTimePicker
            {
                Location = new Point(12, 164),
                Size = new Size(310, 26),
                Format = DateTimePickerFormat.Short
            };
            gb.Controls.Add(dtpHetHan);

            var btnThem = TaoNut("Thêm", 12, 206, 96);
            var btnSua = TaoNut("Sửa", 118, 206, 96);
            var btnXoa = TaoNut("Xoá", 224, 206, 98);
            var btnMoi = TaoNut("Làm mới", 12, 246, 96);
            btnThem.Click += BtnThemDocGia_Click;
            btnSua.Click += BtnSuaDocGia_Click;
            btnXoa.Click += BtnXoaDocGia_Click;
            btnMoi.Click += (s, e) => LamMoiDocGia();
            gb.Controls.AddRange(new Control[] { btnThem, btnSua, btnXoa, btnMoi });

            var lblTim = TaoNhan("Tìm theo tên / email", 360, 12);
            txtTimDocGia = TaoOChu(360, 32, 300);
            txtTimDocGia.TextChanged += (s, e) => TaiDocGia();
            var btnTim = TaoNut("Xoá lọc", 670, 30, 80);
            btnTim.Click += (s, e) => { txtTimDocGia.Clear(); TaiDocGia(); };

            dgvDocGia = TaoLuoi(360, 68, 790, 540);
            dgvDocGia.SelectionChanged += DgvDocGia_SelectionChanged;

            tab.Controls.Add(gb);
            tab.Controls.Add(lblTim);
            tab.Controls.Add(txtTimDocGia);
            tab.Controls.Add(btnTim);
            tab.Controls.Add(dgvDocGia);
            return tab;
        }

        private TabPage TaoTabDauSach()
        {
            var tab = new TabPage("2. Đầu sách") { Padding = new Padding(6), Size = KichThuocTab() };

            var gb = new GroupBox
            {
                Text = "Thông tin đầu sách",
                Location = new Point(6, 6),
                Size = new Size(340, 300)
            };

            gb.Controls.Add(TaoNhan("Tên sách", 12, 28));
            txtTenSach = TaoOChu(12, 48, 310);
            gb.Controls.Add(txtTenSach);

            gb.Controls.Add(TaoNhan("Thể loại", 12, 86));
            cboTheLoai = new ComboBox
            {
                Location = new Point(12, 106),
                Size = new Size(310, 26),
                DropDownStyle = ComboBoxStyle.DropDown
            };
            gb.Controls.Add(cboTheLoai);

            gb.Controls.Add(TaoNhan("Số lượng còn", 12, 144));
            numSoLuong = new NumericUpDown
            {
                Location = new Point(12, 164),
                Size = new Size(120, 26),
                Maximum = 10000,
                Minimum = 0
            };
            gb.Controls.Add(numSoLuong);

            var btnThem = TaoNut("Thêm", 12, 206, 96);
            var btnSua = TaoNut("Sửa", 118, 206, 96);
            var btnXoa = TaoNut("Xoá", 224, 206, 98);
            var btnMoi = TaoNut("Làm mới", 12, 246, 96);
            btnThem.Click += BtnThemSach_Click;
            btnSua.Click += BtnSuaSach_Click;
            btnXoa.Click += BtnXoaSach_Click;
            btnMoi.Click += (s, e) => LamMoiSach();
            gb.Controls.AddRange(new Control[] { btnThem, btnSua, btnXoa, btnMoi });

            var lblTim = TaoNhan("Tìm theo tên sách / thể loại", 360, 12);
            txtTimSach = TaoOChu(360, 32, 300);
            txtTimSach.TextChanged += (s, e) => TaiDauSach();
            var btnTim = TaoNut("Xoá lọc", 670, 30, 80);
            btnTim.Click += (s, e) => { txtTimSach.Clear(); TaiDauSach(); };

            dgvDauSach = TaoLuoi(360, 68, 790, 540);
            dgvDauSach.SelectionChanged += DgvDauSach_SelectionChanged;

            tab.Controls.Add(gb);
            tab.Controls.Add(lblTim);
            tab.Controls.Add(txtTimSach);
            tab.Controls.Add(btnTim);
            tab.Controls.Add(dgvDauSach);
            return tab;
        }

        private TabPage TaoTabMuonTra()
        {
            var tab = new TabPage("3. Mượn / Trả") { Padding = new Padding(6), Size = KichThuocTab() };

            // --- Lọc phiếu ---
            var gbLoc = new GroupBox
            {
                Text = "Lọc phiếu mượn",
                Location = new Point(4, 4),
                Size = new Size(566, 92)
            };

            gbLoc.Controls.Add(TaoNhan("Độc giả", 12, 22));
            cboLocDocGia = new ComboBox
            {
                Location = new Point(12, 44),
                Size = new Size(230, 26),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            gbLoc.Controls.Add(cboLocDocGia);

            gbLoc.Controls.Add(TaoNhan("Trạng thái", 254, 22));
            cboLocTrangThai = new ComboBox
            {
                Location = new Point(254, 44),
                Size = new Size(150, 26),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboLocTrangThai.Items.AddRange(new object[] { "— Tất cả —", "Đang mượn", "Đã trả", "Quá hạn" });
            cboLocTrangThai.SelectedIndex = 0;
            gbLoc.Controls.Add(cboLocTrangThai);

            var btnLoc = TaoNut("Lọc", 400, 42, 70);
            btnLoc.Click += (s, e) => TaiPhieu();
            gbLoc.Controls.Add(btnLoc);

            var btnTra = TaoNut("Trả sách", 476, 42, 80);
            btnTra.Click += BtnTraSach_Click;
            gbLoc.Controls.Add(btnTra);

            dgvPhieu = TaoLuoi(4, 102, 566, 250);
            dgvPhieu.SelectionChanged += DgvPhieu_SelectionChanged;

            var gbCt = new GroupBox
            {
                Text = "Chi tiết phiếu đang chọn",
                Location = new Point(4, 358),
                Size = new Size(566, 210)
            };
            dgvChiTiet = TaoLuoi(10, 24, 546, 170);
            gbCt.Controls.Add(dgvChiTiet);

            // --- Lập phiếu mới ---
            var gbLap = new GroupBox
            {
                Text = "Lập phiếu mượn mới",
                Location = new Point(578, 4),
                Size = new Size(572, 564)
            };

            gbLap.Controls.Add(TaoNhan("Độc giả", 12, 22));
            cboDocGiaMuon = new ComboBox
            {
                Location = new Point(12, 44),
                Size = new Size(300, 26),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboDocGiaMuon.SelectedIndexChanged += CboDocGiaMuon_SelectedIndexChanged;
            gbLap.Controls.Add(cboDocGiaMuon);

            gbLap.Controls.Add(TaoNhan("Hạn trả", 324, 22));
            dtpHanTra = new DateTimePicker
            {
                Location = new Point(324, 44),
                Size = new Size(180, 26),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddDays(14)
            };
            gbLap.Controls.Add(dtpHanTra);

            lblTheDocGia = new Label
            {
                Location = new Point(12, 76),
                Size = new Size(540, 22),
                ForeColor = Color.DimGray,
                Text = "Chọn độc giả để kiểm tra hạn thẻ."
            };
            gbLap.Controls.Add(lblTheDocGia);

            gbLap.Controls.Add(TaoNhan("Đầu sách", 12, 104));
            cboSach = new ComboBox
            {
                Location = new Point(12, 126),
                Size = new Size(330, 26),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            gbLap.Controls.Add(cboSach);

            gbLap.Controls.Add(TaoNhan("SL", 352, 104));
            numMuon = new NumericUpDown
            {
                Location = new Point(352, 126),
                Size = new Size(60, 26),
                Minimum = 1,
                Maximum = 100,
                Value = 1
            };
            gbLap.Controls.Add(numMuon);

            var btnThemSach = TaoNut("Thêm vào phiếu", 424, 124, 132);
            btnThemSach.Click += BtnThemVaoGio_Click;
            gbLap.Controls.Add(btnThemSach);

            dgvGioMuon = TaoLuoi(12, 162, 544, 316);
            gbLap.Controls.Add(dgvGioMuon);

            var btnBo = TaoNut("Bỏ sách chọn", 12, 492, 130);
            btnBo.Click += BtnBoSach_Click;
            gbLap.Controls.Add(btnBo);

            var btnLap = TaoNut("Lập phiếu mượn", 414, 492, 142);
            btnLap.Click += BtnLapPhieu_Click;
            gbLap.Controls.Add(btnLap);

            tab.Controls.Add(gbLoc);
            tab.Controls.Add(dgvPhieu);
            tab.Controls.Add(gbCt);
            tab.Controls.Add(gbLap);
            return tab;
        }

        private TabPage TaoTabQuaHan()
        {
            var tab = new TabPage("4. Cảnh báo quá hạn") { Padding = new Padding(6), Size = KichThuocTab() };

            lblDemQuaHan = new Label
            {
                Location = new Point(10, 12),
                Size = new Size(700, 24),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(160, 40, 40),
                Text = "Chưa tải dữ liệu."
            };

            var btnTai = TaoNut("Tải danh sách", 10, 40, 140);
            btnTai.Click += (s, e) => TaiQuaHan();

            dgvQuaHan = TaoLuoi(10, 80, 1140, 480);

            var lblGhiChu = new Label
            {
                Location = new Point(10, 566),
                Size = new Size(1140, 40),
                ForeColor = Color.DimGray,
                Text = "Điều kiện: HanTra < hôm nay và TrangThai <> N'Đã trả'. " +
                       "Danh sách đọc bằng SqlDataReader trong PhieuMuonDAL.GetQuaHan()."
            };

            tab.Controls.Add(lblDemQuaHan);
            tab.Controls.Add(btnTai);
            tab.Controls.Add(dgvQuaHan);
            tab.Controls.Add(lblGhiChu);
            return tab;
        }

        private TabPage TaoTabThongKe()
        {
            var tab = new TabPage("5. Thống kê") { Padding = new Padding(6), Size = KichThuocTab() };

            var btnXem = TaoNut("Xem thống kê", 10, 12, 130);
            btnXem.Click += (s, e) => TaiThongKe();

            lblTongLuot = new Label
            {
                Location = new Point(156, 18),
                Size = new Size(600, 22),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 56, 100)
            };

            dgvThongKe = TaoLuoi(10, 54, 1140, 500);

            var lblGhiChu = new Label
            {
                Location = new Point(10, 562),
                Size = new Size(1140, 40),
                ForeColor = Color.DimGray,
                Text = "Số lượt mượn tính trên bảng ChiTietMuon, gom nhóm theo DauSach.TheLoai (GROUP BY). " +
                       "Tổng số lượt lấy bằng ExecuteScalar."
            };

            tab.Controls.Add(btnXem);
            tab.Controls.Add(lblTongLuot);
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

        private class MucChon
        {
            public int Ma { get; set; }
            public string Ten { get; set; }
            public override string ToString() { return Ten; }
        }

        private void Bao(string thongBao)
        {
            lblTrangThai.Text = thongBao;
        }

        // =====================================================================
        //  Nạp dữ liệu
        // =====================================================================
        private void KhoiTaoDuLieuTam()
        {
            gioMuon.Columns.Add("MaSach", typeof(int));
            gioMuon.Columns.Add("TenSach", typeof(string));
            gioMuon.Columns.Add("SoLuong", typeof(int));
            dgvGioMuon.DataSource = gioMuon;
            DatTieuDe(dgvGioMuon, "Mã sách", "Tên sách", "Số lượng");
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                TaiDocGia();
                TaiDauSach();
                TaiComboLoc();
                TaiPhieu();
                TaiQuaHan();
                TaiThongKe();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi tải dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TaiDocGia()
        {
            DataTable t = docGiaBus.TimKiem(txtTimDocGia.Text);
            dgvDocGia.DataSource = t;
            DatTieuDe(dgvDocGia, "Mã", "Họ tên", "Email", "Ngày hết hạn thẻ");
            if (dgvDocGia.Columns.Count > 3)
            {
                dgvDocGia.Columns[3].DefaultCellStyle.Format = "dd/MM/yyyy";
            }
            Bao("Có " + t.Rows.Count + " độc giả.");
        }

        private void TaiDauSach()
        {
            DataTable t = dauSachBus.TimKiem(txtTimSach.Text);
            dgvDauSach.DataSource = t;
            DatTieuDe(dgvDauSach, "Mã", "Tên sách", "Thể loại", "Số lượng còn");

            // Nạp lại combo thể loại và combo chọn sách
            object dangChon = cboTheLoai.Text;
            cboTheLoai.Items.Clear();
            DataTable loai = dauSachBus.LayTheLoai();
            foreach (DataRow r in loai.Rows)
            {
                cboTheLoai.Items.Add(r["TheLoai"].ToString());
            }
            cboTheLoai.Text = dangChon as string;

            NapComboSach(t);
            Bao("Có " + t.Rows.Count + " đầu sách.");
        }

        private void NapComboSach(DataTable sach)
        {
            tonSach.Clear();
            cboSach.Items.Clear();
            foreach (DataRow r in sach.Rows)
            {
                int ma = Convert.ToInt32(r["MaSach"]);
                int con = Convert.ToInt32(r["SoLuongCon"]);
                tonSach[ma] = con;
                cboSach.Items.Add(new MucChon
                {
                    Ma = ma,
                    Ten = r["TenSach"] + "  (còn " + con + ")"
                });
            }
            if (cboSach.Items.Count > 0)
            {
                cboSach.SelectedIndex = 0;
            }
            numMuon.Maximum = 100;
        }

        private void TaiComboLoc()
        {
            DataTable t = docGiaBus.LayDanhSach();

            int maCu = 0;
            if (cboLocDocGia.SelectedItem is MucChon)
            {
                maCu = ((MucChon)cboLocDocGia.SelectedItem).Ma;
            }

            cboLocDocGia.Items.Clear();
            cboLocDocGia.Items.Add(new MucChon { Ma = 0, Ten = "— Tất cả độc giả —" });

            int maDgCu = 0;
            if (cboDocGiaMuon.SelectedItem is MucChon)
            {
                maDgCu = ((MucChon)cboDocGiaMuon.SelectedItem).Ma;
            }

            cboDocGiaMuon.Items.Clear();
            foreach (DataRow r in t.Rows)
            {
                var muc = new MucChon
                {
                    Ma = Convert.ToInt32(r["MaDocGia"]),
                    Ten = r["HoTen"] + (Convert.ToDateTime(r["NgayHetHanThe"]).Date < DateTime.Today
                        ? "  [thẻ hết hạn]"
                        : "")
                };
                cboLocDocGia.Items.Add(muc);
                cboDocGiaMuon.Items.Add(new MucChon { Ma = muc.Ma, Ten = muc.Ten });
            }

            cboLocDocGia.SelectedIndex = TimViTri(cboLocDocGia, maCu);
            cboDocGiaMuon.SelectedIndex = TimViTri(cboDocGiaMuon, maDgCu);
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
            return cbo.Items.Count > 0 ? 0 : -1;
        }

        private void TaiPhieu()
        {
            var muc = cboLocDocGia.SelectedItem as MucChon;
            int? maDocGia = (muc == null || muc.Ma == 0) ? (int?)null : muc.Ma;
            string trangThai = cboLocTrangThai.SelectedIndex <= 0
                ? null
                : cboLocTrangThai.SelectedItem.ToString();

            DataTable t = phieuMuonBus.LayDanhSach(maDocGia, trangThai);
            dgvPhieu.DataSource = t;
            DatTieuDe(dgvPhieu, "Mã phiếu", "Mã ĐG", "Độc giả", "Ngày mượn", "Hạn trả", "Trạng thái");
            if (dgvPhieu.Columns.Count >= 6)
            {
                dgvPhieu.Columns[3].DefaultCellStyle.Format = "dd/MM/yyyy";
                dgvPhieu.Columns[4].DefaultCellStyle.Format = "dd/MM/yyyy";
            }
            TaiChiTiet();
            Bao("Tìm thấy " + t.Rows.Count + " phiếu mượn.");
        }

        private void TaiChiTiet()
        {
            if (maPhieuDangChon <= 0)
            {
                dgvChiTiet.DataSource = null;
                return;
            }

            DataTable t = phieuMuonBus.LayChiTiet(maPhieuDangChon);
            dgvChiTiet.DataSource = t;
            DatTieuDe(dgvChiTiet, "Mã phiếu", "Mã sách", "Tên sách", "Số lượng");
        }

        private void TaiQuaHan()
        {
            DataTable t = phieuMuonBus.LayDanhSachQuaHan();
            dgvQuaHan.DataSource = t;
            DatTieuDe(dgvQuaHan, "Mã phiếu", "Độc giả", "Email", "Ngày mượn", "Hạn trả", "Số ngày quá hạn");
            if (dgvQuaHan.Columns.Count >= 6)
            {
                dgvQuaHan.Columns[3].DefaultCellStyle.Format = "dd/MM/yyyy";
                dgvQuaHan.Columns[4].DefaultCellStyle.Format = "dd/MM/yyyy";
            }
            lblDemQuaHan.Text = "Có " + t.Rows.Count + " phiếu quá hạn cần xử lý.";
            Bao("Đã cập nhật trạng thái quá hạn và tải " + t.Rows.Count + " phiếu.");
        }

        private void TaiThongKe()
        {
            DataTable t = phieuMuonBus.ThongKeTheoTheLoai();
            dgvThongKe.DataSource = t;
            DatTieuDe(dgvThongKe, "Thể loại", "Số lượt mượn", "Tổng số cuốn", "Số độc giả");
            lblTongLuot.Text = "Tổng số lượt mượn trong hệ thống: " + phieuMuonBus.DemTongLuotMuon();
            Bao("Thống kê " + t.Rows.Count + " thể loại.");
        }

        // =====================================================================
        //  Sự kiện: Độc giả
        // =====================================================================
        private void DgvDocGia_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvDocGia.CurrentRow == null || dgvDocGia.CurrentRow.Index < 0)
            {
                return;
            }

            DataGridViewRow r = dgvDocGia.CurrentRow;
            if (r.Cells[0].Value == null)
            {
                return;
            }

            maDocGiaDangChon = Convert.ToInt32(r.Cells[0].Value);
            txtHoTen.Text = Convert.ToString(r.Cells[1].Value);
            txtEmail.Text = Convert.ToString(r.Cells[2].Value);
            object ngay = r.Cells[3].Value;
            if (ngay != null && ngay != DBNull.Value)
            {
                dtpHetHan.Value = Convert.ToDateTime(ngay);
            }
        }

        private DocGiaDTO DocGiaTuForm()
        {
            return new DocGiaDTO
            {
                MaDocGia = maDocGiaDangChon,
                HoTen = txtHoTen.Text,
                Email = txtEmail.Text,
                NgayHetHanThe = dtpHetHan.Value.Date
            };
        }

        private void BtnThemDocGia_Click(object sender, EventArgs e)
        {
            try
            {
                DocGiaDTO dg = DocGiaTuForm();
                dg.MaDocGia = 0;
                docGiaBus.Them(dg);
                LamMoiDocGia();
                TaiDocGia();
                TaiComboLoc();
                TaiPhieu();
                Bao("Đã thêm độc giả \"" + dg.HoTen + "\".");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không thêm được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnSuaDocGia_Click(object sender, EventArgs e)
        {
            if (maDocGiaDangChon <= 0)
            {
                MessageBox.Show("Chọn một độc giả trong bảng trước.", "Thông báo");
                return;
            }

            try
            {
                DocGiaDTO dg = DocGiaTuForm();
                docGiaBus.Sua(dg);
                TaiDocGia();
                TaiComboLoc();
                Bao("Đã cập nhật độc giả \"" + dg.HoTen + "\".");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không sửa được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnXoaDocGia_Click(object sender, EventArgs e)
        {
            if (maDocGiaDangChon <= 0)
            {
                MessageBox.Show("Chọn một độc giả trong bảng trước.", "Thông báo");
                return;
            }

            if (MessageBox.Show("Xoá độc giả \"" + txtHoTen.Text + "\"?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                int ma = maDocGiaDangChon;
                docGiaBus.Xoa(ma);
                LamMoiDocGia();
                TaiDocGia();
                TaiComboLoc();
                Bao("Đã xoá độc giả mã " + ma + ".");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không xoá được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LamMoiDocGia()
        {
            maDocGiaDangChon = 0;
            txtHoTen.Clear();
            txtEmail.Clear();
            dtpHetHan.Value = DateTime.Today.AddYears(1);
            txtHoTen.Focus();
        }

        // =====================================================================
        //  Sự kiện: Đầu sách
        // =====================================================================
        private void DgvDauSach_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvDauSach.CurrentRow == null || dgvDauSach.CurrentRow.Index < 0)
            {
                return;
            }

            DataGridViewRow r = dgvDauSach.CurrentRow;
            if (r.Cells[0].Value == null)
            {
                return;
            }

            maSachDangChon = Convert.ToInt32(r.Cells[0].Value);
            txtTenSach.Text = Convert.ToString(r.Cells[1].Value);
            cboTheLoai.Text = Convert.ToString(r.Cells[2].Value);
            numSoLuong.Value = Convert.ToDecimal(r.Cells[3].Value);
        }

        private DauSachDTO SachTuForm()
        {
            return new DauSachDTO
            {
                MaSach = maSachDangChon,
                TenSach = txtTenSach.Text,
                TheLoai = cboTheLoai.Text,
                SoLuongCon = (int)numSoLuong.Value
            };
        }

        private void BtnThemSach_Click(object sender, EventArgs e)
        {
            try
            {
                DauSachDTO s = SachTuForm();
                s.MaSach = 0;
                dauSachBus.Them(s);
                LamMoiSach();
                TaiDauSach();
                Bao("Đã thêm đầu sách \"" + s.TenSach + "\".");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không thêm được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnSuaSach_Click(object sender, EventArgs e)
        {
            if (maSachDangChon <= 0)
            {
                MessageBox.Show("Chọn một đầu sách trong bảng trước.", "Thông báo");
                return;
            }

            try
            {
                DauSachDTO s = SachTuForm();
                dauSachBus.Sua(s);
                TaiDauSach();
                Bao("Đã cập nhật đầu sách \"" + s.TenSach + "\".");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không sửa được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnXoaSach_Click(object sender, EventArgs e)
        {
            if (maSachDangChon <= 0)
            {
                MessageBox.Show("Chọn một đầu sách trong bảng trước.", "Thông báo");
                return;
            }

            if (MessageBox.Show("Xoá đầu sách \"" + txtTenSach.Text + "\"?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                int ma = maSachDangChon;
                dauSachBus.Xoa(ma);
                LamMoiSach();
                TaiDauSach();
                Bao("Đã xoá đầu sách mã " + ma + ".");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không xoá được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LamMoiSach()
        {
            maSachDangChon = 0;
            txtTenSach.Clear();
            cboTheLoai.Text = "";
            numSoLuong.Value = 0;
            txtTenSach.Focus();
        }

        // =====================================================================
        //  Sự kiện: Mượn / Trả
        // =====================================================================
        private void DgvPhieu_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPhieu.CurrentRow == null || dgvPhieu.CurrentRow.Index < 0)
            {
                return;
            }

            object v = dgvPhieu.CurrentRow.Cells[0].Value;
            if (v == null)
            {
                return;
            }

            maPhieuDangChon = Convert.ToInt32(v);
            TaiChiTiet();
        }

        private void CboDocGiaMuon_SelectedIndexChanged(object sender, EventArgs e)
        {
            var muc = cboDocGiaMuon.SelectedItem as MucChon;
            if (muc == null)
            {
                return;
            }

            string thongBao;
            bool conHan = docGiaBus.TheConHan(muc.Ma, out thongBao);
            lblTheDocGia.Text = thongBao;
            lblTheDocGia.ForeColor = conHan ? Color.FromArgb(20, 110, 60) : Color.FromArgb(170, 40, 40);
        }

        private void BtnThemVaoGio_Click(object sender, EventArgs e)
        {
            var muc = cboSach.SelectedItem as MucChon;
            if (muc == null)
            {
                MessageBox.Show("Chưa chọn đầu sách.", "Thông báo");
                return;
            }

            int soLuong = (int)numMuon.Value;
            int daChon = 0;
            foreach (DataRow r in gioMuon.Rows)
            {
                if (Convert.ToInt32(r["MaSach"]) == muc.Ma)
                {
                    daChon = Convert.ToInt32(r["SoLuong"]);
                }
            }

            int con = tonSach.ContainsKey(muc.Ma) ? tonSach[muc.Ma] : 0;
            if (daChon + soLuong > con)
            {
                MessageBox.Show("Sách này chỉ còn " + con + " cuốn, trong phiếu đã chọn " + daChon + ".",
                    "Không đủ sách", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool daCo = false;
            foreach (DataRow r in gioMuon.Rows)
            {
                if (Convert.ToInt32(r["MaSach"]) == muc.Ma)
                {
                    r["SoLuong"] = daChon + soLuong;
                    daCo = true;
                    break;
                }
            }

            if (!daCo)
            {
                gioMuon.Rows.Add(muc.Ma, muc.Ten.Split(new[] { "  (còn" }, StringSplitOptions.None)[0], soLuong);
            }

            Bao("Đã thêm \"" + muc.Ten + "\" x" + soLuong + " vào phiếu.");
        }

        private void BtnBoSach_Click(object sender, EventArgs e)
        {
            if (dgvGioMuon.CurrentRow == null || dgvGioMuon.CurrentRow.Index < 0)
            {
                return;
            }

            gioMuon.Rows.RemoveAt(dgvGioMuon.CurrentRow.Index);
            Bao("Đã bỏ sách khỏi phiếu.");
        }

        private void BtnLapPhieu_Click(object sender, EventArgs e)
        {
            var muc = cboDocGiaMuon.SelectedItem as MucChon;
            if (muc == null)
            {
                MessageBox.Show("Chưa chọn độc giả.", "Thông báo");
                return;
            }

            if (gioMuon.Rows.Count == 0)
            {
                MessageBox.Show("Phiếu mượn chưa có sách nào.", "Thông báo");
                return;
            }

            var ds = new List<ChiTietMuonDTO>();
            foreach (DataRow r in gioMuon.Rows)
            {
                ds.Add(new ChiTietMuonDTO(
                    Convert.ToInt32(r["MaSach"]),
                    Convert.ToString(r["TenSach"]),
                    Convert.ToInt32(r["SoLuong"])));
            }

            try
            {
                int maPhieu = phieuMuonBus.LapPhieu(muc.Ma, dtpHanTra.Value.Date, ds);
                gioMuon.Rows.Clear();
                TaiPhieu();
                TaiDauSach();
                Bao("Đã lập phiếu số " + maPhieu + " cho " + muc.Ten + ".");
                MessageBox.Show("Lập phiếu thành công. Mã phiếu: " + maPhieu,
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không lập được phiếu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnTraSach_Click(object sender, EventArgs e)
        {
            if (maPhieuDangChon <= 0)
            {
                MessageBox.Show("Chọn một phiếu trong bảng trước.", "Thông báo");
                return;
            }

            if (MessageBox.Show("Xác nhận trả sách cho phiếu số " + maPhieuDangChon + "?",
                    "Xác nhận trả sách", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                phieuMuonBus.TraSach(maPhieuDangChon);
                TaiPhieu();
                TaiDauSach();
                TaiQuaHan();
                Bao("Đã trả sách cho phiếu số " + maPhieuDangChon + " và cộng lại số lượng.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không trả được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
