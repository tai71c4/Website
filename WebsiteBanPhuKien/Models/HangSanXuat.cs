using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBanPhuKien.Models
{
    [Table("Hang")]
    public class HangSanXuat
    {
        [Key]
        public int MaHang { get; set; }

        [Required]
        [StringLength(100)]
        public string TenHang { get; set; } = string.Empty;

        public string MoTa { get; set; } = string.Empty;

        // Navigation property
        public ICollection<PhuKien> PhuKiens { get; set; } = new List<PhuKien>();
    }
}