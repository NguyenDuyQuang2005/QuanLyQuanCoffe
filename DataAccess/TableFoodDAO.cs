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
                DatabaseHelper.Param("@name", "%" + keyword + "%"));
            return MapList(table);
        }

        public static bool Insert(string name)
        {
            int rows = DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO TableFood (Name, Status) VALUES (@name, @status);",
                DatabaseHelper.Param("@name", name),
                DatabaseHelper.Param("@status", TableFood.StatusTrong));
            return rows > 0;
        }

        public static bool Update(int id, string name)
        {
            int rows = DatabaseHelper.ExecuteNonQuery(
                "UPDATE TableFood SET Name = @name WHERE Id = @id;",
                DatabaseHelper.Param("@name", name),
                DatabaseHelper.Param("@id", id));
            return rows > 0;
        }

        public static bool UpdateStatus(int id, string status)
        {
            using (SQLiteConnection connection = DatabaseHelper.GetConnection())
            {
                return UpdateStatus(connection, id, status);
            }
        }

        public static bool UpdateStatus(SQLiteConnection connection, int id, string status)
        {
            int rows = DatabaseHelper.ExecuteNonQuery(
                connection,
                "UPDATE TableFood SET Status = @status WHERE Id = @id;",
                DatabaseHelper.Param("@status", status),
                DatabaseHelper.Param("@id", id));
            return rows > 0;
        }

        /// <summary>
        /// Đồng bộ trạng thái bàn theo hóa đơn chưa thanh toán (sửa lệch dữ liệu cũ).
        /// </summary>
        public static void SyncStatusWithUnpaidBills()
        {
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE TableFood SET Status = @coKhach WHERE Id IN (SELECT IdTable FROM Bill WHERE Status = @unpaid);",
                DatabaseHelper.Param("@coKhach", TableFood.StatusCoKhach),
                DatabaseHelper.Param("@unpaid", BillDAO.StatusChuaThanhToan));
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE TableFood SET Status = @trong WHERE Id NOT IN (SELECT IdTable FROM Bill WHERE Status = @unpaid);",
                DatabaseHelper.Param("@trong", TableFood.StatusTrong),
                DatabaseHelper.Param("@unpaid", BillDAO.StatusChuaThanhToan));
        }

        public static bool Delete(int id)
        {
            object countObj = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM Bill WHERE IdTable = @id;",
                DatabaseHelper.Param("@id", id));
            if (Convert.ToInt32(countObj) > 0)
            {
                return false;
            }

            int rows = DatabaseHelper.ExecuteNonQuery(
                "DELETE FROM TableFood WHERE Id = @id;",
                DatabaseHelper.Param("@id", id));
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
