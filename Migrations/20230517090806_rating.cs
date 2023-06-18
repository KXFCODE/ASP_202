using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP_202.Migrations
{
    /// <inheritdoc />
    public partial class rating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.RenameTable(
                name: "Rate",
                newName: "Rates");

            migrationBuilder.RenameTable(
                name: "Post",
                newName: "Posts");
            

            migrationBuilder.CreateIndex(
                name: "IX_Themes_AuthorId",
                table: "Themes",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_AuthorId",
                table: "Sections",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_AuthorId",
                table: "Posts",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_ReplyId",
                table: "Posts",
                column: "ReplyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropIndex(
                name: "IX_Themes_AuthorId",
                table: "Themes");

            migrationBuilder.DropIndex(
                name: "IX_Sections_AuthorId",
                table: "Sections");

            migrationBuilder.DropIndex(
                name: "IX_Posts_AuthorId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_ReplyId",
                table: "Posts");

            migrationBuilder.RenameTable(
                name: "Rates",
                newName: "Rate");

            migrationBuilder.RenameTable(
                name: "Posts",
                newName: "Post");
        }
    }
}
