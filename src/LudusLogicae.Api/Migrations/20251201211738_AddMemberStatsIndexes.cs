using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LudusLogicae.Api.Migrations
{
    public partial class AddMemberStatsIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_member_last_debate",
                table: "Members",
                column: "LastDebateDate",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "ix_member_status_last_debate",
                table: "Members",
                columns: new[] { "Status", "LastDebateDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_member_status_points",
                table: "Members",
                columns: new[] { "Status", "Points" },
                descending: new[] { false, true });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_member_last_debate",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "ix_member_status_last_debate",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "ix_member_status_points",
                table: "Members");
        }
    }
}
