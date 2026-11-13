/* =====================================================================
   BilliardsDB - He thong quan ly ban bida & dich vu giai tri
   Bai 6.8 - Lap trinh .NET (WinForms + ADO.NET, kien truc 3-Tier)
   ---------------------------------------------------------------------
   Script duoc ung dung tu dong chay khi khoi dong (DAL/DbInitializer.cs)
   neu CSDL chua co bang / chua co du lieu mau. Van co the mo bang SSMS
   va bam Execute de tao CSDL thu cong.
   Script viet theo huong idempotent: chay lai nhieu lan khong loi,
   khong nhan doi du lieu mau.
   ===================================================================== */

/* ------------------------- 1. Bang BanBida ------------------------ */
IF OBJECT_ID(N'dbo.BanBida', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BanBida
    (
        MaBan      INT IDENTITY(1,1) NOT NULL,
        TenBan     NVARCHAR(50)      NOT NULL,
        LoaiBan    NVARCHAR(30)      NOT NULL,
        GiaTheoGio DECIMAL(18,2)     NOT NULL,
        TrangThai  NVARCHAR(20)      NOT NULL CONSTRAINT DF_BanBida_TrangThai DEFAULT (N'Trống'),
        CONSTRAINT PK_BanBida PRIMARY KEY (MaBan),
        CONSTRAINT CK_BanBida_LoaiBan CHECK (LoaiBan IN (N'Pool (Lỗ)', N'Carom (Phăng)', N'Snooker')),
        CONSTRAINT CK_BanBida_TrangThai CHECK (TrangThai IN (N'Trống', N'Đang chơi')),
        CONSTRAINT CK_BanBida_GiaTheoGio CHECK (GiaTheoGio >= 0)
    );
END
GO

/* ---------------------- 2. Bang MenuDichVu ------------------------ */
/* TruKho = 1: dich vu tru kho khi thanh toan (nuoc uong, mi xao...)
   TruKho = 0: dich vu khong tru kho (thue gay, khan cho thue...)        */
IF OBJECT_ID(N'dbo.MenuDichVu', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MenuDichVu
    (
        MaDV    INT IDENTITY(1,1) NOT NULL,
        TenDV   NVARCHAR(100)     NOT NULL,
        DonGia  DECIMAL(18,2)     NOT NULL,
        TonKho  INT               NOT NULL CONSTRAINT DF_MenuDichVu_TonKho DEFAULT (0),
        TruKho  BIT               NOT NULL CONSTRAINT DF_MenuDichVu_TruKho DEFAULT (1),
        CONSTRAINT PK_MenuDichVu PRIMARY KEY (MaDV),
        CONSTRAINT CK_MenuDichVu_TonKho CHECK (TonKho >= 0),
        CONSTRAINT CK_MenuDichVu_DonGia CHECK (DonGia >= 0)
    );
END
GO

/* ------------------------ 3. Bang HoaDon -------------------------- */
IF OBJECT_ID(N'dbo.HoaDon', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HoaDon
    (
        MaHD        INT IDENTITY(1,1) NOT NULL,
        MaBan       INT               NOT NULL,
        GioBatDau   DATETIME          NOT NULL,
        GioKetThuc  DATETIME          NULL,
        TienGioChoi DECIMAL(18,2)     NOT NULL CONSTRAINT DF_HoaDon_TienGioChoi DEFAULT (0),
        TienDichVu  DECIMAL(18,2)     NOT NULL CONSTRAINT DF_HoaDon_TienDichVu DEFAULT (0),
        TrangThai   BIT               NOT NULL CONSTRAINT DF_HoaDon_TrangThai DEFAULT (0),
        CONSTRAINT PK_HoaDon PRIMARY KEY (MaHD),
        CONSTRAINT FK_HoaDon_BanBida FOREIGN KEY (MaBan) REFERENCES dbo.BanBida (MaBan)
    );
END
GO

/* --------------------- 4. Bang ChiTietHoaDon ---------------------- */
IF OBJECT_ID(N'dbo.ChiTietHoaDon', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ChiTietHoaDon
    (
        MaHD    INT           NOT NULL,
        MaDV    INT           NOT NULL,
        SoLuong INT           NOT NULL,
        DonGia  DECIMAL(18,2) NOT NULL,
        CONSTRAINT PK_ChiTietHoaDon PRIMARY KEY (MaHD, MaDV),
        CONSTRAINT FK_ChiTietHoaDon_HoaDon FOREIGN KEY (MaHD) REFERENCES dbo.HoaDon (MaHD) ON DELETE CASCADE,
        CONSTRAINT FK_ChiTietHoaDon_MenuDichVu FOREIGN KEY (MaDV) REFERENCES dbo.MenuDichVu (MaDV),
        CONSTRAINT CK_ChiTietHoaDon_SoLuong CHECK (SoLuong > 0),
        CONSTRAINT CK_ChiTietHoaDon_DonGia CHECK (DonGia >= 0)
    );
END
GO

/* =====================================================================
   DU LIEU MAU
   Ngay thang tinh tuong doi so voi GETDATE() de kich ban demo
   (ban dang choi, hoa don da thanh toan) luon dung o moi thoi diem.
   ===================================================================== */

IF NOT EXISTS (SELECT 1 FROM dbo.BanBida)
BEGIN
    INSERT INTO dbo.BanBida (TenBan, LoaiBan, GiaTheoGio, TrangThai) VALUES
        (N'Bàn 1',  N'Pool (Lỗ)',       60000,  N'Trống'),
        (N'Bàn 2',  N'Pool (Lỗ)',       60000,  N'Trống'),
        (N'Bàn 3',  N'Pool (Lỗ)',       70000,  N'Trống'),
        (N'Bàn 4',  N'Pool (Lỗ)',       70000,  N'Trống'),
        (N'Bàn 5',  N'Carom (Phăng)',   80000,  N'Trống'),
        (N'Bàn 6',  N'Carom (Phăng)',   80000,  N'Trống'),
        (N'Bàn 7',  N'Snooker',        120000,  N'Trống'),
        (N'Bàn 8',  N'Snooker',        120000,  N'Trống'),
        (N'Bàn 9',  N'Pool (Lỗ)',       60000,  N'Trống'),
        (N'Bàn 10', N'Carom (Phăng)',   90000,  N'Trống');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MenuDichVu)
BEGIN
    INSERT INTO dbo.MenuDichVu (TenDV, DonGia, TonKho, TruKho) VALUES
        (N'Sting',            15000,  48, 1),
        (N'Coca Cola',        15000,  36, 1),
        (N'Nước suối',        10000,  60, 1),
        (N'Mì xào bò',        35000,  20, 1),
        (N'Khăn lạnh',         5000,  50, 1),
        (N'Trà đá',            5000, 100, 1),
        (N'Thuê gậy Carbon',  30000,   0, 0),
        (N'Thuê gậy gỗ',      15000,   0, 0);
END
GO

/* Hoa don mau:
   - 1 hoa don dang choi (GioKetThuc NULL, TrangThai 0) o ban 3 kem chi tiet
   - 1 hoa don da thanh toan trong qua khu o ban 2                           */
IF NOT EXISTS (SELECT 1 FROM dbo.HoaDon)
BEGIN
    DECLARE @hdDangChoi INT, @hdDaThanhToan INT;

    INSERT INTO dbo.HoaDon (MaBan, GioBatDau, GioKetThuc, TienGioChoi, TienDichVu, TrangThai)
    VALUES (3, DATEADD(MINUTE, -75, GETDATE()), NULL, 0, 95000, 0);
    SET @hdDangChoi = CAST(SCOPE_IDENTITY() AS INT);

    /* 160 phut = 11 khoi 15 phut = 2.75 gio x 60.000 = 165.000 */
    INSERT INTO dbo.HoaDon (MaBan, GioBatDau, GioKetThuc, TienGioChoi, TienDichVu, TrangThai)
    VALUES (2, DATEADD(MINUTE, -1560, GETDATE()), DATEADD(MINUTE, -1400, GETDATE()), 165000, 65000, 1);
    SET @hdDaThanhToan = CAST(SCOPE_IDENTITY() AS INT);

    INSERT INTO dbo.ChiTietHoaDon (MaHD, MaDV, SoLuong, DonGia) VALUES
        (@hdDangChoi,   1, 2, 15000),   -- 2 Sting
        (@hdDangChoi,   4, 1, 35000),   -- 1 Mi xao bo
        (@hdDangChoi,   7, 1, 30000),   -- 1 thue gay Carbon (khong tru kho)
        (@hdDaThanhToan, 2, 3, 15000),  -- 3 Coca
        (@hdDaThanhToan, 3, 2, 10000);  -- 2 Nuoc suoi

    UPDATE dbo.BanBida SET TrangThai = N'Đang chơi' WHERE MaBan = 3;
END
GO
