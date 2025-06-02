using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBanPhuKien.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Avatar { get; set; }

        public bool TrangThai { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
        public ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();
        public ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
    }
}