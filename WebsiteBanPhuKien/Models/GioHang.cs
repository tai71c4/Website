using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBanPhuKien.Models
{
    [Table("GioHang")]
    public class GioHang
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public int MaPhuKien { get; set; }

        [Required]
        public int SoLuong { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;

        [ForeignKey("MaPhuKien")]
        public PhuKien PhuKien { get; set; } = null!;
    }
}