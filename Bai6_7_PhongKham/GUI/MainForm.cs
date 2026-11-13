using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Bai6_7_PhongKham.BUS;
using Bai6_7_PhongKham.DTO;

namespace Bai6_7_PhongKham.GUI
{
    /// <summary>
    /// Giao diện chính (Presentation Layer). Form chỉ gọi xuống BUS,
    /// không viết câu SQL nào và không mở kết nối trực tiếp.
    /// </summary>
    public class MainForm : Form
    {
        // ---------- BUS ----------
        private readonly BenhNhanBUS benhNhanBus = new BenhNhanBUS();
        private readonly HoSoKhamBUS hoSoBus = new HoSoKhamBUS();
        private readonly ThuocBUS thuocBus = new ThuocBUS();
        private readonly DonThuocBUS donThuocBus = new DonThuocBUS();

        // ---------- Trạng thái ----------
        private readonly DataTable donTam = new DataTable();
        private readonly Dictionary<int, ThuocDTO> thuocTheoMa = new Dictionary<int, ThuocDTO>();
        private BenhNhanDTO benhNhanHienTai;
        private HoSoKhamDTO hoSoHienTai;
        private int maThuocDangChon;
        private int maDonDangChon;
        private bool dangNapDuLieu;

        // ---------- Điều khiển ----------
        private TabControl tabs;
        private ToolStripStatusLabel lblTrangThai;

        // Tab 1
        private TextBox txtSoDienThoai, txtHoTen, txtTrieuChung, txtChanDoan;
        private NumericUpDown numNamSinh;
        private Label lblKetQuaTraCuu;
        private DataGridView dgvHoSo;

        // Tab 2
        private TextBox txtTenThuoc, txtTimThuoc;
        private ComboBox cboDonViTinh;
        private NumericUpDown numDonGia, numTon, numNhapTon;
        private DataGridView dgvThuoc;
        private Label lblThuocDangChon;

        // Tab 3
        private ComboBox cboHoSo, cboThuoc;
        private Label lblThongTinHoSo, lblTongDon;
        private NumericUpDown numTienKham, numSoLuongThuoc;
        private TextBox txtLieuDung;
        private DataGridView dgvDonTam;

        // Tab 4
        private DataGridView dgvDonCho, dgvChiTietDon;
        private Label lblDonDangChon, lblTienKham, lblTienThuoc, lblTongCong;
        private Button btnThanhToan;
        private ComboBox cboLocDon;
        private decimal tienThuocDangChon;

        public MainForm()
        {
            KhoiTaoGiaoDien();
            KhoiTaoBangTam();
        }

        // =====================================================================
        //  Dựng giao diện
        // =====================================================================
        private void KhoiTaoGiaoDien()
        {
            Text = "Bài 6.7 - Hệ thống quản lý phòng khám và kê đơn thuốc";
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1180, 720);
            MinimumSize = new Size(1024, 660);
            Font = new Font("Segoe UI", 9.5F);

            tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(TaoTabTiepNhan());
            tabs.TabPages.Add(TaoTabThuoc());
            tabs.TabPages.Add(TaoTabKeDon());
            tabs.TabPages.Add(TaoTabThanhToan());

            var status = new StatusStrip { SizingGrip = false };
            lblTrangThai = new ToolStripStatusLabel("Sẵn sàng.");
            status.Items.Add(lblTrangThai);

            Controls.Add(tabs);
            Controls.Add(status);

            Load += MainForm_Load;
        }

        /// <summary>
        /// TabPage mặc định 200x100; gắn Anchor cho control con ngay sẽ làm lưới bị kéo giãn sai.
        /// Vì vậy cấp sẵn kích thước vùng hiển thị ước tính của trang.
        /// </summary>
        private Size KichThuocTab()
        {
            return new Size(1172, 664);
        }

        private TabPage TaoTabTiepNhan()
        {
            var tab = new TabPage("1. Tiếp nhận") { Padding = new Padding(6), Size = KichThuocTab() };

            var gb = new GroupBox
            {
                Text = "Tra cứu và tiếp nhận bệnh nhân",
                Location = new Point(6, 6),
                Size = new Size(410, 640)
            };

            gb.Controls.Add(TaoNhan("Số điện thoại", 12, 24));
            txtSoDienThoai = TaoOChu(12, 44, 250);
            txtSoDienThoai.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    TraCuuBenhNhan();
                    e.SuppressKeyPress = true;
                }
            };
            gb.Controls.Add(txtSoDienThoai);

            var btnTraCuu = TaoNut("Tra cứu", 272, 42, 120);
            btnTraCuu.Click += (s, e) => TraCuuBenhNhan();
            gb.Controls.Add(btnTraCuu);

            lblKetQuaTraCuu = new Label
            {
                Location = new Point(12, 78),
                Size = new Size(380, 22),
                ForeColor = Color.DimGray,
                Text = "Nhập số điện thoại rồi bấm Tra cứu."
            };
            gb.Controls.Add(lblKetQuaTraCuu);

            gb.Controls.Add(TaoNhan("Họ tên bệnh nhân", 12, 110));
            txtHoTen = TaoOChu(12, 130, 380);
            gb.Controls.Add(txtHoTen);

            gb.Controls.Add(TaoNhan("Năm sinh", 12, 168));
            numNamSinh = new NumericUpDown
            {
                Location = new Point(12, 188),
                Size = new Size(120, 26),
                Minimum = 0,
                Maximum = DateTime.Today.Year,
                Value = 1995
            };
            gb.Controls.Add(numNamSinh);

            gb.Controls.Add(TaoNhan("Triệu chứng", 12, 226));
            txtTrieuChung = new TextBox
            {
                Location = new Point(12, 246),
                Size = new Size(380, 90),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            gb.Controls.Add(txtTrieuChung);

            gb.Controls.Add(TaoNhan("Chẩn đoán", 12, 346));
            txtChanDoan = new TextBox
            {
                Location = new Point(12, 366),
                Size = new Size(380, 60),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            gb.Controls.Add(txtChanDoan);

            var btnLuuBN = TaoNut("Lưu bệnh nhân", 12, 444, 124);
            var btnTaoHoSo = TaoNut("Tạo hồ sơ khám", 146, 444, 140);
            var btnMoi = TaoNut("Làm mới", 296, 444, 96);
            btnLuuBN.Click += BtnLuuBenhNhan_Click;
            btnTaoHoSo.Click += BtnTaoHoSo_Click;
            btnMoi.Click += (s, e) => LamMoiTiepNhan();
            gb.Controls.AddRange(new Control[] { btnLuuBN, btnTaoHoSo, btnMoi });

            var lblDs = TaoNhan("Hồ sơ khám gần đây", 430, 10);
            lblDs.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);

            dgvHoSo = TaoLuoi(430, 36, 730, 520);
            dgvHoSo.SelectionChanged += DgvHoSo_SelectionChanged;

            var btnSuaChanDoan = TaoNut("Cập nhật chẩn đoán", 430, 566, 170);
            btnSuaChanDoan.Click += BtnCapNhatChanDoan_Click;

            tab.Controls.Add(gb);
            tab.Controls.Add(lblDs);
            tab.Controls.Add(dgvHoSo);
            tab.Controls.Add(btnSuaChanDoan);
            return tab;
        }

        private TabPage TaoTabThuoc()
        {
            var tab = new TabPage("2. Danh mục thuốc") { Padding = new Padding(6), Size = KichThuocTab() };

            var gb = new GroupBox
            {
                Text = "Thông tin thuốc",
                Location = new Point(6, 6),
                Size = new Size(410, 300)
            };

            gb.Controls.Add(TaoNhan("Tên thuốc", 12, 24));
            txtTenThuoc = TaoOChu(12, 44, 380);
            gb.Controls.Add(txtTenThuoc);

            gb.Controls.Add(TaoNhan("Đơn vị tính", 12, 82));
            cboDonViTinh = new ComboBox
            {
                Location = new Point(12, 102),
                Size = new Size(150, 26),
                DropDownStyle = ComboBoxStyle.DropDown
            };
            cboDonViTinh.Items.AddRange(new object[] { "Viên", "Vỉ", "Chai", "Ống", "Gói", "Hộp" });
            gb.Controls.Add(cboDonViTinh);

            gb.Controls.Add(TaoNhan("Đơn giá (đ)", 178, 82));
            numDonGia = new NumericUpDown
            {
                Location = new Point(178, 102),
                Size = new Size(140, 26),
                Maximum = 100000000,
                Increment = 500,
                ThousandsSeparator = true
            };
            gb.Controls.Add(numDonGia);

            gb.Controls.Add(TaoNhan("Số lượng tồn", 12, 140));
            numTon = new NumericUpDown
            {
                Location = new Point(12, 160),
                Size = new Size(120, 26),
                Maximum = 100000
            };
            gb.Controls.Add(numTon);

            var btnThem = TaoNut("Thêm", 12, 200, 90);
            var btnSua = TaoNut("Sửa", 110, 200, 90);
            var btnXoa = TaoNut("Xoá", 208, 200, 90);
            var btnMoi = TaoNut("Làm mới", 306, 200, 86);
            btnThem.Click += BtnThemThuoc_Click;
            btnSua.Click += BtnSuaThuoc_Click;
            btnXoa.Click += BtnXoaThuoc_Click;
            btnMoi.Click += (s, e) => LamMoiThuoc();
            gb.Controls.AddRange(new Control[] { btnThem, btnSua, btnXoa, btnMoi });

            var gbNhap = new GroupBox
            {
                Text = "Nhập thêm hàng vào kho",
                Location = new Point(6, 318),
                Size = new Size(410, 110)
            };

            lblThuocDangChon = new Label
            {
                Location = new Point(12, 24),
                Size = new Size(380, 22),
                ForeColor = Color.DimGray,
                Text = "Chọn một thuốc trong bảng bên phải."
            };
            gbNhap.Controls.Add(lblThuocDangChon);

            numNhapTon = new NumericUpDown
            {
                Location = new Point(12, 56),
                Size = new Size(120, 26),
                Minimum = 1,
                Maximum = 100000,
                Value = 10
            };
            gbNhap.Controls.Add(numNhapTon);

            var btnNhap = TaoNut("Nhập vào kho", 142, 54, 140);
            btnNhap.Click += BtnNhapTon_Click;
            gbNhap.Controls.Add(btnNhap);

            var lblTim = TaoNhan("Tìm theo tên thuốc", 430, 10);
            txtTimThuoc = TaoOChu(430, 32, 300);
            txtTimThuoc.TextChanged += (s, e) => TaiDanhMucThuoc();
            var btnXoaLoc = TaoNut("Xoá lọc", 740, 30, 90);
            btnXoaLoc.Click += (s, e) => { txtTimThuoc.Clear(); TaiDanhMucThuoc(); };

            dgvThuoc = TaoLuoi(430, 68, 730, 578);
            dgvThuoc.SelectionChanged += DgvThuoc_SelectionChanged;

            tab.Controls.Add(gb);
            tab.Controls.Add(gbNhap);
            tab.Controls.Add(lblTim);
            tab.Controls.Add(txtTimThuoc);
            tab.Controls.Add(btnXoaLoc);
            tab.Controls.Add(dgvThuoc);
            return tab;
        }

        private TabPage TaoTabKeDon()
        {
            var tab = new TabPage("3. Kê đơn thuốc") { Padding = new Padding(6), Size = KichThuocTab() };

            var gbHoSo = new GroupBox
            {
                Text = "1. Chọn hồ sơ khám",
                Location = new Point(6, 6),
                Size = new Size(430, 230)
            };

            cboHoSo = new ComboBox
            {
                Location = new Point(12, 26),
                Size = new Size(404, 26),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboHoSo.SelectedIndexChanged += CboHoSo_SelectedIndexChanged;
            gbHoSo.Controls.Add(cboHoSo);

            lblThongTinHoSo = new Label
            {
                Location = new Point(12, 60),
                Size = new Size(404, 60),
                ForeColor = Color.DimGray,
                Text = "Chọn hồ sơ để xem thông tin bệnh nhân."
            };
            gbHoSo.Controls.Add(lblThongTinHoSo);

            gbHoSo.Controls.Add(TaoNhan("Tiền khám (đ)", 12, 126));
            numTienKham = new NumericUpDown
            {
                Location = new Point(12, 146),
                Size = new Size(140, 26),
                Maximum = 10000000,
                Increment = 10000,
                ThousandsSeparator = true,
                Value = 50000
            };
            numTienKham.ValueChanged += (s, e) => CapNhatTongDon();
            gbHoSo.Controls.Add(numTienKham);

            var btnXemDonCu = TaoNut("Đơn của hồ sơ này", 168, 144, 150);
            btnXemDonCu.Click += BtnXemDonCuaHoSo_Click;
            gbHoSo.Controls.Add(btnXemDonCu);

            var gbThuoc = new GroupBox
            {
                Text = "2. Chọn thuốc để kê",
                Location = new Point(6, 244),
                Size = new Size(430, 350)
            };

            cboThuoc = new ComboBox
            {
                Location = new Point(12, 26),
                Size = new Size(404, 26),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            gbThuoc.Controls.Add(cboThuoc);

            gbThuoc.Controls.Add(TaoNhan("Số lượng", 12, 64));
            numSoLuongThuoc = new NumericUpDown
            {
                Location = new Point(12, 84),
                Size = new Size(90, 26),
                Minimum = 1,
                Maximum = 10000,
                Value = 10
            };
            gbThuoc.Controls.Add(numSoLuongThuoc);

            gbThuoc.Controls.Add(TaoNhan("Liều dùng", 118, 64));
            txtLieuDung = TaoOChu(118, 84, 298);
            txtLieuDung.Text = "Sáng 1 viên, chiều 1 viên sau ăn";
            gbThuoc.Controls.Add(txtLieuDung);

            var btnThemThuoc = TaoNut("Thêm vào đơn", 12, 124, 150);
            btnThemThuoc.Click += BtnThemVaoDon_Click;
            gbThuoc.Controls.Add(btnThemThuoc);

            var btnBoDong = TaoNut("Bỏ dòng chọn", 174, 124, 150);
            btnBoDong.Click += BtnBoDong_Click;
            gbThuoc.Controls.Add(btnBoDong);

            var lblHuongDan = new Label
            {
                Location = new Point(12, 168),
                Size = new Size(404, 150),
                ForeColor = Color.DimGray,
                Text = "Lưu ý: khi bấm \"Lưu && Xuất đơn\", hệ thống mở SqlTransaction để " +
                       "tạo DonThuoc, tạo ChiTietDonThuoc và trừ SoLuongTon trong bảng Thuoc. " +
                       "Nếu một loại thuốc không đủ tồn, toàn bộ đơn bị huỷ (Rollback)."
            };
            gbThuoc.Controls.Add(lblHuongDan);

            var gbDon = new GroupBox
            {
                Text = "3. Đơn thuốc đang lập",
                Location = new Point(448, 6),
                Size = new Size(718, 588)
            };

            dgvDonTam = TaoLuoi(12, 24, 694, 440);
            gbDon.Controls.Add(dgvDonTam);

            lblTongDon = new Label
            {
                Location = new Point(12, 476),
                Size = new Size(520, 26),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 56, 100),
                Text = "Tổng tạm tính: 0 đ"
            };
            gbDon.Controls.Add(lblTongDon);

            var btnLuuDon = TaoNut("Lưu && Xuất đơn", 546, 470, 160);
            btnLuuDon.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnLuuDon.Click += BtnLuuDon_Click;
            gbDon.Controls.Add(btnLuuDon);

            tab.Controls.Add(gbHoSo);
            tab.Controls.Add(gbThuoc);
            tab.Controls.Add(gbDon);
            return tab;
        }

        private TabPage TaoTabThanhToan()
        {
            var tab = new TabPage("4. Thanh toán") { Padding = new Padding(6), Size = KichThuocTab() };

            var gbCho = new GroupBox
            {
                Text = "Danh sách đơn thuốc",
                Location = new Point(6, 6),
                Size = new Size(600, 610)
            };

            cboLocDon = new ComboBox
            {
                Location = new Point(12, 24),
                Size = new Size(200, 26),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboLocDon.Items.AddRange(new object[] { "— Tất cả —", "Chờ lấy thuốc", "Đã thanh toán" });
            cboLocDon.SelectedIndex = 0;
            cboLocDon.SelectedIndexChanged += (s, e) => TaiDanhSachDon();
            gbCho.Controls.Add(cboLocDon);

            var btnTaiLai = TaoNut("Tải lại", 224, 22, 100);
            btnTaiLai.Click += (s, e) => TaiDanhSachDon();
            gbCho.Controls.Add(btnTaiLai);

            dgvDonCho = TaoLuoi(12, 60, 576, 536);
            dgvDonCho.SelectionChanged += DgvDonCho_SelectionChanged;
            gbCho.Controls.Add(dgvDonCho);

            var gbChiTiet = new GroupBox
            {
                Text = "Chi tiết và thanh toán",
                Location = new Point(616, 6),
                Size = new Size(550, 610)
            };

            lblDonDangChon = new Label
            {
                Location = new Point(12, 24),
                Size = new Size(526, 22),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 56, 100),
                Text = "Chưa chọn đơn."
            };
            gbChiTiet.Controls.Add(lblDonDangChon);

            dgvChiTietDon = TaoLuoi(12, 50, 526, 320);
            gbChiTiet.Controls.Add(dgvChiTietDon);

            lblTienKham = new Label
            {
                Location = new Point(12, 386),
                Size = new Size(526, 22),
                Text = "Tiền khám: 0 đ"
            };
            lblTienThuoc = new Label
            {
                Location = new Point(12, 410),
                Size = new Size(526, 22),
                Text = "Tiền thuốc: 0 đ"
            };
            lblTongCong = new Label
            {
                Location = new Point(12, 436),
                Size = new Size(526, 26),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(160, 40, 40),
                Text = "Tổng cộng: 0 đ"
            };
            gbChiTiet.Controls.Add(lblTienKham);
            gbChiTiet.Controls.Add(lblTienThuoc);
            gbChiTiet.Controls.Add(lblTongCong);

            btnThanhToan = TaoNut("Thanh toán", 12, 480, 170);
            btnThanhToan.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnThanhToan.Click += BtnThanhToan_Click;
            gbChiTiet.Controls.Add(btnThanhToan);

            var btnInHoaDon = TaoNut("Xem lại hoá đơn", 194, 480, 160);
            btnInHoaDon.Click += BtnXemHoaDon_Click;
            gbChiTiet.Controls.Add(btnInHoaDon);

            var lblGhiChu = new Label
            {
                Location = new Point(12, 526),
                Size = new Size(526, 66),
                ForeColor = Color.DimGray,
                Text = "Tổng tiền = Tiền khám + Tổng (Số lượng × Đơn giá). " +
                       "Sau khi thanh toán, trạng thái đơn chuyển sang \"Đã thanh toán\"."
            };
            gbChiTiet.Controls.Add(lblGhiChu);

            tab.Controls.Add(gbCho);
            tab.Controls.Add(gbChiTiet);
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

        private static string Tien(decimal soTien)
        {
            return soTien.ToString("#,##0") + " đ";
        }

        // =====================================================================
        //  Nạp dữ liệu
        // =====================================================================
        private void KhoiTaoBangTam()
        {
            donTam.Columns.Add("MaThuoc", typeof(int));
            donTam.Columns.Add("TenThuoc", typeof(string));
            donTam.Columns.Add("DonViTinh", typeof(string));
            donTam.Columns.Add("DonGia", typeof(decimal));
            donTam.Columns.Add("SoLuong", typeof(int));
            donTam.Columns.Add("LieuDung", typeof(string));
            donTam.Columns.Add("ThanhTien", typeof(decimal));

            dgvDonTam.DataSource = donTam;
            DatTieuDe(dgvDonTam, "Mã", "Tên thuốc", "ĐVT", "Đơn giá", "SL", "Liều dùng", "Thành tiền");
            if (dgvDonTam.Columns.Count >= 7)
            {
                dgvDonTam.Columns[3].DefaultCellStyle.Format = "N0";
                dgvDonTam.Columns[6].DefaultCellStyle.Format = "N0";
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                TaiHoSo();
                TaiDanhMucThuoc();
                TaiComboHoSo();
                TaiComboThuoc();
                TaiDanhSachDon();
                CapNhatTongDon();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi tải dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TaiHoSo()
        {
            DataTable t = hoSoBus.LayDanhSach();
            dgvHoSo.DataSource = t;
            DatTieuDe(dgvHoSo, "Mã hồ sơ", "Mã BN", "Bệnh nhân", "SĐT", "Ngày khám", "Triệu chứng", "Chẩn đoán");
            if (dgvHoSo.Columns.Count >= 5)
            {
                dgvHoSo.Columns[4].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            }
            Bao("Có " + t.Rows.Count + " hồ sơ khám.");
        }

        private void TaiDanhMucThuoc()
        {
            DataTable t = thuocBus.TimKiem(txtTimThuoc.Text);
            thuocTheoMa.Clear();
            foreach (DataRow r in t.Rows)
            {
                var th = new ThuocDTO
                {
                    MaThuoc = Convert.ToInt32(r["MaThuoc"]),
                    TenThuoc = Convert.ToString(r["TenThuoc"]),
                    DonViTinh = Convert.ToString(r["DonViTinh"]),
                    DonGia = Convert.ToDecimal(r["DonGia"]),
                    SoLuongTon = Convert.ToInt32(r["SoLuongTon"])
                };
                thuocTheoMa[th.MaThuoc] = th;
            }

            dgvThuoc.DataSource = t;
            DatTieuDe(dgvThuoc, "Mã", "Tên thuốc", "Đơn vị tính", "Đơn giá", "Tồn kho");
            if (dgvThuoc.Columns.Count >= 4)
            {
                dgvThuoc.Columns[3].DefaultCellStyle.Format = "N0";
            }

            TaiComboThuoc();
            Bao("Danh mục có " + t.Rows.Count + " loại thuốc.");
        }

        private void TaiComboThuoc()
        {
            int maCu = 0;
            if (cboThuoc.SelectedItem is MucChon)
            {
                maCu = ((MucChon)cboThuoc.SelectedItem).Ma;
            }

            cboThuoc.Items.Clear();
            foreach (ThuocDTO th in thuocTheoMa.Values)
            {
                cboThuoc.Items.Add(new MucChon
                {
                    Ma = th.MaThuoc,
                    Ten = th.TenThuoc + " - " + Tien(th.DonGia) + " (" + th.DonViTinh + ", tồn " + th.SoLuongTon + ")"
                });
            }

            if (cboThuoc.Items.Count > 0)
            {
                int viTri = 0;
                for (int i = 0; i < cboThuoc.Items.Count; i++)
                {
                    if (((MucChon)cboThuoc.Items[i]).Ma == maCu)
                    {
                        viTri = i;
                    }
                }
                cboThuoc.SelectedIndex = viTri;
            }
        }

        private void TaiComboHoSo()
        {
            int maCu = 0;
            if (cboHoSo.SelectedItem is MucChon)
            {
                maCu = ((MucChon)cboHoSo.SelectedItem).Ma;
            }

            dangNapDuLieu = true;
            cboHoSo.Items.Clear();
            DataTable t = hoSoBus.LayDanhSach();
            foreach (DataRow r in t.Rows)
            {
                cboHoSo.Items.Add(new MucChon
                {
                    Ma = Convert.ToInt32(r["MaHoSo"]),
                    Ten = "#" + r["MaHoSo"] + " - " + r["TenBenhNhan"] + " - " +
                          Convert.ToDateTime(r["NgayKham"]).ToString("dd/MM/yyyy")
                });
            }

            if (cboHoSo.Items.Count > 0)
            {
                int viTri = 0;
                for (int i = 0; i < cboHoSo.Items.Count; i++)
                {
                    if (((MucChon)cboHoSo.Items[i]).Ma == maCu)
                    {
                        viTri = i;
                    }
                }
                cboHoSo.SelectedIndex = viTri;
            }
            dangNapDuLieu = false;
            HienThiThongTinHoSo();
        }

        private void TaiDanhSachDon()
        {
            string trangThai = cboLocDon.SelectedIndex <= 0 ? null : cboLocDon.SelectedItem.ToString();
            DataTable t = donThuocBus.LayDanhSach(trangThai);
            dgvDonCho.DataSource = t;
            DatTieuDe(dgvDonCho, "Mã đơn", "Mã HS", "Bệnh nhân", "Ngày khám", "Tiền khám", "Tổng tiền", "Trạng thái");
            if (dgvDonCho.Columns.Count >= 6)
            {
                dgvDonCho.Columns[1].FillWeight = 60;
                dgvDonCho.Columns[2].FillWeight = 130;
                dgvDonCho.Columns[6].FillWeight = 120;
                dgvDonCho.Columns[3].DefaultCellStyle.Format = "dd/MM/yyyy";
                dgvDonCho.Columns[4].DefaultCellStyle.Format = "N0";
                dgvDonCho.Columns[5].DefaultCellStyle.Format = "N0";
            }
            Bao("Có " + t.Rows.Count + " đơn thuốc.");
        }

        // =====================================================================
        //  Tab 1: tiếp nhận
        // =====================================================================
        private void TraCuuBenhNhan()
        {
            string sdt = txtSoDienThoai.Text.Trim();
            if (sdt.Length == 0)
            {
                MessageBox.Show("Nhập số điện thoại cần tra cứu.", "Thông báo");
                return;
            }

            BenhNhanDTO bn = benhNhanBus.TraCuu(sdt);
            if (bn == null)
            {
                benhNhanHienTai = null;
                txtHoTen.Clear();
                txtTrieuChung.Clear();
                txtChanDoan.Clear();
                lblKetQuaTraCuu.ForeColor = Color.FromArgb(170, 100, 20);
                lblKetQuaTraCuu.Text = "Không tìm thấy bệnh nhân. Nhập thông tin để tạo mới.";
                txtHoTen.Focus();
            }
            else
            {
                benhNhanHienTai = bn;
                txtHoTen.Text = bn.HoTen;
                numNamSinh.Value = bn.NamSinh.HasValue ? bn.NamSinh.Value : 1990;
                lblKetQuaTraCuu.ForeColor = Color.FromArgb(20, 110, 60);
                lblKetQuaTraCuu.Text = "Đã tìm thấy: " + bn.HoTen + " (mã " + bn.MaBN + ").";
            }
            Bao("Tra cứu số điện thoại " + sdt + ".");
        }

        private void BtnLuuBenhNhan_Click(object sender, EventArgs e)
        {
            try
            {
                var bn = new BenhNhanDTO
                {
                    MaBN = benhNhanHienTai == null ? 0 : benhNhanHienTai.MaBN,
                    HoTen = txtHoTen.Text,
                    SoDienThoai = txtSoDienThoai.Text.Trim(),
                    NamSinh = (int)numNamSinh.Value
                };

                int maBN = benhNhanBus.Luu(bn);
                benhNhanHienTai = benhNhanBus.LayTheoMa(maBN);
                lblKetQuaTraCuu.ForeColor = Color.FromArgb(20, 110, 60);
                lblKetQuaTraCuu.Text = "Đã lưu bệnh nhân mã " + maBN + ". Có thể tạo hồ sơ khám.";
                Bao("Đã lưu bệnh nhân " + bn.HoTen + ".");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không lưu được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnTaoHoSo_Click(object sender, EventArgs e)
        {
            try
            {
                if (benhNhanHienTai == null)
                {
                    throw new InvalidOperationException("Hãy tra cứu hoặc lưu bệnh nhân trước khi tạo hồ sơ.");
                }

                int maHoSo = hoSoBus.TaoHoSo(benhNhanHienTai.MaBN, txtTrieuChung.Text, txtChanDoan.Text);
                TaiHoSo();
                TaiComboHoSo();

                // Chuyển sang tab kê đơn và chọn đúng hồ sơ vừa tạo
                for (int i = 0; i < cboHoSo.Items.Count; i++)
                {
                    if (((MucChon)cboHoSo.Items[i]).Ma == maHoSo)
                    {
                        cboHoSo.SelectedIndex = i;
                        break;
                    }
                }
                tabs.SelectedIndex = 2;
                Bao("Đã tạo hồ sơ khám số " + maHoSo + ".");
                MessageBox.Show("Đã tạo hồ sơ khám số " + maHoSo + ".\nChuyển sang bước kê đơn thuốc.",
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không tạo được hồ sơ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnCapNhatChanDoan_Click(object sender, EventArgs e)
        {
            try
            {
                if (hoSoHienTai == null)
                {
                    MessageBox.Show("Chọn một hồ sơ trong bảng.", "Thông báo");
                    return;
                }

                hoSoBus.CapNhatChanDoan(hoSoHienTai.MaHoSo, txtChanDoan.Text);
                TaiHoSo();
                TaiComboHoSo();
                Bao("Đã cập nhật chẩn đoán hồ sơ " + hoSoHienTai.MaHoSo + ".");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không cập nhật được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DgvHoSo_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvHoSo.CurrentRow == null || dgvHoSo.CurrentRow.Index < 0)
            {
                return;
            }

            object v = dgvHoSo.CurrentRow.Cells[0].Value;
            if (v == null)
            {
                return;
            }

            hoSoHienTai = hoSoBus.LayTheoMa(Convert.ToInt32(v));
            if (hoSoHienTai == null)
            {
                return;
            }

            benhNhanHienTai = benhNhanBus.LayTheoMa(hoSoHienTai.MaBN);
            txtSoDienThoai.Text = hoSoHienTai.SoDienThoai;
            txtHoTen.Text = hoSoHienTai.TenBenhNhan;
            txtTrieuChung.Text = hoSoHienTai.TrieuChung;
            txtChanDoan.Text = hoSoHienTai.ChanDoan;
            numNamSinh.Value = benhNhanHienTai != null && benhNhanHienTai.NamSinh.HasValue
                ? Math.Min(numNamSinh.Maximum, benhNhanHienTai.NamSinh.Value)
                : numNamSinh.Value;
            lblKetQuaTraCuu.ForeColor = Color.FromArgb(20, 110, 60);
            lblKetQuaTraCuu.Text = "Đang xem hồ sơ #" + hoSoHienTai.MaHoSo + " của " + hoSoHienTai.TenBenhNhan + ".";
        }

        private void LamMoiTiepNhan()
        {
            benhNhanHienTai = null;
            hoSoHienTai = null;
            txtSoDienThoai.Clear();
            txtHoTen.Clear();
            txtTrieuChung.Clear();
            txtChanDoan.Clear();
            numNamSinh.Value = 1995;
            lblKetQuaTraCuu.ForeColor = Color.DimGray;
            lblKetQuaTraCuu.Text = "Nhập số điện thoại rồi bấm Tra cứu.";
            txtSoDienThoai.Focus();
        }

        // =====================================================================
        //  Tab 2: danh mục thuốc
        // =====================================================================
        private void DgvThuoc_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvThuoc.CurrentRow == null || dgvThuoc.CurrentRow.Index < 0)
            {
                return;
            }

            object v = dgvThuoc.CurrentRow.Cells[0].Value;
            if (v == null)
            {
                return;
            }

            maThuocDangChon = Convert.ToInt32(v);
            DataGridViewRow r = dgvThuoc.CurrentRow;
            txtTenThuoc.Text = Convert.ToString(r.Cells[1].Value);
            cboDonViTinh.Text = Convert.ToString(r.Cells[2].Value);
            numDonGia.Value = Math.Min(numDonGia.Maximum, Convert.ToDecimal(r.Cells[3].Value));
            numTon.Value = Math.Min(numTon.Maximum, Convert.ToDecimal(r.Cells[4].Value));
            lblThuocDangChon.Text = "Đang chọn: " + txtTenThuoc.Text + " (mã " + maThuocDangChon + ").";
        }

        private ThuocDTO ThuocTuForm()
        {
            return new ThuocDTO
            {
                MaThuoc = maThuocDangChon,
                TenThuoc = txtTenThuoc.Text,
                DonViTinh = cboDonViTinh.Text,
                DonGia = numDonGia.Value,
                SoLuongTon = (int)numTon.Value
            };
        }

        private void BtnThemThuoc_Click(object sender, EventArgs e)
        {
            try
            {
                ThuocDTO t = ThuocTuForm();
                t.MaThuoc = 0;
                thuocBus.Them(t);
                LamMoiThuoc();
                TaiDanhMucThuoc();
                Bao("Đã thêm thuốc \"" + t.TenThuoc + "\".");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không thêm được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnSuaThuoc_Click(object sender, EventArgs e)
        {
            if (maThuocDangChon <= 0)
            {
                MessageBox.Show("Chọn một thuốc trong bảng.", "Thông báo");
                return;
            }

            try
            {
                ThuocDTO t = ThuocTuForm();
                thuocBus.Sua(t);
                TaiDanhMucThuoc();
                Bao("Đã cập nhật thuốc \"" + t.TenThuoc + "\".");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không sửa được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnXoaThuoc_Click(object sender, EventArgs e)
        {
            if (maThuocDangChon <= 0)
            {
                MessageBox.Show("Chọn một thuốc trong bảng.", "Thông báo");
                return;
            }

            if (MessageBox.Show("Xoá thuốc \"" + txtTenThuoc.Text + "\"?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                int ma = maThuocDangChon;
                thuocBus.Xoa(ma);
                LamMoiThuoc();
                TaiDanhMucThuoc();
                Bao("Đã xoá thuốc mã " + ma + ".");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không xoá được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnNhapTon_Click(object sender, EventArgs e)
        {
            try
            {
                thuocBus.NhapThemTon(maThuocDangChon, (int)numNhapTon.Value);
                TaiDanhMucThuoc();
                Bao("Đã nhập thêm " + numNhapTon.Value + " vào kho thuốc \"" + txtTenThuoc.Text + "\".");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không nhập được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LamMoiThuoc()
        {
            maThuocDangChon = 0;
            txtTenThuoc.Clear();
            cboDonViTinh.Text = "Viên";
            numDonGia.Value = 0;
            numTon.Value = 0;
            lblThuocDangChon.Text = "Chọn một thuốc trong bảng bên phải.";
            txtTenThuoc.Focus();
        }

        // =====================================================================
        //  Tab 3: kê đơn
        // =====================================================================
        private void HienThiThongTinHoSo()
        {
            var muc = cboHoSo.SelectedItem as MucChon;
            if (muc == null)
            {
                lblThongTinHoSo.Text = "Chưa có hồ sơ khám. Hãy tạo ở tab 1.";
                return;
            }

            HoSoKhamDTO hs = hoSoBus.LayTheoMa(muc.Ma);
            if (hs == null)
            {
                return;
            }

            lblThongTinHoSo.Text = "Bệnh nhân: " + hs.TenBenhNhan + " - SĐT " + hs.SoDienThoai +
                                   "\nNgày khám: " + hs.NgayKham.ToString("dd/MM/yyyy HH:mm") +
                                   "\nChẩn đoán: " + hs.ChanDoan;
        }

        private void CboHoSo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dangNapDuLieu)
            {
                return;
            }

            HienThiThongTinHoSo();
            donTam.Rows.Clear();
            CapNhatTongDon();
            Bao("Đã chọn hồ sơ để kê đơn.");
        }

        private void BtnThemVaoDon_Click(object sender, EventArgs e)
        {
            var muc = cboThuoc.SelectedItem as MucChon;
            if (muc == null)
            {
                MessageBox.Show("Chưa chọn thuốc.", "Thông báo");
                return;
            }

            if (!thuocTheoMa.ContainsKey(muc.Ma))
            {
                MessageBox.Show("Thuốc không còn trong danh mục.", "Thông báo");
                return;
            }

            ThuocDTO th = thuocTheoMa[muc.Ma];
            int soLuong = (int)numSoLuongThuoc.Value;
            int daCo = 0;
            foreach (DataRow r in donTam.Rows)
            {
                if (Convert.ToInt32(r["MaThuoc"]) == muc.Ma)
                {
                    daCo = Convert.ToInt32(r["SoLuong"]);
                }
            }

            if (daCo + soLuong > th.SoLuongTon)
            {
                MessageBox.Show("Thuốc \"" + th.TenThuoc + "\" chỉ còn " + th.SoLuongTon +
                                " " + th.DonViTinh + ", trong đơn đã chọn " + daCo + ".",
                    "Không đủ tồn kho", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool daCoDong = false;
            foreach (DataRow r in donTam.Rows)
            {
                if (Convert.ToInt32(r["MaThuoc"]) == muc.Ma)
                {
                    r["SoLuong"] = daCo + soLuong;
                    r["ThanhTien"] = (daCo + soLuong) * th.DonGia;
                    daCoDong = true;
                    break;
                }
            }

            if (!daCoDong)
            {
                donTam.Rows.Add(th.MaThuoc, th.TenThuoc, th.DonViTinh, th.DonGia,
                    soLuong, txtLieuDung.Text.Trim(), soLuong * th.DonGia);
            }

            CapNhatTongDon();
            Bao("Đã thêm " + th.TenThuoc + " x" + soLuong + " vào đơn.");
        }

        private void BtnBoDong_Click(object sender, EventArgs e)
        {
            if (dgvDonTam.CurrentRow == null || dgvDonTam.CurrentRow.Index < 0)
            {
                return;
            }

            donTam.Rows.RemoveAt(dgvDonTam.CurrentRow.Index);
            CapNhatTongDon();
            Bao("Đã bỏ một dòng thuốc khỏi đơn.");
        }

        private void CapNhatTongDon()
        {
            decimal tongThuoc = 0;
            foreach (DataRow r in donTam.Rows)
            {
                tongThuoc += Convert.ToDecimal(r["ThanhTien"]);
            }

            lblTongDon.Text = "Tổng tạm tính: " + Tien(numTienKham.Value + tongThuoc) +
                              "  (khám " + Tien(numTienKham.Value) + " + thuốc " + Tien(tongThuoc) + ")";
        }

        private void BtnLuuDon_Click(object sender, EventArgs e)
        {
            var muc = cboHoSo.SelectedItem as MucChon;
            if (muc == null)
            {
                MessageBox.Show("Chưa chọn hồ sơ khám.", "Thông báo");
                return;
            }

            if (donTam.Rows.Count == 0)
            {
                MessageBox.Show("Đơn thuốc chưa có thuốc nào.", "Thông báo");
                return;
            }

            var ds = new List<ChiTietDonThuocDTO>();
            foreach (DataRow r in donTam.Rows)
            {
                ds.Add(new ChiTietDonThuocDTO(
                    Convert.ToInt32(r["MaThuoc"]),
                    Convert.ToString(r["TenThuoc"]),
                    Convert.ToString(r["DonViTinh"]),
                    Convert.ToDecimal(r["DonGia"]),
                    Convert.ToInt32(r["SoLuong"]),
                    Convert.ToString(r["LieuDung"])));
            }

            try
            {
                int maDon = donThuocBus.LuuDon(muc.Ma, numTienKham.Value, ds);

                donTam.Rows.Clear();
                CapNhatTongDon();
                TaiDanhMucThuoc();
                TaiDanhSachDon();
                Bao("Đã lưu đơn " + maDon + " và trừ tồn kho.");
                MessageBox.Show("Đã lưu đơn thuốc số " + maDon + " và xuất kho thành công.",
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\nToàn bộ đơn đã được huỷ (Rollback).",
                    "Không lưu được đơn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnXemDonCuaHoSo_Click(object sender, EventArgs e)
        {
            var muc = cboHoSo.SelectedItem as MucChon;
            if (muc == null)
            {
                return;
            }

            tabs.SelectedIndex = 3;
            cboLocDon.SelectedIndex = 0;
            TaiDanhSachDon();

            for (int i = 0; i < dgvDonCho.Rows.Count; i++)
            {
                object maHoSo = dgvDonCho.Rows[i].Cells[1].Value;
                if (maHoSo != null && Convert.ToInt32(maHoSo) == muc.Ma)
                {
                    dgvDonCho.CurrentCell = dgvDonCho.Rows[i].Cells[0];
                    break;
                }
            }
        }

        // =====================================================================
        //  Tab 4: thanh toán
        // =====================================================================
        private void DgvDonCho_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvDonCho.CurrentRow == null || dgvDonCho.CurrentRow.Index < 0)
            {
                return;
            }

            object v = dgvDonCho.CurrentRow.Cells[0].Value;
            if (v == null)
            {
                return;
            }

            maDonDangChon = Convert.ToInt32(v);
            HienThiChiTietDon(maDonDangChon);
        }

        private void HienThiChiTietDon(int maDon)
        {
            DonThuocDTO don = donThuocBus.LayTheoMa(maDon);
            if (don == null)
            {
                return;
            }

            DataTable ct = donThuocBus.LayChiTiet(maDon);
            dgvChiTietDon.DataSource = ct;
            DatTieuDe(dgvChiTietDon, "Đơn", "Mã", "Tên thuốc", "ĐVT", "Đơn giá", "SL", "Liều dùng", "Thành tiền");
            dgvChiTietDon.ColumnHeadersHeight = 46;
            if (dgvChiTietDon.Columns.Count >= 8)
            {
                dgvChiTietDon.Columns[0].FillWeight = 50;
                dgvChiTietDon.Columns[1].FillWeight = 55;
                dgvChiTietDon.Columns[2].FillWeight = 135;
                dgvChiTietDon.Columns[6].FillWeight = 150;
                dgvChiTietDon.Columns[4].DefaultCellStyle.Format = "N0";
                dgvChiTietDon.Columns[7].DefaultCellStyle.Format = "N0";
            }

            decimal tongThuoc = 0;
            foreach (DataRow r in ct.Rows)
            {
                tongThuoc += Convert.ToDecimal(r["ThanhTien"]);
            }
            tienThuocDangChon = tongThuoc;

            lblDonDangChon.Text = "Đơn #" + don.MaDon + " - " + don.TenBenhNhan + " - " + don.TrangThai;
            lblTienKham.Text = "Tiền khám: " + Tien(don.TienKham);
            lblTienThuoc.Text = "Tiền thuốc: " + Tien(tongThuoc);
            lblTongCong.Text = "Tổng cộng: " + Tien(don.TienKham + tongThuoc);
            btnThanhToan.Enabled = don.TrangThai == "Chờ lấy thuốc";
        }

        private void BtnThanhToan_Click(object sender, EventArgs e)
        {
            if (maDonDangChon <= 0)
            {
                MessageBox.Show("Chọn một đơn trong bảng.", "Thông báo");
                return;
            }

            DonThuocDTO don = donThuocBus.LayTheoMa(maDonDangChon);
            if (don == null)
            {
                return;
            }

            string hoi = "Thanh toán đơn #" + don.MaDon + "?\n\n" +
                         "Tiền khám: " + Tien(don.TienKham) + "\n" +
                         "Tiền thuốc: " + Tien(tienThuocDangChon) + "\n" +
                         "Tổng cộng: " + Tien(don.TienKham + tienThuocDangChon);
            if (MessageBox.Show(hoi, "Xác nhận thanh toán", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                decimal tienThuoc;
                decimal tongTien = donThuocBus.ThanhToan(maDonDangChon, out tienThuoc);
                TaiDanhSachDon();
                HienThiChiTietDon(maDonDangChon);
                Bao("Đã thanh toán đơn " + maDonDangChon + " - tổng " + Tien(tongTien) + ".");

                MessageBox.Show(
                    "HOÁ ĐƠN KHÁM BỆNH\n" +
                    "--------------------------------\n" +
                    "Mã đơn: " + don.MaDon + "\n" +
                    "Bệnh nhân: " + don.TenBenhNhan + "\n" +
                    "Tiền khám: " + Tien(don.TienKham) + "\n" +
                    "Tiền thuốc: " + Tien(tienThuoc) + "\n" +
                    "--------------------------------\n" +
                    "TỔNG CỘNG: " + Tien(tongTien) + "\n" +
                    "Trạng thái: Đã thanh toán",
                    "Hoá đơn", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không thanh toán được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnXemHoaDon_Click(object sender, EventArgs e)
        {
            if (maDonDangChon <= 0)
            {
                return;
            }

            DonThuocDTO don = donThuocBus.LayTheoMa(maDonDangChon);
            if (don == null)
            {
                return;
            }

            DataTable ct = donThuocBus.LayChiTiet(maDonDangChon);
            string noiDung = "Đơn #" + don.MaDon + " - " + don.TenBenhNhan + " (" + don.TrangThai + ")\n\n";
            foreach (DataRow r in ct.Rows)
            {
                noiDung += "- " + r["TenThuoc"] + ": " + r["SoLuong"] + " " + r["DonViTinh"] +
                           " x " + Tien(Convert.ToDecimal(r["DonGia"])) + " = " +
                           Tien(Convert.ToDecimal(r["ThanhTien"])) + "\n";
            }
            noiDung += "\nTiền khám: " + Tien(don.TienKham) +
                       "\nTổng cộng: " + Tien(don.TienKham + tienThuocDangChon);

            MessageBox.Show(noiDung, "Chi tiết đơn thuốc", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
