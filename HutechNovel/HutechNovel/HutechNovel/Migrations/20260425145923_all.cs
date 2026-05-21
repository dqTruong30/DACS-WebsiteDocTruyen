using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HutechNovel.Migrations
{
    /// <inheritdoc />
    public partial class all : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    KhaiSinh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoChuongDaDoc = table.Column<int>(type: "int", nullable: false),
                    SoBinhLuan = table.Column<int>(type: "int", nullable: false),
                    SoPhutDaDoc = table.Column<int>(type: "int", nullable: false),
                    VeDaySach = table.Column<int>(type: "int", nullable: false),
                    NgayDiemDanhCuoi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CaiDatMauNen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CaiDatFontChu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CaiDatCoChu = table.Column<int>(type: "int", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Avatar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TacGias",
                columns: table => new
                {
                    MaTacGia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenTacGia = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TieuSu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TacGias", x => x.MaTacGia);
                });

            migrationBuilder.CreateTable(
                name: "Thes",
                columns: table => new
                {
                    MaThe = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenThe = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Thes", x => x.MaThe);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Truyens",
                columns: table => new
                {
                    MaTruyen = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TieuDe = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnhBia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TongSoChuong = table.Column<int>(type: "int", nullable: false),
                    TongLuotXem = table.Column<int>(type: "int", nullable: false),
                    DiemDanhGiaTrungBinh = table.Column<double>(type: "float", nullable: false),
                    DiemTrending = table.Column<double>(type: "float", nullable: false),
                    MaTacGia = table.Column<int>(type: "int", nullable: false),
                    TacGiaMaTacGia = table.Column<int>(type: "int", nullable: false),
                    NguoiDangId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Truyens", x => x.MaTruyen);
                    table.ForeignKey(
                        name: "FK_Truyens_AspNetUsers_NguoiDangId",
                        column: x => x.NguoiDangId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Truyens_TacGias_TacGiaMaTacGia",
                        column: x => x.TacGiaMaTacGia,
                        principalTable: "TacGias",
                        principalColumn: "MaTacGia",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chuongs",
                columns: table => new
                {
                    MaChuong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TieuDe = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SoChuong = table.Column<int>(type: "int", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayHenGio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    MaTruyen = table.Column<int>(type: "int", nullable: false),
                    TruyenMaTruyen = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chuongs", x => x.MaChuong);
                    table.ForeignKey(
                        name: "FK_Chuongs_Truyens_TruyenMaTruyen",
                        column: x => x.TruyenMaTruyen,
                        principalTable: "Truyens",
                        principalColumn: "MaTruyen",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DanhDaus",
                columns: table => new
                {
                    MaDanhDau = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NguoiDungId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MaTruyen = table.Column<int>(type: "int", nullable: false),
                    TruyenMaTruyen = table.Column<int>(type: "int", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhDaus", x => x.MaDanhDau);
                    table.ForeignKey(
                        name: "FK_DanhDaus_AspNetUsers_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DanhDaus_Truyens_TruyenMaTruyen",
                        column: x => x.TruyenMaTruyen,
                        principalTable: "Truyens",
                        principalColumn: "MaTruyen",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DanhGias",
                columns: table => new
                {
                    MaDanhGia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NguoiDungId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MaTruyen = table.Column<int>(type: "int", nullable: false),
                    TruyenMaTruyen = table.Column<int>(type: "int", nullable: false),
                    DiemSo = table.Column<int>(type: "int", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGias", x => x.MaDanhGia);
                    table.ForeignKey(
                        name: "FK_DanhGias_AspNetUsers_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DanhGias_Truyens_TruyenMaTruyen",
                        column: x => x.TruyenMaTruyen,
                        principalTable: "Truyens",
                        principalColumn: "MaTruyen",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DayTruyens",
                columns: table => new
                {
                    MaDay = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NguoiDungId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MaTruyen = table.Column<int>(type: "int", nullable: false),
                    TruyenMaTruyen = table.Column<int>(type: "int", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayTruyens", x => x.MaDay);
                    table.ForeignKey(
                        name: "FK_DayTruyens_AspNetUsers_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DayTruyens_Truyens_TruyenMaTruyen",
                        column: x => x.TruyenMaTruyen,
                        principalTable: "Truyens",
                        principalColumn: "MaTruyen",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LuotXems",
                columns: table => new
                {
                    MaLuotXem = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTruyen = table.Column<int>(type: "int", nullable: false),
                    TruyenMaTruyen = table.Column<int>(type: "int", nullable: false),
                    MaNguoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NguoiDungId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ThoiGianXem = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LuotXems", x => x.MaLuotXem);
                    table.ForeignKey(
                        name: "FK_LuotXems_AspNetUsers_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LuotXems_Truyens_TruyenMaTruyen",
                        column: x => x.TruyenMaTruyen,
                        principalTable: "Truyens",
                        principalColumn: "MaTruyen",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Truyen_Thes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTruyen = table.Column<int>(type: "int", nullable: false),
                    TruyenMaTruyen = table.Column<int>(type: "int", nullable: false),
                    MaThe = table.Column<int>(type: "int", nullable: false),
                    TheMaThe = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Truyen_Thes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Truyen_Thes_Thes_TheMaThe",
                        column: x => x.TheMaThe,
                        principalTable: "Thes",
                        principalColumn: "MaThe",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Truyen_Thes_Truyens_TruyenMaTruyen",
                        column: x => x.TruyenMaTruyen,
                        principalTable: "Truyens",
                        principalColumn: "MaTruyen",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YeuThichs",
                columns: table => new
                {
                    MaYeuThich = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NguoiDungId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaTruyen = table.Column<int>(type: "int", nullable: false),
                    TruyenMaTruyen = table.Column<int>(type: "int", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuThichs", x => x.MaYeuThich);
                    table.ForeignKey(
                        name: "FK_YeuThichs_AspNetUsers_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YeuThichs_Truyens_TruyenMaTruyen",
                        column: x => x.TruyenMaTruyen,
                        principalTable: "Truyens",
                        principalColumn: "MaTruyen",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BinhLuans",
                columns: table => new
                {
                    MaBinhLuan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaNguoiDung = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaTruyen = table.Column<int>(type: "int", nullable: false),
                    MaChuong = table.Column<int>(type: "int", nullable: true),
                    MaBinhLuanCha = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BinhLuans", x => x.MaBinhLuan);
                    table.ForeignKey(
                        name: "FK_BinhLuans_AspNetUsers_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BinhLuans_BinhLuans_MaBinhLuanCha",
                        column: x => x.MaBinhLuanCha,
                        principalTable: "BinhLuans",
                        principalColumn: "MaBinhLuan");
                    table.ForeignKey(
                        name: "FK_BinhLuans_Chuongs_MaChuong",
                        column: x => x.MaChuong,
                        principalTable: "Chuongs",
                        principalColumn: "MaChuong");
                    table.ForeignKey(
                        name: "FK_BinhLuans_Truyens_MaTruyen",
                        column: x => x.MaTruyen,
                        principalTable: "Truyens",
                        principalColumn: "MaTruyen");
                });

            migrationBuilder.CreateTable(
                name: "LichSuDocs",
                columns: table => new
                {
                    MaLichSu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NguoiDungId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MaChuong = table.Column<int>(type: "int", nullable: false),
                    ChuongMaChuong = table.Column<int>(type: "int", nullable: false),
                    ThoiGianDoc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ViTriDoc = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuDocs", x => x.MaLichSu);
                    table.ForeignKey(
                        name: "FK_LichSuDocs_AspNetUsers_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LichSuDocs_Chuongs_ChuongMaChuong",
                        column: x => x.ChuongMaChuong,
                        principalTable: "Chuongs",
                        principalColumn: "MaChuong",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NoiDungChuongs",
                columns: table => new
                {
                    MaNoiDung = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaChuong = table.Column<int>(type: "int", nullable: false),
                    ChuongMaChuong = table.Column<int>(type: "int", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoaiNoiDung = table.Column<int>(type: "int", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoiDungChuongs", x => x.MaNoiDung);
                    table.ForeignKey(
                        name: "FK_NoiDungChuongs_Chuongs_ChuongMaChuong",
                        column: x => x.ChuongMaChuong,
                        principalTable: "Chuongs",
                        principalColumn: "MaChuong",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BinhLuans_MaBinhLuanCha",
                table: "BinhLuans",
                column: "MaBinhLuanCha");

            migrationBuilder.CreateIndex(
                name: "IX_BinhLuans_MaChuong",
                table: "BinhLuans",
                column: "MaChuong");

            migrationBuilder.CreateIndex(
                name: "IX_BinhLuans_MaNguoiDung",
                table: "BinhLuans",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_BinhLuans_MaTruyen",
                table: "BinhLuans",
                column: "MaTruyen");

            migrationBuilder.CreateIndex(
                name: "IX_Chuongs_MaTruyen_SoChuong",
                table: "Chuongs",
                columns: new[] { "MaTruyen", "SoChuong" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chuongs_TruyenMaTruyen",
                table: "Chuongs",
                column: "TruyenMaTruyen");

            migrationBuilder.CreateIndex(
                name: "IX_DanhDaus_NguoiDungId",
                table: "DanhDaus",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_DanhDaus_TruyenMaTruyen",
                table: "DanhDaus",
                column: "TruyenMaTruyen");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGias_NguoiDungId",
                table: "DanhGias",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGias_TruyenMaTruyen",
                table: "DanhGias",
                column: "TruyenMaTruyen");

            migrationBuilder.CreateIndex(
                name: "IX_DayTruyens_NguoiDungId",
                table: "DayTruyens",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_DayTruyens_TruyenMaTruyen",
                table: "DayTruyens",
                column: "TruyenMaTruyen");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuDocs_ChuongMaChuong",
                table: "LichSuDocs",
                column: "ChuongMaChuong");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuDocs_NguoiDungId",
                table: "LichSuDocs",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_LuotXems_NguoiDungId",
                table: "LuotXems",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_LuotXems_TruyenMaTruyen",
                table: "LuotXems",
                column: "TruyenMaTruyen");

            migrationBuilder.CreateIndex(
                name: "IX_NoiDungChuongs_ChuongMaChuong",
                table: "NoiDungChuongs",
                column: "ChuongMaChuong");

            migrationBuilder.CreateIndex(
                name: "IX_NoiDungChuongs_MaChuong_LoaiNoiDung",
                table: "NoiDungChuongs",
                columns: new[] { "MaChuong", "LoaiNoiDung" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Thes_TenThe",
                table: "Thes",
                column: "TenThe",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Truyen_Thes_TheMaThe",
                table: "Truyen_Thes",
                column: "TheMaThe");

            migrationBuilder.CreateIndex(
                name: "IX_Truyen_Thes_TruyenMaTruyen",
                table: "Truyen_Thes",
                column: "TruyenMaTruyen");

            migrationBuilder.CreateIndex(
                name: "IX_Truyens_NguoiDangId",
                table: "Truyens",
                column: "NguoiDangId");

            migrationBuilder.CreateIndex(
                name: "IX_Truyens_TacGiaMaTacGia",
                table: "Truyens",
                column: "TacGiaMaTacGia");

            migrationBuilder.CreateIndex(
                name: "IX_YeuThichs_NguoiDungId",
                table: "YeuThichs",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_YeuThichs_TruyenMaTruyen",
                table: "YeuThichs",
                column: "TruyenMaTruyen");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BinhLuans");

            migrationBuilder.DropTable(
                name: "DanhDaus");

            migrationBuilder.DropTable(
                name: "DanhGias");

            migrationBuilder.DropTable(
                name: "DayTruyens");

            migrationBuilder.DropTable(
                name: "LichSuDocs");

            migrationBuilder.DropTable(
                name: "LuotXems");

            migrationBuilder.DropTable(
                name: "NoiDungChuongs");

            migrationBuilder.DropTable(
                name: "Truyen_Thes");

            migrationBuilder.DropTable(
                name: "YeuThichs");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Chuongs");

            migrationBuilder.DropTable(
                name: "Thes");

            migrationBuilder.DropTable(
                name: "Truyens");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "TacGias");
        }
    }
}
