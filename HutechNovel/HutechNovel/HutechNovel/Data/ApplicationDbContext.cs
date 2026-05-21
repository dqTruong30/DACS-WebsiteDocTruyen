using HutechNovel.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HutechNovel.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // ── Bảng chính ──────────────────────────────────────────────────────────
        public DbSet<Truyen> Truyens { get; set; }
        public DbSet<Chuong> Chuongs { get; set; }
        public DbSet<NoiDungChuong> NoiDungChuongs { get; set; }
        public DbSet<TacGia> TacGias { get; set; }
        public DbSet<The> Thes { get; set; }
        public DbSet<BinhLuan> BinhLuans { get; set; }

        // ── Tương tác người dùng ─────────────────────────────────────────────────
        public DbSet<DanhDau> DanhDaus { get; set; }
        public DbSet<YeuThich> YeuThichs { get; set; }
        public DbSet<LichSuDoc> LichSuDocs { get; set; }
        public DbSet<DanhGia> DanhGias { get; set; }
        public DbSet<LuotXem> LuotXems { get; set; }
        public DbSet<DayTruyen> DayTruyens { get; set; }
        public DbSet<TheoDoiTruyen> TheoDoiTruyens { get; set; }
        public DbSet<NhatKyQuanTri> NhatKyQuanTris { get; set; }
        public DbSet<BinhLuanCamXuc> BinhLuanCamXucs { get; set; }

        // ── Cấu hình hệ thống ────────────────────────────────────────────────────
        public DbSet<CauHinhHeThong> CauHinhHeThongs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique constraints
            modelBuilder.Entity<The>()
                .HasIndex(t => t.TenThe).IsUnique();

            modelBuilder.Entity<Chuong>()
                .HasIndex(c => new { c.MaTruyen, c.SoChuong }).IsUnique();

            modelBuilder.Entity<NoiDungChuong>()
                .HasIndex(nd => new { nd.MaChuong, nd.LoaiNoiDung }).IsUnique();

            modelBuilder.Entity<TheoDoiTruyen>()
                .HasIndex(t => new { t.MaNguoiDung, t.MaTruyen }).IsUnique();

            modelBuilder.Entity<BinhLuanCamXuc>()
                .HasIndex(x => new { x.MaNguoiDung, x.MaBinhLuan }).IsUnique();

            // Cấu hình các quan hệ để tránh lỗi Multiple Cascade Paths của SQL Server
            modelBuilder.Entity<DanhDau>()
                .HasOne(x => x.NguoiDung)
                .WithMany(u => u.DanhDaus)
                .HasForeignKey(x => x.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DanhDau>()
                .HasOne(x => x.Truyen)
                .WithMany(t => t.DanhDaus)
                .HasForeignKey(x => x.MaTruyen)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<YeuThich>()
                .HasOne(x => x.NguoiDung)
                .WithMany(u => u.YeuThichs)
                .HasForeignKey(x => x.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<YeuThich>()
                .HasOne(x => x.Truyen)
                .WithMany(t => t.YeuThichs)
                .HasForeignKey(x => x.MaTruyen)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DanhGia>()
                .HasOne(x => x.NguoiDung)
                .WithMany(u => u.DanhGias)
                .HasForeignKey(x => x.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DanhGia>()
                .HasOne(x => x.Truyen)
                .WithMany(t => t.DanhGias)
                .HasForeignKey(x => x.MaTruyen)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DayTruyen>()
                .HasOne(x => x.NguoiDung)
                .WithMany(u => u.DayTruyens)
                .HasForeignKey(x => x.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DayTruyen>()
                .HasOne(x => x.Truyen)
                .WithMany(t => t.DayTruyens)
                .HasForeignKey(x => x.MaTruyen)
                .OnDelete(DeleteBehavior.Cascade);

            // Tắt Cascade Delete đối với Lượt Xem -> Người Dùng để tránh xung đột
            modelBuilder.Entity<LuotXem>()
                .HasOne(x => x.NguoiDung)
                .WithMany(u => u.LuotXems)
                .HasForeignKey(x => x.MaNguoiDung)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LuotXem>()
                .HasOne(x => x.Truyen)
                .WithMany(t => t.LuotXems)
                .HasForeignKey(x => x.MaTruyen)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LichSuDoc>()
                .HasOne(x => x.NguoiDung)
                .WithMany(u => u.LichSuDocs)
                .HasForeignKey(x => x.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LichSuDoc>()
                .HasOne(x => x.Chuong)
                .WithMany(c => c.LichSuDocs)
                .HasForeignKey(x => x.MaChuong)
                .OnDelete(DeleteBehavior.Cascade);

            // BinhLuan — tắt cascade để tránh multiple-cascade-path
            modelBuilder.Entity<BinhLuan>()
                .HasOne(b => b.Truyen)
                .WithMany(t => t.BinhLuans)
                .HasForeignKey(b => b.MaTruyen)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BinhLuan>()
                .HasOne(b => b.Chuong)
                .WithMany(c => c.BinhLuans)
                .HasForeignKey(b => b.MaChuong)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BinhLuan>()
                .HasOne(b => b.NguoiDung)
                .WithMany(u => u.BinhLuans)
                .HasForeignKey(b => b.MaNguoiDung)
                .OnDelete(DeleteBehavior.NoAction);

            // Bình luận đa cấp (reply)
            modelBuilder.Entity<BinhLuan>()
                .HasOne(b => b.BinhLuanCha)
                .WithMany(b => b.BinhLuanCons)
                .HasForeignKey(b => b.MaBinhLuanCha)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BinhLuanCamXuc>()
                .HasOne(x => x.NguoiDung)
                .WithMany()
                .HasForeignKey(x => x.MaNguoiDung)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BinhLuanCamXuc>()
                .HasOne(x => x.BinhLuan)
                .WithMany()
                .HasForeignKey(x => x.MaBinhLuan)
                .OnDelete(DeleteBehavior.Cascade);

            // Uploader (người đăng truyện)
            modelBuilder.Entity<Truyen>()
                .HasOne(t => t.NguoiDang)
                .WithMany(u => u.TruyenDaDangs)
                .HasForeignKey(t => t.NguoiDangId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Truyen>()
                .HasOne(t => t.TacGia)
                .WithMany(tg => tg.Truyens)
                .HasForeignKey(t => t.MaTacGia)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
