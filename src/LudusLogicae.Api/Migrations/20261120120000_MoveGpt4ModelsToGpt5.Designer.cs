
using System;
using System.Collections.Generic;
using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Debates.Models;
using LudusLogicae.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LudusLogicae.Api.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261120120000_MoveGpt4ModelsToGpt5")]
    partial class MoveGpt4ModelsToGpt5
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "pg_trgm");
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("LudusLogicae.Api.Features.Debates.Entities.Debate", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<List<BallotItem>>("Ballots")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("jsonb")
                        .HasDefaultValueSql("'[]'::jsonb");

                    b.Property<DateTimeOffset?>("CompletedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Context")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("FailedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("NMembers")
                        .HasColumnType("integer");

                    b.Property<int>("NRounds")
                        .HasColumnType("integer");

                    b.Property<string>("Phase")
                        .IsRequired()
                        .HasMaxLength(16)
                        .HasColumnType("character varying(16)");

                    b.Property<Guid?>("PreviousAttemptId")
                        .HasColumnType("uuid");

                    b.Property<string>("Question")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DebateResult>("Result")
                        .HasColumnType("jsonb");

                    b.Property<DateTimeOffset?>("StartedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DebateState>("State")
                        .HasColumnType("jsonb")
                        .HasAnnotation("Npgsql:StorageParameter:storage", "external");

                    b.HasKey("Id");

                    b.HasIndex("CreatedAt")
                        .HasDatabaseName("ix_debate_created");

                    b.HasIndex("Phase")
                        .HasDatabaseName("ix_debate_phase");

                    b.HasIndex("PreviousAttemptId")
                        .HasDatabaseName("ix_debate_previous_attempt");

                    b.HasIndex("Question")
                        .HasDatabaseName("ix_debate_question");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("Question"), "gin");
                    NpgsqlIndexBuilderExtensions.HasOperators(b.HasIndex("Question"), new[] { "gin_trgm_ops" });

                    b.HasIndex("Result")
                        .HasDatabaseName("ix_debate_result");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("Result"), "gin");

                    b.HasIndex("Phase", "CreatedAt")
                        .HasDatabaseName("ix_debate_phase_created_desc");

                    b.ToTable("Debates");
                });

            modelBuilder.Entity("LudusLogicae.Api.Features.Debates.Entities.DebateParticipant", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Amendment")
                        .HasColumnType("text");

                    b.PrimitiveCollection<List<int>>("BallotRanking")
                        .HasColumnType("jsonb");

                    b.Property<string>("BehaviorExtras")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<double>("BordaScore")
                        .HasColumnType("double precision");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<List<CrossQuestion>>("CrossQuestions")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("jsonb")
                        .HasDefaultValueSql("'[]'::jsonb");

                    b.Property<Guid>("DebateId")
                        .HasColumnType("uuid");

                    b.Property<string>("FinalAnswer")
                        .HasColumnType("text");

                    b.Property<double>("FinalScore")
                        .HasColumnType("double precision");

                    b.Property<string>("InitialAnswer")
                        .HasColumnType("text");

                    b.Property<bool>("IsWinner")
                        .HasColumnType("boolean");

                    b.Property<Guid>("MemberId")
                        .HasColumnType("uuid");

                    b.Property<string>("Model")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<string>("Provider")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<List<LudusLogicae.Api.Features.Debates.Entities.QuestionResponse>>("QuestionResponses")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("jsonb")
                        .HasDefaultValueSql("'[]'::jsonb");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<int>("SlotIndex")
                        .HasColumnType("integer");

                    b.Property<string>("StyleHint")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<double>("Temperature")
                        .HasColumnType("double precision");

                    b.HasKey("Id");

                    b.HasIndex("DebateId")
                        .HasDatabaseName("ix_participant_debate");

                    b.HasIndex("MemberId")
                        .HasDatabaseName("ix_participant_member");

                    b.HasIndex("DebateId", "IsWinner")
                        .HasDatabaseName("ix_participant_debate_winner");

                    b.HasIndex("DebateId", "SlotIndex")
                        .IsUnique()
                        .HasDatabaseName("ux_participant_debate_slot");

                    b.ToTable("DebateParticipants");
                });

            modelBuilder.Entity("LudusLogicae.Api.Features.Members.Entities.Member", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("BehaviorExtras")
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("DebatesCount")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset?>("LastDebateDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("LastPlaceCount")
                        .HasColumnType("integer");

                    b.Property<string>("Model")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<int>("Points")
                        .HasColumnType("integer");

                    b.Property<string>("Provider")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(16)
                        .HasColumnType("character varying(16)");

                    b.Property<string>("StyleHint")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<double>("Temperature")
                        .HasColumnType("double precision");

                    b.Property<int>("WinsCount")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CreatedAt")
                        .HasDatabaseName("ix_member_created");

                    b.HasIndex("LastDebateDate")
                        .IsDescending()
                        .HasDatabaseName("ix_member_last_debate");

                    b.HasIndex("Name")
                        .HasDatabaseName("ix_member_name");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("Name"), "gin");
                    NpgsqlIndexBuilderExtensions.HasOperators(b.HasIndex("Name"), new[] { "gin_trgm_ops" });

                    b.HasIndex("Role")
                        .HasDatabaseName("ix_member_role");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("Role"), "gin");
                    NpgsqlIndexBuilderExtensions.HasOperators(b.HasIndex("Role"), new[] { "gin_trgm_ops" });

                    b.HasIndex("Status")
                        .HasDatabaseName("ix_member_status");

                    b.HasIndex("Status", "LastDebateDate")
                        .IsDescending(false, true)
                        .HasDatabaseName("ix_member_status_last_debate");

                    b.HasIndex("Status", "Points")
                        .IsDescending(false, true)
                        .HasDatabaseName("ix_member_status_points");

                    b.ToTable("Members");
                });

            modelBuilder.Entity("LudusLogicae.Api.Features.Members.Entities.MemberIntroductionCache", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Introduction")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("MemberId")
                        .HasColumnType("uuid");

                    b.Property<string>("Model")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Provider")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("Id");

                    b.HasIndex("CreatedAt")
                        .HasDatabaseName("ix_intro_created");

                    b.HasIndex("MemberId")
                        .HasDatabaseName("ix_intro_member");

                    b.HasIndex("Provider", "Model")
                        .HasDatabaseName("ix_intro_provider_model");

                    b.HasIndex("MemberId", "Provider", "Model", "CreatedAt")
                        .HasDatabaseName("ix_intro_member_prov_model_date");

                    b.ToTable("MemberIntroductionCaches");
                });

            modelBuilder.Entity("LudusLogicae.Api.Features.Debates.Entities.DebateParticipant", b =>
                {
                    b.HasOne("LudusLogicae.Api.Features.Debates.Entities.Debate", "Debate")
                        .WithMany("Participants")
                        .HasForeignKey("DebateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("LudusLogicae.Api.Features.Members.Entities.Member", "Member")
                        .WithMany()
                        .HasForeignKey("MemberId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Debate");

                    b.Navigation("Member");
                });

            modelBuilder.Entity("LudusLogicae.Api.Features.Members.Entities.MemberIntroductionCache", b =>
                {
                    b.HasOne("LudusLogicae.Api.Features.Members.Entities.Member", "Member")
                        .WithMany()
                        .HasForeignKey("MemberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Member");
                });

            modelBuilder.Entity("LudusLogicae.Api.Features.Debates.Entities.Debate", b =>
                {
                    b.Navigation("Participants");
                });
#pragma warning restore 612, 618
        }
    }
}
