using System.Security.Cryptography;
using System.Text;

namespace QuanLiQuanCoffe.DataAccess
{
    /// <summary>
    /// Băm mật khẩu bằng SHA-256 (một chiều).
    /// Không giải mã được; khi đăng nhập băm mật khẩu nhập vào rồi so sánh.
    /// </summary>
    public static class PasswordHelper
    {
        public static string Hash(string password)
        {
            if (password == null)
            {
                password = "";
            }

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                int i;
                for (i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        /// <summary>
        /// SHA-256 dạng hex luôn dài 64 ký tự.
        /// </summary>
        public static bool IsSha256Hash(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length != 64)
            {
                return false;
            }

            int i;
            for (i = 0; i < value.Length; i++)
            {
                char c = value[i];
                bool laHex = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
                if (!laHex)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
