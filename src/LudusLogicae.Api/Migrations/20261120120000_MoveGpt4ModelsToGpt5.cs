using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LudusLogicae.Api.Migrations
{
    public partial class MoveGpt4ModelsToGpt5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""Members""
                SET ""Model"" = REPLACE(""Model"", 'gpt-4.1', 'gpt-5')
                WHERE ""Model"" LIKE 'gpt-4.1%'
            ");

            migrationBuilder.Sql(@"
                UPDATE ""DebateParticipants"" AS p
                SET ""Model"" = REPLACE(p.""Model"", 'gpt-4.1', 'gpt-5')
                FROM ""Debates"" AS d
                WHERE d.""Id"" = p.""DebateId""
                  AND d.""Phase"" = 'Created'
                  AND p.""Model"" LIKE 'gpt-4.1%'
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
