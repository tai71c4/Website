using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebsiteBanPhuKien.Models;

namespace WebsiteBanPhuKien.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> DanhGiaSanPham(int maPhuKien)
        {
            var phuKien = await _context.PhuKiens
                .Include(p => p.LoaiPhuKien)
                .Include(p => p.HangSanXuat)
                .FirstOrDefaultAsync(p => p.MaPhuKien == maPhuKien);

            if (phuKien == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }
            
            var danhGiaCu = await _context.DanhGias
                .FirstOrDefaultAsync(d => d.MaPhuKien == maPhuKien && d.UserId == user.Id);

            ViewBag.PhuKien = phuKien;
            return View(danhGiaCu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DanhGiaSanPham(int maPhuKien, int soSao, string binhLuan)
        {
            var phuKien = await _context.PhuKiens.FindAsync(maPhuKien);
            if (phuKien == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var danhGiaCu = await _context.DanhGias
                .FirstOrDefaultAsync(d => d.MaPhuKien == maPhuKien && d.UserId == user.Id);

            if (danhGiaCu != null)
            {
                danhGiaCu.SoSao = soSao;
                danhGiaCu.BinhLuan = binhLuan;
                danhGiaCu.NgayDanhGia = DateTime.Now;
            }
            else
            {
                var danhGia = new DanhGia
                {
                    MaPhuKien = maPhuKien,
                    UserId = user.Id,
                    SoSao = soSao,
                    BinhLuan = binhLuan,
                    NgayDanhGia = DateTime.Now
                };
                _context.DanhGias.Add(danhGia);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("ChiTiet", "Product", new { id = maPhuKien });
        }
    }
}