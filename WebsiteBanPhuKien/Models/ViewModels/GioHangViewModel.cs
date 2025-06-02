namespace WebsiteBanPhuKien.Models.ViewModels
{
    public class GioHangViewModel
    {
        public int MaPhuKien { get; set; }
        public string TenPhuKien { get; set; } = string.Empty;
        public string HinhAnh { get; set; } = string.Empty;
        public decimal Gia { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien { get; set; }
    }
}