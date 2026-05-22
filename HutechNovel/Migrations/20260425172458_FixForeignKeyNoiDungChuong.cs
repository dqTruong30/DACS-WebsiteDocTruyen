using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HutechNovel.Migrations
{
    /// <inheritdoc />
    public partial class FixForeignKeyNoiDungChuong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NoiDungChuongs_Chuongs_ChuongMaChuong",
                table: "NoiDungChuongs");

            migrationBuilder.DropIndex(
                name: "IX_NoiDungChuongs_ChuongMaChuong",
                table: "NoiDungChuongs");

            migrationBuilder.DropColumn(
                name: "ChuongMaChuong",
                table: "NoiDungChuongs");

            migrationBuilder.AddForeignKey(
                name: "FK_NoiDungChuongs_Chuongs_MaChuong",
                table: "NoiDungChuongs",
                column: "MaChuong",
                principalTable: "Chuongs",
                principalColumn: "MaChuong",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NoiDungChuongs_Chuongs_MaChuong",
                table: "NoiDungChuongs");

            migrationBuilder.AddColumn<int>(
                name: "ChuongMaChuong",
                table: "NoiDungChuongs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_NoiDungChuongs_ChuongMaChuong",
                table: "NoiDungChuongs",
                column: "ChuongMaChuong");

            migrationBuilder.AddForeignKey(
                name: "FK_NoiDungChuongs_Chuongs_ChuongMaChuong",
                table: "NoiDungChuongs",
                column: "ChuongMaChuong",
                principalTable: "Chuongs",
                principalColumn: "MaChuong",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
