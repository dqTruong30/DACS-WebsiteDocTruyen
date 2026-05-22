using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HutechNovel.Migrations
{
    /// <inheritdoc />
    public partial class updateGiaoDien : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DanhDaus_AspNetUsers_NguoiDungId",
                table: "DanhDaus");

            migrationBuilder.DropForeignKey(
                name: "FK_DanhDaus_Truyens_TruyenMaTruyen",
                table: "DanhDaus");

            migrationBuilder.DropForeignKey(
                name: "FK_DanhGias_AspNetUsers_NguoiDungId",
                table: "DanhGias");

            migrationBuilder.DropForeignKey(
                name: "FK_DanhGias_Truyens_TruyenMaTruyen",
                table: "DanhGias");

            migrationBuilder.DropForeignKey(
                name: "FK_DayTruyens_AspNetUsers_NguoiDungId",
                table: "DayTruyens");

            migrationBuilder.DropForeignKey(
                name: "FK_DayTruyens_Truyens_TruyenMaTruyen",
                table: "DayTruyens");

            migrationBuilder.DropForeignKey(
                name: "FK_LichSuDocs_AspNetUsers_NguoiDungId",
                table: "LichSuDocs");

            migrationBuilder.DropForeignKey(
                name: "FK_LichSuDocs_Chuongs_ChuongMaChuong",
                table: "LichSuDocs");

            migrationBuilder.DropForeignKey(
                name: "FK_LuotXems_AspNetUsers_NguoiDungId",
                table: "LuotXems");

            migrationBuilder.DropForeignKey(
                name: "FK_LuotXems_Truyens_TruyenMaTruyen",
                table: "LuotXems");

            migrationBuilder.DropForeignKey(
                name: "FK_Truyens_TacGias_TacGiaMaTacGia",
                table: "Truyens");

            migrationBuilder.DropForeignKey(
                name: "FK_YeuThichs_AspNetUsers_NguoiDungId",
                table: "YeuThichs");

            migrationBuilder.DropForeignKey(
                name: "FK_YeuThichs_Truyens_TruyenMaTruyen",
                table: "YeuThichs");

            migrationBuilder.DropIndex(
                name: "IX_YeuThichs_NguoiDungId",
                table: "YeuThichs");

            migrationBuilder.DropIndex(
                name: "IX_YeuThichs_TruyenMaTruyen",
                table: "YeuThichs");

            migrationBuilder.DropIndex(
                name: "IX_Truyens_TacGiaMaTacGia",
                table: "Truyens");

            migrationBuilder.DropIndex(
                name: "IX_LuotXems_NguoiDungId",
                table: "LuotXems");

            migrationBuilder.DropIndex(
                name: "IX_LuotXems_TruyenMaTruyen",
                table: "LuotXems");

            migrationBuilder.DropIndex(
                name: "IX_LichSuDocs_ChuongMaChuong",
                table: "LichSuDocs");

            migrationBuilder.DropIndex(
                name: "IX_LichSuDocs_NguoiDungId",
                table: "LichSuDocs");

            migrationBuilder.DropIndex(
                name: "IX_DayTruyens_NguoiDungId",
                table: "DayTruyens");

            migrationBuilder.DropIndex(
                name: "IX_DayTruyens_TruyenMaTruyen",
                table: "DayTruyens");

            migrationBuilder.DropIndex(
                name: "IX_DanhGias_NguoiDungId",
                table: "DanhGias");

            migrationBuilder.DropIndex(
                name: "IX_DanhGias_TruyenMaTruyen",
                table: "DanhGias");

            migrationBuilder.DropIndex(
                name: "IX_DanhDaus_NguoiDungId",
                table: "DanhDaus");

            migrationBuilder.DropIndex(
                name: "IX_DanhDaus_TruyenMaTruyen",
                table: "DanhDaus");

            migrationBuilder.DropColumn(
                name: "NguoiDungId",
                table: "YeuThichs");

            migrationBuilder.DropColumn(
                name: "TruyenMaTruyen",
                table: "YeuThichs");

            migrationBuilder.DropColumn(
                name: "TacGiaMaTacGia",
                table: "Truyens");

            migrationBuilder.DropColumn(
                name: "NguoiDungId",
                table: "LuotXems");

            migrationBuilder.DropColumn(
                name: "TruyenMaTruyen",
                table: "LuotXems");

            migrationBuilder.DropColumn(
                name: "ChuongMaChuong",
                table: "LichSuDocs");

            migrationBuilder.DropColumn(
                name: "NguoiDungId",
                table: "LichSuDocs");

            migrationBuilder.DropColumn(
                name: "NguoiDungId",
                table: "DayTruyens");

            migrationBuilder.DropColumn(
                name: "TruyenMaTruyen",
                table: "DayTruyens");

            migrationBuilder.DropColumn(
                name: "NguoiDungId",
                table: "DanhGias");

            migrationBuilder.DropColumn(
                name: "TruyenMaTruyen",
                table: "DanhGias");

            migrationBuilder.DropColumn(
                name: "NguoiDungId",
                table: "DanhDaus");

            migrationBuilder.DropColumn(
                name: "TruyenMaTruyen",
                table: "DanhDaus");

            migrationBuilder.AlterColumn<string>(
                name: "MaNguoiDung",
                table: "YeuThichs",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MaNguoiDung",
                table: "LuotXems",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MaNguoiDung",
                table: "LichSuDocs",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MaNguoiDung",
                table: "DayTruyens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MaNguoiDung",
                table: "DanhGias",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MaNguoiDung",
                table: "DanhDaus",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

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
                name: "IX_YeuThichs_MaNguoiDung",
                table: "YeuThichs",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_YeuThichs_MaTruyen",
                table: "YeuThichs",
                column: "MaTruyen");

            migrationBuilder.CreateIndex(
                name: "IX_Truyens_MaTacGia",
                table: "Truyens",
                column: "MaTacGia");

            migrationBuilder.CreateIndex(
                name: "IX_LuotXems_MaNguoiDung",
                table: "LuotXems",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_LuotXems_MaTruyen",
                table: "LuotXems",
                column: "MaTruyen");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuDocs_MaChuong",
                table: "LichSuDocs",
                column: "MaChuong");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuDocs_MaNguoiDung",
                table: "LichSuDocs",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_DayTruyens_MaNguoiDung",
                table: "DayTruyens",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_DayTruyens_MaTruyen",
                table: "DayTruyens",
                column: "MaTruyen");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGias_MaNguoiDung",
                table: "DanhGias",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGias_MaTruyen",
                table: "DanhGias",
                column: "MaTruyen");

            migrationBuilder.CreateIndex(
                name: "IX_DanhDaus_MaNguoiDung",
                table: "DanhDaus",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_DanhDaus_MaTruyen",
                table: "DanhDaus",
                column: "MaTruyen");

            migrationBuilder.CreateIndex(
                name: "IX_TheoDoiTruyens_MaNguoiDung_MaTruyen",
                table: "TheoDoiTruyens",
                columns: new[] { "MaNguoiDung", "MaTruyen" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TheoDoiTruyens_MaTruyen",
                table: "TheoDoiTruyens",
                column: "MaTruyen");

            migrationBuilder.AddForeignKey(
                name: "FK_DanhDaus_AspNetUsers_MaNguoiDung",
                table: "DanhDaus",
                column: "MaNguoiDung",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DanhDaus_Truyens_MaTruyen",
                table: "DanhDaus",
                column: "MaTruyen",
                principalTable: "Truyens",
                principalColumn: "MaTruyen",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DanhGias_AspNetUsers_MaNguoiDung",
                table: "DanhGias",
                column: "MaNguoiDung",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DanhGias_Truyens_MaTruyen",
                table: "DanhGias",
                column: "MaTruyen",
                principalTable: "Truyens",
                principalColumn: "MaTruyen",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DayTruyens_AspNetUsers_MaNguoiDung",
                table: "DayTruyens",
                column: "MaNguoiDung",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DayTruyens_Truyens_MaTruyen",
                table: "DayTruyens",
                column: "MaTruyen",
                principalTable: "Truyens",
                principalColumn: "MaTruyen",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuDocs_AspNetUsers_MaNguoiDung",
                table: "LichSuDocs",
                column: "MaNguoiDung",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuDocs_Chuongs_MaChuong",
                table: "LichSuDocs",
                column: "MaChuong",
                principalTable: "Chuongs",
                principalColumn: "MaChuong",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LuotXems_AspNetUsers_MaNguoiDung",
                table: "LuotXems",
                column: "MaNguoiDung",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LuotXems_Truyens_MaTruyen",
                table: "LuotXems",
                column: "MaTruyen",
                principalTable: "Truyens",
                principalColumn: "MaTruyen",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Truyens_TacGias_MaTacGia",
                table: "Truyens",
                column: "MaTacGia",
                principalTable: "TacGias",
                principalColumn: "MaTacGia",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YeuThichs_AspNetUsers_MaNguoiDung",
                table: "YeuThichs",
                column: "MaNguoiDung",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YeuThichs_Truyens_MaTruyen",
                table: "YeuThichs",
                column: "MaTruyen",
                principalTable: "Truyens",
                principalColumn: "MaTruyen",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DanhDaus_AspNetUsers_MaNguoiDung",
                table: "DanhDaus");

            migrationBuilder.DropForeignKey(
                name: "FK_DanhDaus_Truyens_MaTruyen",
                table: "DanhDaus");

            migrationBuilder.DropForeignKey(
                name: "FK_DanhGias_AspNetUsers_MaNguoiDung",
                table: "DanhGias");

            migrationBuilder.DropForeignKey(
                name: "FK_DanhGias_Truyens_MaTruyen",
                table: "DanhGias");

            migrationBuilder.DropForeignKey(
                name: "FK_DayTruyens_AspNetUsers_MaNguoiDung",
                table: "DayTruyens");

            migrationBuilder.DropForeignKey(
                name: "FK_DayTruyens_Truyens_MaTruyen",
                table: "DayTruyens");

            migrationBuilder.DropForeignKey(
                name: "FK_LichSuDocs_AspNetUsers_MaNguoiDung",
                table: "LichSuDocs");

            migrationBuilder.DropForeignKey(
                name: "FK_LichSuDocs_Chuongs_MaChuong",
                table: "LichSuDocs");

            migrationBuilder.DropForeignKey(
                name: "FK_LuotXems_AspNetUsers_MaNguoiDung",
                table: "LuotXems");

            migrationBuilder.DropForeignKey(
                name: "FK_LuotXems_Truyens_MaTruyen",
                table: "LuotXems");

            migrationBuilder.DropForeignKey(
                name: "FK_Truyens_TacGias_MaTacGia",
                table: "Truyens");

            migrationBuilder.DropForeignKey(
                name: "FK_YeuThichs_AspNetUsers_MaNguoiDung",
                table: "YeuThichs");

            migrationBuilder.DropForeignKey(
                name: "FK_YeuThichs_Truyens_MaTruyen",
                table: "YeuThichs");

            migrationBuilder.DropTable(
                name: "TheoDoiTruyens");

            migrationBuilder.DropIndex(
                name: "IX_YeuThichs_MaNguoiDung",
                table: "YeuThichs");

            migrationBuilder.DropIndex(
                name: "IX_YeuThichs_MaTruyen",
                table: "YeuThichs");

            migrationBuilder.DropIndex(
                name: "IX_Truyens_MaTacGia",
                table: "Truyens");

            migrationBuilder.DropIndex(
                name: "IX_LuotXems_MaNguoiDung",
                table: "LuotXems");

            migrationBuilder.DropIndex(
                name: "IX_LuotXems_MaTruyen",
                table: "LuotXems");

            migrationBuilder.DropIndex(
                name: "IX_LichSuDocs_MaChuong",
                table: "LichSuDocs");

            migrationBuilder.DropIndex(
                name: "IX_LichSuDocs_MaNguoiDung",
                table: "LichSuDocs");

            migrationBuilder.DropIndex(
                name: "IX_DayTruyens_MaNguoiDung",
                table: "DayTruyens");

            migrationBuilder.DropIndex(
                name: "IX_DayTruyens_MaTruyen",
                table: "DayTruyens");

            migrationBuilder.DropIndex(
                name: "IX_DanhGias_MaNguoiDung",
                table: "DanhGias");

            migrationBuilder.DropIndex(
                name: "IX_DanhGias_MaTruyen",
                table: "DanhGias");

            migrationBuilder.DropIndex(
                name: "IX_DanhDaus_MaNguoiDung",
                table: "DanhDaus");

            migrationBuilder.DropIndex(
                name: "IX_DanhDaus_MaTruyen",
                table: "DanhDaus");

            migrationBuilder.AlterColumn<string>(
                name: "MaNguoiDung",
                table: "YeuThichs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "NguoiDungId",
                table: "YeuThichs",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TruyenMaTruyen",
                table: "YeuThichs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TacGiaMaTacGia",
                table: "Truyens",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "MaNguoiDung",
                table: "LuotXems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguoiDungId",
                table: "LuotXems",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TruyenMaTruyen",
                table: "LuotXems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "MaNguoiDung",
                table: "LichSuDocs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "ChuongMaChuong",
                table: "LichSuDocs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NguoiDungId",
                table: "LichSuDocs",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MaNguoiDung",
                table: "DayTruyens",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "NguoiDungId",
                table: "DayTruyens",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TruyenMaTruyen",
                table: "DayTruyens",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "MaNguoiDung",
                table: "DanhGias",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "NguoiDungId",
                table: "DanhGias",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TruyenMaTruyen",
                table: "DanhGias",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "MaNguoiDung",
                table: "DanhDaus",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "NguoiDungId",
                table: "DanhDaus",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TruyenMaTruyen",
                table: "DanhDaus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_YeuThichs_NguoiDungId",
                table: "YeuThichs",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_YeuThichs_TruyenMaTruyen",
                table: "YeuThichs",
                column: "TruyenMaTruyen");

            migrationBuilder.CreateIndex(
                name: "IX_Truyens_TacGiaMaTacGia",
                table: "Truyens",
                column: "TacGiaMaTacGia");

            migrationBuilder.CreateIndex(
                name: "IX_LuotXems_NguoiDungId",
                table: "LuotXems",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_LuotXems_TruyenMaTruyen",
                table: "LuotXems",
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
                name: "IX_DayTruyens_NguoiDungId",
                table: "DayTruyens",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_DayTruyens_TruyenMaTruyen",
                table: "DayTruyens",
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
                name: "IX_DanhDaus_NguoiDungId",
                table: "DanhDaus",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_DanhDaus_TruyenMaTruyen",
                table: "DanhDaus",
                column: "TruyenMaTruyen");

            migrationBuilder.AddForeignKey(
                name: "FK_DanhDaus_AspNetUsers_NguoiDungId",
                table: "DanhDaus",
                column: "NguoiDungId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DanhDaus_Truyens_TruyenMaTruyen",
                table: "DanhDaus",
                column: "TruyenMaTruyen",
                principalTable: "Truyens",
                principalColumn: "MaTruyen",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DanhGias_AspNetUsers_NguoiDungId",
                table: "DanhGias",
                column: "NguoiDungId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DanhGias_Truyens_TruyenMaTruyen",
                table: "DanhGias",
                column: "TruyenMaTruyen",
                principalTable: "Truyens",
                principalColumn: "MaTruyen",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DayTruyens_AspNetUsers_NguoiDungId",
                table: "DayTruyens",
                column: "NguoiDungId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DayTruyens_Truyens_TruyenMaTruyen",
                table: "DayTruyens",
                column: "TruyenMaTruyen",
                principalTable: "Truyens",
                principalColumn: "MaTruyen",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuDocs_AspNetUsers_NguoiDungId",
                table: "LichSuDocs",
                column: "NguoiDungId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuDocs_Chuongs_ChuongMaChuong",
                table: "LichSuDocs",
                column: "ChuongMaChuong",
                principalTable: "Chuongs",
                principalColumn: "MaChuong",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LuotXems_AspNetUsers_NguoiDungId",
                table: "LuotXems",
                column: "NguoiDungId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LuotXems_Truyens_TruyenMaTruyen",
                table: "LuotXems",
                column: "TruyenMaTruyen",
                principalTable: "Truyens",
                principalColumn: "MaTruyen",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Truyens_TacGias_TacGiaMaTacGia",
                table: "Truyens",
                column: "TacGiaMaTacGia",
                principalTable: "TacGias",
                principalColumn: "MaTacGia",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YeuThichs_AspNetUsers_NguoiDungId",
                table: "YeuThichs",
                column: "NguoiDungId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YeuThichs_Truyens_TruyenMaTruyen",
                table: "YeuThichs",
                column: "TruyenMaTruyen",
                principalTable: "Truyens",
                principalColumn: "MaTruyen",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
