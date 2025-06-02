namespace WebsiteBanPhuKien.Models.ViewModels
{
    public class DonHangViewModel
    {
        public DonHang DonHang { get; set; } = null!;
        public string TenNguoiNhan { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string DiaChiGiao { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
        public string PhuongThucThanhToan { get; set; } = string.Empty;
    }
}