using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HutechNovel.Migrations
{
    /// <inheritdoc />
    public partial class AddNhiemVu_HutechXu_Cultivation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chuongs_MaTruyen_SoChuong",
                table: "Chuongs");

            migrationBuilder.AddColumn<int>(
                name: "DiemKinhNghiem",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HutechXu",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "NhiemVus",
                columns: table => new
                {
                    MaNhiemVu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenNhiemVu = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoaiNhiemVu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PhanThuongXu = table.Column<int>(type: "int", nullable: false),
                    PhanThuongKinhNghiem = table.Column<int>(type: "int", nullable: false),
                    LoaiDieuKien = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GiaTriYeuCau = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhiemVus", x => x.MaNhiemVu);
                });

            migrationBuilder.CreateTable(
                name: "LichSuNhiemVus",
                columns: table => new
                {
                    MaLichSu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaNhiemVu = table.Column<int>(type: "int", nullable: false),
                    TienDo = table.Column<int>(type: "int", nullable: false),
                    DaHoanThanh = table.Column<bool>(type: "bit", nullable: false),
                    DaNhanThuong = table.Column<bool>(type: "bit", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuNhiemVus", x => x.MaLichSu);
                    table.ForeignKey(
                        name: "FK_LichSuNhiemVus_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LichSuNhiemVus_NhiemVus_MaNhiemVu",
                        column: x => x.MaNhiemVu,
                        principalTable: "NhiemVus",
                        principalColumn: "MaNhiemVu",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chuongs_MaTruyen_SoChuong",
                table: "Chuongs",
                columns: new[] { "MaTruyen", "SoChuong" });

            migrationBuilder.CreateIndex(
                name: "IX_LichSuNhiemVus_MaNhiemVu",
                table: "LichSuNhiemVus",
                column: "MaNhiemVu");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuNhiemVus_UserId",
                table: "LichSuNhiemVus",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LichSuNhiemVus");

            migrationBuilder.DropTable(
                name: "NhiemVus");

            migrationBuilder.DropIndex(
                name: "IX_Chuongs_MaTruyen_SoChuong",
                table: "Chuongs");

            migrationBuilder.DropColumn(
                name: "DiemKinhNghiem",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "HutechXu",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_Chuongs_MaTruyen_SoChuong",
                table: "Chuongs",
                columns: new[] { "MaTruyen", "SoChuong" },
                unique: true);
        }
    }
}
