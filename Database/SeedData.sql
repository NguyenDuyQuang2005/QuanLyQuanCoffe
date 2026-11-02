-- ============================================================
-- Dữ liệu mẫu - dùng để kiểm tra các chức năng
-- Chỉ chèn khi bảng Account còn trống
-- ============================================================

PRAGMA foreign_keys = ON;

-- Tài khoản: người dùng vẫn gõ 123456; CSDL lưu SHA-256 của "123456"
-- 8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92
INSERT INTO Account (UserName, DisplayName, PassWord, Type) VALUES
    ('admin', 'Quản trị viên', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', 1),
    ('nhanvien', 'Nhân viên A', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', 0);

INSERT INTO Category (Name) VALUES
    ('Cà phê'),
    ('Trà'),
    ('Nước ép'),
    ('Bánh');

INSERT INTO Food (Name, IdCategory, Price) VALUES
    ('Cà phê đen', 1, 20000),
    ('Cà phê sữa', 1, 25000),
    ('Bạc xỉu', 1, 30000),
    ('Trà đào', 2, 35000),
    ('Trà sữa', 2, 30000),
    ('Nước cam', 3, 28000),
    ('Sinh tố bơ', 3, 40000),
    ('Bánh mì bơ', 4, 18000);

INSERT INTO TableFood (Name, Status) VALUES
    ('Bàn 1', 'Trống'),
    ('Bàn 2', 'Trống'),
    ('Bàn 3', 'Trống'),
    ('Bàn 4', 'Trống'),
    ('Bàn 5', 'Trống'),
    ('Bàn 6', 'Trống'),
    ('Bàn 7', 'Trống'),
    ('Bàn 8', 'Trống');

-- Hai hóa đơn đã thanh toán (phục vụ bước thống kê doanh thu)
INSERT INTO Bill (DateCheckIn, DateCheckOut, IdTable, Status, Discount) VALUES
    ('2026-10-10 08:15:00', '2026-10-10 09:00:00', 1, 1, 0),
    ('2026-10-12 14:20:00', '2026-10-12 15:10:00', 2, 1, 10);

INSERT INTO BillDetail (IdBill, IdFood, Count) VALUES
    (1, 1, 2),
    (1, 2, 1),
    (2, 4, 1),
    (2, 8, 2);
