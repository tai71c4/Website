using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBanPhuKien.Models
{
    [Table("ChiTietDonHang")]
    public class ChiTietDonHang
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MaDon { get; set; }

        [Required]
        public int MaPhuKien { get; set; }

        [Required]
        public int SoLuong { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DonGia { get; set; }

        // Navigation properties
        [ForeignKey("MaDon")]
        public DonHang DonHang { get; set; } = null!;

        [ForeignKey("MaPhuKien")]
        public PhuKien PhuKien { get; set; } = null!;
    }
}