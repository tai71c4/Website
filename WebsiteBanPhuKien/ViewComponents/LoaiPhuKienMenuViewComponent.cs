using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteBanPhuKien.Models;

namespace WebsiteBanPhuKien.ViewComponents
{
    public class LoaiPhuKienMenuViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public LoaiPhuKienMenuViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var loaiPhuKiens = await _context.LoaiPhuKiens.ToListAsync();
            return View(loaiPhuKiens);
        }
    }
}