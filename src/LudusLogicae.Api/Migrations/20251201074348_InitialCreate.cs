using System;
using System.Collections.Generic;
using LudusLogicae.Api.Features.Debates.Entities;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LudusLogicae.Api.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateTable(
                name: "Debates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Question = table.Column<string>(type: "text", nullable: false),
                    Context = table.Column<string>(type: "text", nullable: true),
                    NMembers = table.Column<int>(type: "integer", nullable: false),
                    NRounds = table.Column<int>(type: "integer", nullable: false),
                    Phase = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    PreviousAttemptId = table.Column<Guid>(type: "uuid", nullable: true),
                    Ballots = table.Column<List<BallotItem>>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    Result = table.Column<DebateResult>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Debates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StyleHint = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BehaviorExtras = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: false),
                    Temperature = table.Column<double>(type: "double precision", nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DebateParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DebateId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    SlotIndex = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StyleHint = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BehaviorExtras = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Temperature = table.Column<double>(type: "double precision", nullable: false),
                    InitialAnswer = table.Column<string>(type: "text", nullable: true),
                    CrossQuestions = table.Column<List<CrossQuestion>>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    QuestionResponses = table.Column<List<QuestionResponse>>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    Amendment = table.Column<string>(type: "text", nullable: true),
                    FinalAnswer = table.Column<string>(type: "text", nullable: true),
                    BallotRanking = table.Column<List<int>>(type: "jsonb", nullable: true),
                    BordaScore = table.Column<double>(type: "double precision", nullable: false),
                    FinalScore = table.Column<double>(type: "double precision", nullable: false),
                    IsWinner = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DebateParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DebateParticipants_Debates_DebateId",
                        column: x => x.DebateId,
                        principalTable: "Debates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DebateParticipants_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MemberIntroductionCaches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    Introduction = table.Column<string>(type: "text", nullable: false),
                    Provider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberIntroductionCaches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberIntroductionCaches_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_participant_debate",
                table: "DebateParticipants",
                column: "DebateId");

            migrationBuilder.CreateIndex(
                name: "ix_participant_debate_winner",
                table: "DebateParticipants",
                columns: new[] { "DebateId", "IsWinner" });

            migrationBuilder.CreateIndex(
                name: "ix_participant_member",
                table: "DebateParticipants",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "ux_participant_debate_slot",
                table: "DebateParticipants",
                columns: new[] { "DebateId", "SlotIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_debate_created",
                table: "Debates",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "ix_debate_phase",
                table: "Debates",
                column: "Phase");

            migrationBuilder.CreateIndex(
                name: "ix_debate_previous_attempt",
                table: "Debates",
                column: "PreviousAttemptId");

            migrationBuilder.CreateIndex(
                name: "ix_intro_created",
                table: "MemberIntroductionCaches",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "ix_intro_member",
                table: "MemberIntroductionCaches",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "ix_intro_member_prov_model_date",
                table: "MemberIntroductionCaches",
                columns: new[] { "MemberId", "Provider", "Model", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "ix_intro_provider_model",
                table: "MemberIntroductionCaches",
                columns: new[] { "Provider", "Model" });

            migrationBuilder.CreateIndex(
                name: "ix_member_created",
                table: "Members",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "ix_member_name",
                table: "Members",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "ix_member_role",
                table: "Members",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "ix_member_status",
                table: "Members",
                column: "Status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DebateParticipants");

            migrationBuilder.DropTable(
                name: "MemberIntroductionCaches");

            migrationBuilder.DropTable(
                name: "Debates");

            migrationBuilder.DropTable(
                name: "Members");
        }
    }
}
