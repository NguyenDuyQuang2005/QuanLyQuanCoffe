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
                "SELECT d.Id, d.IdFood, f.Name AS FoodName, d.Count, d.UnitPriceAtSale AS Price " +
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
                if (row["Price"] == DBNull.Value)
                {
                    throw new InvalidOperationException(
                        "Hóa đơn " + idBill + " có món chưa xác định đơn giá lúc bán. " +
                        "Không thể tính tiền chính xác. Hãy kiểm tra dữ liệu cũ trước khi thanh toán.");
                }

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
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException("count", "Số lượng món phải lớn hơn 0.");
            }

            int unpaid = Convert.ToInt32(DatabaseHelper.ExecuteScalar(connection,
                "SELECT COUNT(*) FROM Bill WHERE Id=@bill AND Status=0;",
                DatabaseHelper.Param("@bill", idBill)));
            if (unpaid != 1) throw new InvalidOperationException("Hóa đơn không còn mở.");

            DataTable table = DatabaseHelper.ExecuteQuery(
                connection,
                "SELECT Id, Count, UnitPriceAtSale FROM BillDetail WHERE IdBill = @bill AND IdFood = @food;",
                DatabaseHelper.Param("@bill", idBill),
                DatabaseHelper.Param("@food", idFood));

            if (table.Rows.Count > 0)
            {
                // Dòng cũ thiếu snapshot: không tự suy diễn bằng giá thực đơn hiện tại.
                if (table.Rows[0]["UnitPriceAtSale"] == DBNull.Value)
                {
                    throw new InvalidOperationException(
                        "Dòng món cũ chưa có đơn giá lúc bán. " +
                        "Hãy xử lý dữ liệu này trước khi thêm số lượng.");
                }

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
                // Chụp giá hiện tại đúng tại thời điểm thêm dòng món vào hóa đơn.
                int rows = DatabaseHelper.ExecuteNonQuery(
                    connection,
                    "INSERT INTO BillDetail (IdBill, IdFood, Count, UnitPriceAtSale) " +
                    "SELECT @bill, Id, @count, Price FROM Food WHERE Id = @food;",
                    DatabaseHelper.Param("@bill", idBill),
                    DatabaseHelper.Param("@food", idFood),
                    DatabaseHelper.Param("@count", count));

                if (rows != 1)
                {
                    throw new InvalidOperationException("Món không tồn tại hoặc đã bị xóa.");
                }
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
                "SELECT d.Count FROM BillDetail d INNER JOIN Bill b ON d.IdBill=b.Id " +
                "WHERE d.Id = @id AND b.Status=0;",
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
                "UPDATE BillDetail SET Count = @count WHERE Id = @id " +
                "AND EXISTS (SELECT 1 FROM Bill WHERE Id=BillDetail.IdBill AND Status=0);",
                DatabaseHelper.Param("@count", count),
                DatabaseHelper.Param("@id", id));
            return rows > 0;
        }

        public static bool Delete(int id)
        {
            int rows = DatabaseHelper.ExecuteNonQuery(
                "DELETE FROM BillDetail WHERE Id = @id " +
                "AND EXISTS (SELECT 1 FROM Bill WHERE Id=BillDetail.IdBill AND Status=0);",
                DatabaseHelper.Param("@id", id));
            return rows > 0;
        }
    }
}
