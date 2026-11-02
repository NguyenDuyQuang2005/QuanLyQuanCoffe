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
                DatabaseHelper.Param("@bill", idBill));

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
            using (SQLiteConnection connection = DatabaseHelper.GetConnection())
            {
                InsertOrIncrease(connection, idBill, idFood, count);
            }
        }

        public static void InsertOrIncrease(SQLiteConnection connection, int idBill, int idFood, int count)
        {
            DataTable table = DatabaseHelper.ExecuteQuery(
                connection,
                "SELECT Id, Count FROM BillDetail WHERE IdBill = @bill AND IdFood = @food;",
                DatabaseHelper.Param("@bill", idBill),
                DatabaseHelper.Param("@food", idFood));

            if (table.Rows.Count > 0)
            {
                int id = Convert.ToInt32(table.Rows[0]["Id"]);
                int oldCount = Convert.ToInt32(table.Rows[0]["Count"]);
                DatabaseHelper.ExecuteNonQuery(
                    connection,
                    "UPDATE BillDetail SET Count = @count WHERE Id = @id;",
                    DatabaseHelper.Param("@count", oldCount + count),
                    DatabaseHelper.Param("@id", id));
            }
            else
            {
                DatabaseHelper.ExecuteNonQuery(
                    connection,
                    "INSERT INTO BillDetail (IdBill, IdFood, Count) VALUES (@bill, @food, @count);",
                    DatabaseHelper.Param("@bill", idBill),
                    DatabaseHelper.Param("@food", idFood),
                    DatabaseHelper.Param("@count", count));
            }
        }

        /// <summary>
        /// Giảm số lượng; nếu còn &lt;= 0 thì xóa dòng.
        /// </summary>
        public static bool Decrease(int id, int amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            DataTable table = DatabaseHelper.ExecuteQuery(
                "SELECT Count FROM BillDetail WHERE Id = @id;",
                DatabaseHelper.Param("@id", id));
            if (table.Rows.Count == 0)
            {
                return false;
            }

            int count = Convert.ToInt32(table.Rows[0]["Count"]) - amount;
            if (count <= 0)
            {
                return Delete(id);
            }

            int rows = DatabaseHelper.ExecuteNonQuery(
                "UPDATE BillDetail SET Count = @count WHERE Id = @id;",
                DatabaseHelper.Param("@count", count),
                DatabaseHelper.Param("@id", id));
            return rows > 0;
        }

        public static bool Delete(int id)
        {
            int rows = DatabaseHelper.ExecuteNonQuery(
                "DELETE FROM BillDetail WHERE Id = @id;",
                DatabaseHelper.Param("@id", id));
            return rows > 0;
        }
    }
}
