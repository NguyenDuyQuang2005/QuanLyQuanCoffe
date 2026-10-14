namespace QuanLiQuanCoffe.Models
{
    /// <summary>
    /// Bàn trong quán. Status: "Trống" hoặc "Có khách".
    /// </summary>
    public class TableFood
    {
        public const string StatusTrong = "Trống";
        public const string StatusCoKhach = "Có khách";

        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
    }
}
