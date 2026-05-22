using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using HutechNovel.Data;
using HutechNovel.Models;
using HutechNovel.Services;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.Configure<SmtpEmailOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
builder.Services.AddHostedService<ScheduledChapterPublisher>();
builder.Services.AddHostedService<TrendingScoreUpdaterService>();

builder.Services.AddHttpClient();
builder.Services.AddScoped<IGeminiService, GeminiService>();



// 2. Cấu hình Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// --- THÊM ĐOẠN NÀY ĐỂ BẢO MẬT KHU VỰC ADMIN ---
builder.Services.ConfigureApplicationCookie(options =>
{
    // Bắt sự kiện khi hệ thống định đẩy user chưa đăng nhập ra trang Login
    options.Events.OnRedirectToLogin = context =>
    {
        // Nếu cố tình truy cập khu vực Admin mà chưa đăng nhập -> Ép thành lỗi 404
        if (context.Request.Path.StartsWithSegments("/Admin", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        }
        // Còn lại (vd: cố bình luận, v.v.) thì vẫn đẩy ra trang Login public bình thường
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };

    // Bắt sự kiện khi user ĐÃ đăng nhập nhưng KHÔNG CÓ QUYỀN (vd: User thường cố vào Admin)
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/Admin", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});



// 2.5. Cấu hình Đăng nhập bằng Google
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
        options.ClientId = googleAuthNSection["ClientId"] ?? string.Empty;
        options.ClientSecret = googleAuthNSection["ClientSecret"] ?? string.Empty;
    });



builder.Services.AddControllersWithViews();


var app = builder.Build();

// 3. Khởi tạo dữ liệu hệ thống (Roles & Admin Account)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    // Tạo các quyền nếu chưa có
    string[] roleNames = { "Admin", "Uploader", "User" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Tự động tạo tài khoản Admin cứng
    var adminEmail = "admin@gmail.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        var newAdmin = new ApplicationUser
        {
            UserName = "admin",
            Email = adminEmail,
            EmailConfirmed = true,
            HoTen = "Hệ Thống Admin",
            VeDaySach = 999
        };

        // MẬT KHẨU MẶC ĐỊNH: Admin@123
        var createAdmin = await userManager.CreateAsync(newAdmin, "Admin@123");
        if (createAdmin.Succeeded)
        {
            await userManager.AddToRoleAsync(newAdmin, "Admin");
        }
    }
    else
    {
        // Nếu đã có user nhưng chưa có quyền Admin thì gán thêm
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }// Tự động tạo dòng Cấu hình hệ thống mặc định nếu chưa có
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
    await EnsureDatabaseCompatibilityAsync(dbContext);
    if (!dbContext.CauHinhHeThongs.Any())
    {
        dbContext.CauHinhHeThongs.Add(new CauHinhHeThong
        {
            // KHÔNG GÁN Id Ở ĐÂY NỮA, để EF Core và SQL Server tự làm việc
            TenWebsite = "HutechNovel",
            CheDoBaoTri = false
        });
        await dbContext.SaveChangesAsync();
    }

    var defaultTags = new[]
    {
        "Huyền huyễn", "Đồng nhân", "Dị năng", "Đô thị", "Linh dị",
        "Ngôn tình", "Light Novel", "Võng du", "Khoa học viễn tưởng", "Lịch sử"
    };

    foreach (var tagName in defaultTags)
    {
        if (!await dbContext.Thes.AnyAsync(t => t.TenThe == tagName))
        {
            dbContext.Thes.Add(new The { TenThe = tagName });
        }
    }

    await dbContext.SaveChangesAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseMiddleware<HutechNovel.Middleware.MaintenanceMiddleware>();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapRazorPages();
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=AdminDashboard}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapRazorPages();
app.Run();

static async Task EnsureDatabaseCompatibilityAsync(ApplicationDbContext dbContext)
{
    await dbContext.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID(N'[dbo].[BinhLuans]', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.BinhLuans', N'DaGhim') IS NULL
        ALTER TABLE [dbo].[BinhLuans] ADD [DaGhim] bit NOT NULL CONSTRAINT [DF_BinhLuans_DaGhim] DEFAULT CAST(0 AS bit);

    IF COL_LENGTH(N'dbo.BinhLuans', N'LaSpoiler') IS NULL
        ALTER TABLE [dbo].[BinhLuans] ADD [LaSpoiler] bit NOT NULL CONSTRAINT [DF_BinhLuans_LaSpoiler] DEFAULT CAST(0 AS bit);

    IF COL_LENGTH(N'dbo.BinhLuans', N'SoBaoCao') IS NULL
        ALTER TABLE [dbo].[BinhLuans] ADD [SoBaoCao] int NOT NULL CONSTRAINT [DF_BinhLuans_SoBaoCao] DEFAULT 0;

    IF COL_LENGTH(N'dbo.BinhLuans', N'SoCamXuc') IS NULL
        ALTER TABLE [dbo].[BinhLuans] ADD [SoCamXuc] int NOT NULL CONSTRAINT [DF_BinhLuans_SoCamXuc] DEFAULT 0;
END

IF OBJECT_ID(N'[dbo].[NhatKyQuanTris]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[NhatKyQuanTris] (
        [MaNhatKy] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_NhatKyQuanTris] PRIMARY KEY,
        [MaNguoiDung] nvarchar(max) NULL,
        [HanhDong] nvarchar(max) NOT NULL,
        [DoiTuong] nvarchar(max) NOT NULL,
        [MaDoiTuong] int NULL,
        [NoiDung] nvarchar(max) NOT NULL,
        [NgayTao] datetime2 NOT NULL
    );
END

IF OBJECT_ID(N'[dbo].[BinhLuanCamXucs]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[BinhLuanCamXucs] (
        [MaCamXuc] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_BinhLuanCamXucs] PRIMARY KEY,
        [MaNguoiDung] nvarchar(450) NOT NULL,
        [MaBinhLuan] int NOT NULL,
        [NgayTao] datetime2 NOT NULL,
        CONSTRAINT [FK_BinhLuanCamXucs_AspNetUsers_MaNguoiDung] FOREIGN KEY ([MaNguoiDung]) REFERENCES [dbo].[AspNetUsers] ([Id]),
        CONSTRAINT [FK_BinhLuanCamXucs_BinhLuans_MaBinhLuan] FOREIGN KEY ([MaBinhLuan]) REFERENCES [dbo].[BinhLuans] ([MaBinhLuan]) ON DELETE CASCADE
    );
END

IF OBJECT_ID(N'[dbo].[BinhLuanCamXucs]', N'U') IS NOT NULL
    AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BinhLuanCamXucs_MaNguoiDung_MaBinhLuan' AND object_id = OBJECT_ID(N'[dbo].[BinhLuanCamXucs]'))
BEGIN
    CREATE UNIQUE INDEX [IX_BinhLuanCamXucs_MaNguoiDung_MaBinhLuan] ON [dbo].[BinhLuanCamXucs] ([MaNguoiDung], [MaBinhLuan]);
END");
}
