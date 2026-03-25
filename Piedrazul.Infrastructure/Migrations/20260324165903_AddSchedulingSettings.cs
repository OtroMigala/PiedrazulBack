using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Piedrazul.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSchedulingSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SchedulingSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WeeksAhead = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulingSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchedulingSettings");
        }
    }
}
