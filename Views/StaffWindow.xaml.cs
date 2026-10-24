using System;
using System.Windows;
using System.Windows.Controls;
using QuanLiQuanCoffe.DataAccess;
using QuanLiQuanCoffe.Models;

namespace QuanLiQuanCoffe.Views
{
    public partial class StaffWindow : UserControl
    {
        private readonly string tenDangNhapHienTai;
        private bool dangSua;

        public StaffWindow(Account adminHienTai)
        {
            InitializeComponent();
            tenDangNhapHienTai = adminHienTai.UserName;
            dangSua = false;
            cboQuyen.SelectedIndex = 0;
            NapDanhSach();
        }

        private void NapDanhSach()
        {
            dgNhanVien.ItemsSource = AccountDAO.GetAll();
        }

        private void DgNhanVien_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Account item = dgNhanVien.SelectedItem as Account;
            if (item == null)
            {
                return;
            }

            dangSua = true;
            txtTenDangNhap.Text = item.UserName;
            txtTenDangNhap.IsEnabled = false;
            txtTenHienThi.Text = item.DisplayName;
            txtMatKhau.Clear();
            cboQuyen.SelectedIndex = item.IsAdmin ? 1 : 0;
        }

        private int LayQuyen()
        {
            ComboBoxItem item = cboQuyen.SelectedItem as ComboBoxItem;
            if (item == null)
            {
                return Account.TypeNhanVien;
            }

            return Convert.ToInt32(item.Tag.ToString());
        }

        private void BtnThem_Click(object sender, RoutedEventArgs e)
        {
            string userName = txtTenDangNhap.Text.Trim();
            string displayName = txtTenHienThi.Text.Trim();
            string passWord = txtMatKhau.Password;

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(displayName))
            {
                MessageBox.Show("Nhập tên đăng nhập và tên hiển thị.", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(passWord))
            {
                MessageBox.Show("Nhập mật khẩu khi thêm tài khoản.", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (AccountDAO.Exists(userName))
            {
                MessageBox.Show("Tên đăng nhập đã tồn tại.", "Không thêm được",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Account account = new Account();
            account.UserName = userName;
            account.DisplayName = displayName;
            account.PassWord = passWord;
            account.Type = LayQuyen();
            AccountDAO.Insert(account);
            NapDanhSach();
            XoaForm();
        }

        private void BtnSua_Click(object sender, RoutedEventArgs e)
        {
            if (!dangSua)
            {
                MessageBox.Show("Hãy chọn tài khoản cần sửa.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string displayName = txtTenHienThi.Text.Trim();
            if (string.IsNullOrWhiteSpace(displayName))
            {
                MessageBox.Show("Nhập tên hiển thị.", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int typeMoi = LayQuyen();
            if (txtTenDangNhap.Text == tenDangNhapHienTai && typeMoi != Account.TypeAdmin)
            {
                MessageBox.Show("Không thể tự hạ quyền tài khoản đang đăng nhập.", "Không sửa được",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Account dangChon = dgNhanVien.SelectedItem as Account;
            if (dangChon != null && dangChon.IsAdmin && typeMoi != Account.TypeAdmin && AccountDAO.CountAdmin() <= 1)
            {
                MessageBox.Show("Phải còn ít nhất một tài khoản Admin.", "Không sửa được",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Account account = new Account();
            account.UserName = txtTenDangNhap.Text;
            account.DisplayName = displayName;
            account.PassWord = txtMatKhau.Password;
            account.Type = typeMoi;
            AccountDAO.Update(account);
            NapDanhSach();
        }

        private void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (!dangSua)
            {
                MessageBox.Show("Hãy chọn tài khoản cần xóa.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string userName = txtTenDangNhap.Text;
            if (userName == tenDangNhapHienTai)
            {
                MessageBox.Show("Không thể xóa tài khoản đang đăng nhập.", "Không xóa được",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Account dangChon = dgNhanVien.SelectedItem as Account;
            if (dangChon != null && dangChon.IsAdmin && AccountDAO.CountAdmin() <= 1)
            {
                MessageBox.Show("Không thể xóa Admin cuối cùng.", "Không xóa được",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult xacNhan = MessageBox.Show("Xóa tài khoản " + userName + "?", "Xác nhận",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (xacNhan != MessageBoxResult.Yes)
            {
                return;
            }

            AccountDAO.Delete(userName);
            NapDanhSach();
            XoaForm();
        }

        private void BtnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            XoaForm();
            NapDanhSach();
        }

        private void XoaForm()
        {
            dangSua = false;
            txtTenDangNhap.Clear();
            txtTenDangNhap.IsEnabled = true;
            txtTenHienThi.Clear();
            txtMatKhau.Clear();
            cboQuyen.SelectedIndex = 0;
            dgNhanVien.UnselectAll();
        }
    }
}
