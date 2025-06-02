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
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductController> _logger;

        public ProductController(AppDbContext context, ILogger<ProductController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> DanhSach(int? loaiId, int? hangId, string searchString, int page = 1)
        {
            try
            {
                int pageSize = 9;
                
                // Lấy tất cả sản phẩm với SQL thuần để tránh lỗi null
                string baseSql = @"
                    SELECT p.MaPhuKien, p.TenPhuKien, p.Gia, 
                           ISNULL(p.MoTa, '') as MoTa, 
                           ISNULL(p.HinhAnh, '') as HinhAnh, 
                           p.SoLuong, p.MaLoai, p.MaHang, 
                           p.CreatedAt, p.UpdatedAt
                    FROM PhuKien p";
                
                // Lấy tất cả sản phẩm trước
                var allProducts = await _context.PhuKiens.FromSqlRaw(baseSql).ToListAsync();
                
                // Lọc sản phẩm theo các điều kiện
                var filteredProducts = allProducts;
                
                if (loaiId.HasValue)
                {
                    filteredProducts = filteredProducts.Where(p => p.MaLoai == loaiId).ToList();
                    ViewBag.Loai = await _context.LoaiPhuKiens.FindAsync(loaiId);
                }
                
                if (hangId.HasValue)
                {
                    filteredProducts = filteredProducts.Where(p => p.MaHang == hangId).ToList();
                    ViewBag.Hang = await _context.HangSanXuats.FindAsync(hangId);
                }
                
                if (!string.IsNullOrEmpty(searchString))
                {
                    filteredProducts = filteredProducts.Where(p => 
                        p.TenPhuKien.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                
                // Đếm tổng số sản phẩm sau khi lọc
                var count = filteredProducts.Count;
                
                // Phân trang
                var items = filteredProducts
                    .OrderBy(p => p.MaPhuKien)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                
                // Tải thông tin loại và hãng riêng biệt
                foreach (var item in items)
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

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling(count / (double)pageSize);
                ViewBag.LoaiPhuKiens = await _context.LoaiPhuKiens.ToListAsync();
                ViewBag.HangSanXuats = await _context.HangSanXuats.ToListAsync();
                ViewBag.SearchString = searchString;
                ViewBag.LoaiId = loaiId;
                ViewBag.HangId = hangId;

                return View(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hiển thị danh sách sản phẩm");
                ViewBag.ErrorMessage = "Có lỗi xảy ra khi tải danh sách sản phẩm.";
                return View(new List<PhuKien>());
            }
        }

        public async Task<IActionResult> ChiTiet(int id)
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
                
                var sanPham = await _context.PhuKiens
                    .FromSqlRaw(sql, id)
                    .FirstOrDefaultAsync();

                if (sanPham == null)
                {
                    return NotFound();
                }

                // Tải thông tin loại và hãng riêng biệt
                var loai = await _context.LoaiPhuKiens.FindAsync(sanPham.MaLoai);
                var hang = await _context.HangSanXuats.FindAsync(sanPham.MaHang);
                if (loai != null) sanPham.LoaiPhuKien = loai;
                if (hang != null) sanPham.HangSanXuat = hang;
                
                // Đảm bảo các thuộc tính string không null
                sanPham.TenPhuKien ??= string.Empty;
                sanPham.MoTa ??= string.Empty;
                sanPham.HinhAnh ??= string.Empty;

                // Lấy sản phẩm cùng loại
                var sanPhamCungLoai = await _context.PhuKiens
                    .FromSqlRaw(@"
                        SELECT p.MaPhuKien, p.TenPhuKien, p.Gia, 
                               ISNULL(p.MoTa, '') as MoTa, 
                               ISNULL(p.HinhAnh, '') as HinhAnh, 
                               p.SoLuong, p.MaLoai, p.MaHang, 
                               p.CreatedAt, p.UpdatedAt
                        FROM PhuKien p
                        WHERE p.MaLoai = {0} AND p.MaPhuKien != {1}
                        ORDER BY p.MaPhuKien
                        OFFSET 0 ROWS FETCH NEXT 4 ROWS ONLY", 
                        sanPham.MaLoai, id)
                    .ToListAsync();

                // Tải thông tin loại và hãng cho sản phẩm cùng loại
                foreach (var item in sanPhamCungLoai)
                {
                    var itemLoai = await _context.LoaiPhuKiens.FindAsync(item.MaLoai);
                    var itemHang = await _context.HangSanXuats.FindAsync(item.MaHang);
                    if (itemLoai != null) item.LoaiPhuKien = itemLoai;
                    if (itemHang != null) item.HangSanXuat = itemHang;
                    
                    // Đảm bảo các thuộc tính string không null
                    item.TenPhuKien ??= string.Empty;
                    item.MoTa ??= string.Empty;
                    item.HinhAnh ??= string.Empty;
                }

                // Lấy đánh giá
                var danhGias = await _context.DanhGias
                    .Where(d => d.MaPhuKien == id)
                    .ToListAsync();

                // Tải thông tin người dùng cho đánh giá
                foreach (var danhGia in danhGias)
                {
                    var user = await _context.Users.FindAsync(danhGia.UserId);
                    if (user != null) danhGia.User = user;
                }

                var diemTrungBinh = danhGias.Any() ? danhGias.Average(d => d.SoSao) : 0;
                var tongDanhGia = danhGias.Count;

                var viewModel = new ChiTietSanPhamViewModel
                {
                    SanPham = sanPham,
                    SanPhamCungLoai = sanPhamCungLoai,
                    DanhGias = danhGias,
                    DiemTrungBinh = diemTrungBinh,
                    TongDanhGia = tongDanhGia
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hiển thị chi tiết sản phẩm");
                return RedirectToAction("DanhSach");
            }
        }

        public async Task<IActionResult> TheoLoai(int id)
        {
            try
            {
                var loai = await _context.LoaiPhuKiens.FindAsync(id);
                if (loai == null)
                {
                    return NotFound();
                }

                // Sử dụng SQL thuần để tránh lỗi SqlNullValueException
                var products = await _context.PhuKiens
                    .FromSqlRaw(@"
                        SELECT p.MaPhuKien, p.TenPhuKien, p.Gia, 
                               ISNULL(p.MoTa, '') as MoTa, 
                               ISNULL(p.HinhAnh, '') as HinhAnh, 
                               p.SoLuong, p.MaLoai, p.MaHang, 
                               p.CreatedAt, p.UpdatedAt
                        FROM PhuKien p
                        WHERE p.MaLoai = {0}", id)
                    .ToListAsync();

                // Tải thông tin loại và hãng riêng biệt
                foreach (var item in products)
                {
                    item.LoaiPhuKien = loai; // Loại đã được kiểm tra null ở trên
                    var hang = await _context.HangSanXuats.FindAsync(item.MaHang);
                    if (hang != null) item.HangSanXuat = hang;
                    
                    // Đảm bảo các thuộc tính string không null
                    item.TenPhuKien ??= string.Empty;
                    item.MoTa ??= string.Empty;
                    item.HinhAnh ??= string.Empty;
                }

                ViewBag.Loai = loai;
                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hiển thị sản phẩm theo loại");
                return RedirectToAction("DanhSach");
            }
        }
    }
}