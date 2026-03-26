using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Piedrazul.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Duplicate of AddAuditLog migration - no-op
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op
        }
    }
}
