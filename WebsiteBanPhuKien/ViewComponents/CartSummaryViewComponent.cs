using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteBanPhuKien.Models;
using WebsiteBanPhuKien.Controllers;

namespace WebsiteBanPhuKien.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartSummaryViewComponent(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int itemCount = 0;

            // Nếu người dùng đã đăng nhập, lấy số lượng từ database
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user != null)
                {
                    itemCount = await _context.GioHangs
                        .Where(g => g.UserId == user.Id)
                        .SumAsync(g => g.SoLuong);
                }
            }
            else
            {
                // Nếu chưa đăng nhập, lấy số lượng từ session
                var cart = HttpContext.Session.GetString("Cart");
                if (!string.IsNullOrEmpty(cart))
                {
                    var cartItems = System.Text.Json.JsonSerializer.Deserialize<List<CartItem>>(cart);
                    if (cartItems != null)
                    {
                        itemCount = cartItems.Sum(i => i.SoLuong);
                    }
                }
            }

            return View(itemCount);
        }
    }

    // Duplicate CartItem class to avoid reference issues
    public class CartItem
    {
        public int MaPhuKien { get; set; }
        public int SoLuong { get; set; }
    }
}