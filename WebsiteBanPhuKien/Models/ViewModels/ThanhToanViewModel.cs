using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace WebsiteBanPhuKien.Models.ViewModels
{
    public class ThanhToanViewModel
    {
        public PhuKien? SanPham { get; set; }
        public DonHang DonHang { get; set; } = null!;
        public int SoLuong { get; set; }
        
        // Thêm thuộc tính cho giỏ hàng
        public List<GioHangViewModel>? GioHangItems { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        [Display(Name = "Phương thức thanh toán")]
        public string PhuongThucThanhToan { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Vui lòng nhập tên người nhận")]
        [Display(Name = "Tên người nhận")]
        public string TenNguoiNhan { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string SoDienThoai { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string DiaChiGiao { get; set; } = string.Empty;
        
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }
    }
}