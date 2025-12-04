using LudusLogicae.Api.Features.Debates.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LudusLogicae.Api.Migrations
{
    public partial class AddDebateState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DebateState>(
                name: "State",
                table: "Debates",
                type: "jsonb",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                table: "Debates");
        }
    }
}
