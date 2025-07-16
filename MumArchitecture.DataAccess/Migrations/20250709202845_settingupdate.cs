using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MumArchitecture.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class settingupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Setting");

            migrationBuilder.AlterColumn<string>(
                name: "SettingType",
                table: "Setting",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SettingType",
                table: "Setting",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Setting",
                type: "text",
                nullable: true);
        }
    }
}
