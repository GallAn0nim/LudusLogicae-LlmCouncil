using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LudusLogicae.Api.Migrations
{
    public partial class AddSearchIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_member_name",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "ix_member_role",
                table: "Members");

            migrationBuilder.CreateIndex(
                name: "ix_member_name",
                table: "Members",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_member_role",
                table: "Members",
                column: "Role")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_debate_question",
                table: "Debates",
                column: "Question")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_member_name",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "ix_member_role",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "ix_debate_question",
                table: "Debates");

            migrationBuilder.CreateIndex(
                name: "ix_member_name",
                table: "Members",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "ix_member_role",
                table: "Members",
                column: "Role");
        }
    }
}
