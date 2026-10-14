namespace QuanLiQuanCoffe.Models
{
    /// <summary>
    /// Một dòng món trong hóa đơn (số lượng Count).
    /// </summary>
    public class BillDetail
    {
        public int Id { get; set; }
        public int IdBill { get; set; }
        public int IdFood { get; set; }
        public int Count { get; set; }
    }
}
