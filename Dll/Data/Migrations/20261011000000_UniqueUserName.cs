using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dll.Data.Migrations
{
    /// <summary>
    /// Makes ApUser.UserName unique (two accounts can no longer share a user name) and marks existing
    /// accounts with IsActive = NULL as active.
    /// Before applying, check for duplicates and names longer than 256 characters:
    ///   SELECT UserName, COUNT(*) FROM ApUser GROUP BY UserName HAVING COUNT(*) > 1;
    ///   SELECT Id, UserName FROM ApUser WHERE LEN(UserName) > 256;
    /// </summary>
    public partial class UniqueUserName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "ApUser",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            // Accounts created before this change never had IsActive set (NULL). Only IsActive = 1 may sign in
            // now, so mark those existing accounts active. Disabled accounts (IsActive = 0) stay disabled.
            migrationBuilder.Sql("UPDATE ApUser SET IsActive = 1 WHERE IsActive IS NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_ApUser_UserName",
                table: "ApUser",
                column: "UserName",
                unique: true,
                filter: "[UserName] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApUser_UserName",
                table: "ApUser");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "ApUser",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);
        }
    }
}
