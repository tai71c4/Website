using System.ComponentModel.DataAnnotations;

namespace WebsiteBanPhuKien.Models
{
    public class LienHe
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(256)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? SoDienThoai { get; set; }

        public string NoiDung { get; set; } = string.Empty;
        public DateTime NgayGui { get; set; } = DateTime.Now;
    }
}