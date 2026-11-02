namespace QuanLiQuanCoffe.Models
{
    /// <summary>
    /// Một hóa đơn đã thanh toán, dùng cho màn thống kê.
    /// </summary>
    public class RevenueItem
    {
        public int BillId { get; set; }
        public string DateCheckOut { get; set; }
        public string TableName { get; set; }
        public int Discount { get; set; }
        public double Total { get; set; }
    }
}
