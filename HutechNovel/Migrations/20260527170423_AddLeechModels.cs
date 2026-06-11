using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HutechNovel.Migrations
{
    /// <inheritdoc />
    public partial class AddLeechModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CauHinhLeeches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Domain = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TitleSelector = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentSelector = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NextChapterSelector = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHinhLeeches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TienTrinhLeeches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTruyen = table.Column<int>(type: "int", nullable: false),
                    UrlHienTai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoChuongDaCao = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ThongBaoLoi = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TienTrinhLeeches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TienTrinhLeeches_Truyens_MaTruyen",
                        column: x => x.MaTruyen,
                        principalTable: "Truyens",
                        principalColumn: "MaTruyen",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TienTrinhLeeches_MaTruyen",
                table: "TienTrinhLeeches",
                column: "MaTruyen");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CauHinhLeeches");

            migrationBuilder.DropTable(
                name: "TienTrinhLeeches");
        }
    }
}
