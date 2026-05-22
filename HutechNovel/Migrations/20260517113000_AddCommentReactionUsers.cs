using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HutechNovel.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentReactionUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BinhLuanCamXucs",
                columns: table => new
                {
                    MaCamXuc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaBinhLuan = table.Column<int>(type: "int", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BinhLuanCamXucs", x => x.MaCamXuc);
                    table.ForeignKey(
                        name: "FK_BinhLuanCamXucs_AspNetUsers_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BinhLuanCamXucs_BinhLuans_MaBinhLuan",
                        column: x => x.MaBinhLuan,
                        principalTable: "BinhLuans",
                        principalColumn: "MaBinhLuan",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BinhLuanCamXucs_MaBinhLuan",
                table: "BinhLuanCamXucs",
                column: "MaBinhLuan");

            migrationBuilder.CreateIndex(
                name: "IX_BinhLuanCamXucs_MaNguoiDung_MaBinhLuan",
                table: "BinhLuanCamXucs",
                columns: new[] { "MaNguoiDung", "MaBinhLuan" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BinhLuanCamXucs");
        }
    }
}
