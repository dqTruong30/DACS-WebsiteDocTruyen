using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HutechNovel.Migrations
{
    /// <inheritdoc />
    public partial class AddLaPhuChuong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "LaPhuChuong",
                table: "Chuongs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LaPhuChuong",
                table: "Chuongs");
        }
    }
}
