using System.Windows;
using QuanLiQuanCoffe.Models;
using System.Windows.Controls;

namespace QuanLiQuanCoffe.Views
{
    public partial class MainWindow : Window
    {
        private readonly Account taiKhoanHienTai;

        private Button nutDangChon;

        public MainWindow(Account account)
        {
            InitializeComponent();
            taiKhoanHienTai = account;
            ApDungPhanQuyen();
            CapNhatMenu(btnTrangChu, "Trang chủ");
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


        private void HienThiNoiDung(
            UserControl giaoDien,
            Button nutMenu,
            string tieuDe)
        {
            // Ẩn trang chủ
            pnlTrangChu.Visibility = Visibility.Collapsed;

            // Hiển thị giao diện chức năng
            noiDungChinh.Content = giaoDien;
            noiDungChinh.Visibility = Visibility.Visible;

            // Cập nhật Sidebar và tiêu đề trang
            CapNhatMenu(nutMenu, tieuDe);
        }

        private void BtnGoiMon_Click(object sender, RoutedEventArgs e)
        {
            HienThiNoiDung(
                new OrderWindow(),
                btnGoiMon,
                "Gọi món & Thanh toán"
            );
        }

        private void BtnMon_Click(object sender, RoutedEventArgs e)
        {
            HienThiNoiDung(
                new FoodWindow(taiKhoanHienTai),
                btnMon,
                "Quản lý món & Danh mục"
            );
        }


        private void BtnBan_Click(object sender, RoutedEventArgs e)
        {
            HienThiNoiDung(
                new TableWindow(taiKhoanHienTai),
                btnBan,
                "Quản lý bàn"
            );
        }

        private void BtnNhanVien_Click(object sender, RoutedEventArgs e)
        {
            if (!taiKhoanHienTai.IsAdmin)
            {
                MessageBox.Show("Chỉ Admin được quản lý nhân viên.", "Không có quyền",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            HienThiNoiDung(
                            new StaffWindow(taiKhoanHienTai),
                            btnNhanVien,
                            "Quản lý nhân viên"
                        );
        }

        private void BtnThongKe_Click(object sender, RoutedEventArgs e)
        {
            if (!taiKhoanHienTai.IsAdmin)
            {
                MessageBox.Show("Chỉ Admin được xem thống kê.", "Không có quyền",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            HienThiNoiDung( new RevenueWindow(),
                            btnThongKe,
                            "Thống kê doanh thu"
                        );
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


        private void BtnTrangChu_Click(object sender, RoutedEventArgs e)
        {
            // Bỏ màn hình chức năng đang hiển thị
            noiDungChinh.Content = null;

            // Ẩn vùng chức năng
            noiDungChinh.Visibility = Visibility.Collapsed;

            // Hiển thị lại trang chủ
            pnlTrangChu.Visibility = Visibility.Visible;

            // Đánh dấu nút Trang chủ
            CapNhatMenu(btnTrangChu, "Trang chủ");
        }


        private void CapNhatMenu(Button nutMoi, string tieuDe)
        {
            // Bỏ trạng thái được chọn của nút cũ
            if (nutDangChon != null)
            {
                nutDangChon.Tag = null;
            }

            // Cập nhật nút được chọn
            nutDangChon = nutMoi;

            // Đánh dấu nút mới
            if (nutDangChon != null)
            {
                nutDangChon.Tag = "Selected";
            }

            // Cập nhật tiêu đề vùng làm việc
            txtTieuDeTrang.Text = tieuDe;
        }

    }
}
