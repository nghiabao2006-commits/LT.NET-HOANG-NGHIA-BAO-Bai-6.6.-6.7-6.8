/* =====================================================================
   LibraryDB - He thong quan ly muon tra sach thu vien
   Bai 6.6 - Lap trinh .NET (WinForms + ADO.NET, kien truc 3-Tier)
   ---------------------------------------------------------------------
   Script nay duoc ung dung tu dong chay khi khoi dong (DAL/DbInitializer.cs)
   neu CSDL chua co bang / chua co du lieu. Van co the mo bang SSMS va
   bam Execute de tao CSDL thu cong.
   Script viet theo huong idempotent: chay lai nhieu lan khong loi,
   khong nhan doi du lieu mau.
   ===================================================================== */

/* ------------------------- 1. Bang DocGia ------------------------- */
IF OBJECT_ID(N'dbo.DocGia', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DocGia
    (
        MaDocGia      INT IDENTITY(1,1) NOT NULL,
        HoTen         NVARCHAR(100)     NOT NULL,
        Email         VARCHAR(100)      NULL,
        NgayHetHanThe DATETIME          NOT NULL,
        CONSTRAINT PK_DocGia PRIMARY KEY (MaDocGia),
        CONSTRAINT UQ_DocGia_Email UNIQUE (Email)
    );
END
GO

/* ------------------------- 2. Bang DauSach ------------------------ */
IF OBJECT_ID(N'dbo.DauSach', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DauSach
    (
        MaSach      INT IDENTITY(1,1) NOT NULL,
        TenSach     NVARCHAR(200)     NOT NULL,
        TheLoai     NVARCHAR(50)      NULL,
        SoLuongCon  INT               NOT NULL,
        CONSTRAINT PK_DauSach PRIMARY KEY (MaSach),
        CONSTRAINT CK_DauSach_SoLuongCon CHECK (SoLuongCon >= 0)
    );
END
GO

/* ----------------------- 3. Bang PhieuMuon ------------------------ */
IF OBJECT_ID(N'dbo.PhieuMuon', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PhieuMuon
    (
        MaPhieu   INT IDENTITY(1,1)  NOT NULL,
        MaDocGia  INT                NOT NULL,
        NgayMuon  DATETIME           NOT NULL CONSTRAINT DF_PhieuMuon_NgayMuon DEFAULT (GETDATE()),
        HanTra    DATETIME           NOT NULL,
        TrangThai NVARCHAR(50)       NOT NULL CONSTRAINT DF_PhieuMuon_TrangThai DEFAULT (N'Đang mượn'),
        CONSTRAINT PK_PhieuMuon PRIMARY KEY (MaPhieu),
        CONSTRAINT FK_PhieuMuon_DocGia FOREIGN KEY (MaDocGia) REFERENCES dbo.DocGia (MaDocGia),
        CONSTRAINT CK_PhieuMuon_TrangThai CHECK (TrangThai IN (N'Đang mượn', N'Đã trả', N'Quá hạn'))
    );
END
GO

/* ---------------------- 4. Bang ChiTietMuon ----------------------- */
IF OBJECT_ID(N'dbo.ChiTietMuon', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ChiTietMuon
    (
        MaPhieu INT NOT NULL,
        MaSach  INT NOT NULL,
        SoLuong INT NOT NULL,
        CONSTRAINT PK_ChiTietMuon PRIMARY KEY (MaPhieu, MaSach),
        CONSTRAINT FK_ChiTietMuon_PhieuMuon FOREIGN KEY (MaPhieu) REFERENCES dbo.PhieuMuon (MaPhieu) ON DELETE CASCADE,
        CONSTRAINT FK_ChiTietMuon_DauSach  FOREIGN KEY (MaSach)  REFERENCES dbo.DauSach (MaSach),
        CONSTRAINT CK_ChiTietMuon_SoLuong CHECK (SoLuong > 0)
    );
END
GO

/* =====================================================================
   DU LIEU MAU
   Ngay thang duoc tinh tuong doi so voi GETDATE() de kich ban demo
   (phieu qua han, the het han) luon dung o moi thoi diem chay.
   ===================================================================== */

IF NOT EXISTS (SELECT 1 FROM dbo.DocGia)
BEGIN
    INSERT INTO dbo.DocGia (HoTen, Email, NgayHetHanThe) VALUES
        (N'Nguyễn Văn An',   'an.nv@sv.edu.vn',    DATEADD(DAY,  365, GETDATE())),
        (N'Trần Thị Bình',   'binh.tt@sv.edu.vn',  DATEADD(DAY,  180, GETDATE())),
        (N'Lê Hoàng Cường',  'cuong.lh@sv.edu.vn', DATEADD(DAY,  -20, GETDATE())),
        (N'Phạm Minh Dung',  'dung.pm@sv.edu.vn',  DATEADD(DAY,   90, GETDATE())),
        (N'Vũ Quốc Huy',     'huy.vq@sv.edu.vn',   DATEADD(DAY,  400, GETDATE())),
        (N'Đỗ Thanh Lan',    'lan.dt@sv.edu.vn',   DATEADD(DAY,   30, GETDATE()));
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.DauSach)
BEGIN
    INSERT INTO dbo.DauSach (TenSach, TheLoai, SoLuongCon) VALUES
        (N'Lập trình C# căn bản',              N'Công nghệ thông tin', 5),
        (N'Cấu trúc dữ liệu và giải thuật',     N'Công nghệ thông tin', 3),
        (N'Mạng máy tính',                      N'Công nghệ thông tin', 4),
        (N'Kinh tế vi mô',                      N'Kinh tế',             6),
        (N'Quản trị học',                       N'Kinh tế',             2),
        (N'Marketing căn bản',                  N'Kinh tế',             5),
        (N'Truyện Kiều',                        N'Văn học',             7),
        (N'Số đỏ',                              N'Văn học',             4),
        (N'Nhà giả kim',                        N'Văn học',             3);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.PhieuMuon)
BEGIN
    INSERT INTO dbo.PhieuMuon (MaDocGia, NgayMuon, HanTra, TrangThai) VALUES
        (1, DATEADD(DAY, -20, GETDATE()), DATEADD(DAY,  -6, GETDATE()), N'Đang mượn'), -- qua han 6 ngay
        (2, DATEADD(DAY, -40, GETDATE()), DATEADD(DAY, -26, GETDATE()), N'Đang mượn'), -- qua han 26 ngay
        (3, DATEADD(DAY, -10, GETDATE()), DATEADD(DAY,   4, GETDATE()), N'Đang mượn'), -- con han
        (4, DATEADD(DAY, -60, GETDATE()), DATEADD(DAY, -46, GETDATE()), N'Đã trả'),
        (1, DATEADD(DAY,  -5, GETDATE()), DATEADD(DAY,   9, GETDATE()), N'Đang mượn'), -- con han
        (5, DATEADD(DAY, -90, GETDATE()), DATEADD(DAY, -76, GETDATE()), N'Đã trả');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.ChiTietMuon)
BEGIN
    INSERT INTO dbo.ChiTietMuon (MaPhieu, MaSach, SoLuong) VALUES
        (1, 1, 1),
        (1, 4, 2),
        (2, 2, 1),
        (3, 7, 1),
        (3, 9, 1),
        (4, 3, 1),
        (5, 6, 2),
        (6, 8, 1),
        (6, 5, 1);
END
GO
