using System;
using System.Collections.Generic;
using System.Windows;
using QuanLiQuanCoffe.DataAccess;
using QuanLiQuanCoffe.Models;

namespace QuanLiQuanCoffe.Views
{
    public partial class RevenueWindow : Window
    {
        public RevenueWindow()
        {
            InitializeComponent();
            dpNgay.SelectedDate = DateTime.Today;
            ThongKe();
        }

        private void BtnThongKe_Click(object sender, RoutedEventArgs e)
        {
            ThongKe();
        }

        private void ThongKe()
        {
            if (dpNgay.SelectedDate == null)
            {
                MessageBox.Show("Hãy chọn ngày.", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime ngay = dpNgay.SelectedDate.Value;
            bool theoThang = cboKieu.SelectedIndex == 1;
            List<RevenueItem> list = BillDAO.GetRevenue(ngay, theoThang);
            dgDoanhThu.ItemsSource = list;

            double tong = 0;
            int i;
            for (i = 0; i < list.Count; i++)
            {
                tong += list[i].Total;
            }

            string nhan;
            if (theoThang)
            {
                nhan = "Doanh thu tháng " + ngay.Month + "/" + ngay.Year + ": ";
            }
            else
            {
                nhan = "Doanh thu ngày " + ngay.ToString("dd/MM/yyyy") + ": ";
            }

            txtTong.Text = nhan + tong.ToString("N0") + " đ  (" + list.Count + " hóa đơn)";
        }
    }
}
