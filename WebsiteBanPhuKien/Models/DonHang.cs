using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBanPhuKien.Models
{
    [Table("DonHang")]
    public class DonHang
    {
        [Key]
        public int MaDon { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public DateTime NgayDat { get; set; }

        public DateTime? NgayGiao { get; set; }

        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TongTien { get; set; }

        [Required]
        public bool ThanhToan { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;

        [ForeignKey("CreatedBy")]
        public ApplicationUser? CreatedByUser { get; set; }

        [ForeignKey("UpdatedBy")]
        public ApplicationUser? UpdatedByUser { get; set; }

        public ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
    }
}