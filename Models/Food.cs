namespace QuanLiQuanCoffe.Models
{
    /// <summary>
    /// Món ăn / đồ uống. IdCategory trỏ tới bảng Category.
    /// </summary>
    public class Food
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int IdCategory { get; set; }
        public double Price { get; set; }

        /// <summary>
        /// Tên danh mục (không lưu DB, dùng khi hiển thị DataGrid).
        /// </summary>
        public string CategoryName { get; set; }
    }
}
