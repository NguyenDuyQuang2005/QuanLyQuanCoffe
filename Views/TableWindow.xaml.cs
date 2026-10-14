using System;
using System.Windows;
using System.Windows.Controls;
using QuanLiQuanCoffe.DataAccess;
using QuanLiQuanCoffe.Models;

namespace QuanLiQuanCoffe.Views
{
    public partial class TableWindow : Window
    {
        private int idBanChon;

        public TableWindow()
        {
            InitializeComponent();
            idBanChon = 0;
            NapBan();
        }

        private void NapBan()
        {
            dgBan.ItemsSource = TableFoodDAO.Search(txtTim.Text.Trim());
        }

        private void DgBan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TableFood item = dgBan.SelectedItem as TableFood;
            if (item == null)
            {
                return;
            }

            idBanChon = item.Id;
            txtTenBan.Text = item.Name;
            txtTrangThai.Text = "Trạng thái hiện tại: " + item.Status + " (đổi khi gọi món / thanh toán)";
        }

        private void BtnTim_Click(object sender, RoutedEventArgs e)
        {
            NapBan();
        }

        private void BtnThem_Click(object sender, RoutedEventArgs e)
        {
            string ten = txtTenBan.Text.Trim();
            if (string.IsNullOrWhiteSpace(ten))
            {
                MessageBox.Show("Vui lòng nhập tên bàn.", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                TableFoodDAO.Insert(ten);
                NapBan();
                XoaForm();
            }
            catch (Exception)
            {
                MessageBox.Show("Không thêm được bàn (có thể trùng tên).", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSua_Click(object sender, RoutedEventArgs e)
        {
            if (idBanChon <= 0)
            {
                MessageBox.Show("Hãy chọn bàn cần sửa.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string ten = txtTenBan.Text.Trim();
            if (string.IsNullOrWhiteSpace(ten))
            {
                MessageBox.Show("Vui lòng nhập tên bàn.", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                TableFoodDAO.Update(idBanChon, ten);
                NapBan();
            }
            catch (Exception)
            {
                MessageBox.Show("Không sửa được bàn (có thể trùng tên).", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (idBanChon <= 0)
            {
                MessageBox.Show("Hãy chọn bàn cần xóa.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult xacNhan = MessageBox.Show("Xóa bàn đã chọn?", "Xác nhận",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (xacNhan != MessageBoxResult.Yes)
            {
                return;
            }

            if (!TableFoodDAO.Delete(idBanChon))
            {
                MessageBox.Show("Không thể xóa bàn đã phát sinh hóa đơn.", "Không xóa được",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NapBan();
            XoaForm();
        }

        private void BtnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            txtTim.Clear();
            XoaForm();
            NapBan();
        }

        private void XoaForm()
        {
            idBanChon = 0;
            txtTenBan.Clear();
            txtTrangThai.Text = "";
            dgBan.UnselectAll();
        }
    }
}
