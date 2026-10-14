-- ============================================================
-- Script tạo bảng - Ứng dụng Quản lý quán cà phê (SQLite)
-- Chạy tự động khi ứng dụng khởi động lần đầu (DatabaseHelper)
-- ============================================================

-- Bật kiểm tra khóa ngoại (SQLite mặc định TẮT, bắt buộc bật)
PRAGMA foreign_keys = ON;

-- ------------------------------------------------------------
-- 1. Account: tài khoản đăng nhập
--    Type = 1: Admin | Type = 0: Nhân viên
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Account (
    UserName    TEXT    NOT NULL PRIMARY KEY,
    DisplayName TEXT    NOT NULL,
    PassWord    TEXT    NOT NULL,
    Type        INTEGER NOT NULL DEFAULT 0
);

-- ------------------------------------------------------------
-- 2. Category: danh mục món (Cà phê, Trà, Nước ép, Bánh...)
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Category (
    Id   INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT    NOT NULL UNIQUE
);

-- ------------------------------------------------------------
-- 3. Food: món ăn/uống
--    IdCategory -> Category.Id
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Food (
    Id         INTEGER PRIMARY KEY AUTOINCREMENT,
    Name       TEXT    NOT NULL,
    IdCategory INTEGER NOT NULL,
    Price      REAL    NOT NULL CHECK (Price >= 0),
    FOREIGN KEY (IdCategory) REFERENCES Category(Id)
);

-- ------------------------------------------------------------
-- 4. TableFood: bàn trong quán
--    Status: 'Trống' hoặc 'Có khách'
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS TableFood (
    Id     INTEGER PRIMARY KEY AUTOINCREMENT,
    Name   TEXT    NOT NULL UNIQUE,
    Status TEXT    NOT NULL DEFAULT 'Trống'
);

-- ------------------------------------------------------------
-- 5. Bill: hóa đơn theo bàn
--    Status = 0: chưa thanh toán | Status = 1: đã thanh toán
--    DateCheckOut = NULL khi chưa thanh toán
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Bill (
    Id           INTEGER PRIMARY KEY AUTOINCREMENT,
    DateCheckIn  TEXT    NOT NULL,
    DateCheckOut TEXT,
    IdTable      INTEGER NOT NULL,
    Status       INTEGER NOT NULL DEFAULT 0,
    Discount     INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY (IdTable) REFERENCES TableFood(Id)
);

-- ------------------------------------------------------------
-- 6. BillDetail: chi tiết món trong hóa đơn
--    Xóa hóa đơn thì xóa luôn chi tiết (ON DELETE CASCADE)
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS BillDetail (
    Id     INTEGER PRIMARY KEY AUTOINCREMENT,
    IdBill INTEGER NOT NULL,
    IdFood INTEGER NOT NULL,
    Count  INTEGER NOT NULL DEFAULT 1 CHECK (Count > 0),
    FOREIGN KEY (IdBill) REFERENCES Bill(Id) ON DELETE CASCADE,
    FOREIGN KEY (IdFood) REFERENCES Food(Id)
);
