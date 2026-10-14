using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using QuanLiQuanCoffe.Models;

namespace QuanLiQuanCoffe.DataAccess
{
    public static class CategoryDAO
    {
        public static List<Category> GetAll()
        {
            DataTable table = DatabaseHelper.ExecuteQuery(
                "SELECT Id, Name FROM Category ORDER BY Name;");
            return MapList(table);
        }

        public static List<Category> Search(string keyword)
        {
            DataTable table = DatabaseHelper.ExecuteQuery(
                "SELECT Id, Name FROM Category WHERE Name LIKE @name ORDER BY Name;",
                new SQLiteParameter("@name", "%" + keyword + "%"));
            return MapList(table);
        }

        public static bool Insert(string name)
        {
            int rows = DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO Category (Name) VALUES (@name);",
                new SQLiteParameter("@name", name));
            return rows > 0;
        }

        public static bool Update(int id, string name)
        {
            int rows = DatabaseHelper.ExecuteNonQuery(
                "UPDATE Category SET Name = @name WHERE Id = @id;",
                new SQLiteParameter("@name", name),
                new SQLiteParameter("@id", id));
            return rows > 0;
        }

        public static bool Delete(int id)
        {
            object countObj = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM Food WHERE IdCategory = @id;",
                new SQLiteParameter("@id", id));
            if (Convert.ToInt32(countObj) > 0)
            {
                return false;
            }

            int rows = DatabaseHelper.ExecuteNonQuery(
                "DELETE FROM Category WHERE Id = @id;",
                new SQLiteParameter("@id", id));
            return rows > 0;
        }

        public static int CountFoods(int idCategory)
        {
            object countObj = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM Food WHERE IdCategory = @id;",
                new SQLiteParameter("@id", idCategory));
            return Convert.ToInt32(countObj);
        }

        private static List<Category> MapList(DataTable table)
        {
            List<Category> list = new List<Category>();
            int i;
            for (i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                Category item = new Category();
                item.Id = Convert.ToInt32(row["Id"]);
                item.Name = row["Name"].ToString();
                list.Add(item);
            }

            return list;
        }
    }
}
