using System.Windows;
using QuanLiQuanCoffe.DataAccess;
using QuanLiQuanCoffe.Models;

namespace QuanLiQuanCoffe.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            txtTenDangNhap.Focus();
        }

        private void BtnDangNhap_Click(object sender, RoutedEventArgs e)
        {
            string userName = txtTenDangNhap.Text.Trim();
            string passWord = txtMatKhau.Password;

            if (string.IsNullOrWhiteSpace(userName))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập.", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTenDangNhap.Focus();
                return;
            }

            if (string.IsNullOrEmpty(passWord))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu.", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtMatKhau.Focus();
                return;
            }

            Account account = AccountDAO.Login(userName, passWord);
            if (account == null)
            {
                MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu.", "Đăng nhập thất bại",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtMatKhau.Clear();
                txtMatKhau.Focus();
                return;
            }

            MainWindow mainWindow = new MainWindow(account);
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
            Close();
        }
    }
}
