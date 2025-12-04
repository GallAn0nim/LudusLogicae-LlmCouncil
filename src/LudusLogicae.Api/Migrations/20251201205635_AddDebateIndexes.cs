using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LudusLogicae.Api.Migrations
{
    public partial class AddDebateIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_debate_phase_created_desc",
                table: "Debates",
                columns: new[] { "Phase", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "ix_debate_result",
                table: "Debates",
                column: "Result")
                .Annotation("Npgsql:IndexMethod", "gin");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_debate_phase_created_desc",
                table: "Debates");

            migrationBuilder.DropIndex(
                name: "ix_debate_result",
                table: "Debates");
        }
    }
}
