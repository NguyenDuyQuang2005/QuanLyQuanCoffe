using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using QuanLiQuanCoffe.Models;

namespace QuanLiQuanCoffe.DataAccess
{
    public static class TableFoodDAO
    {
        public static List<TableFood> GetAll()
        {
            DataTable table = DatabaseHelper.ExecuteQuery(
                "SELECT Id, Name, Status FROM TableFood ORDER BY Name;");
            return MapList(table);
        }

        public static List<TableFood> Search(string keyword)
        {
            DataTable table = DatabaseHelper.ExecuteQuery(
                "SELECT Id, Name, Status FROM TableFood WHERE Name LIKE @name ORDER BY Name;",
                new SQLiteParameter("@name", "%" + keyword + "%"));
            return MapList(table);
        }

        public static bool Insert(string name)
        {
            int rows = DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO TableFood (Name, Status) VALUES (@name, @status);",
                new SQLiteParameter("@name", name),
                new SQLiteParameter("@status", TableFood.StatusTrong));
            return rows > 0;
        }

        public static bool Update(int id, string name)
        {
            int rows = DatabaseHelper.ExecuteNonQuery(
                "UPDATE TableFood SET Name = @name WHERE Id = @id;",
                new SQLiteParameter("@name", name),
                new SQLiteParameter("@id", id));
            return rows > 0;
        }

        public static bool UpdateStatus(int id, string status)
        {
            int rows = DatabaseHelper.ExecuteNonQuery(
                "UPDATE TableFood SET Status = @status WHERE Id = @id;",
                new SQLiteParameter("@status", status),
                new SQLiteParameter("@id", id));
            return rows > 0;
        }

        public static bool Delete(int id)
        {
            object countObj = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM Bill WHERE IdTable = @id;",
                new SQLiteParameter("@id", id));
            if (Convert.ToInt32(countObj) > 0)
            {
                return false;
            }

            int rows = DatabaseHelper.ExecuteNonQuery(
                "DELETE FROM TableFood WHERE Id = @id;",
                new SQLiteParameter("@id", id));
            return rows > 0;
        }

        private static List<TableFood> MapList(DataTable table)
        {
            List<TableFood> list = new List<TableFood>();
            int i;
            for (i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                TableFood item = new TableFood();
                item.Id = Convert.ToInt32(row["Id"]);
                item.Name = row["Name"].ToString();
                item.Status = row["Status"].ToString();
                list.Add(item);
            }

            return list;
        }
    }
}
