using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using QuanLiQuanCoffe.DataAccess;
using QuanLiQuanCoffe.Models;

namespace QuanLiQuanCoffe.Views
{
    public partial class FoodWindow : Window
    {
        private int idDanhMucChon;
        private int idMonChon;

        public FoodWindow()
        {
            InitializeComponent();
            idDanhMucChon = 0;
            idMonChon = 0;
            NapDanhMuc();
            NapComboDanhMuc();
            NapMon();
        }

        private void NapDanhMuc()
        {
            dgDanhMuc.ItemsSource = CategoryDAO.Search(txtTimDanhMuc.Text.Trim());
        }

        private void NapComboDanhMuc()
        {
            List<Category> danhMuc = CategoryDAO.GetAll();
            cboDanhMucMon.ItemsSource = danhMuc;

            List<Category> loc = new List<Category>();
            Category tatCa = new Category();
            tatCa.Id = 0;
            tatCa.Name = "Tất cả danh mục";
            loc.Add(tatCa);
            loc.AddRange(CategoryDAO.GetAll());
            cboLocDanhMuc.ItemsSource = loc;
            cboLocDanhMuc.SelectedIndex = 0;
        }

        private void NapMon()
        {
            int idLoc = 0;
            if (cboLocDanhMuc.SelectedValue != null)
            {
                idLoc = Convert.ToInt32(cboLocDanhMuc.SelectedValue);
            }

            dgMon.ItemsSource = FoodDAO.Search(txtTimMon.Text.Trim(), idLoc);
        }

        private void DgDanhMuc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Category item = dgDanhMuc.SelectedItem as Category;
            if (item == null)
            {
                return;
            }

            idDanhMucChon = item.Id;
            txtTenDanhMuc.Text = item.Name;
        }

        private void BtnTimDanhMuc_Click(object sender, RoutedEventArgs e)
        {
            NapDanhMuc();
        }

        private void BtnThemDanhMuc_Click(object sender, RoutedEventArgs e)
        {
            string ten = txtTenDanhMuc.Text.Trim();
            if (string.IsNullOrWhiteSpace(ten))
            {
                MessageBox.Show("Vui lòng nhập tên danh mục.", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                CategoryDAO.Insert(ten);
                NapComboDanhMuc();
                NapDanhMuc();
                XoaFormDanhMuc();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thêm được danh mục (có thể trùng tên).\n" + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSuaDanhMuc_Click(object sender, RoutedEventArgs e)
        {
            if (idDanhMucChon <= 0)
            {
                MessageBox.Show("Hãy chọn danh mục cần sửa.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string ten = txtTenDanhMuc.Text.Trim();
            if (string.IsNullOrWhiteSpace(ten))
            {
                MessageBox.Show("Vui lòng nhập tên danh mục.", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                CategoryDAO.Update(idDanhMucChon, ten);
                NapComboDanhMuc();
                NapDanhMuc();
                NapMon();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không sửa được danh mục.\n" + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnXoaDanhMuc_Click(object sender, RoutedEventArgs e)
        {
            if (idDanhMucChon <= 0)
            {
                MessageBox.Show("Hãy chọn danh mục cần xóa.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (CategoryDAO.CountFoods(idDanhMucChon) > 0)
            {
                MessageBox.Show("Không thể xóa danh mục đang chứa món.", "Không xóa được",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult xacNhan = MessageBox.Show("Xóa danh mục đã chọn?", "Xác nhận",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (xacNhan != MessageBoxResult.Yes)
            {
                return;
            }

            CategoryDAO.Delete(idDanhMucChon);
            NapComboDanhMuc();
            NapDanhMuc();
            XoaFormDanhMuc();
        }

        private void BtnLamMoiDanhMuc_Click(object sender, RoutedEventArgs e)
        {
            txtTimDanhMuc.Clear();
            XoaFormDanhMuc();
            NapDanhMuc();
        }

        private void XoaFormDanhMuc()
        {
            idDanhMucChon = 0;
            txtTenDanhMuc.Clear();
            dgDanhMuc.UnselectAll();
        }

        private void DgMon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Food item = dgMon.SelectedItem as Food;
            if (item == null)
            {
                return;
            }

            idMonChon = item.Id;
            txtTenMon.Text = item.Name;
            txtGia.Text = item.Price.ToString("0");
            cboDanhMucMon.SelectedValue = item.IdCategory;
        }

        private void BtnTimMon_Click(object sender, RoutedEventArgs e)
        {
            NapMon();
        }

        private bool TryLayThongTinMon(out string ten, out int idCategory, out double gia)
        {
            ten = txtTenMon.Text.Trim();
            idCategory = 0;
            gia = 0;

            if (string.IsNullOrWhiteSpace(ten))
            {
                MessageBox.Show("Vui lòng nhập tên món.", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (cboDanhMucMon.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn danh mục.", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            idCategory = Convert.ToInt32(cboDanhMucMon.SelectedValue);
            if (!double.TryParse(txtGia.Text.Trim(), out gia) || gia < 0)
            {
                MessageBox.Show("Giá phải là số không âm.", "Sai định dạng",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void BtnThemMon_Click(object sender, RoutedEventArgs e)
        {
            string ten;
            int idCategory;
            double gia;
            if (!TryLayThongTinMon(out ten, out idCategory, out gia))
            {
                return;
            }

            FoodDAO.Insert(ten, idCategory, gia);
            NapMon();
            XoaFormMon();
        }

        private void BtnSuaMon_Click(object sender, RoutedEventArgs e)
        {
            if (idMonChon <= 0)
            {
                MessageBox.Show("Hãy chọn món cần sửa.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string ten;
            int idCategory;
            double gia;
            if (!TryLayThongTinMon(out ten, out idCategory, out gia))
            {
                return;
            }

            FoodDAO.Update(idMonChon, ten, idCategory, gia);
            NapMon();
        }

        private void BtnXoaMon_Click(object sender, RoutedEventArgs e)
        {
            if (idMonChon <= 0)
            {
                MessageBox.Show("Hãy chọn món cần xóa.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult xacNhan = MessageBox.Show("Xóa món đã chọn?", "Xác nhận",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (xacNhan != MessageBoxResult.Yes)
            {
                return;
            }

            if (!FoodDAO.Delete(idMonChon))
            {
                MessageBox.Show("Không thể xóa món đã có trong hóa đơn.", "Không xóa được",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NapMon();
            XoaFormMon();
        }

        private void BtnLamMoiMon_Click(object sender, RoutedEventArgs e)
        {
            txtTimMon.Clear();
            if (cboLocDanhMuc.Items.Count > 0)
            {
                cboLocDanhMuc.SelectedIndex = 0;
            }

            XoaFormMon();
            NapMon();
        }

        private void XoaFormMon()
        {
            idMonChon = 0;
            txtTenMon.Clear();
            txtGia.Clear();
            dgMon.UnselectAll();
        }
    }
}
