using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebsiteBanPhuKien.Models;
using WebsiteBanPhuKien.Models.ViewModels;

namespace WebsiteBanPhuKien.Controllers
{
    public class NewsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NewsController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> DanhSach(int? chuDeId, int page = 1)
        {
            int pageSize = 6;
            var query = _context.TinTucs.AsQueryable();

            if (chuDeId.HasValue)
            {
                query = query.Where(t => t.MaChuDe == chuDeId);
                ViewBag.ChuDe = await _context.ChuDeTinTucs.FindAsync(chuDeId);
            }

            var count = await query.CountAsync();
            var tinTucs = await query
                .OrderByDescending(t => t.NgayDang) // Đã có OrderBy
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(t => t.ChuDe)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)System.Math.Ceiling(count / (double)pageSize);
            ViewBag.ChuDeList = await _context.ChuDeTinTucs.ToListAsync();

            return View(tinTucs);
        }

        public async Task<IActionResult> ChiTiet(int id)
        {
            var tinTuc = await _context.TinTucs
                .Include(t => t.ChuDe)
                .FirstOrDefaultAsync(t => t.MaTin == id);

            if (tinTuc == null)
            {
                return NotFound();
            }

            var binhLuans = await _context.BinhLuans
                .Include(b => b.User)
                .Where(b => b.MaTin == id)
                .OrderByDescending(b => b.NgayDang)
                .ToListAsync();

            var tinTucLienQuan = await _context.TinTucs
                .Where(t => t.MaChuDe == tinTuc.MaChuDe && t.MaTin != id)
                .OrderByDescending(t => t.NgayDang)
                .Take(3)
                .ToListAsync();

            var viewModel = new TinTucViewModel
            {
                TinTuc = tinTuc,
                BinhLuans = binhLuans,
                TinTucLienQuan = tinTucLienQuan
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemBinhLuan(int maTin, string noiDung)
        {
            if (string.IsNullOrWhiteSpace(noiDung))
            {
                TempData["ErrorMessage"] = "Nội dung bình luận không được để trống.";
                return RedirectToAction(nameof(ChiTiet), new { id = maTin });
            }

            var tinTuc = await _context.TinTucs.FindAsync(maTin);
            if (tinTuc == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var binhLuan = new BinhLuan
            {
                MaTin = maTin,
                UserId = user.Id,
                NoiDung = noiDung,
                NgayDang = DateTime.Now
            };

            _context.BinhLuans.Add(binhLuan);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ChiTiet), new { id = maTin });
        }
    }
}