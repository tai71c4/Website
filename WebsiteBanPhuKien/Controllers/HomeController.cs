using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using WebsiteBanPhuKien.Models;

namespace WebsiteBanPhuKien.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Lấy sản phẩm mới
                var sanPhamMoi = await _context.PhuKiens
                    .FromSqlRaw(@"
                        SELECT p.MaPhuKien, p.TenPhuKien, p.Gia, 
                               ISNULL(p.MoTa, '') as MoTa, 
                               ISNULL(p.HinhAnh, '') as HinhAnh, 
                               p.SoLuong, p.MaLoai, p.MaHang, 
                               p.CreatedAt, p.UpdatedAt
                        FROM PhuKien p
                        ORDER BY p.CreatedAt DESC
                        OFFSET 0 ROWS FETCH NEXT 8 ROWS ONLY")
                    .ToListAsync();

                // Tải thông tin loại và hãng riêng biệt
                foreach (var item in sanPhamMoi)
                {
                    var loai = await _context.LoaiPhuKiens.FindAsync(item.MaLoai);
                    var hang = await _context.HangSanXuats.FindAsync(item.MaHang);
                    if (loai != null) item.LoaiPhuKien = loai;
                    if (hang != null) item.HangSanXuat = hang;
                }

                // Lấy tin tức mới
                var tinTucMoi = await _context.TinTucs
                    .OrderByDescending(t => t.NgayDang)
                    .Take(3)
                    .ToListAsync();

                ViewBag.SanPhamMoi = sanPhamMoi;
                ViewBag.TinTucMoi = tinTucMoi;

                // Danh mục nổi bật
                ViewBag.DanhMucNoiBat = new List<string>
                {
                    "Ốp lưng điện thoại",
                    "Kính cường lực",
                    "Cáp sạc & củ sạc",
                    "Pin dự phòng",
                    "Tai nghe & âm thanh",
                    "Thiết bị chuyển đổi",
                    "Phụ kiện tiện ích"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy dữ liệu cho trang chủ");
                // Không làm gì, để ViewBag là null và view sẽ xử lý
            }

            return View();
        }

        public IActionResult GioiThieu()
        {
            return View();
        }

        public IActionResult LienHe()
        {
            return View();
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}