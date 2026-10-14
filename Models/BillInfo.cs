namespace QuanLiQuanCoffe.Models
{
    /// <summary>
    /// Dòng hiển thị trên hóa đơn (không phải bảng CSDL).
    /// </summary>
    public class BillInfo
    {
        public int Id { get; set; }
        public int IdFood { get; set; }
        public string FoodName { get; set; }
        public int Count { get; set; }
        public double Price { get; set; }

        public double Total
        {
            get { return Price * Count; }
        }
    }
}
