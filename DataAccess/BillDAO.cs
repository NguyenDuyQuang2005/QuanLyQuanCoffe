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
                object countObj = DatabaseHelper.ExecuteScalar(
                    connection,
                    "SELECT COUNT(*) FROM BillDetail WHERE IdBill = @id;",
                    DatabaseHelper.Param("@id", billId));
                if (Convert.ToInt32(countObj) > 0)
                {
                    return false;
                }

                DatabaseHelper.ExecuteNonQuery(
                    connection,
                    "DELETE FROM Bill WHERE Id = @id AND Status = @unpaid;",
                    DatabaseHelper.Param("@id", billId),
                    DatabaseHelper.Param("@unpaid", StatusChuaThanhToan));
                TableFoodDAO.UpdateStatus(connection, idTable, TableFood.StatusTrong);
                return true;
            });
        }

        public static bool CheckOutAndFreeTable(int billId, int idTable, int discount)
        {
            return DatabaseHelper.RunInTransaction(delegate(SQLiteConnection connection)
            {
                if (!CheckOut(connection, billId, discount))
                {
                    return false;
                }

                TableFoodDAO.UpdateStatus(connection, idTable, TableFood.StatusTrong);
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
                "SUM(f.Price * d.Count) AS SubTotal " +
                "FROM Bill b " +
                "INNER JOIN TableFood t ON b.IdTable = t.Id " +
                "INNER JOIN BillDetail d ON d.IdBill = b.Id " +
                "INNER JOIN Food f ON d.IdFood = f.Id " +
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
