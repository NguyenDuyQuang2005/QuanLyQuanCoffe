using System;

namespace QuanLiQuanCoffe.Models
{
    /// <summary>
    /// Hóa đơn theo bàn.
    /// Status = 0: chưa thanh toán, Status = 1: đã thanh toán.
    /// </summary>
    public class Bill
    {
        public int Id { get; set; }
        public DateTime DateCheckIn { get; set; }
        public DateTime? DateCheckOut { get; set; }
        public int IdTable { get; set; }
        public int Status { get; set; }
        public int Discount { get; set; }
    }
}
