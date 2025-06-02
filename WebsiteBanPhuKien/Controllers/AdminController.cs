using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebsiteBanPhuKien.Models;

namespace WebsiteBanPhuKien.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public AdminController(
            AppDbContext context, 
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        public async Task<IActionResult> Dashboard()
        {
            int totalUsers = 0;
            int totalProducts = 0;
            int totalOrders = 0;
            decimal totalRevenue = 0;
            List<DonHang> recentOrders = new List<DonHang>();

            try
            {
                totalUsers = await _context.Users.CountAsync();
            }
            catch (Exception) { }

            try
            {
                totalProducts = await _context.PhuKiens.CountAsync();
            }
            catch (Exception) { }

            try
            {
                totalOrders = await _context.DonHangs.CountAsync();
            }
            catch (Exception) { }

            try
            {
                var completedOrders = await _context.DonHangs
                    .Where(d => d.TrangThai == "Đã giao")
                    .ToListAsync();

                if (completedOrders.Any())
                {
                    totalRevenue = completedOrders.Sum(d => d.TongTien);
                }
            }
            catch (Exception) { }

            try
            {
                recentOrders = await _context.DonHangs
                    .Include(d => d.User)
                    .OrderByDescending(d => d.NgayDat)
                    .Take(5)
                    .ToListAsync();
            }
            catch (Exception) { }

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.RecentOrders = recentOrders;

            return View();
        }

        public async Task<IActionResult> QuanLyNguoiDung()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> QuanLySanPham()
        {
            List<PhuKien> products = new List<PhuKien>();
            
            try
            {
                // Sử dụng SQL thuần để tránh lỗi SqlNullValueException
                string sql = @"
                    SELECT p.MaPhuKien, p.TenPhuKien, p.Gia, 
                           ISNULL(p.MoTa, '') as MoTa, 
                           ISNULL(p.HinhAnh, '') as HinhAnh, 
                           p.SoLuong, p.MaLoai, p.MaHang, 
                           p.CreatedAt, p.UpdatedAt
                    FROM PhuKien p";
                
                // Kiểm tra xem có sản phẩm nào trong cơ sở dữ liệu không
                var productCount = await _context.PhuKiens.CountAsync();
                Console.WriteLine($"Số lượng sản phẩm trong cơ sở dữ liệu: {productCount}");
                
                // Thử lấy dữ liệu sử dụng SQL thuần để tránh lỗi
                products = await _context.PhuKiens.FromSqlRaw(sql).ToListAsync();
                Console.WriteLine($"Đã lấy được {products.Count} sản phẩm từ cơ sở dữ liệu");
                
                // Tải thông tin loại và hãng riêng biệt
                foreach (var item in products)
                {
                    var loai = await _context.LoaiPhuKiens.FindAsync(item.MaLoai);
                    var hang = await _context.HangSanXuats.FindAsync(item.MaHang);
                    if (loai != null) item.LoaiPhuKien = loai;
                    if (hang != null) item.HangSanXuat = hang;
                    
                    // Đảm bảo các thuộc tính string không null
                    item.TenPhuKien ??= string.Empty;
                    item.MoTa ??= string.Empty;
                    item.HinhAnh ??= string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy danh sách sản phẩm: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                // Nếu có lỗi, trả về danh sách rỗng
            }

            // Tạo dữ liệu mẫu nếu chưa có
            if (!await _context.LoaiPhuKiens.AnyAsync())
            {
                var loaiPhuKiens = new List<LoaiPhuKien>
                {
                    new LoaiPhuKien { TenLoai = "Ốp lưng", MoTa = "Ốp lưng điện thoại" },
                    new LoaiPhuKien { TenLoai = "Tai nghe", MoTa = "Tai nghe có dây và không dây" },
                    new LoaiPhuKien { TenLoai = "Sạc dự phòng", MoTa = "Pin dự phòng" },
                    new LoaiPhuKien { TenLoai = "Cáp sạc", MoTa = "Cáp sạc các loại" }
                };
                _context.LoaiPhuKiens.AddRange(loaiPhuKiens);
                await _context.SaveChangesAsync();
            }

            if (!await _context.HangSanXuats.AnyAsync())
            {
                var hangSanXuats = new List<HangSanXuat>
                {
                    new HangSanXuat { TenHang = "Apple", MoTa = "Phụ kiện Apple" },
                    new HangSanXuat { TenHang = "Samsung", MoTa = "Phụ kiện Samsung" },
                    new HangSanXuat { TenHang = "Xiaomi", MoTa = "Phụ kiện Xiaomi" },
                    new HangSanXuat { TenHang = "Anker", MoTa = "Phụ kiện Anker" }
                };
                _context.HangSanXuats.AddRange(hangSanXuats);
                await _context.SaveChangesAsync();
            }

            ViewBag.LoaiPhuKiens = await _context.LoaiPhuKiens.ToListAsync();
            ViewBag.HangSanXuats = await _context.HangSanXuats.ToListAsync();

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> GetSanPham(int id)
        {
            try
            {
                // Sử dụng SQL thuần để tránh lỗi SqlNullValueException
                string sql = @"
                    SELECT p.MaPhuKien, p.TenPhuKien, p.Gia, 
                           ISNULL(p.MoTa, '') as MoTa, 
                           ISNULL(p.HinhAnh, '') as HinhAnh, 
                           p.SoLuong, p.MaLoai, p.MaHang, 
                           p.CreatedAt, p.UpdatedAt
                    FROM PhuKien p
                    WHERE p.MaPhuKien = {0}";
                
                var product = await _context.PhuKiens
                    .FromSqlRaw(sql, id)
                    .FirstOrDefaultAsync();
                
                if (product == null)
                {
                    return NotFound();
                }
                
                // Đảm bảo các thuộc tính string không null
                product.TenPhuKien ??= string.Empty;
                product.MoTa ??= string.Empty;
                product.HinhAnh ??= string.Empty;
                
                return Json(product);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemSanPham(PhuKien phuKien, IFormFile ImageFile)
        {
            try
            {
                // Kiểm tra xem MaLoai và MaHang có tồn tại không
                var loaiExists = await _context.LoaiPhuKiens.AnyAsync(l => l.MaLoai == phuKien.MaLoai);
                var hangExists = await _context.HangSanXuats.AnyAsync(h => h.MaHang == phuKien.MaHang);

                if (!loaiExists || !hangExists)
                {
                    if (!loaiExists)
                        ModelState.AddModelError("MaLoai", "Loại phụ kiện không tồn tại");
                    if (!hangExists)
                        ModelState.AddModelError("MaHang", "Hãng sản xuất không tồn tại");

                    ViewBag.LoaiPhuKiens = await _context.LoaiPhuKiens.ToListAsync();
                    ViewBag.HangSanXuats = await _context.HangSanXuats.ToListAsync();
                    return View("QuanLySanPham");
                }

                // Xử lý tải lên hình ảnh nếu có
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(ImageFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }

                    phuKien.HinhAnh = $"/uploads/{uniqueFileName}";
                }

                phuKien.CreatedAt = DateTime.Now;
                _context.PhuKiens.Add(phuKien);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm sản phẩm thành công.";
                return RedirectToAction(nameof(QuanLySanPham));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi: {ex.Message}");
                ViewBag.LoaiPhuKiens = await _context.LoaiPhuKiens.ToListAsync();
                ViewBag.HangSanXuats = await _context.HangSanXuats.ToListAsync();
                return View("QuanLySanPham");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaSanPham(PhuKien phuKien, IFormFile ImageFile)
        {
            try
            {
                var existingProduct = await _context.PhuKiens.FindAsync(phuKien.MaPhuKien);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                // Kiểm tra xem MaLoai và MaHang có tồn tại không
                var loaiExists = await _context.LoaiPhuKiens.AnyAsync(l => l.MaLoai == phuKien.MaLoai);
                var hangExists = await _context.HangSanXuats.AnyAsync(h => h.MaHang == phuKien.MaHang);

                if (!loaiExists || !hangExists)
                {
                    if (!loaiExists)
                        ModelState.AddModelError("MaLoai", "Loại phụ kiện không tồn tại");
                    if (!hangExists)
                        ModelState.AddModelError("MaHang", "Hãng sản xuất không tồn tại");

                    ViewBag.LoaiPhuKiens = await _context.LoaiPhuKiens.ToListAsync();
                    ViewBag.HangSanXuats = await _context.HangSanXuats.ToListAsync();
                    return View("QuanLySanPham");
                }

                // Xử lý tải lên hình ảnh nếu có
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(ImageFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }

                    // Xóa hình ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(existingProduct.HinhAnh) && existingProduct.HinhAnh.StartsWith("/uploads/"))
                    {
                        var oldImagePath = Path.Combine(_environment.WebRootPath, existingProduct.HinhAnh.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    phuKien.HinhAnh = $"/uploads/{uniqueFileName}";
                }
                else
                {
                    // Giữ nguyên hình ảnh cũ nếu không có hình mới
                    phuKien.HinhAnh = existingProduct.HinhAnh;
                }

                existingProduct.TenPhuKien = phuKien.TenPhuKien;
                existingProduct.Gia = phuKien.Gia;
                existingProduct.MoTa = phuKien.MoTa;
                existingProduct.HinhAnh = phuKien.HinhAnh;
                existingProduct.SoLuong = phuKien.SoLuong;
                existingProduct.MaLoai = phuKien.MaLoai;
                existingProduct.MaHang = phuKien.MaHang;
                existingProduct.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công.";
                return RedirectToAction(nameof(QuanLySanPham));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi: {ex.Message}");
                ViewBag.LoaiPhuKiens = await _context.LoaiPhuKiens.ToListAsync();
                ViewBag.HangSanXuats = await _context.HangSanXuats.ToListAsync();
                return View("QuanLySanPham");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaSanPham(int id)
        {
            try
            {
                // Kiểm tra xem sản phẩm có liên kết với ChiTietDonHang không
                var hasOrders = await _context.ChiTietDonHangs.AnyAsync(c => c.MaPhuKien == id);
                if (hasOrders)
                {
                    TempData["ErrorMessage"] = "Không thể xóa sản phẩm này vì đã có trong đơn hàng.";
                    return RedirectToAction(nameof(QuanLySanPham));
                }

                // Xóa các mục giỏ hàng liên quan
                var cartItems = await _context.GioHangs.Where(g => g.MaPhuKien == id).ToListAsync();
                if (cartItems.Any())
                {
                    _context.GioHangs.RemoveRange(cartItems);
                    await _context.SaveChangesAsync();
                }

                // Xóa các đánh giá liên quan
                var reviews = await _context.DanhGias.Where(d => d.MaPhuKien == id).ToListAsync();
                if (reviews.Any())
                {
                    _context.DanhGias.RemoveRange(reviews);
                    await _context.SaveChangesAsync();
                }

                // Lấy thông tin hình ảnh trước khi xóa sản phẩm
                var imagePath = await _context.PhuKiens
                    .Where(p => p.MaPhuKien == id)
                    .Select(p => p.HinhAnh)
                    .FirstOrDefaultAsync();

                // Xóa sản phẩm bằng SQL trực tiếp để tránh lỗi
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM PhuKien WHERE MaPhuKien = {0}", id);

                // Xóa hình ảnh nếu có
                if (!string.IsNullOrEmpty(imagePath) && imagePath.StartsWith("/uploads/"))
                {
                    var fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }

                TempData["SuccessMessage"] = "Xóa sản phẩm thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa sản phẩm: {ex.Message}";
            }
            
            return RedirectToAction(nameof(QuanLySanPham));
        }

        public async Task<IActionResult> QuanLyDonHang()
        {
            List<DonHang> orders = new List<DonHang>();
            
            try
            {
                orders = await _context.DonHangs
                    .Include(d => d.User)
                    .OrderByDescending(d => d.NgayDat)
                    .ToListAsync();
            }
            catch (Exception)
            {
                // Nếu có lỗi, trả về danh sách rỗng
            }

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> GetDonHang(int id)
        {
            try
            {
                var donHang = await _context.DonHangs
                    .Include(d => d.User)
                    .Include(d => d.ChiTietDonHangs)
                        .ThenInclude(c => c.PhuKien)
                    .FirstOrDefaultAsync(d => d.MaDon == id);

                if (donHang == null)
                {
                    return NotFound();
                }

                return Json(donHang);
            }
            catch (Exception)
            {
                return Json(new { error = "Không thể lấy thông tin đơn hàng" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTrangThaiDonHang(int id, string trangThai)
        {
            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            donHang.TrangThai = trangThai;
            donHang.UpdatedAt = DateTime.Now;
            donHang.UpdatedBy = user.Id;

            if (trangThai == "Đã giao")
            {
                donHang.NgayGiao = DateTime.Now;
                donHang.ThanhToan = true;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật trạng thái đơn hàng thành công.";
            return RedirectToAction(nameof(QuanLyDonHang));
        }

        public async Task<IActionResult> QuanLyTinTuc()
        {
            List<TinTuc> tinTucs = new List<TinTuc>();
            
            try
            {
                tinTucs = await _context.TinTucs
                    .Include(t => t.ChuDe)
                    .OrderByDescending(t => t.NgayDang)
                    .ToListAsync();
            }
            catch (Exception)
            {
                // Nếu có lỗi, trả về danh sách rỗng
            }

            // Tạo dữ liệu mẫu nếu chưa có
            if (!await _context.ChuDeTinTucs.AnyAsync())
            {
                var chuDeTinTucs = new List<ChuDeTinTuc>
                {
                    new ChuDeTinTuc { TenChuDe = "Tin công nghệ", MoTa = "Tin tức về công nghệ mới" },
                    new ChuDeTinTuc { TenChuDe = "Đánh giá sản phẩm", MoTa = "Đánh giá chi tiết sản phẩm" },
                    new ChuDeTinTuc { TenChuDe = "Mẹo hay", MoTa = "Mẹo hay khi sử dụng thiết bị" },
                    new ChuDeTinTuc { TenChuDe = "Khuyến mãi", MoTa = "Thông tin khuyến mãi" }
                };
                _context.ChuDeTinTucs.AddRange(chuDeTinTucs);
                await _context.SaveChangesAsync();
            }

            ViewBag.ChuDeTinTucs = await _context.ChuDeTinTucs.ToListAsync();

            return View(tinTucs);
        }

        [HttpGet]
        public async Task<IActionResult> GetTinTuc(int id)
        {
            try
            {
                var tinTuc = await _context.TinTucs.FindAsync(id);
                if (tinTuc == null)
                {
                    return NotFound();
                }
                return Json(tinTuc);
            }
            catch (Exception)
            {
                return Json(new { error = "Không thể lấy thông tin tin tức" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemTinTuc(TinTuc tinTuc, IFormFile ImageFile)
        {
            try
            {
                // Kiểm tra xem MaChuDe có tồn tại không
                var chuDeExists = await _context.ChuDeTinTucs.AnyAsync(c => c.MaChuDe == tinTuc.MaChuDe);
                if (!chuDeExists)
                {
                    ModelState.AddModelError("MaChuDe", "Chủ đề không tồn tại");
                    ViewBag.ChuDeTinTucs = await _context.ChuDeTinTucs.ToListAsync();
                    return View("QuanLyTinTuc");
                }

                // Xử lý tải lên hình ảnh nếu có
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(ImageFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }

                    tinTuc.HinhAnh = $"/uploads/{uniqueFileName}";
                }

                tinTuc.NgayDang = DateTime.Now;
                _context.TinTucs.Add(tinTuc);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm tin tức thành công.";
                return RedirectToAction(nameof(QuanLyTinTuc));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi: {ex.Message}");
                ViewBag.ChuDeTinTucs = await _context.ChuDeTinTucs.ToListAsync();
                return View("QuanLyTinTuc");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaTinTuc(TinTuc tinTuc, IFormFile ImageFile)
        {
            try
            {
                var existingTinTuc = await _context.TinTucs.FindAsync(tinTuc.MaTin);
                if (existingTinTuc == null)
                {
                    return NotFound();
                }

                // Kiểm tra xem MaChuDe có tồn tại không
                var chuDeExists = await _context.ChuDeTinTucs.AnyAsync(c => c.MaChuDe == tinTuc.MaChuDe);
                if (!chuDeExists)
                {
                    ModelState.AddModelError("MaChuDe", "Chủ đề không tồn tại");
                    ViewBag.ChuDeTinTucs = await _context.ChuDeTinTucs.ToListAsync();
                    return View("QuanLyTinTuc");
                }

                // Xử lý tải lên hình ảnh nếu có
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(ImageFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }

                    // Xóa hình ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(existingTinTuc.HinhAnh) && existingTinTuc.HinhAnh.StartsWith("/uploads/"))
                    {
                        var oldImagePath = Path.Combine(_environment.WebRootPath, existingTinTuc.HinhAnh.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    tinTuc.HinhAnh = $"/uploads/{uniqueFileName}";
                }
                else
                {
                    // Giữ nguyên hình ảnh cũ nếu không có hình mới
                    tinTuc.HinhAnh = existingTinTuc.HinhAnh;
                }

                existingTinTuc.TieuDe = tinTuc.TieuDe;
                existingTinTuc.NoiDung = tinTuc.NoiDung;
                existingTinTuc.HinhAnh = tinTuc.HinhAnh;
                existingTinTuc.MaChuDe = tinTuc.MaChuDe;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật tin tức thành công.";
                return RedirectToAction(nameof(QuanLyTinTuc));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi: {ex.Message}");
                ViewBag.ChuDeTinTucs = await _context.ChuDeTinTucs.ToListAsync();
                return View("QuanLyTinTuc");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaTinTuc(int id)
        {
            var tinTuc = await _context.TinTucs.FindAsync(id);
            if (tinTuc == null)
            {
                return NotFound();
            }

            // Xóa hình ảnh nếu có
            if (!string.IsNullOrEmpty(tinTuc.HinhAnh) && tinTuc.HinhAnh.StartsWith("/uploads/"))
            {
                var imagePath = Path.Combine(_environment.WebRootPath, tinTuc.HinhAnh.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.TinTucs.Remove(tinTuc);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Xóa tin tức thành công.";
            return RedirectToAction(nameof(QuanLyTinTuc));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaNguoiDung(ApplicationUser model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FindAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                user.HoTen = model.HoTen;
                user.PhoneNumber = model.PhoneNumber;
                user.Avatar = model.Avatar;
                user.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thông tin người dùng thành công.";
                return RedirectToAction(nameof(QuanLyNguoiDung));
            }

            return View("QuanLyNguoiDung");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KhoaNguoiDung(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.TrangThai = false;
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Khóa tài khoản người dùng thành công.";
            return RedirectToAction(nameof(QuanLyNguoiDung));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoKhoaNguoiDung(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.TrangThai = true;
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Mở khóa tài khoản người dùng thành công.";
            return RedirectToAction(nameof(QuanLyNguoiDung));
        }

        [HttpGet]
        public async Task<IActionResult> GetNguoiDung(Guid id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                var roles = await _userManager.GetRolesAsync(user);

                var result = new
                {
                    user.Id,
                    user.HoTen,
                    user.Email,
                    user.PhoneNumber,
                    user.Avatar,
                    user.TrangThai,
                    user.CreatedAt,
                    user.UpdatedAt,
                    Roles = roles
                };

                return Json(result);
            }
            catch (Exception)
            {
                return Json(new { error = "Không thể lấy thông tin người dùng" });
            }
        }
    }
}