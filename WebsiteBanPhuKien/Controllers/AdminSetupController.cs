using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebsiteBanPhuKien.Models;
using WebsiteBanPhuKien.Models.ViewModels;

namespace WebsiteBanPhuKien.Controllers
{
    public class AdminSetupController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public AdminSetupController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole<Guid>> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Kiểm tra xem đã có admin chưa
            var adminExists = await _userManager.GetUsersInRoleAsync("Admin");
            if (adminExists.Count > 0)
            {
                return View("AdminExists");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(AdminSetupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kiểm tra xem đã có admin chưa
            var adminExists = await _userManager.GetUsersInRoleAsync("Admin");
            if (adminExists.Count > 0)
            {
                return View("AdminExists");
            }

            // Tạo role Admin nếu chưa có
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>("Admin"));
            }

            // Tạo role User nếu chưa có
            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>("User"));
            }

            // Tạo tài khoản admin
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                HoTen = model.HoTen,
                PhoneNumber = model.PhoneNumber,
                TrangThai = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Success");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        public IActionResult Success()
        {
            return View();
        }

        public IActionResult AdminExists()
        {
            return View();
        }
    }
}