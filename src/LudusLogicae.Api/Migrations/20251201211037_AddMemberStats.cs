using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LudusLogicae.Api.Migrations
{
    public partial class AddMemberStats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DebatesCount",
                table: "Members",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastDebateDate",
                table: "Members",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastPlaceCount",
                table: "Members",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Points",
                table: "Members",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WinsCount",
                table: "Members",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DebatesCount",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "LastDebateDate",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "LastPlaceCount",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "Points",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "WinsCount",
                table: "Members");
        }
    }
}
