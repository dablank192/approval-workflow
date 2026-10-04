using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApprovalWorkflow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WorkflowStepCompositeIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkFlowSteps_WorkFlowId",
                table: "WorkFlowSteps");

            migrationBuilder.CreateIndex(
                name: "IX_WorkFlowSteps_WorkFlowId_StepOrder",
                table: "WorkFlowSteps",
                columns: new[] { "WorkFlowId", "StepOrder" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkFlowSteps_WorkFlowId_StepOrder",
                table: "WorkFlowSteps");

            migrationBuilder.CreateIndex(
                name: "IX_WorkFlowSteps_WorkFlowId",
                table: "WorkFlowSteps",
                column: "WorkFlowId");
        }
    }
}
