using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBanPhuKien.Models
{
    [Table("LoaiPhuKien")]
    public class LoaiPhuKien
    {
        [Key]
        public int MaLoai { get; set; }

        [Required]
        [StringLength(100)]
        public string TenLoai { get; set; } = string.Empty;

        public string MoTa { get; set; } = string.Empty;

        // Navigation property
        public ICollection<PhuKien> PhuKiens { get; set; } = new List<PhuKien>();
    }
}