using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApprovalWorkflow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompositeUniqueIndexForWpMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkSpaceMember_WorkSpaceId",
                table: "WorkSpaceMember");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSpaceMember_WorkSpaceId_UserId",
                table: "WorkSpaceMember",
                columns: new[] { "WorkSpaceId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkSpaceMember_WorkSpaceId_UserId",
                table: "WorkSpaceMember");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSpaceMember_WorkSpaceId",
                table: "WorkSpaceMember",
                column: "WorkSpaceId");
        }
    }
}
