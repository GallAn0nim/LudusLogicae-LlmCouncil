using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LudusLogicae.Api.Migrations
{
    public partial class ResetMemberPointsAndStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""Members""
                SET ""Points"" = 10,
                    ""Status"" = 'Active'
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No rollback - we don't know the previous values
        }
    }
}
