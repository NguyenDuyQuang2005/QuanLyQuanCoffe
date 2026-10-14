using System.Windows;
using QuanLiQuanCoffe.Models;

namespace QuanLiQuanCoffe.Views
{
    public partial class MainWindow : Window
    {
        private readonly Account taiKhoanHienTai;

        public MainWindow(Account account)
        {
            InitializeComponent();
            taiKhoanHienTai = account;
            ApDungPhanQuyen();
        }

        private void ApDungPhanQuyen()
        {
            txtChao.Text = "Xin chào, " + taiKhoanHienTai.DisplayName;
            txtQuyen.Text = "Quyền: " + taiKhoanHienTai.TypeName;
            txtNguoiDung.Text = taiKhoanHienTai.UserName + "  |  " + taiKhoanHienTai.TypeName;

            if (!taiKhoanHienTai.IsAdmin)
            {
                btnNhanVien.Visibility = Visibility.Collapsed;
                btnThongKe.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnGoiMon_Click(object sender, RoutedEventArgs e)
        {
            OrderWindow window = new OrderWindow();
            window.Owner = this;
            window.ShowDialog();
        }

        private void BtnMon_Click(object sender, RoutedEventArgs e)
        {
            FoodWindow window = new FoodWindow();
            window.Owner = this;
            window.ShowDialog();
        }

        private void BtnBan_Click(object sender, RoutedEventArgs e)
        {
            TableWindow window = new TableWindow();
            window.Owner = this;
            window.ShowDialog();
        }

        private void BtnNhanVien_Click(object sender, RoutedEventArgs e)
        {
            if (!taiKhoanHienTai.IsAdmin)
            {
                MessageBox.Show("Chỉ Admin được quản lý nhân viên.", "Không có quyền",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            StaffWindow window = new StaffWindow(taiKhoanHienTai);
            window.Owner = this;
            window.ShowDialog();
        }

        private void BtnThongKe_Click(object sender, RoutedEventArgs e)
        {
            if (!taiKhoanHienTai.IsAdmin)
            {
                MessageBox.Show("Chỉ Admin được xem thống kê.", "Không có quyền",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            RevenueWindow window = new RevenueWindow();
            window.Owner = this;
            window.ShowDialog();
        }

        private void MenuDangXuat_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            Application.Current.MainWindow = loginWindow;
            loginWindow.Show();
            Close();
        }

        private void MenuThoat_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
