using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBanPhuKien.Models
{
    [Table("TinTuc")]
    public class TinTuc
    {
        [Key]
        public int MaTin { get; set; }

        [Required]
        [StringLength(200)]
        public string TieuDe { get; set; } = string.Empty;

        public string NoiDung { get; set; } = string.Empty;
        public string HinhAnh { get; set; } = string.Empty;
        public DateTime NgayDang { get; set; } = DateTime.Now;
        public int? MaChuDe { get; set; }

        // Navigation properties
        [ForeignKey("MaChuDe")]
        public ChuDeTinTuc? ChuDe { get; set; }

        public ICollection<BinhLuan> BinhLuans { get; set; } = new List<BinhLuan>();
    }
}