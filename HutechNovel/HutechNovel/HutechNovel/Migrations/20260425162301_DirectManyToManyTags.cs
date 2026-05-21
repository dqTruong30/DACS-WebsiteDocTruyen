using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HutechNovel.Migrations
{
    /// <inheritdoc />
    public partial class DirectManyToManyTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Truyen_Thes");

            migrationBuilder.CreateTable(
                name: "TheTruyen",
                columns: table => new
                {
                    ThesMaThe = table.Column<int>(type: "int", nullable: false),
                    TruyensMaTruyen = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TheTruyen", x => new { x.ThesMaThe, x.TruyensMaTruyen });
                    table.ForeignKey(
                        name: "FK_TheTruyen_Thes_ThesMaThe",
                        column: x => x.ThesMaThe,
                        principalTable: "Thes",
                        principalColumn: "MaThe",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TheTruyen_Truyens_TruyensMaTruyen",
                        column: x => x.TruyensMaTruyen,
                        principalTable: "Truyens",
                        principalColumn: "MaTruyen",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TheTruyen_TruyensMaTruyen",
                table: "TheTruyen",
                column: "TruyensMaTruyen");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TheTruyen");

            migrationBuilder.CreateTable(
                name: "Truyen_Thes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TheMaThe = table.Column<int>(type: "int", nullable: false),
                    TruyenMaTruyen = table.Column<int>(type: "int", nullable: false),
                    MaThe = table.Column<int>(type: "int", nullable: false),
                    MaTruyen = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_Truyen_Thes_TheMaThe",
                table: "Truyen_Thes",
                column: "TheMaThe");

            migrationBuilder.CreateIndex(
                name: "IX_Truyen_Thes_TruyenMaTruyen",
                table: "Truyen_Thes",
                column: "TruyenMaTruyen");
        }
    }
}
