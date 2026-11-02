using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using QuanLiQuanCoffe.Models;

namespace QuanLiQuanCoffe.DataAccess
{
    public static class AccountDAO
    {
        public static Account Login(string userName, string passWord)
        {
            DataTable table = DatabaseHelper.ExecuteQuery(
                "SELECT UserName, DisplayName, PassWord, Type FROM Account " +
                "WHERE UserName = @userName AND PassWord = @passWord;",
                DatabaseHelper.Param("@userName", userName),
                DatabaseHelper.Param("@passWord", PasswordHelper.Hash(passWord)));

            if (table.Rows.Count == 0)
            {
                return null;
            }

            return MapAccount(table.Rows[0]);
        }

        public static List<Account> GetAll()
        {
            DataTable table = DatabaseHelper.ExecuteQuery(
                "SELECT UserName, DisplayName, PassWord, Type FROM Account ORDER BY UserName;");
            List<Account> list = new List<Account>();
            int i;
            for (i = 0; i < table.Rows.Count; i++)
            {
                list.Add(MapAccount(table.Rows[i]));
            }

            return list;
        }

        public static bool Exists(string userName)
        {
            object countObj = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM Account WHERE UserName = @userName;",
                DatabaseHelper.Param("@userName", userName));
            return Convert.ToInt32(countObj) > 0;
        }

        public static int CountAdmin()
        {
            object countObj = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM Account WHERE Type = @type;",
                DatabaseHelper.Param("@type", Account.TypeAdmin));
            return Convert.ToInt32(countObj);
        }

        public static bool Insert(Account account)
        {
            int rows = DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO Account (UserName, DisplayName, PassWord, Type) " +
                "VALUES (@user, @display, @pass, @type);",
                DatabaseHelper.Param("@user", account.UserName),
                DatabaseHelper.Param("@display", account.DisplayName),
                DatabaseHelper.Param("@pass", PasswordHelper.Hash(account.PassWord)),
                DatabaseHelper.Param("@type", account.Type));
            return rows > 0;
        }

        /// <summary>
        /// Nếu PassWord rỗng thì giữ mật khẩu cũ.
        /// </summary>
        public static bool Update(Account account)
        {
            string sql;
            SQLiteParameter[] parameters;
            if (string.IsNullOrEmpty(account.PassWord))
            {
                sql = "UPDATE Account SET DisplayName = @display, Type = @type WHERE UserName = @user;";
                parameters = new SQLiteParameter[]
                {
                    DatabaseHelper.Param("@display", account.DisplayName),
                    DatabaseHelper.Param("@type", account.Type),
                    DatabaseHelper.Param("@user", account.UserName)
                };
            }
            else
            {
                sql = "UPDATE Account SET DisplayName = @display, PassWord = @pass, Type = @type WHERE UserName = @user;";
                parameters = new SQLiteParameter[]
                {
                    DatabaseHelper.Param("@display", account.DisplayName),
                    DatabaseHelper.Param("@pass", PasswordHelper.Hash(account.PassWord)),
                    DatabaseHelper.Param("@type", account.Type),
                    DatabaseHelper.Param("@user", account.UserName)
                };
            }

            return DatabaseHelper.ExecuteNonQuery(sql, parameters) > 0;
        }

        /// <summary>
        /// Đổi mật khẩu chữ thường còn trong CSDL cũ sang SHA-256.
        /// </summary>
        public static void HashPlaintextPasswordsIfNeeded()
        {
            List<Account> list = GetAll();
            int i;
            for (i = 0; i < list.Count; i++)
            {
                if (PasswordHelper.IsSha256Hash(list[i].PassWord))
                {
                    continue;
                }

                DatabaseHelper.ExecuteNonQuery(
                    "UPDATE Account SET PassWord = @pass WHERE UserName = @user;",
                    DatabaseHelper.Param("@pass", PasswordHelper.Hash(list[i].PassWord)),
                    DatabaseHelper.Param("@user", list[i].UserName));
            }
        }

        public static bool Delete(string userName)
        {
            int rows = DatabaseHelper.ExecuteNonQuery(
                "DELETE FROM Account WHERE UserName = @user;",
                DatabaseHelper.Param("@user", userName));
            return rows > 0;
        }

        private static Account MapAccount(DataRow row)
        {
            Account account = new Account();
            account.UserName = row["UserName"].ToString();
            account.DisplayName = row["DisplayName"].ToString();
            account.PassWord = row["PassWord"].ToString();
            account.Type = int.Parse(row["Type"].ToString());
            return account;
        }
    }
}
