using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebsiteBanPhuKien.Models;

namespace WebsiteBanPhuKien.Controllers
{
    public class ContactController : Controller
    {
        private readonly AppDbContext _context;

        public ContactController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult FormLienHe()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FormLienHe(LienHe lienHe)
        {
            if (ModelState.IsValid)
            {
                lienHe.NgayGui = DateTime.Now;
                _context.LienHes.Add(lienHe);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cảm ơn bạn đã gửi thông tin liên hệ. Chúng tôi sẽ phản hồi sớm nhất có thể.";
                return RedirectToAction(nameof(FormLienHe));
            }
            return View(lienHe);
        }
    }
}