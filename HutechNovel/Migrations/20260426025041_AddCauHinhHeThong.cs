using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HutechNovel.Migrations
{
    /// <inheritdoc />
    public partial class AddCauHinhHeThong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CauHinhHeThongs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenWebsite = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ThongBaoToanCuc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CheDoBaoTri = table.Column<bool>(type: "bit", nullable: false),
                    TieuDeSEO = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MoTaSEO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailLienHe = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHinhHeThongs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CauHinhHeThongs");
        }
    }
}
