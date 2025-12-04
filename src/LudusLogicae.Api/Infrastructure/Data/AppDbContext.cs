using LudusLogicae.Api.Features.Debates.Entities;
using LudusLogicae.Api.Features.Members.Entities;
using Microsoft.EntityFrameworkCore;

namespace LudusLogicae.Api.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Member> Members => Set<Member>();
    public DbSet<Debate> Debates => Set<Debate>();
    public DbSet<DebateParticipant> DebateParticipants => Set<DebateParticipant>();
    public DbSet<MemberIntroductionCache> MemberIntroductionCaches => Set<MemberIntroductionCache>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.HasPostgresExtension("pg_trgm");

        b.Entity<Member>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Role).HasMaxLength(100).IsRequired();
            e.Property(x => x.StyleHint).HasMaxLength(200);
            e.Property(x => x.BehaviorExtras).HasMaxLength(1000);

            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);

            e.HasIndex(x => x.Status).HasDatabaseName("ix_member_status");
            e.HasIndex(x => x.CreatedAt).HasDatabaseName("ix_member_created");
            e.HasIndex(x => x.Name)
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops")
                .HasDatabaseName("ix_member_name");
            e.HasIndex(x => x.Role)
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops")
                .HasDatabaseName("ix_member_role");

            e.HasIndex(x => new { x.Status, x.Points })
                .IsDescending(false, true)
                .HasDatabaseName("ix_member_status_points");
            e.HasIndex(x => x.LastDebateDate)
                .IsDescending(true)
                .HasDatabaseName("ix_member_last_debate");
            e.HasIndex(x => new { x.Status, x.LastDebateDate })
                .IsDescending(false, true)
                .HasDatabaseName("ix_member_status_last_debate");
        });

        b.Entity<Debate>(e =>
        {
            e.Property(x => x.Phase).HasConversion<string>().HasMaxLength(16);

            e.Property(x => x.Question).IsRequired();
            e.Property(x => x.Context);

            e.Property(x => x.Ballots)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'[]'::jsonb");
            e.Property(x => x.Result)
                .HasColumnType("jsonb");
            e.Property(x => x.State)
                .HasColumnType("jsonb")
                .HasAnnotation("Npgsql:StorageParameter:storage",
                    "external");

            e.HasIndex(x => x.Phase).HasDatabaseName("ix_debate_phase");
            e.HasIndex(x => new { x.Phase, x.CreatedAt })
                .HasDatabaseName("ix_debate_phase_created_desc");
            e.HasIndex(x => x.CreatedAt).HasDatabaseName("ix_debate_created");
            e.HasIndex(x => x.PreviousAttemptId).HasDatabaseName("ix_debate_previous_attempt");
            e.HasIndex(x => x.Question)
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops")
                .HasDatabaseName("ix_debate_question");
            e.HasIndex(x => x.Result)
                .HasMethod("gin")
                .HasDatabaseName("ix_debate_result");
        });

        b.Entity<DebateParticipant>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Role).HasMaxLength(100);
            e.Property(x => x.StyleHint).HasMaxLength(200);
            e.Property(x => x.BehaviorExtras).HasMaxLength(1000);
            e.Property(x => x.Model).HasMaxLength(100);

            e.Property(x => x.CrossQuestions)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'[]'::jsonb");
            e.Property(x => x.QuestionResponses)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'[]'::jsonb");
            e.Property(x => x.BallotRanking)
                .HasColumnType("jsonb");

            e.HasOne(x => x.Debate)
                .WithMany(d => d.Participants)
                .HasForeignKey(x => x.DebateId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Member)
                .WithMany()
                .HasForeignKey(x => x.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.DebateId).HasDatabaseName("ix_participant_debate");
            e.HasIndex(x => new { x.DebateId, x.IsWinner })
                .HasDatabaseName("ix_participant_debate_winner");
            e.HasIndex(x => new { x.DebateId, x.SlotIndex })
                .IsUnique()
                .HasDatabaseName("ux_participant_debate_slot");
            e.HasIndex(x => x.MemberId).HasDatabaseName("ix_participant_member");
        });

        b.Entity<MemberIntroductionCache>(e =>
        {
            e.HasOne(x => x.Member)
                .WithMany()
                .HasForeignKey(x => x.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            e.Property(x => x.Provider).HasMaxLength(100);
            e.Property(x => x.Model).HasMaxLength(100);

            e.HasIndex(x => x.MemberId).HasDatabaseName("ix_intro_member");
            e.HasIndex(x => new { x.Provider, x.Model })
                .HasDatabaseName("ix_intro_provider_model");
            e.HasIndex(x => x.CreatedAt).HasDatabaseName("ix_intro_created");
            e.HasIndex(x => new { x.MemberId, x.Provider, x.Model, x.CreatedAt })
                .HasDatabaseName("ix_intro_member_prov_model_date");
        });
    }
}
