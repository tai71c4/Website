using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBanPhuKien.Models
{
    public class BinhLuan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MaTin { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public string NoiDung { get; set; } = string.Empty;
        public DateTime NgayDang { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("MaTin")]
        public TinTuc TinTuc { get; set; } = null!;

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;
    }
}