using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBanPhuKien.Models
{
    public class DanhGia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public int MaPhuKien { get; set; }

        [Range(1, 5)]
        public int SoSao { get; set; }

        public string BinhLuan { get; set; } = string.Empty;

        public DateTime NgayDanhGia { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;

        [ForeignKey("MaPhuKien")]
        public PhuKien PhuKien { get; set; } = null!;
    }
}