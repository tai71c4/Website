using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebsiteBanPhuKien.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Thêm Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Thêm cấu hình để hiển thị lỗi chi tiết trong môi trường Development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
    builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Sử dụng Session
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Tạo dữ liệu mẫu khi khởi động ứng dụng
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
        
        // Tạo loại phụ kiện mẫu nếu chưa có
        if (!context.LoaiPhuKiens.Any())
        {
            context.LoaiPhuKiens.Add(new LoaiPhuKien { TenLoai = "Ốp lưng", MoTa = "Ốp lưng điện thoại" });
            context.LoaiPhuKiens.Add(new LoaiPhuKien { TenLoai = "Tai nghe", MoTa = "Tai nghe có dây và không dây" });
            context.SaveChanges();
        }

        // Tạo hãng sản xuất mẫu nếu chưa có
        if (!context.HangSanXuats.Any())
        {
            context.HangSanXuats.Add(new HangSanXuat { TenHang = "Apple", MoTa = "Phụ kiện Apple" });
            context.HangSanXuats.Add(new HangSanXuat { TenHang = "Samsung", MoTa = "Phụ kiện Samsung" });
            context.SaveChanges();
        }

        // Tạo sản phẩm mẫu nếu chưa có
        if (!context.PhuKiens.Any())
        {
            var loai1 = context.LoaiPhuKiens.FirstOrDefault(l => l.TenLoai == "Ốp lưng");
            var loai2 = context.LoaiPhuKiens.FirstOrDefault(l => l.TenLoai == "Tai nghe");
            var hang1 = context.HangSanXuats.FirstOrDefault(h => h.TenHang == "Apple");
            var hang2 = context.HangSanXuats.FirstOrDefault(h => h.TenHang == "Samsung");

            if (loai1 != null && hang1 != null)
            {
                context.PhuKiens.Add(new PhuKien
                {
                    TenPhuKien = "Ốp lưng iPhone 15",
                    Gia = 250000,
                    MoTa = "Ốp lưng chính hãng cho iPhone 15",
                    HinhAnh = "/img/oplungtrong 16.jpg",
                    SoLuong = 50,
                    MaLoai = loai1.MaLoai,
                    MaHang = hang1.MaHang,
                    CreatedAt = DateTime.Now
                });
            }

            if (loai2 != null && hang1 != null)
            {
                context.PhuKiens.Add(new PhuKien
                {
                    TenPhuKien = "Tai nghe AirPods Pro",
                    Gia = 5500000,
                    MoTa = "Tai nghe không dây AirPods Pro chính hãng",
                    HinhAnh = "/img/airpods.jpg",
                    SoLuong = 20,
                    MaLoai = loai2.MaLoai,
                    MaHang = hang1.MaHang,
                    CreatedAt = DateTime.Now
                });
            }

            if (loai1 != null && hang2 != null)
            {
                context.PhuKiens.Add(new PhuKien
                {
                    TenPhuKien = "Ốp lưng Samsung Galaxy S23",
                    Gia = 200000,
                    MoTa = "Ốp lưng chính hãng cho Samsung Galaxy S23",
                    HinhAnh = "/img/oplungdeos23.jpg",
                    SoLuong = 30,
                    MaLoai = loai1.MaLoai,
                    MaHang = hang2.MaHang,
                    CreatedAt = DateTime.Now
                });
            }

            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi khi tạo dữ liệu mẫu.");
    }
}

app.Run();