namespace WebsiteBanPhuKien.Models.ViewModels
{
    public class TinTucViewModel
    {
        public TinTuc TinTuc { get; set; } = null!;
        public List<BinhLuan> BinhLuans { get; set; } = new List<BinhLuan>();
        public List<TinTuc> TinTucLienQuan { get; set; } = new List<TinTuc>();
    }
}