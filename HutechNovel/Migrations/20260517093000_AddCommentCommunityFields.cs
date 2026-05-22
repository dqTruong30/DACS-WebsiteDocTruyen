using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HutechNovel.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentCommunityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DaGhim",
                table: "BinhLuans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LaSpoiler",
                table: "BinhLuans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SoBaoCao",
                table: "BinhLuans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SoCamXuc",
                table: "BinhLuans",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaGhim",
                table: "BinhLuans");

            migrationBuilder.DropColumn(
                name: "LaSpoiler",
                table: "BinhLuans");

            migrationBuilder.DropColumn(
                name: "SoBaoCao",
                table: "BinhLuans");

            migrationBuilder.DropColumn(
                name: "SoCamXuc",
                table: "BinhLuans");
        }
    }
}
