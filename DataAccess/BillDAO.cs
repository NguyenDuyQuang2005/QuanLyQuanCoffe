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
            DataTable table = DatabaseHelper.ExecuteQuery(
                "SELECT Id, DateCheckIn, DateCheckOut, IdTable, Status, Discount " +
                "FROM Bill WHERE IdTable = @table AND Status = @status ORDER BY Id DESC LIMIT 1;",
                new SQLiteParameter("@table", idTable),
                new SQLiteParameter("@status", StatusChuaThanhToan));

            if (table.Rows.Count == 0)
            {
                return null;
            }

            return MapBill(table.Rows[0]);
        }

        public static int Insert(int idTable)
        {
            return DatabaseHelper.ExecuteInsert(
                "INSERT INTO Bill (DateCheckIn, DateCheckOut, IdTable, Status, Discount) " +
                "VALUES (@checkIn, NULL, @table, @status, 0);",
                new SQLiteParameter("@checkIn", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                new SQLiteParameter("@table", idTable),
                new SQLiteParameter("@status", StatusChuaThanhToan));
        }

        public static bool CheckOut(int billId, int discount)
        {
            int rows = DatabaseHelper.ExecuteNonQuery(
                "UPDATE Bill SET Status = @status, DateCheckOut = @checkOut, Discount = @discount WHERE Id = @id;",
                new SQLiteParameter("@status", StatusDaThanhToan),
                new SQLiteParameter("@checkOut", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                new SQLiteParameter("@discount", discount),
                new SQLiteParameter("@id", billId));
            return rows > 0;
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
                new SQLiteParameter("@status", StatusDaThanhToan),
                new SQLiteParameter("@ngay", ngay.ToString("yyyy-MM-dd")));

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
