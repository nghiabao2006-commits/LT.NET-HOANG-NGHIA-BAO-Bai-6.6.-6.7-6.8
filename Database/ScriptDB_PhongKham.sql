/* =====================================================================
   ClinicDB - He thong quan ly phong kham va ke don thuoc
   Bai 6.7 - Lap trinh .NET (WinForms + ADO.NET, kien truc 3-Tier)
   ---------------------------------------------------------------------
   Script idempotent: ung dung tu chay luc khoi dong (DAL/DbInitializer.cs)
   neu CSDL chua co bang / chua co du lieu mau.
   ===================================================================== */

/* ------------------------ 1. Bang BenhNhan ------------------------ */
IF OBJECT_ID(N'dbo.BenhNhan', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BenhNhan
    (
        MaBN        INT IDENTITY(1,1) NOT NULL,
        HoTen       NVARCHAR(100)     NOT NULL,
        SoDienThoai VARCHAR(15)       NULL,
        NamSinh     INT               NULL,
        CONSTRAINT PK_BenhNhan PRIMARY KEY (MaBN),
        CONSTRAINT UQ_BenhNhan_SoDienThoai UNIQUE (SoDienThoai)
    );
END
GO

/* ------------------------ 2. Bang HoSoKham ----------------------- */
IF OBJECT_ID(N'dbo.HoSoKham', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HoSoKham
    (
        MaHoSo    INT IDENTITY(1,1) NOT NULL,
        MaBN      INT               NOT NULL,
        NgayKham  DATETIME          NOT NULL CONSTRAINT DF_HoSoKham_NgayKham DEFAULT (GETDATE()),
        TrieuChung NVARCHAR(MAX)    NULL,
        ChanDoan  NVARCHAR(250)     NULL,
        CONSTRAINT PK_HoSoKham PRIMARY KEY (MaHoSo),
        CONSTRAINT FK_HoSoKham_BenhNhan FOREIGN KEY (MaBN) REFERENCES dbo.BenhNhan (MaBN)
    );
END
GO

/* --------------------------- 3. Bang Thuoc ----------------------- */
IF OBJECT_ID(N'dbo.Thuoc', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Thuoc
    (
        MaThuoc    INT IDENTITY(1,1) NOT NULL,
        TenThuoc   NVARCHAR(150)     NOT NULL,
        DonViTinh  NVARCHAR(20)      NULL,
        DonGia     DECIMAL(18,2)     NOT NULL CONSTRAINT DF_Thuoc_DonGia DEFAULT (0),
        SoLuongTon INT               NOT NULL CONSTRAINT DF_Thuoc_SoLuongTon DEFAULT (0),
        CONSTRAINT PK_Thuoc PRIMARY KEY (MaThuoc),
        CONSTRAINT CK_Thuoc_DonGia CHECK (DonGia >= 0),
        CONSTRAINT CK_Thuoc_SoLuongTon CHECK (SoLuongTon >= 0)
    );
END
GO

/* ------------------------- 4. Bang DonThuoc ---------------------- */
IF OBJECT_ID(N'dbo.DonThuoc', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DonThuoc
    (
        MaDon     INT IDENTITY(1,1) NOT NULL,
        MaHoSo    INT               NOT NULL,
        TienKham  DECIMAL(18,2)     NOT NULL CONSTRAINT DF_DonThuoc_TienKham DEFAULT (0),
        TongTien  DECIMAL(18,2)     NOT NULL CONSTRAINT DF_DonThuoc_TongTien DEFAULT (0),
        TrangThai NVARCHAR(30)      NOT NULL CONSTRAINT DF_DonThuoc_TrangThai DEFAULT (N'Chờ lấy thuốc'),
        CONSTRAINT PK_DonThuoc PRIMARY KEY (MaDon),
        CONSTRAINT FK_DonThuoc_HoSoKham FOREIGN KEY (MaHoSo) REFERENCES dbo.HoSoKham (MaHoSo),
        CONSTRAINT CK_DonThuoc_TrangThai CHECK (TrangThai IN (N'Chờ lấy thuốc', N'Đã thanh toán')),
        CONSTRAINT CK_DonThuoc_Tien CHECK (TienKham >= 0 AND TongTien >= 0)
    );
END
GO

/* --------------------- 5. Bang ChiTietDonThuoc ------------------- */
IF OBJECT_ID(N'dbo.ChiTietDonThuoc', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ChiTietDonThuoc
    (
        MaDon    INT          NOT NULL,
        MaThuoc  INT          NOT NULL,
        SoLuong  INT          NOT NULL,
        LieuDung NVARCHAR(100) NULL,
        CONSTRAINT PK_ChiTietDonThuoc PRIMARY KEY (MaDon, MaThuoc),
        CONSTRAINT FK_ChiTietDonThuoc_DonThuoc FOREIGN KEY (MaDon) REFERENCES dbo.DonThuoc (MaDon) ON DELETE CASCADE,
        CONSTRAINT FK_ChiTietDonThuoc_Thuoc    FOREIGN KEY (MaThuoc) REFERENCES dbo.Thuoc (MaThuoc),
        CONSTRAINT CK_ChiTietDonThuoc_SoLuong CHECK (SoLuong > 0)
    );
END
GO

/* =====================================================================
   DU LIEU MAU
   Ngay kham tinh tuong doi so voi GETDATE() de luon co du lieu gan day.
   ===================================================================== */

IF NOT EXISTS (SELECT 1 FROM dbo.BenhNhan)
BEGIN
    INSERT INTO dbo.BenhNhan (HoTen, SoDienThoai, NamSinh) VALUES
        (N'Nguyễn Thị Hoa',   '0901234567', 1990),
        (N'Trần Văn Minh',    '0912345678', 1985),
        (N'Lê Thị Lan',       '0987654321', 2001),
        (N'Phạm Quốc Bảo',    '0905556667', 1978),
        (N'Hoàng Thu Hà',     '0938111222', 1995);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Thuoc)
BEGIN
    INSERT INTO dbo.Thuoc (TenThuoc, DonViTinh, DonGia, SoLuongTon) VALUES
        (N'Paracetamol 500mg',    N'Viên', 1500,  500),
        (N'Amoxicillin 500mg',    N'Viên', 3000,  300),
        (N'Vitamin C 500mg',      N'Viên',  800, 1000),
        (N'Omeprazol 20mg',       N'Viên', 2500,  200),
        (N'Siro ho trẻ em',       N'Chai', 45000,  60),
        (N'Nước muối sinh lý',    N'Chai', 12000, 120),
        (N'Berberin 100mg',       N'Viên', 1000,  400),
        (N'Ibuprofen 400mg',      N'Viên', 2000,  250);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.HoSoKham)
BEGIN
    INSERT INTO dbo.HoSoKham (MaBN, NgayKham, TrieuChung, ChanDoan) VALUES
        (1, DATEADD(DAY, -2, GETDATE()), N'Sốt cao, đau họng, ho khan',          N'Viêm họng cấp'),
        (2, DATEADD(DAY, -1, GETDATE()), N'Đau dạ dày sau khi ăn, ợ chua',       N'Viêm dạ dày'),
        (3, DATEADD(DAY, -1, GETDATE()), N'Đau đầu, chóng mặt, mất ngủ',        N'Thiếu máu nhẹ'),
        (4, GETDATE(),                    N'Đau lưng, mỏi cơ sau khi lao động',  N'Căng cơ cấp');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.DonThuoc)
BEGIN
    -- Don 1: dang cho lay thuoc (TongTien = 50000 + 10x1500 + 14x3000 + 20x800)
    INSERT INTO dbo.DonThuoc (MaHoSo, TienKham, TongTien, TrangThai)
        VALUES (1, 50000, 123000, N'Chờ lấy thuốc');

    INSERT INTO dbo.ChiTietDonThuoc (MaDon, MaThuoc, SoLuong, LieuDung) VALUES
        (1, 1, 10, N'Sáng 1 viên, chiều 1 viên sau ăn'),
        (1, 2, 14, N'Sáng 1 viên, tối 1 viên'),
        (1, 3, 20, N'Ngày 2 viên');

    -- Don 2: da thanh toan (TongTien = 50000 + 20x2500 + 20x1000 = 120000)
    INSERT INTO dbo.DonThuoc (MaHoSo, TienKham, TongTien, TrangThai)
        VALUES (2, 50000, 120000, N'Đã thanh toán');

    INSERT INTO dbo.ChiTietDonThuoc (MaDon, MaThuoc, SoLuong, LieuDung) VALUES
        (2, 4, 20, N'Sáng 1 viên trước ăn 30 phút'),
        (2, 7, 20, N'Ngày 3 lần, mỗi lần 1 viên');
END
GO
