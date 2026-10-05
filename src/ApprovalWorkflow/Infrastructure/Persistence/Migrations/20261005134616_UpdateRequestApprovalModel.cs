using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApprovalWorkflow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRequestApprovalModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestApproval_User_ApprovalUserId",
                table: "RequestApproval");

            migrationBuilder.DropForeignKey(
                name: "FK_RequestApproval_WorkFlow_WorkFlowId",
                table: "RequestApproval");

            migrationBuilder.RenameColumn(
                name: "WorkFlowId",
                table: "RequestApproval",
                newName: "WorkFlowStepId");

            migrationBuilder.RenameColumn(
                name: "ApprovalUserId",
                table: "RequestApproval",
                newName: "ApproverUserId");

            migrationBuilder.RenameIndex(
                name: "IX_RequestApproval_WorkFlowId",
                table: "RequestApproval",
                newName: "IX_RequestApproval_WorkFlowStepId");

            migrationBuilder.RenameIndex(
                name: "IX_RequestApproval_ApprovalUserId",
                table: "RequestApproval",
                newName: "IX_RequestApproval_ApproverUserId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SubmittedAt",
                table: "Requests",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletedAt",
                table: "Requests",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ActedAt",
                table: "RequestApproval",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestApproval_User_ApproverUserId",
                table: "RequestApproval",
                column: "ApproverUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestApproval_WorkFlowSteps_WorkFlowStepId",
                table: "RequestApproval",
                column: "WorkFlowStepId",
                principalTable: "WorkFlowSteps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestApproval_User_ApproverUserId",
                table: "RequestApproval");

            migrationBuilder.DropForeignKey(
                name: "FK_RequestApproval_WorkFlowSteps_WorkFlowStepId",
                table: "RequestApproval");

            migrationBuilder.RenameColumn(
                name: "WorkFlowStepId",
                table: "RequestApproval",
                newName: "WorkFlowId");

            migrationBuilder.RenameColumn(
                name: "ApproverUserId",
                table: "RequestApproval",
                newName: "ApprovalUserId");

            migrationBuilder.RenameIndex(
                name: "IX_RequestApproval_WorkFlowStepId",
                table: "RequestApproval",
                newName: "IX_RequestApproval_WorkFlowId");

            migrationBuilder.RenameIndex(
                name: "IX_RequestApproval_ApproverUserId",
                table: "RequestApproval",
                newName: "IX_RequestApproval_ApprovalUserId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SubmittedAt",
                table: "Requests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletedAt",
                table: "Requests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ActedAt",
                table: "RequestApproval",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestApproval_User_ApprovalUserId",
                table: "RequestApproval",
                column: "ApprovalUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestApproval_WorkFlow_WorkFlowId",
                table: "RequestApproval",
                column: "WorkFlowId",
                principalTable: "WorkFlow",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
