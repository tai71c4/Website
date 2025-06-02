using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBanPhuKien.Models
{
    [Table("PhuKien")]
    public class PhuKien
    {
        [Key]
        public int MaPhuKien { get; set; }

        [Required]
        [StringLength(200)]
        public string TenPhuKien { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Gia { get; set; }

        public string MoTa { get; set; } = string.Empty;
        public string HinhAnh { get; set; } = string.Empty;

        [Required]
        public int SoLuong { get; set; }

        [Required]
        public int MaLoai { get; set; }

        [Required]
        public int MaHang { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("MaLoai")]
        public LoaiPhuKien LoaiPhuKien { get; set; } = null!;

        [ForeignKey("MaHang")]
        public HangSanXuat HangSanXuat { get; set; } = null!;

        public ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();
        public ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
        public ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
    }
}