using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using QuanLiQuanCoffe.Models;

namespace QuanLiQuanCoffe.DataAccess
{
    public static class BillDetailDAO
    {
        public static List<BillInfo> GetByBill(int idBill)
        {
            DataTable table = DatabaseHelper.ExecuteQuery(
                "SELECT d.Id, d.IdFood, f.Name AS FoodName, d.Count, f.Price " +
                "FROM BillDetail d INNER JOIN Food f ON d.IdFood = f.Id " +
                "WHERE d.IdBill = @bill ORDER BY d.Id;",
                new SQLiteParameter("@bill", idBill));

            List<BillInfo> list = new List<BillInfo>();
            int i;
            for (i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                BillInfo item = new BillInfo();
                item.Id = Convert.ToInt32(row["Id"]);
                item.IdFood = Convert.ToInt32(row["IdFood"]);
                item.FoodName = row["FoodName"].ToString();
                item.Count = Convert.ToInt32(row["Count"]);
                item.Price = Convert.ToDouble(row["Price"]);
                list.Add(item);
            }

            return list;
        }

        /// <summary>
        /// Nếu món đã có trên hóa đơn thì cộng dồn số lượng.
        /// </summary>
        public static void InsertOrIncrease(int idBill, int idFood, int count)
        {
            DataTable table = DatabaseHelper.ExecuteQuery(
                "SELECT Id, Count FROM BillDetail WHERE IdBill = @bill AND IdFood = @food;",
                new SQLiteParameter("@bill", idBill),
                new SQLiteParameter("@food", idFood));

            if (table.Rows.Count > 0)
            {
                int id = Convert.ToInt32(table.Rows[0]["Id"]);
                int oldCount = Convert.ToInt32(table.Rows[0]["Count"]);
                DatabaseHelper.ExecuteNonQuery(
                    "UPDATE BillDetail SET Count = @count WHERE Id = @id;",
                    new SQLiteParameter("@count", oldCount + count),
                    new SQLiteParameter("@id", id));
            }
            else
            {
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO BillDetail (IdBill, IdFood, Count) VALUES (@bill, @food, @count);",
                    new SQLiteParameter("@bill", idBill),
                    new SQLiteParameter("@food", idFood),
                    new SQLiteParameter("@count", count));
            }
        }

        public static bool Delete(int id)
        {
            int rows = DatabaseHelper.ExecuteNonQuery(
                "DELETE FROM BillDetail WHERE Id = @id;",
                new SQLiteParameter("@id", id));
            return rows > 0;
        }
    }
}
