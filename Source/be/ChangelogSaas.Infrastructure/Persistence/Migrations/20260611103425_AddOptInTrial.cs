using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangelogSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOptInTrial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Applied to DB as no-op; actual schema changes are in AddProjectIsLocked
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
