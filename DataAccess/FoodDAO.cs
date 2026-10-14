using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using QuanLiQuanCoffe.Models;

namespace QuanLiQuanCoffe.DataAccess
{
    public static class FoodDAO
    {
        public static List<Food> GetAll()
        {
            return Search("", 0);
        }

        public static List<Food> GetByCategory(int idCategory)
        {
            return Search("", idCategory);
        }

        /// <summary>
        /// Tìm theo tên. idCategory = 0 nghĩa là mọi danh mục.
        /// </summary>
        public static List<Food> Search(string keyword, int idCategory)
        {
            string sql =
                "SELECT f.Id, f.Name, f.IdCategory, f.Price, c.Name AS CategoryName " +
                "FROM Food f INNER JOIN Category c ON f.IdCategory = c.Id " +
                "WHERE f.Name LIKE @name";

            if (idCategory > 0)
            {
                sql += " AND f.IdCategory = @cat";
            }

            sql += " ORDER BY f.Name;";

            DataTable table;
            if (idCategory > 0)
            {
                table = DatabaseHelper.ExecuteQuery(
                    sql,
                    new SQLiteParameter("@name", "%" + keyword + "%"),
                    new SQLiteParameter("@cat", idCategory));
            }
            else
            {
                table = DatabaseHelper.ExecuteQuery(
                    sql,
                    new SQLiteParameter("@name", "%" + keyword + "%"));
            }

            List<Food> list = new List<Food>();
            int i;
            for (i = 0; i < table.Rows.Count; i++)
            {
                list.Add(MapFood(table.Rows[i]));
            }

            return list;
        }

        public static bool Insert(string name, int idCategory, double price)
        {
            int rows = DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO Food (Name, IdCategory, Price) VALUES (@name, @cat, @price);",
                new SQLiteParameter("@name", name),
                new SQLiteParameter("@cat", idCategory),
                new SQLiteParameter("@price", price));
            return rows > 0;
        }

        public static bool Update(int id, string name, int idCategory, double price)
        {
            int rows = DatabaseHelper.ExecuteNonQuery(
                "UPDATE Food SET Name = @name, IdCategory = @cat, Price = @price WHERE Id = @id;",
                new SQLiteParameter("@name", name),
                new SQLiteParameter("@cat", idCategory),
                new SQLiteParameter("@price", price),
                new SQLiteParameter("@id", id));
            return rows > 0;
        }

        public static bool Delete(int id)
        {
            object countObj = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM BillDetail WHERE IdFood = @id;",
                new SQLiteParameter("@id", id));
            if (Convert.ToInt32(countObj) > 0)
            {
                return false;
            }

            int rows = DatabaseHelper.ExecuteNonQuery(
                "DELETE FROM Food WHERE Id = @id;",
                new SQLiteParameter("@id", id));
            return rows > 0;
        }

        private static Food MapFood(DataRow row)
        {
            Food food = new Food();
            food.Id = Convert.ToInt32(row["Id"]);
            food.Name = row["Name"].ToString();
            food.IdCategory = Convert.ToInt32(row["IdCategory"]);
            food.Price = Convert.ToDouble(row["Price"]);
            food.CategoryName = row["CategoryName"].ToString();
            return food;
        }
    }
}
