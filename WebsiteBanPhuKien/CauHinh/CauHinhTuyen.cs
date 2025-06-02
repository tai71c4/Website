using Microsoft.AspNetCore.Builder;

namespace WebsiteBanPhuKien.CauHinh
{
    public static class CauHinhTuyen
    {
        public static void DangKyTuyen(IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}