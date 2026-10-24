using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using QuanLiQuanCoffe.DataAccess;
using QuanLiQuanCoffe.Models;

namespace QuanLiQuanCoffe.Views
{
    public partial class OrderWindow : UserControl
    {
        private int idBanChon;
        private int idHoaDonHienTai;
        private double tamTinh;

        public OrderWindow()
        {
            InitializeComponent();
            idBanChon = 0;
            idHoaDonHienTai = 0;
            tamTinh = 0;
            txtBanDangChon.Text = "Chưa chọn bàn";
            TableFoodDAO.SyncStatusWithUnpaidBills();
            NapBan();
            NapDanhMucVaMon();
            CapNhatTongTien();
        }

        private void NapBan()
        {
            pnlBan.Children.Clear();
            List<TableFood> dsBan = TableFoodDAO.GetAll();
            int i;
            for (i = 0; i < dsBan.Count; i++)
            {
                TableFood ban = dsBan[i];
                Button btn = new Button();
                btn.Content = ban.Name + Environment.NewLine + ban.Status;
                btn.Width = 92;
                btn.Height = 64;
                btn.Margin = new Thickness(4);
                btn.Tag = ban;
                btn.Click += BtnBan_Click;

                if (ban.Status == TableFood.StatusCoKhach)
                {
                    btn.Background = new SolidColorBrush(Color.FromRgb(196, 120, 90));
                    btn.Foreground = Brushes.White;
                }
                else
                {
                    btn.Background = new SolidColorBrush(Color.FromRgb(210, 230, 180));
                }

                if (ban.Id == idBanChon)
                {
                    btn.BorderBrush = new SolidColorBrush(Color.FromRgb(74, 55, 40));
                    btn.BorderThickness = new Thickness(3);
                }

                pnlBan.Children.Add(btn);
            }
        }

        private void NapDanhMucVaMon()
        {
            List<Category> loc = new List<Category>();
            Category tatCa = new Category();
            tatCa.Id = 0;
            tatCa.Name = "Tất cả";
            loc.Add(tatCa);
            loc.AddRange(CategoryDAO.GetAll());
            cboDanhMuc.ItemsSource = loc;
            cboDanhMuc.SelectedIndex = 0;
            NapMonTheoDanhMuc();
        }

        private void NapMonTheoDanhMuc()
        {
            int idCategory = 0;
            if (cboDanhMuc.SelectedValue != null)
            {
                idCategory = Convert.ToInt32(cboDanhMuc.SelectedValue);
            }

            List<Food> dsMon = FoodDAO.GetByCategory(idCategory);
            cboMon.ItemsSource = dsMon;
            if (dsMon.Count > 0)
            {
                cboMon.SelectedIndex = 0;
            }
        }

        private void CboDanhMuc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NapMonTheoDanhMuc();
        }

        private void BtnBan_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null)
            {
                return;
            }

            TableFood ban = btn.Tag as TableFood;
            if (ban == null)
            {
                return;
            }

            idBanChon = ban.Id;
            txtBanDangChon.Text = "Bàn đang chọn: " + ban.Name + " (" + ban.Status + ")";

            Bill bill = BillDAO.GetUnpaidByTable(idBanChon);
            if (bill != null)
            {
                idHoaDonHienTai = bill.Id;
                txtGiamGia.Text = bill.Discount.ToString();
            }
            else
            {
                idHoaDonHienTai = 0;
                txtGiamGia.Text = "0";
            }

            NapHoaDon();
            NapBan();
        }

        private void NapHoaDon()
        {
            if (idHoaDonHienTai <= 0)
            {
                dgHoaDon.ItemsSource = null;
                tamTinh = 0;
                CapNhatTongTien();
                return;
            }

            List<BillInfo> chiTiet = BillDetailDAO.GetByBill(idHoaDonHienTai);
            dgHoaDon.ItemsSource = chiTiet;
            tamTinh = 0;
            int i;
            for (i = 0; i < chiTiet.Count; i++)
            {
                tamTinh += chiTiet[i].Total;
            }

            CapNhatTongTien();
        }

        private void CapNhatTongTien()
        {
            // Kiểm tra các thành phần giao diện đã được khởi tạo chưa
            if (txtGiamGia == null || txtTongTien == null)
                return;

            int giamGia = 0;
            int.TryParse(txtGiamGia.Text.Trim(), out giamGia);

            if (giamGia < 0)
                giamGia = 0;

            if (giamGia > 100)
                giamGia = 100;

            double thanhTien = tamTinh * (100 - giamGia) / 100.0;

            txtTongTien.Text = "Tổng: " + thanhTien.ToString("N0") + " đ";
        }

        private void TxtGiamGia_TextChanged(object sender, TextChangedEventArgs e)
        {
            CapNhatTongTien();
        }

        private void BtnThemMon_Click(object sender, RoutedEventArgs e)
        {
            if (idBanChon <= 0)
            {
                MessageBox.Show("Hãy chọn bàn trước.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (cboMon.SelectedValue == null)
            {
                MessageBox.Show("Hãy chọn món.", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int soLuong;
            if (!int.TryParse(txtSoLuong.Text.Trim(), out soLuong) || soLuong <= 0)
            {
                MessageBox.Show("Số lượng phải là số nguyên dương.", "Sai định dạng",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int idFood = Convert.ToInt32(cboMon.SelectedValue);
            btnThemMon.IsEnabled = false;
            try
            {
                idHoaDonHienTai = BillDAO.AddFoodToTable(idBanChon, idFood, soLuong);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thêm được món.\n" + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            finally
            {
                btnThemMon.IsEnabled = true;
            }

            NapHoaDon();
            NapBan();
            CapNhatTenBan();
        }

        private void CapNhatTenBan()
        {
            List<TableFood> dsBan = TableFoodDAO.GetAll();
            int i;
            for (i = 0; i < dsBan.Count; i++)
            {
                if (dsBan[i].Id == idBanChon)
                {
                    txtBanDangChon.Text = "Bàn đang chọn: " + dsBan[i].Name + " (" + dsBan[i].Status + ")";
                    return;
                }
            }
        }

        private void BtnGiamMon_Click(object sender, RoutedEventArgs e)
        {
            BillInfo dong = dgHoaDon.SelectedItem as BillInfo;
            if (dong == null)
            {
                MessageBox.Show("Hãy chọn một món trên hóa đơn để giảm số lượng.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            BillDetailDAO.Decrease(dong.Id, 1);
            XuLyHoaDonSauKhiXoaMon();
        }

        private void BtnXoaMon_Click(object sender, RoutedEventArgs e)
        {
            BillInfo dong = dgHoaDon.SelectedItem as BillInfo;
            if (dong == null)
            {
                MessageBox.Show("Hãy chọn một món trên hóa đơn để xóa.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            BillDetailDAO.Delete(dong.Id);
            XuLyHoaDonSauKhiXoaMon();
        }

        private void XuLyHoaDonSauKhiXoaMon()
        {
            if (idHoaDonHienTai > 0 && idBanChon > 0)
            {
                BillDAO.CancelIfEmpty(idHoaDonHienTai, idBanChon);
                Bill unpaid = BillDAO.GetUnpaidByTable(idBanChon);
                if (unpaid == null)
                {
                    idHoaDonHienTai = 0;
                    txtGiamGia.Text = "0";
                }
            }

            NapHoaDon();
            NapBan();
            CapNhatTenBan();
        }

        private void BtnThanhToan_Click(object sender, RoutedEventArgs e)
        {
            if (idBanChon <= 0 || idHoaDonHienTai <= 0)
            {
                MessageBox.Show("Chưa có hóa đơn để thanh toán.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            List<BillInfo> chiTiet = BillDetailDAO.GetByBill(idHoaDonHienTai);
            if (chiTiet.Count == 0)
            {
                MessageBox.Show("Hóa đơn chưa có món.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int giamGia;
            if (!int.TryParse(txtGiamGia.Text.Trim(), out giamGia) || giamGia < 0 || giamGia > 100)
            {
                MessageBox.Show("Giảm giá phải từ 0 đến 100.", "Sai định dạng",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            double thanhTien = tamTinh * (100 - giamGia) / 100.0;
            MessageBoxResult xacNhan = MessageBox.Show(
                "Thanh toán " + thanhTien.ToString("N0") + " đ?",
                "Xác nhận thanh toán",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (xacNhan != MessageBoxResult.Yes)
            {
                return;
            }

            btnThanhToan.IsEnabled = false;
            bool thanhCong;
            try
            {
                thanhCong = BillDAO.CheckOutAndFreeTable(idHoaDonHienTai, idBanChon, giamGia);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Thanh toán thất bại.\n" + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            finally
            {
                btnThanhToan.IsEnabled = true;
            }

            if (!thanhCong)
            {
                MessageBox.Show("Hóa đơn đã được thanh toán hoặc không còn hiệu lực.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            idHoaDonHienTai = 0;
            txtGiamGia.Text = "0";
            NapHoaDon();
            NapBan();
            CapNhatTenBan();
            MessageBox.Show("Thanh toán thành công.", "Thông báo",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
