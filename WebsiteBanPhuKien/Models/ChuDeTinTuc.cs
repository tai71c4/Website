using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBanPhuKien.Models
{
    [Table("ChuDe")]
    public class ChuDeTinTuc
    {
        [Key]
        public int MaChuDe { get; set; }

        [Required]
        [StringLength(100)]
        public string TenChuDe { get; set; } = string.Empty;

        public string MoTa { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public ICollection<TinTuc> TinTucs { get; set; } = new List<TinTuc>();
    }
}