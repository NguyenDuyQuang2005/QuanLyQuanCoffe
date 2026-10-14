using System;
using System.Windows;
using QuanLiQuanCoffe.DataAccess;

namespace QuanLiQuanCoffe
{
    public partial class App : Application
    {
        /// <summary>
        /// Khởi tạo cơ sở dữ liệu trước khi mở cửa sổ đầu tiên.
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                DatabaseHelper.InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể khởi tạo cơ sở dữ liệu.\n" + ex.Message,
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}
