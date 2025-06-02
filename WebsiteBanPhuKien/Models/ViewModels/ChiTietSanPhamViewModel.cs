namespace WebsiteBanPhuKien.Models.ViewModels
{
    public class ChiTietSanPhamViewModel
    {
        public PhuKien SanPham { get; set; } = null!;
        public IEnumerable<PhuKien> SanPhamCungLoai { get; set; } = new List<PhuKien>();
        public IEnumerable<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
        public double DiemTrungBinh { get; set; }
        public int TongDanhGia { get; set; }
    }
}