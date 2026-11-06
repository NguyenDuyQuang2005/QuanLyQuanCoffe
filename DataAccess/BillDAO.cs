using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using QuanLiQuanCoffe.Models;

namespace QuanLiQuanCoffe.DataAccess
{
    public static class BillDAO
    {
        public const int StatusChuaThanhToan = 0;
        public const int StatusDaThanhToan = 1;

        public static Bill GetUnpaidByTable(int idTable)
        {
            using (SQLiteConnection connection = DatabaseHelper.GetConnection())
            {
                return GetUnpaidByTable(connection, idTable);
            }
        }

        public static Bill GetUnpaidByTable(SQLiteConnection connection, int idTable)
        {
            DataTable table = DatabaseHelper.ExecuteQuery(
                connection,
                "SELECT Id, DateCheckIn, DateCheckOut, IdTable, Status, Discount " +
                "FROM Bill WHERE IdTable = @table AND Status = @status ORDER BY Id DESC LIMIT 1;",
                DatabaseHelper.Param("@table", idTable),
                DatabaseHelper.Param("@status", StatusChuaThanhToan));

            if (table.Rows.Count == 0)
            {
                return null;
            }

            return MapBill(table.Rows[0]);
        }

        public static int Insert(int idTable)
        {
            using (SQLiteConnection connection = DatabaseHelper.GetConnection())
            {
                return Insert(connection, idTable);
            }
        }

        public static int Insert(SQLiteConnection connection, int idTable)
        {
            int giamGiaMacDinh = 0;
            return DatabaseHelper.ExecuteInsert(
                connection,
                "INSERT INTO Bill (DateCheckIn, IdTable, Status, Discount) " +
                "VALUES (@checkIn, @table, @status, @discount);",
                DatabaseHelper.Param("@checkIn", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                DatabaseHelper.Param("@table", idTable),
                DatabaseHelper.Param("@status", StatusChuaThanhToan),
                DatabaseHelper.Param("@discount", giamGiaMacDinh));
        }

        /// <summary>
        /// Thêm món vào hóa đơn chưa thanh toán của bàn. Tạo hóa đơn mới nếu chưa có.
        /// Chạy trong một transaction để không để hóa đơn rỗng khi thêm món thất bại.
        /// </summary>
        public static int AddFoodToTable(int idTable, int idFood, int count)
        {
            if (idTable <= 0 || idFood <= 0 || count <= 0)
                throw new ArgumentException("Bàn, món và số lượng phải hợp lệ.");
            return DatabaseHelper.RunInTransaction(delegate(SQLiteConnection connection)
            {
                Bill unpaid = GetUnpaidByTable(connection, idTable);
                int billId;
                if (unpaid == null)
                {
                    billId = Insert(connection, idTable);
                    TableFoodDAO.UpdateStatus(connection, idTable, TableFood.StatusCoKhach);
                }
                else
                {
                    billId = unpaid.Id;
                }

                BillDetailDAO.InsertOrIncrease(connection, billId, idFood, count);
                return billId;
            });
        }

        public static bool CheckOut(int billId, int discount)
        {
            using (SQLiteConnection connection = DatabaseHelper.GetConnection())
            {
                return CheckOut(connection, billId, discount);
            }
        }

        public static bool CheckOut(SQLiteConnection connection, int billId, int discount)
        {
            int rows = DatabaseHelper.ExecuteNonQuery(
                connection,
                "UPDATE Bill SET Status = @paid, DateCheckOut = @checkOut, Discount = @discount " +
                "WHERE Id = @id AND Status = @unpaid;",
                DatabaseHelper.Param("@paid", StatusDaThanhToan),
                DatabaseHelper.Param("@checkOut", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                DatabaseHelper.Param("@discount", discount),
                DatabaseHelper.Param("@id", billId),
                DatabaseHelper.Param("@unpaid", StatusChuaThanhToan));
            return rows > 0;
        }

        /// <summary>
        /// Nếu hóa đơn chưa thanh toán không còn món thì xóa và trả bàn về Trống.
        /// </summary>
        public static void CancelIfEmpty(int billId, int idTable)
        {
            DatabaseHelper.RunInTransaction(delegate(SQLiteConnection connection)
            {
                object validBill = DatabaseHelper.ExecuteScalar(connection,
                    "SELECT COUNT(*) FROM Bill WHERE Id=@bill AND IdTable=@table AND Status=0;",
                    DatabaseHelper.Param("@bill", billId),
                    DatabaseHelper.Param("@table", idTable));
                if (Convert.ToInt32(validBill) != 1) return false;

                object detailCount = DatabaseHelper.ExecuteScalar(connection,
                    "SELECT COUNT(*) FROM BillDetail WHERE IdBill=@bill;",
                    DatabaseHelper.Param("@bill", billId));
                if (Convert.ToInt32(detailCount) != 0) return false;

                int deleted = DatabaseHelper.ExecuteNonQuery(connection,
                    "DELETE FROM Bill WHERE Id=@bill AND IdTable=@table AND Status=0;",
                    DatabaseHelper.Param("@bill", billId),
                    DatabaseHelper.Param("@table", idTable));
                if (deleted != 1) return false;

                TableFoodDAO.UpdateStatus(connection, idTable, TableFood.StatusTrong);
                return true;
            });
        }

        public static bool CheckOutAndFreeTable(int billId, int idTable, int discount)
        {
            if (discount < 0 || discount > 100)
                throw new ArgumentOutOfRangeException("discount", "Giảm giá phải trong khoảng 0-100%.");

            return DatabaseHelper.RunInTransaction(delegate(SQLiteConnection connection)
            {
                // Cần đúng hóa đơn mở của đúng bàn, không thể thanh toán hộ bàn khác.
                int valid = Convert.ToInt32(DatabaseHelper.ExecuteScalar(connection,
                    "SELECT COUNT(*) FROM Bill WHERE Id=@bill AND IdTable=@table AND Status=@unpaid;",
                    DatabaseHelper.Param("@bill", billId),
                    DatabaseHelper.Param("@table", idTable),
                    DatabaseHelper.Param("@unpaid", StatusChuaThanhToan)));
                if (valid != 1) return false;

                int detailCount = Convert.ToInt32(DatabaseHelper.ExecuteScalar(connection,
                    "SELECT COUNT(*) FROM BillDetail WHERE IdBill=@bill;",
                    DatabaseHelper.Param("@bill", billId)));
                if (detailCount <= 0)
                    throw new InvalidOperationException("Không thể thanh toán hóa đơn chưa có món.");

                int missing = Convert.ToInt32(DatabaseHelper.ExecuteScalar(connection,
                    "SELECT COUNT(*) FROM BillDetail WHERE IdBill=@bill " +
                    "AND UnitPriceAtSale IS NULL;",
                    DatabaseHelper.Param("@bill", billId)));
                if (missing != 0)
                    throw new InvalidOperationException(
                        "Hóa đơn còn món thiếu đơn giá lịch sử. Không thể thanh toán chính xác.");

                if (!CheckOut(connection, billId, discount)) return false;
                if (!TableFoodDAO.UpdateStatus(connection, idTable, TableFood.StatusTrong))
                    throw new InvalidOperationException("Không cập nhật được trạng thái bàn.");
                return true;
            });
        }

        /// <summary>
        /// Doanh thu hóa đơn đã thanh toán. theoThang = false: đúng ngày; true: cả tháng của ngày đó.
        /// </summary>
        public static List<RevenueItem> GetRevenue(DateTime ngay, bool theoThang)
        {
            string dieuKienNgay;
            if (theoThang)
            {
                dieuKienNgay = "strftime('%Y-%m', b.DateCheckOut) = strftime('%Y-%m', @ngay)";
            }
            else
            {
                dieuKienNgay = "date(b.DateCheckOut) = date(@ngay)";
            }

            string sql =
                "SELECT b.Id, b.DateCheckOut, t.Name AS TableName, b.Discount, " +
                "SUM(d.UnitPriceAtSale * d.Count) AS SubTotal, " +
                "SUM(CASE WHEN d.UnitPriceAtSale IS NULL THEN 1 ELSE 0 END) AS MissingPriceCount " +
                "FROM Bill b " +
                "INNER JOIN TableFood t ON b.IdTable = t.Id " +
                "INNER JOIN BillDetail d ON d.IdBill = b.Id " +
                "WHERE b.Status = @status AND " + dieuKienNgay + " " +
                "GROUP BY b.Id, b.DateCheckOut, t.Name, b.Discount " +
                "ORDER BY b.DateCheckOut;";

            DataTable table = DatabaseHelper.ExecuteQuery(
                sql,
                DatabaseHelper.Param("@status", StatusDaThanhToan),
                DatabaseHelper.Param("@ngay", ngay.ToString("yyyy-MM-dd")));

            List<RevenueItem> list = new List<RevenueItem>();
            int i;
            for (i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                if (Convert.ToInt32(row["MissingPriceCount"]) > 0)
                {
                    throw new InvalidOperationException(
                        "Hóa đơn " + row["Id"] + " chưa có đủ đơn giá lịch sử. " +
                        "Không thể tính doanh thu kỳ này chính xác. " +
                        "Hãy kiểm tra dữ liệu cũ hoặc tạo lại database seed thử nghiệm.");
                }

                RevenueItem item = new RevenueItem();
                item.BillId = Convert.ToInt32(row["Id"]);
                item.DateCheckOut = row["DateCheckOut"].ToString();
                item.TableName = row["TableName"].ToString();
                item.Discount = Convert.ToInt32(row["Discount"]);
                double subTotal = Convert.ToDouble(row["SubTotal"]);
                item.Total = subTotal * (100 - item.Discount) / 100.0;
                list.Add(item);
            }

            return list;
        }

        private static Bill MapBill(DataRow row)
        {
            Bill bill = new Bill();
            bill.Id = Convert.ToInt32(row["Id"]);
            bill.DateCheckIn = DateTime.Parse(row["DateCheckIn"].ToString());
            if (row["DateCheckOut"] != DBNull.Value && row["DateCheckOut"].ToString() != "")
            {
                bill.DateCheckOut = DateTime.Parse(row["DateCheckOut"].ToString());
            }
            else
            {
                bill.DateCheckOut = null;
            }

            bill.IdTable = Convert.ToInt32(row["IdTable"]);
            bill.Status = Convert.ToInt32(row["Status"]);
            bill.Discount = Convert.ToInt32(row["Discount"]);
            return bill;
        }
    }
}
