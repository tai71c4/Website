using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebsiteBanPhuKien.Models
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<PhuKien> PhuKiens { get; set; }
        public DbSet<LoaiPhuKien> LoaiPhuKiens { get; set; }
        public DbSet<HangSanXuat> HangSanXuats { get; set; }
        public DbSet<GioHang> GioHangs { get; set; }
        public DbSet<DonHang> DonHangs { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public DbSet<DanhGia> DanhGias { get; set; }
        public DbSet<ChuDeTinTuc> ChuDeTinTucs { get; set; }
        public DbSet<TinTuc> TinTucs { get; set; }
        public DbSet<BinhLuan> BinhLuans { get; set; }
        public DbSet<LienHe> LienHes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table names to match what's expected
            modelBuilder.Entity<PhuKien>().ToTable("PhuKien");
            modelBuilder.Entity<LoaiPhuKien>().ToTable("LoaiPhuKien");
            modelBuilder.Entity<HangSanXuat>().ToTable("Hang");
            modelBuilder.Entity<GioHang>().ToTable("GioHang");
            modelBuilder.Entity<DonHang>().ToTable("DonHang");
            modelBuilder.Entity<ChiTietDonHang>().ToTable("ChiTietDonHang");
            modelBuilder.Entity<DanhGia>().ToTable("DanhGia");
            modelBuilder.Entity<ChuDeTinTuc>().ToTable("ChuDe");
            modelBuilder.Entity<TinTuc>().ToTable("TinTuc");
            modelBuilder.Entity<BinhLuan>().ToTable("BinhLuan");
            modelBuilder.Entity<LienHe>().ToTable("LienHe");

            // Configure composite key for ChiTietDonHang
            modelBuilder.Entity<ChiTietDonHang>()
                .HasKey(c => c.Id);

            // Configure unique index for GioHang
            modelBuilder.Entity<GioHang>()
                .HasIndex(g => new { g.UserId, g.MaPhuKien })
                .IsUnique();
                
            // Configure ApplicationUser and DonHang relationship
            modelBuilder.Entity<DonHang>()
                .HasOne(d => d.User)
                .WithMany(u => u.DonHangs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Configure CreatedByUser relationship
            modelBuilder.Entity<DonHang>()
                .HasOne(d => d.CreatedByUser)
                .WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Configure UpdatedByUser relationship
            modelBuilder.Entity<DonHang>()
                .HasOne(d => d.UpdatedByUser)
                .WithMany()
                .HasForeignKey(d => d.UpdatedBy)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Configure decimal precision
            modelBuilder.Entity<DonHang>()
                .Property(d => d.TongTien)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<PhuKien>()
                .Property(p => p.Gia)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<ChiTietDonHang>()
                .Property(c => c.DonGia)
                .HasColumnType("decimal(18,2)");
        }
    }
}