using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace QuanLiQuanCoffe.DataAccess
{
    /// <summary>
    /// Lớp trung tâm làm việc với SQLite:
    /// tạo file .db lần đầu, bật khóa ngoại, chạy câu lệnh có tham số.
    /// </summary>
    public static class DatabaseHelper
    {
        private const string DatabaseFileName = "CafeDB.db";

        /// <summary>
        /// Đường dẫn file CSDL nằm cạnh file .exe (thường là bin\Debug).
        /// </summary>
        public static string DatabasePath
        {
            get
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DatabaseFileName);
            }
        }

        /// <summary>
        /// Chuỗi kết nối. Dạng chuẩn: Data Source=CafeDB.db;Version=3;
        /// Ở đây ghép đường dẫn đầy đủ để không phụ thuộc thư mục đang đứng.
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                return "Data Source=" + DatabasePath + ";Version=3;";
            }
        }

        /// <summary>
        /// Mở kết nối và BẬT khóa ngoại. Người gọi phải Dispose (dùng using).
        /// </summary>
        public static SQLiteConnection GetConnection()
        {
            SQLiteConnection connection = new SQLiteConnection(ConnectionString);
            connection.Open();

            // SQLite mặc định tắt foreign key, phải bật trên MỖI kết nối
            using (SQLiteCommand pragmaCommand = new SQLiteCommand("PRAGMA foreign_keys = ON;", connection))
            {
                pragmaCommand.ExecuteNonQuery();
            }

            return connection;
        }

        /// <summary>
        /// Gọi khi ứng dụng khởi động: tạo file + bảng + dữ liệu mẫu nếu cần.
        /// </summary>
        public static void InitializeDatabase()
        {
            using (SQLiteConnection connection = GetConnection())
            {
                CreateTables(connection);
                InsertSampleDataIfEmpty(connection);
            }

            AccountDAO.HashPlaintextPasswordsIfNeeded();
        }

        /// <summary>
        /// Chạy lệnh INSERT/UPDATE/DELETE. Luôn truyền Parameters, không cộng chuỗi SQL.
        /// </summary>
        public static int ExecuteNonQuery(string sql, params SQLiteParameter[] parameters)
        {
            using (SQLiteConnection connection = GetConnection())
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    return command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// INSERT rồi lấy Id vừa tạo (last_insert_rowid) trên cùng một kết nối.
        /// </summary>
        public static int ExecuteInsert(string sql, params SQLiteParameter[] parameters)
        {
            using (SQLiteConnection connection = GetConnection())
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    command.ExecuteNonQuery();
                }

                using (SQLiteCommand idCommand = new SQLiteCommand("SELECT last_insert_rowid();", connection))
                {
                    return Convert.ToInt32(idCommand.ExecuteScalar());
                }
            }
        }

        /// <summary>
        /// Chạy lệnh trả về một giá trị (COUNT, MAX, last_insert_rowid...).
        /// </summary>
        public static object ExecuteScalar(string sql, params SQLiteParameter[] parameters)
        {
            using (SQLiteConnection connection = GetConnection())
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    return command.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// Chạy SELECT, đổ vào DataTable (dùng SQLiteDataAdapter).
        /// </summary>
        public static DataTable ExecuteQuery(string sql, params SQLiteParameter[] parameters)
        {
            DataTable table = new DataTable();

            using (SQLiteConnection connection = GetConnection())
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(command))
                    {
                        adapter.Fill(table);
                    }
                }
            }

            return table;
        }

        private static void CreateTables(SQLiteConnection connection)
        {
            string[] createStatements = new string[]
            {
                @"CREATE TABLE IF NOT EXISTS Account (
                    UserName    TEXT    NOT NULL PRIMARY KEY,
                    DisplayName TEXT    NOT NULL,
                    PassWord    TEXT    NOT NULL,
                    Type        INTEGER NOT NULL DEFAULT 0
                );",
                @"CREATE TABLE IF NOT EXISTS Category (
                    Id   INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT    NOT NULL UNIQUE
                );",
                @"CREATE TABLE IF NOT EXISTS Food (
                    Id         INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name       TEXT    NOT NULL,
                    IdCategory INTEGER NOT NULL,
                    Price      REAL    NOT NULL CHECK (Price >= 0),
                    FOREIGN KEY (IdCategory) REFERENCES Category(Id)
                );",
                @"CREATE TABLE IF NOT EXISTS TableFood (
                    Id     INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name   TEXT    NOT NULL UNIQUE,
                    Status TEXT    NOT NULL DEFAULT 'Trống'
                );",
                @"CREATE TABLE IF NOT EXISTS Bill (
                    Id           INTEGER PRIMARY KEY AUTOINCREMENT,
                    DateCheckIn  TEXT    NOT NULL,
                    DateCheckOut TEXT,
                    IdTable      INTEGER NOT NULL,
                    Status       INTEGER NOT NULL DEFAULT 0,
                    Discount     INTEGER NOT NULL DEFAULT 0,
                    FOREIGN KEY (IdTable) REFERENCES TableFood(Id)
                );",
                @"CREATE TABLE IF NOT EXISTS BillDetail (
                    Id     INTEGER PRIMARY KEY AUTOINCREMENT,
                    IdBill INTEGER NOT NULL,
                    IdFood INTEGER NOT NULL,
                    Count  INTEGER NOT NULL DEFAULT 1 CHECK (Count > 0),
                    FOREIGN KEY (IdBill) REFERENCES Bill(Id) ON DELETE CASCADE,
                    FOREIGN KEY (IdFood) REFERENCES Food(Id)
                );"
            };

            int i;
            for (i = 0; i < createStatements.Length; i++)
            {
                using (SQLiteCommand command = new SQLiteCommand(createStatements[i], connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void InsertSampleDataIfEmpty(SQLiteConnection connection)
        {
            int accountCount;
            using (SQLiteCommand countCommand = new SQLiteCommand("SELECT COUNT(*) FROM Account;", connection))
            {
                accountCount = Convert.ToInt32(countCommand.ExecuteScalar());
            }

            if (accountCount > 0)
            {
                return;
            }

            string matKhauBam = PasswordHelper.Hash("123456");
            using (SQLiteCommand adminCommand = new SQLiteCommand(
                "INSERT INTO Account (UserName, DisplayName, PassWord, Type) VALUES (@user, @display, @pass, @type);",
                connection))
            {
                adminCommand.Parameters.AddWithValue("@user", "admin");
                adminCommand.Parameters.AddWithValue("@display", "Quản trị viên");
                adminCommand.Parameters.AddWithValue("@pass", matKhauBam);
                adminCommand.Parameters.AddWithValue("@type", 1);
                adminCommand.ExecuteNonQuery();
            }

            using (SQLiteCommand staffCommand = new SQLiteCommand(
                "INSERT INTO Account (UserName, DisplayName, PassWord, Type) VALUES (@user, @display, @pass, @type);",
                connection))
            {
                staffCommand.Parameters.AddWithValue("@user", "nhanvien");
                staffCommand.Parameters.AddWithValue("@display", "Nhân viên A");
                staffCommand.Parameters.AddWithValue("@pass", matKhauBam);
                staffCommand.Parameters.AddWithValue("@type", 0);
                staffCommand.ExecuteNonQuery();
            }

            string[] seedStatements = new string[]
            {
                @"INSERT INTO Category (Name) VALUES
                    ('Cà phê'),
                    ('Trà'),
                    ('Nước ép'),
                    ('Bánh');",
                @"INSERT INTO Food (Name, IdCategory, Price) VALUES
                    ('Cà phê đen', 1, 20000),
                    ('Cà phê sữa', 1, 25000),
                    ('Bạc xỉu', 1, 30000),
                    ('Trà đào', 2, 35000),
                    ('Trà sữa', 2, 30000),
                    ('Nước cam', 3, 28000),
                    ('Sinh tố bơ', 3, 40000),
                    ('Bánh mì bơ', 4, 18000);",
                @"INSERT INTO TableFood (Name, Status) VALUES
                    ('Bàn 1', 'Trống'),
                    ('Bàn 2', 'Trống'),
                    ('Bàn 3', 'Trống'),
                    ('Bàn 4', 'Trống'),
                    ('Bàn 5', 'Trống'),
                    ('Bàn 6', 'Trống'),
                    ('Bàn 7', 'Trống'),
                    ('Bàn 8', 'Trống');",
                @"INSERT INTO Bill (DateCheckIn, DateCheckOut, IdTable, Status, Discount) VALUES
                    ('2026-10-10 08:15:00', '2026-10-10 09:00:00', 1, 1, 0),
                    ('2026-10-12 14:20:00', '2026-10-12 15:10:00', 2, 1, 10);",
                @"INSERT INTO BillDetail (IdBill, IdFood, Count) VALUES
                    (1, 1, 2),
                    (1, 2, 1),
                    (2, 4, 1),
                    (2, 8, 2);"
            };

            int i;
            for (i = 0; i < seedStatements.Length; i++)
            {
                using (SQLiteCommand command = new SQLiteCommand(seedStatements[i], connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
