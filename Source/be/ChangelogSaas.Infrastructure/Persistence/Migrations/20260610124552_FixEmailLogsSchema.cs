using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangelogSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixEmailLogsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SendAt",
                table: "EmailLogs",
                newName: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "EmailLogs",
                newName: "SendAt");
        }
    }
}
