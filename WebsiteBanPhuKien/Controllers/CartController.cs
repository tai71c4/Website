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

namespace WebsiteBanPhuKien.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CartController> _logger;

        public CartController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<CartController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> ThemVaoGioHang(int maPhuKien, int soLuong = 1)
        {
            try
            {
                // Kiểm tra sản phẩm
                var sanPham = await _context.PhuKiens.FindAsync(maPhuKien);
                if (sanPham == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                    return RedirectToAction("DanhSach", "Product");
                }

                // Kiểm tra số lượng
                if (sanPham.SoLuong < soLuong)
                {
                    TempData["ErrorMessage"] = "Số lượng sản phẩm không đủ.";
                    return RedirectToAction("ChiTiet", "Product", new { id = maPhuKien });
                }

                // Nếu người dùng đã đăng nhập, lưu vào database
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        try
                        {
                            // Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa
                            var gioHang = await _context.GioHangs
                                .FirstOrDefaultAsync(g => g.UserId == user.Id && g.MaPhuKien == maPhuKien);

                            if (gioHang != null)
                            {
                                // Nếu đã có, cập nhật số lượng
                                gioHang.SoLuong += soLuong;
                                _context.Update(gioHang);
                            }
                            else
                            {
                                // Nếu chưa có, thêm mới
                                gioHang = new GioHang
                                {
                                    UserId = user.Id,
                                    MaPhuKien = maPhuKien,
                                    SoLuong = soLuong
                                };
                                _context.GioHangs.Add(gioHang);
                            }

                            await _context.SaveChangesAsync();
                            TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng.";
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Lỗi khi thêm vào giỏ hàng (DB): {Message}", ex.Message);
                            TempData["ErrorMessage"] = "Có lỗi xảy ra khi thêm sản phẩm vào giỏ hàng.";
                            return RedirectToAction("ChiTiet", "Product", new { id = maPhuKien });
                        }
                    }
                }
                else
                {
                    try
                    {
                        // Nếu chưa đăng nhập, lưu vào session
                        var cart = HttpContext.Session.GetString("Cart");
                        var cartItems = string.IsNullOrEmpty(cart)
                            ? new List<CartItem>()
                            : System.Text.Json.JsonSerializer.Deserialize<List<CartItem>>(cart) ?? new List<CartItem>();

                        var existingItem = cartItems.FirstOrDefault(i => i.MaPhuKien == maPhuKien);
                        if (existingItem != null)
                        {
                            existingItem.SoLuong += soLuong;
                        }
                        else
                        {
                            cartItems.Add(new CartItem
                            {
                                MaPhuKien = maPhuKien,
                                SoLuong = soLuong
                            });
                        }

                        HttpContext.Session.SetString("Cart", System.Text.Json.JsonSerializer.Serialize(cartItems));
                        TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng.";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Lỗi khi thêm vào giỏ hàng (Session): {Message}", ex.Message);
                        TempData["ErrorMessage"] = "Có lỗi xảy ra khi thêm sản phẩm vào giỏ hàng.";
                        return RedirectToAction("ChiTiet", "Product", new { id = maPhuKien });
                    }
                }
                
                // Chuyển hướng đến trang giỏ hàng
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm sản phẩm vào giỏ hàng: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi thêm sản phẩm vào giỏ hàng.";
                return RedirectToAction("ChiTiet", "Product", new { id = maPhuKien });
            }
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                List<Models.ViewModels.GioHangViewModel> gioHangItems = new List<Models.ViewModels.GioHangViewModel>();

                // Nếu người dùng đã đăng nhập, lấy giỏ hàng từ database
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        try
                        {
                            var items = await _context.GioHangs
                                .Where(g => g.UserId == user.Id)
                                .ToListAsync();

                            foreach (var item in items)
                            {
                                var sanPham = await _context.PhuKiens.FindAsync(item.MaPhuKien);
                                if (sanPham != null)
                                {
                                    gioHangItems.Add(new Models.ViewModels.GioHangViewModel
                                    {
                                        MaPhuKien = sanPham.MaPhuKien,
                                        TenPhuKien = sanPham.TenPhuKien,
                                        HinhAnh = sanPham.HinhAnh ?? string.Empty,
                                        Gia = sanPham.Gia,
                                        SoLuong = item.SoLuong,
                                        ThanhTien = sanPham.Gia * item.SoLuong
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Lỗi khi lấy giỏ hàng từ DB: {Message}", ex.Message);
                        }
                    }
                }
                else
                {
                    // Nếu chưa đăng nhập, lấy giỏ hàng từ session
                    var cart = HttpContext.Session.GetString("Cart");
                    if (!string.IsNullOrEmpty(cart))
                    {
                        try
                        {
                            var cartItems = System.Text.Json.JsonSerializer.Deserialize<List<CartItem>>(cart);
                            if (cartItems != null)
                            {
                                foreach (var item in cartItems)
                                {
                                    var sanPham = await _context.PhuKiens.FindAsync(item.MaPhuKien);
                                    if (sanPham != null)
                                    {
                                        gioHangItems.Add(new Models.ViewModels.GioHangViewModel
                                        {
                                            MaPhuKien = sanPham.MaPhuKien,
                                            TenPhuKien = sanPham.TenPhuKien,
                                            HinhAnh = sanPham.HinhAnh ?? string.Empty,
                                            Gia = sanPham.Gia,
                                            SoLuong = item.SoLuong,
                                            ThanhTien = sanPham.Gia * item.SoLuong
                                        });
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Lỗi khi lấy giỏ hàng từ Session: {Message}", ex.Message);
                        }
                    }
                }

                // Lưu thông tin giỏ hàng vào session để sử dụng ở trang thanh toán
                if (gioHangItems.Any())
                {
                    HttpContext.Session.SetString("GioHang", System.Text.Json.JsonSerializer.Serialize(gioHangItems));
                }

                return View(gioHangItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hiển thị giỏ hàng: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi hiển thị giỏ hàng.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CapNhatGioHang(int maPhuKien, int soLuong)
        {
            try
            {
                if (soLuong <= 0)
                {
                    return await XoaKhoiGioHang(maPhuKien);
                }

                // Kiểm tra sản phẩm
                var sanPham = await _context.PhuKiens.FindAsync(maPhuKien);
                if (sanPham == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                    return RedirectToAction("Index");
                }

                // Kiểm tra số lượng
                if (sanPham.SoLuong < soLuong)
                {
                    TempData["ErrorMessage"] = "Số lượng sản phẩm không đủ.";
                    return RedirectToAction("Index");
                }

                // Nếu người dùng đã đăng nhập, cập nhật trong database
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        var gioHang = await _context.GioHangs
                            .FirstOrDefaultAsync(g => g.UserId == user.Id && g.MaPhuKien == maPhuKien);

                        if (gioHang != null)
                        {
                            gioHang.SoLuong = soLuong;
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    // Nếu chưa đăng nhập, cập nhật trong session
                    var cart = HttpContext.Session.GetString("Cart");
                    if (!string.IsNullOrEmpty(cart))
                    {
                        var cartItems = System.Text.Json.JsonSerializer.Deserialize<List<CartItem>>(cart);
                        if (cartItems != null)
                        {
                            var item = cartItems.FirstOrDefault(i => i.MaPhuKien == maPhuKien);
                            if (item != null)
                            {
                                item.SoLuong = soLuong;
                                HttpContext.Session.SetString("Cart", System.Text.Json.JsonSerializer.Serialize(cartItems));
                            }
                        }
                    }
                }

                TempData["SuccessMessage"] = "Đã cập nhật giỏ hàng.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật giỏ hàng: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật giỏ hàng.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> XoaKhoiGioHang(int maPhuKien)
        {
            try
            {
                // Nếu người dùng đã đăng nhập, xóa từ database
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        var gioHang = await _context.GioHangs
                            .FirstOrDefaultAsync(g => g.UserId == user.Id && g.MaPhuKien == maPhuKien);

                        if (gioHang != null)
                        {
                            _context.GioHangs.Remove(gioHang);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    // Nếu chưa đăng nhập, xóa từ session
                    var cart = HttpContext.Session.GetString("Cart");
                    if (!string.IsNullOrEmpty(cart))
                    {
                        var cartItems = System.Text.Json.JsonSerializer.Deserialize<List<CartItem>>(cart);
                        if (cartItems != null)
                        {
                            cartItems.RemoveAll(i => i.MaPhuKien == maPhuKien);
                            HttpContext.Session.SetString("Cart", System.Text.Json.JsonSerializer.Serialize(cartItems));
                        }
                    }
                }

                TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa sản phẩm khỏi giỏ hàng: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa sản phẩm khỏi giỏ hàng.";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> XoaGioHang()
        {
            try
            {
                // Nếu người dùng đã đăng nhập, xóa từ database
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        var gioHangItems = await _context.GioHangs
                            .Where(g => g.UserId == user.Id)
                            .ToListAsync();

                        _context.GioHangs.RemoveRange(gioHangItems);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    // Nếu chưa đăng nhập, xóa từ session
                    HttpContext.Session.Remove("Cart");
                }

                TempData["SuccessMessage"] = "Đã xóa tất cả sản phẩm trong giỏ hàng.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa giỏ hàng: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa giỏ hàng.";
                return RedirectToAction("Index");
            }
        }
    }

    // Class để lưu thông tin giỏ hàng trong session
    public class CartItem
    {
        public int MaPhuKien { get; set; }
        public int SoLuong { get; set; }
    }
}