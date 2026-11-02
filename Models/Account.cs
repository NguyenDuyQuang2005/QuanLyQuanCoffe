namespace QuanLiQuanCoffe.Models
{
    /// <summary>
    /// Tài khoản đăng nhập.
    /// Type = 1: Admin, Type = 0: Nhân viên.
    /// </summary>
    public class Account
    {
        public const int TypeNhanVien = 0;
        public const int TypeAdmin = 1;

        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string PassWord { get; set; }
        public int Type { get; set; }

        public bool IsAdmin
        {
            get { return Type == TypeAdmin; }
        }

        public string TypeName
        {
            get { return IsAdmin ? "Admin" : "Nhân viên"; }
        }
    }
}
