using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HutechNovel.Migrations
{
    /// <inheritdoc />
    public partial class CleanUpShadowProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chuongs_Truyens_TruyenMaTruyen",
                table: "Chuongs");

            migrationBuilder.DropIndex(
                name: "IX_Chuongs_TruyenMaTruyen",
                table: "Chuongs");

            migrationBuilder.DropColumn(
                name: "TruyenMaTruyen",
                table: "Chuongs");

            migrationBuilder.AddForeignKey(
                name: "FK_Chuongs_Truyens_MaTruyen",
                table: "Chuongs",
                column: "MaTruyen",
                principalTable: "Truyens",
                principalColumn: "MaTruyen",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chuongs_Truyens_MaTruyen",
                table: "Chuongs");

            migrationBuilder.AddColumn<int>(
                name: "TruyenMaTruyen",
                table: "Chuongs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Chuongs_TruyenMaTruyen",
                table: "Chuongs",
                column: "TruyenMaTruyen");

            migrationBuilder.AddForeignKey(
                name: "FK_Chuongs_Truyens_TruyenMaTruyen",
                table: "Chuongs",
                column: "TruyenMaTruyen",
                principalTable: "Truyens",
                principalColumn: "MaTruyen",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
