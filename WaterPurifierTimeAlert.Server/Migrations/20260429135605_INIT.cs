using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaterPurifierTimeAlert.Server.Migrations
{
    /// <inheritdoc />
    public partial class INIT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExchangeFilter",
                columns: table => new
                {
                    FilterName = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    LastExchnageDate = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeFilter", x => x.FilterName);
                });

            migrationBuilder.CreateTable(
                name: "FilterType",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ExpireTime = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterType", x => x.Name);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExchangeFilter");

            migrationBuilder.DropTable(
                name: "FilterType");
        }
    }
}
