using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace portfolioAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddContactMessageWithFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "ContactMessages");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "ContactMessages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "FileData",
                table: "ContactMessages",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "ContactMessages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "ContactMessages");

            migrationBuilder.DropColumn(
                name: "FileData",
                table: "ContactMessages");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "ContactMessages");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "ContactMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
