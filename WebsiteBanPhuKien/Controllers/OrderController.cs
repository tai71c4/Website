using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteBanPhuKien.Models;
using WebsiteBanPhuKien.Models.ViewModels;
using System.Collections.Generic;

namespace WebsiteBanPhuKien.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<OrderController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [Authorize]
        public async Task<IActionResult> MuaNgay(int id)
        {
            try
            {
                // Lấy thông tin sản phẩm
                var sanPham = await _context.PhuKiens
                    .FromSqlRaw(@"
                        SELECT p.MaPhuKien, p.TenPhuKien, p.Gia, 
                               ISNULL(p.MoTa, '') as MoTa, 
                               ISNULL(p.HinhAnh, '') as HinhAnh, 
                               p.SoLuong, p.MaLoai, p.MaHang, 
                               p.CreatedAt, p.UpdatedAt
                        FROM PhuKien p
                        WHERE p.MaPhuKien = {0}", id)
                    .FirstOrDefaultAsync();

                if (sanPham == null)
                {
                    return NotFound();
                }

                // Kiểm tra số lượng
                if (sanPham.SoLuong <= 0)
                {
                    TempData["ErrorMessage"] = "Sản phẩm đã hết hàng.";
                    return RedirectToAction("ChiTiet", "Product", new { id });
                }

                // Lấy thông tin người dùng
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Challenge();
                }

                // Tạo đơn hàng tạm thời
                var donHang = new DonHang
                {
                    UserId = user.Id,
                    NgayDat = DateTime.Now,
                    TrangThai = "Chờ thanh toán",
                    TongTien = sanPham.Gia,
                    ThanhToan = false,
                    CreatedAt = DateTime.Now,
                    CreatedBy = user.Id
                };

                // Tạo chi tiết đơn hàng
                var chiTietDonHang = new ChiTietDonHang
                {
                    MaPhuKien = sanPham.MaPhuKien,
                    SoLuong = 1,
                    DonGia = sanPham.Gia
                };

                // Lưu vào session để sử dụng ở trang thanh toán
                HttpContext.Session.SetString("MuaNgay_SanPham", System.Text.Json.JsonSerializer.Serialize(sanPham));
                HttpContext.Session.SetString("MuaNgay_DonHang", System.Text.Json.JsonSerializer.Serialize(donHang));
                HttpContext.Session.SetString("MuaNgay_ChiTiet", System.Text.Json.JsonSerializer.Serialize(chiTietDonHang));

                // Lưu thông tin thanh toán vào session
                var thanhToanInfo = new ThanhToanInfo
                {
                    TenNguoiNhan = user.HoTen,
                    SoDienThoai = user.PhoneNumber ?? string.Empty,
                    DiaChiGiao = string.Empty,
                    GhiChu = string.Empty,
                    PhuongThucThanhToan = "MoMo"
                };
                HttpContext.Session.SetString("ThanhToanInfo", System.Text.Json.JsonSerializer.Serialize(thanhToanInfo));

                // Tạo view model cho trang thanh toán
                var viewModel = new ThanhToanViewModel
                {
                    DonHang = donHang,
                    SanPham = sanPham,
                    SoLuong = 1,
                    PhuongThucThanhToan = "MoMo",
                    TenNguoiNhan = user.HoTen,
                    SoDienThoai = user.PhoneNumber ?? string.Empty,
                    DiaChiGiao = string.Empty,
                    GhiChu = string.Empty
                };

                // Chuyển đến trang thanh toán
                return View("ThanhToan", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi mua ngay sản phẩm: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý yêu cầu.";
                return RedirectToAction("ChiTiet", "Product", new { id });
            }
        }

        [Authorize]
        public async Task<IActionResult> ThanhToan()
        {
            try
            {
                // Kiểm tra xem có phải từ giỏ hàng chuyển sang không
                var gioHangJson = HttpContext.Session.GetString("GioHang");
                if (!string.IsNullOrEmpty(gioHangJson))
                {
                    // Xử lý thanh toán từ giỏ hàng
                    var gioHangItems = System.Text.Json.JsonSerializer.Deserialize<List<Models.ViewModels.GioHangViewModel>>(gioHangJson);
                    if (gioHangItems != null && gioHangItems.Any())
                    {
                        // Lấy thông tin người dùng
                        var user = await _userManager.GetUserAsync(User);
                        if (user == null)
                        {
                            return Challenge();
                        }

                        // Tạo đơn hàng
                        var donHang = new DonHang
                        {
                            UserId = user.Id,
                            NgayDat = DateTime.Now,
                            TrangThai = "Chờ thanh toán",
                            TongTien = gioHangItems.Sum(i => i.ThanhTien),
                            ThanhToan = false,
                            CreatedAt = DateTime.Now,
                            CreatedBy = user.Id
                        };

                        // Lưu thông tin thanh toán vào session
                        var thanhToanInfo = new ThanhToanInfo
                        {
                            TenNguoiNhan = user.HoTen,
                            SoDienThoai = user.PhoneNumber ?? string.Empty,
                            DiaChiGiao = string.Empty,
                            GhiChu = string.Empty,
                            PhuongThucThanhToan = "MoMo"
                        };
                        HttpContext.Session.SetString("ThanhToanInfo", System.Text.Json.JsonSerializer.Serialize(thanhToanInfo));
                        HttpContext.Session.SetString("DonHang_GioHang", System.Text.Json.JsonSerializer.Serialize(donHang));

                        // Tạo view model cho trang thanh toán
                        var viewModel = new ThanhToanViewModel
                        {
                            DonHang = donHang,
                            GioHangItems = gioHangItems,
                            PhuongThucThanhToan = "MoMo",
                            TenNguoiNhan = user.HoTen,
                            SoDienThoai = user.PhoneNumber ?? string.Empty,
                            DiaChiGiao = string.Empty,
                            GhiChu = string.Empty
                        };

                        return View(viewModel);
                    }
                }

                // Nếu không phải từ giỏ hàng, xử lý như cũ (mua ngay)
                var sanPhamJson = HttpContext.Session.GetString("MuaNgay_SanPham");
                var donHangJson = HttpContext.Session.GetString("MuaNgay_DonHang");
                var chiTietJson = HttpContext.Session.GetString("MuaNgay_ChiTiet");
                var thanhToanInfoJson = HttpContext.Session.GetString("ThanhToanInfo");

                if (string.IsNullOrEmpty(sanPhamJson) || string.IsNullOrEmpty(donHangJson) || string.IsNullOrEmpty(chiTietJson))
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin đơn hàng. Vui lòng kiểm tra giỏ hàng của bạn.";
                    return RedirectToAction("Index", "Cart");
                }

                var sanPham = System.Text.Json.JsonSerializer.Deserialize<PhuKien>(sanPhamJson);
                var donHangMuaNgay = System.Text.Json.JsonSerializer.Deserialize<DonHang>(donHangJson);
                var chiTietMuaNgay = System.Text.Json.JsonSerializer.Deserialize<ChiTietDonHang>(chiTietJson);
                var thanhToanInfoMuaNgay = string.IsNullOrEmpty(thanhToanInfoJson) 
                    ? new ThanhToanInfo() 
                    : System.Text.Json.JsonSerializer.Deserialize<ThanhToanInfo>(thanhToanInfoJson);

                if (sanPham == null || donHangMuaNgay == null || chiTietMuaNgay == null || thanhToanInfoMuaNgay == null)
                {
                    TempData["ErrorMessage"] = "Lỗi khi xử lý thông tin đơn hàng.";
                    return RedirectToAction("Index", "Home");
                }

                var viewModelMuaNgay = new ThanhToanViewModel
                {
                    DonHang = donHangMuaNgay,
                    SanPham = sanPham,
                    SoLuong = chiTietMuaNgay.SoLuong,
                    PhuongThucThanhToan = thanhToanInfoMuaNgay.PhuongThucThanhToan,
                    TenNguoiNhan = thanhToanInfoMuaNgay.TenNguoiNhan,
                    SoDienThoai = thanhToanInfoMuaNgay.SoDienThoai,
                    DiaChiGiao = thanhToanInfoMuaNgay.DiaChiGiao,
                    GhiChu = thanhToanInfoMuaNgay.GhiChu
                };

                return View(viewModelMuaNgay);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hiển thị trang thanh toán: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý yêu cầu.";
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanThanhToan(ThanhToanViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("ThanhToan", model);
                }

                // Lưu thông tin thanh toán vào session
                var thanhToanInfo = new ThanhToanInfo
                {
                    TenNguoiNhan = model.TenNguoiNhan,
                    SoDienThoai = model.SoDienThoai,
                    DiaChiGiao = model.DiaChiGiao,
                    GhiChu = model.GhiChu,
                    PhuongThucThanhToan = model.PhuongThucThanhToan
                };
                HttpContext.Session.SetString("ThanhToanInfo", System.Text.Json.JsonSerializer.Serialize(thanhToanInfo));

                // Kiểm tra xem có phải từ giỏ hàng chuyển sang không
                var gioHangJson = HttpContext.Session.GetString("GioHang");
                if (!string.IsNullOrEmpty(gioHangJson))
                {
                    // Xử lý thanh toán từ giỏ hàng
                    var gioHangItems = System.Text.Json.JsonSerializer.Deserialize<List<Models.ViewModels.GioHangViewModel>>(gioHangJson);
                    if (gioHangItems != null && gioHangItems.Any())
                    {
                        // Lấy thông tin người dùng
                        var user = await _userManager.GetUserAsync(User);
                        if (user == null)
                        {
                            return Challenge();
                        }

                        // Tạo đơn hàng
                        var donHang = new DonHang
                        {
                            UserId = user.Id,
                            NgayDat = DateTime.Now,
                            TrangThai = "Chờ thanh toán",
                            TongTien = gioHangItems.Sum(i => i.ThanhTien),
                            ThanhToan = false,
                            CreatedAt = DateTime.Now,
                            CreatedBy = user.Id
                        };

                        // Lưu đơn hàng vào cơ sở dữ liệu
                        _context.DonHangs.Add(donHang);
                        await _context.SaveChangesAsync();

                        // Lưu chi tiết đơn hàng
                        foreach (var item in gioHangItems)
                        {
                            var chiTietItem = new ChiTietDonHang
                            {
                                MaDon = donHang.MaDon,
                                MaPhuKien = item.MaPhuKien,
                                SoLuong = item.SoLuong,
                                DonGia = item.Gia
                            };
                            _context.ChiTietDonHangs.Add(chiTietItem);

                            // Cập nhật số lượng sản phẩm
                            var phuKienItem = await _context.PhuKiens.FindAsync(item.MaPhuKien);
                            if (phuKienItem != null)
                            {
                                phuKienItem.SoLuong -= item.SoLuong;
                                _context.PhuKiens.Update(phuKienItem);
                            }
                        }

                        await _context.SaveChangesAsync();

                        // Xóa giỏ hàng
                        if (User.Identity != null && User.Identity.IsAuthenticated)
                        {
                            var gioHangDb = await _context.GioHangs.Where(g => g.UserId == user.Id).ToListAsync();
                            _context.GioHangs.RemoveRange(gioHangDb);
                            await _context.SaveChangesAsync();
                        }
                        HttpContext.Session.Remove("GioHang");
                        HttpContext.Session.Remove("Cart");

                        // Chuyển đến trang xác nhận thanh toán
                        return RedirectToAction("ThanhToanThanhCong", new { id = donHang.MaDon });
                    }
                }

                // Nếu không phải từ giỏ hàng, xử lý như cũ (mua ngay)
                var sanPhamJson = HttpContext.Session.GetString("MuaNgay_SanPham");
                var donHangJson = HttpContext.Session.GetString("MuaNgay_DonHang");
                var chiTietJson = HttpContext.Session.GetString("MuaNgay_ChiTiet");

                if (string.IsNullOrEmpty(sanPhamJson) || string.IsNullOrEmpty(donHangJson) || string.IsNullOrEmpty(chiTietJson))
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin đơn hàng.";
                    return RedirectToAction("Index", "Home");
                }

                var sanPham = System.Text.Json.JsonSerializer.Deserialize<PhuKien>(sanPhamJson);
                var donHangXacNhan = System.Text.Json.JsonSerializer.Deserialize<DonHang>(donHangJson);
                var chiTietXacNhan = System.Text.Json.JsonSerializer.Deserialize<ChiTietDonHang>(chiTietJson);

                if (sanPham == null || donHangXacNhan == null || chiTietXacNhan == null)
                {
                    TempData["ErrorMessage"] = "Lỗi khi xử lý thông tin đơn hàng.";
                    return RedirectToAction("Index", "Home");
                }

                // Lưu đơn hàng vào cơ sở dữ liệu
                _context.DonHangs.Add(donHangXacNhan);
                await _context.SaveChangesAsync();

                // Lưu chi tiết đơn hàng
                chiTietXacNhan.MaDon = donHangXacNhan.MaDon;
                _context.ChiTietDonHangs.Add(chiTietXacNhan);

                // Cập nhật số lượng sản phẩm
                var sanPhamDb = await _context.PhuKiens.FindAsync(sanPham.MaPhuKien);
                if (sanPhamDb != null)
                {
                    sanPhamDb.SoLuong -= chiTietXacNhan.SoLuong;
                    _context.PhuKiens.Update(sanPhamDb);
                }

                await _context.SaveChangesAsync();

                // Xóa session
                HttpContext.Session.Remove("MuaNgay_SanPham");
                HttpContext.Session.Remove("MuaNgay_DonHang");
                HttpContext.Session.Remove("MuaNgay_ChiTiet");

                // Chuyển đến trang xác nhận thanh toán
                return RedirectToAction("ThanhToanThanhCong", new { id = donHangXacNhan.MaDon });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xác nhận thanh toán: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi xử lý thanh toán.");
                return View("ThanhToan", model);
            }
        }

        [Authorize]
        public async Task<IActionResult> ThanhToanThanhCong(int id)
        {
            try
            {
                var donHang = await _context.DonHangs
                    .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.PhuKien)
                    .FirstOrDefaultAsync(d => d.MaDon == id);

                if (donHang == null)
                {
                    return NotFound();
                }

                // Lấy thông tin thanh toán từ session
                var thanhToanInfoJson = HttpContext.Session.GetString("ThanhToanInfo");
                var thanhToanInfo = string.IsNullOrEmpty(thanhToanInfoJson)
                    ? new ThanhToanInfo()
                    : System.Text.Json.JsonSerializer.Deserialize<ThanhToanInfo>(thanhToanInfoJson);

                if (thanhToanInfo == null)
                {
                    thanhToanInfo = new ThanhToanInfo();
                }

                var viewModel = new DonHangViewModel
                {
                    DonHang = donHang,
                    TenNguoiNhan = thanhToanInfo.TenNguoiNhan,
                    SoDienThoai = thanhToanInfo.SoDienThoai,
                    DiaChiGiao = thanhToanInfo.DiaChiGiao,
                    GhiChu = thanhToanInfo.GhiChu,
                    PhuongThucThanhToan = thanhToanInfo.PhuongThucThanhToan
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hiển thị trang thanh toán thành công: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý yêu cầu.";
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize]
        public async Task<IActionResult> LichSu()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var donHangs = await _context.DonHangs
                .Where(d => d.UserId == user.Id)
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();

            return View(donHangs);
        }

        [Authorize]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(c => c.PhuKien)
                .FirstOrDefaultAsync(d => d.MaDon == id && d.UserId == user.Id);

            if (donHang == null)
            {
                return NotFound();
            }

            return View(donHang);
        }
    }

    // Class để lưu thông tin thanh toán trong session
    public class ThanhToanInfo
    {
        public string TenNguoiNhan { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string DiaChiGiao { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
        public string PhuongThucThanhToan { get; set; } = string.Empty;
    }
}