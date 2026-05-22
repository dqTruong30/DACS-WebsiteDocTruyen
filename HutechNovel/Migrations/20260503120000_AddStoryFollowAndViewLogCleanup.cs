using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HutechNovel.Migrations
{
    [Migration("20260503120000_AddStoryFollowAndViewLogCleanup")]
    public partial class AddStoryFollowAndViewLogCleanup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Truyens SET MaTacGia = TacGiaMaTacGia WHERE TacGiaMaTacGia IS NOT NULL AND (MaTacGia = 0 OR MaTacGia IS NULL)");
            migrationBuilder.Sql("UPDATE LichSuDocs SET MaChuong = ChuongMaChuong WHERE ChuongMaChuong IS NOT NULL AND (MaChuong = 0 OR MaChuong IS NULL)");
            migrationBuilder.Sql("UPDATE DanhDaus SET MaTruyen = TruyenMaTruyen WHERE TruyenMaTruyen IS NOT NULL AND (MaTruyen = 0 OR MaTruyen IS NULL)");
            migrationBuilder.Sql("UPDATE DanhGias SET MaTruyen = TruyenMaTruyen WHERE TruyenMaTruyen IS NOT NULL AND (MaTruyen = 0 OR MaTruyen IS NULL)");
            migrationBuilder.Sql("UPDATE DayTruyens SET MaTruyen = TruyenMaTruyen WHERE TruyenMaTruyen IS NOT NULL AND (MaTruyen = 0 OR MaTruyen IS NULL)");
            migrationBuilder.Sql("UPDATE LuotXems SET MaTruyen = TruyenMaTruyen WHERE TruyenMaTruyen IS NOT NULL AND (MaTruyen = 0 OR MaTruyen IS NULL)");
            migrationBuilder.Sql("UPDATE YeuThichs SET MaTruyen = TruyenMaTruyen WHERE TruyenMaTruyen IS NOT NULL AND (MaTruyen = 0 OR MaTruyen IS NULL)");

            DropShadowRelationship(migrationBuilder, "DanhDaus", "NguoiDungId", "FK_DanhDaus_AspNetUsers_NguoiDungId", "IX_DanhDaus_NguoiDungId");
            DropShadowRelationship(migrationBuilder, "DanhDaus", "TruyenMaTruyen", "FK_DanhDaus_Truyens_TruyenMaTruyen", "IX_DanhDaus_TruyenMaTruyen");
            DropShadowRelationship(migrationBuilder, "DanhGias", "NguoiDungId", "FK_DanhGias_AspNetUsers_NguoiDungId", "IX_DanhGias_NguoiDungId");
            DropShadowRelationship(migrationBuilder, "DanhGias", "TruyenMaTruyen", "FK_DanhGias_Truyens_TruyenMaTruyen", "IX_DanhGias_TruyenMaTruyen");
            DropShadowRelationship(migrationBuilder, "DayTruyens", "NguoiDungId", "FK_DayTruyens_AspNetUsers_NguoiDungId", "IX_DayTruyens_NguoiDungId");
            DropShadowRelationship(migrationBuilder, "DayTruyens", "TruyenMaTruyen", "FK_DayTruyens_Truyens_TruyenMaTruyen", "IX_DayTruyens_TruyenMaTruyen");
            DropShadowRelationship(migrationBuilder, "LichSuDocs", "NguoiDungId", "FK_LichSuDocs_AspNetUsers_NguoiDungId", "IX_LichSuDocs_NguoiDungId");
            DropShadowRelationship(migrationBuilder, "LichSuDocs", "ChuongMaChuong", "FK_LichSuDocs_Chuongs_ChuongMaChuong", "IX_LichSuDocs_ChuongMaChuong");
            DropShadowRelationship(migrationBuilder, "LuotXems", "NguoiDungId", "FK_LuotXems_AspNetUsers_NguoiDungId", "IX_LuotXems_NguoiDungId");
            DropShadowRelationship(migrationBuilder, "LuotXems", "TruyenMaTruyen", "FK_LuotXems_Truyens_TruyenMaTruyen", "IX_LuotXems_TruyenMaTruyen");
            DropShadowRelationship(migrationBuilder, "YeuThichs", "NguoiDungId", "FK_YeuThichs_AspNetUsers_NguoiDungId", "IX_YeuThichs_NguoiDungId");
            DropShadowRelationship(migrationBuilder, "YeuThichs", "TruyenMaTruyen", "FK_YeuThichs_Truyens_TruyenMaTruyen", "IX_YeuThichs_TruyenMaTruyen");
            DropShadowRelationship(migrationBuilder, "Truyens", "TacGiaMaTacGia", "FK_Truyens_TacGias_TacGiaMaTacGia", "IX_Truyens_TacGiaMaTacGia");

            AlterUserColumn(migrationBuilder, "DanhDaus", nullable: false);
            AlterUserColumn(migrationBuilder, "DanhGias", nullable: false);
            AlterUserColumn(migrationBuilder, "DayTruyens", nullable: false);
            AlterUserColumn(migrationBuilder, "LichSuDocs", nullable: false);
            AlterUserColumn(migrationBuilder, "LuotXems", nullable: true);
            AlterUserColumn(migrationBuilder, "YeuThichs", nullable: false);

            CreateInteractionIndexesAndKeys(migrationBuilder);

            migrationBuilder.CreateTable(
                name: "TheoDoiTruyens",
                columns: table => new
                {
                    MaTheoDoi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaTruyen = table.Column<int>(type: "int", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TheoDoiTruyens", x => x.MaTheoDoi);
                    table.ForeignKey(
                        name: "FK_TheoDoiTruyens_AspNetUsers_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TheoDoiTruyens_Truyens_MaTruyen",
                        column: x => x.MaTruyen,
                        principalTable: "Truyens",
                        principalColumn: "MaTruyen",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TheoDoiTruyens_MaTruyen",
                table: "TheoDoiTruyens",
                column: "MaTruyen");

            migrationBuilder.CreateIndex(
                name: "IX_TheoDoiTruyens_MaNguoiDung_MaTruyen",
                table: "TheoDoiTruyens",
                columns: new[] { "MaNguoiDung", "MaTruyen" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "TheoDoiTruyens");

            migrationBuilder.DropForeignKey(name: "FK_LuotXems_AspNetUsers_MaNguoiDung", table: "LuotXems");
            migrationBuilder.AlterColumn<string>(
                name: "MaNguoiDung",
                table: "LuotXems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }

        private static void DropShadowRelationship(MigrationBuilder migrationBuilder, string table, string column, string foreignKey, string index)
        {
            migrationBuilder.DropForeignKey(name: foreignKey, table: table);
            migrationBuilder.DropIndex(name: index, table: table);
            migrationBuilder.DropColumn(name: column, table: table);
        }

        private static void AlterUserColumn(MigrationBuilder migrationBuilder, string table, bool nullable)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MaNguoiDung",
                table: table,
                type: "nvarchar(450)",
                nullable: nullable,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: nullable);
        }

        private static void CreateInteractionIndexesAndKeys(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(name: "IX_DanhDaus_MaNguoiDung", table: "DanhDaus", column: "MaNguoiDung");
            migrationBuilder.CreateIndex(name: "IX_DanhDaus_MaTruyen", table: "DanhDaus", column: "MaTruyen");
            migrationBuilder.CreateIndex(name: "IX_DanhGias_MaNguoiDung", table: "DanhGias", column: "MaNguoiDung");
            migrationBuilder.CreateIndex(name: "IX_DanhGias_MaTruyen", table: "DanhGias", column: "MaTruyen");
            migrationBuilder.CreateIndex(name: "IX_DayTruyens_MaNguoiDung", table: "DayTruyens", column: "MaNguoiDung");
            migrationBuilder.CreateIndex(name: "IX_DayTruyens_MaTruyen", table: "DayTruyens", column: "MaTruyen");
            migrationBuilder.CreateIndex(name: "IX_LichSuDocs_MaNguoiDung", table: "LichSuDocs", column: "MaNguoiDung");
            migrationBuilder.CreateIndex(name: "IX_LichSuDocs_MaChuong", table: "LichSuDocs", column: "MaChuong");
            migrationBuilder.CreateIndex(name: "IX_LuotXems_MaNguoiDung", table: "LuotXems", column: "MaNguoiDung");
            migrationBuilder.CreateIndex(name: "IX_LuotXems_MaTruyen", table: "LuotXems", column: "MaTruyen");
            migrationBuilder.CreateIndex(name: "IX_YeuThichs_MaNguoiDung", table: "YeuThichs", column: "MaNguoiDung");
            migrationBuilder.CreateIndex(name: "IX_YeuThichs_MaTruyen", table: "YeuThichs", column: "MaTruyen");
            migrationBuilder.CreateIndex(name: "IX_Truyens_MaTacGia", table: "Truyens", column: "MaTacGia");

            migrationBuilder.AddForeignKey(name: "FK_DanhDaus_AspNetUsers_MaNguoiDung", table: "DanhDaus", column: "MaNguoiDung", principalTable: "AspNetUsers", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_DanhDaus_Truyens_MaTruyen", table: "DanhDaus", column: "MaTruyen", principalTable: "Truyens", principalColumn: "MaTruyen", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_DanhGias_AspNetUsers_MaNguoiDung", table: "DanhGias", column: "MaNguoiDung", principalTable: "AspNetUsers", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_DanhGias_Truyens_MaTruyen", table: "DanhGias", column: "MaTruyen", principalTable: "Truyens", principalColumn: "MaTruyen", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_DayTruyens_AspNetUsers_MaNguoiDung", table: "DayTruyens", column: "MaNguoiDung", principalTable: "AspNetUsers", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_DayTruyens_Truyens_MaTruyen", table: "DayTruyens", column: "MaTruyen", principalTable: "Truyens", principalColumn: "MaTruyen", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_LichSuDocs_AspNetUsers_MaNguoiDung", table: "LichSuDocs", column: "MaNguoiDung", principalTable: "AspNetUsers", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_LichSuDocs_Chuongs_MaChuong", table: "LichSuDocs", column: "MaChuong", principalTable: "Chuongs", principalColumn: "MaChuong", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_LuotXems_AspNetUsers_MaNguoiDung", table: "LuotXems", column: "MaNguoiDung", principalTable: "AspNetUsers", principalColumn: "Id", onDelete: ReferentialAction.NoAction);
            migrationBuilder.AddForeignKey(name: "FK_LuotXems_Truyens_MaTruyen", table: "LuotXems", column: "MaTruyen", principalTable: "Truyens", principalColumn: "MaTruyen", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_YeuThichs_AspNetUsers_MaNguoiDung", table: "YeuThichs", column: "MaNguoiDung", principalTable: "AspNetUsers", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_YeuThichs_Truyens_MaTruyen", table: "YeuThichs", column: "MaTruyen", principalTable: "Truyens", principalColumn: "MaTruyen", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_Truyens_TacGias_MaTacGia", table: "Truyens", column: "MaTacGia", principalTable: "TacGias", principalColumn: "MaTacGia", onDelete: ReferentialAction.Cascade);
        }
    }
}
