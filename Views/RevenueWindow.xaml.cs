using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

using QuanLiQuanCoffe.DataAccess;
using QuanLiQuanCoffe.Models;

using ClosedXML.Excel;

using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace QuanLiQuanCoffe.Views
{
    public partial class RevenueWindow : UserControl
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

            List<RevenueItem> list;
            try
            {
                list = BillDAO.GetRevenue(ngay, theoThang);
            }
            catch (InvalidOperationException ex)
            {
                dgDoanhThu.ItemsSource = null;
                txtTong.Text = "Chưa thể tính doanh thu: thiếu đơn giá lịch sử.";
                MessageBox.Show(ex.Message, "Doanh thu chưa đầy đủ",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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

            txtTong.Text = nhan + tong.ToString("N0") + " đ  (" +
                           list.Count + " hóa đơn)";
        }

        // Lấy danh sách hóa đơn đang hiển thị
        private List<RevenueItem> LayDanhSachDoanhThu()
        {
            List<RevenueItem> list = dgDoanhThu.ItemsSource as List<RevenueItem>;

            if (list == null)
            {
                list = new List<RevenueItem>();
            }

            return list;
        }

        // Tạo tên kỳ báo cáo
        private string LayTenKyBaoCao()
        {
            DateTime ngay = dpNgay.SelectedDate ?? DateTime.Today;

            if (cboKieu.SelectedIndex == 1)
            {
                return "Thang_" + ngay.Month + "_" + ngay.Year;
            }

            return "Ngay_" + ngay.ToString("dd_MM_yyyy");
        }

        // Tính tổng doanh thu
        private double TinhTongDoanhThu(List<RevenueItem> list)
        {
            double tong = 0;

            int i;
            for (i = 0; i < list.Count; i++)
            {
                tong += list[i].Total;
            }

            return tong;
        }

        // =========================
        // XUẤT FILE EXCEL
        // =========================
        private void BtnXuatExcel_Click(object sender, RoutedEventArgs e)
        {
            List<RevenueItem> list = LayDanhSachDoanhThu();

            if (list.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất Excel.",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Title = "Lưu báo cáo doanh thu Excel";
            dialog.Filter = "Excel Workbook (*.xlsx)|*.xlsx";
            dialog.FileName = "BáoCáoDoanhThu_" + LayTenKyBaoCao() + ".xlsx";

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                using (XLWorkbook workbook = new XLWorkbook())
                {
                    IXLWorksheet sheet = workbook.Worksheets.Add("Doanh thu");

                    // Tiêu đề báo cáo
                    sheet.Cell("A1").Value = "BÁO CÁO DOANH THU";
                    sheet.Range("A1:E1").Merge();

                    sheet.Cell("A2").Value = txtTong.Text;
                    sheet.Range("A2:E2").Merge();

                    // Tiêu đề các cột
                    sheet.Cell("A4").Value = "Mã hóa đơn";
                    sheet.Cell("B4").Value = "Thời gian thanh toán";
                    sheet.Cell("C4").Value = "Bàn";
                    sheet.Cell("D4").Value = "Giảm (%)";
                    sheet.Cell("E4").Value = "Doanh thu (đ)";

                    // Ghi dữ liệu
                    int dong = 5;
                    int i;

                    for (i = 0; i < list.Count; i++)
                    {
                        sheet.Cell(dong, 1).Value = list[i].BillId;
                        sheet.Cell(dong, 2).Value =
                            Convert.ToString(list[i].DateCheckOut);
                        sheet.Cell(dong, 3).Value = list[i].TableName;
                        sheet.Cell(dong, 4).Value = list[i].Discount;
                        sheet.Cell(dong, 5).Value = list[i].Total;

                        dong++;
                    }

                    // Dòng tổng doanh thu
                    sheet.Cell(dong + 1, 4).Value = "Tổng doanh thu:";
                    sheet.Cell(dong + 1, 5).Value = TinhTongDoanhThu(list);

                    // Định dạng tiêu đề
                    sheet.Range("A1:E1").Style.Font.Bold = true;
                    sheet.Range("A1:E1").Style.Font.FontSize = 18;
                    sheet.Range("A1:E1").Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Center;

                    sheet.Range("A2:E2").Merge();
                    sheet.Range("A2:E2").Style.Font.Bold = true;

                    // Định dạng hàng tiêu đề cột
                    sheet.Range("A4:E4").Style.Font.Bold = true;
                    sheet.Range("A4:E4").Style.Fill.BackgroundColor =
                        XLColor.LightGray;

                    sheet.Range("A4:E4").Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Center;

                    // Định dạng tiền
                    sheet.Column(5).Style.NumberFormat.Format = "#,##0";

                    sheet.Range(dong + 1, 4, dong + 1, 5)
                         .Style.Font.Bold = true;

                    // Căn chỉnh kích thước
                    sheet.Columns().AdjustToContents();

                    // Kẻ viền bảng
                    sheet.Range(4, 1, dong - 1, 5).Style.Border.OutsideBorder =
                        XLBorderStyleValues.Thin;

                    sheet.Range(4, 1, dong - 1, 5).Style.Border.InsideBorder =
                        XLBorderStyleValues.Thin;

                    workbook.SaveAs(dialog.FileName);
                }

                MessageBox.Show("Xuất file Excel thành công!",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể xuất Excel.\nChi tiết: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =========================
        // XUẤT FILE PDF
        // =========================
        private void BtnXuatPDF_Click(object sender, RoutedEventArgs e)
        {
            List<RevenueItem> list = LayDanhSachDoanhThu();

            if (list.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất PDF.",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Title = "Lưu báo cáo doanh thu PDF";
            dialog.Filter = "PDF Document (*.pdf)|*.pdf";
            dialog.FileName = "BaoCaoDoanhThu_" + LayTenKyBaoCao() + ".pdf";

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                PdfDocument document = new PdfDocument();
                document.Info.Title = "Báo cáo doanh thu";

                // Font sử dụng trong báo cáo
                XFont fontTieuDe = new XFont("Arial", 18, XFontStyleEx.Bold);
                XFont fontThuong = new XFont("Arial", 10, XFontStyleEx.Regular);
                XFont fontDam = new XFont("Arial", 10, XFontStyleEx.Bold);

                PdfPage page = null;
                XGraphics graphics = null;

                double y = 0;

                // Tạo trang PDF mới
                Action taoTrangMoi = () =>
                {
                    page = document.AddPage();
                    page.Size = PdfSharp.PageSize.A4;
                    page.Orientation = PdfSharp.PageOrientation.Landscape;

                    graphics = XGraphics.FromPdfPage(page);

                    y = 35;

                    graphics.DrawString(
                        "BAO CAO DOANH THU",
                        fontTieuDe,
                        XBrushes.Black,
                        new XPoint(250, y));

                    y += 30;

                    graphics.DrawString(
                        txtTong.Text,
                        fontDam,
                        XBrushes.Black,
                        new XPoint(35, y));

                    y += 35;

                    // Tiêu đề cột
                    graphics.DrawString("Mã HĐ", fontDam,
                        XBrushes.Black, new XPoint(35, y));

                    graphics.DrawString("Thời gian thanh toán", fontDam,
                        XBrushes.Black, new XPoint(100, y));

                    graphics.DrawString("Bàn", fontDam,
                        XBrushes.Black, new XPoint(280, y));

                    graphics.DrawString("Giảm (%)", fontDam,
                        XBrushes.Black, new XPoint(440, y));

                    graphics.DrawString("Doanh thu (đ)", fontDam,
                        XBrushes.Black, new XPoint(550, y));

                    y += 10;

                    graphics.DrawLine(
                        XPens.Black,
                        35, y,
                        page.Width - 35, y);

                    y += 20;
                };

                taoTrangMoi();

                int i;

                for (i = 0; i < list.Count; i++)
                {
                    // Nếu gần hết trang thì tạo trang mới
                    if (y > page.Height - 50)
                    {
                        taoTrangMoi();
                    }

                    graphics.DrawString(
                        Convert.ToString(list[i].BillId),
                        fontThuong,
                        XBrushes.Black,
                        new XPoint(35, y));

                    graphics.DrawString(
                        Convert.ToString(list[i].DateCheckOut),
                        fontThuong,
                        XBrushes.Black,
                        new XPoint(100, y));

                    graphics.DrawString(
                        Convert.ToString(list[i].TableName),
                        fontThuong,
                        XBrushes.Black,
                        new XPoint(280, y));

                    graphics.DrawString(
                        Convert.ToString(list[i].Discount),
                        fontThuong,
                        XBrushes.Black,
                        new XPoint(440, y));

                    graphics.DrawString(
                        list[i].Total.ToString("N0"),
                        fontThuong,
                        XBrushes.Black,
                        new XPoint(550, y));

                    y += 22;
                }

                // Tổng doanh thu ở cuối báo cáo
                if (y > page.Height - 55)
                {
                    taoTrangMoi();
                }

                y += 10;

                graphics.DrawLine(
                    XPens.Black,
                    35, y,
                    page.Width - 35, y);

                y += 25;

                graphics.DrawString(
                    "TONG DOANH THU: " +
                    TinhTongDoanhThu(list).ToString("N0") + " VND",
                    fontDam,
                    XBrushes.Black,
                    new XPoint(35, y));

                document.Save(dialog.FileName);
                document.Close();

                MessageBox.Show("Xuất file PDF thành công!",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể xuất PDF.\nChi tiết: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}