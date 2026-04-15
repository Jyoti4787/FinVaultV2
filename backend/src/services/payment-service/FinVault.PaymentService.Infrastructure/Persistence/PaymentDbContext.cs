// ==================================================================
// FILE : PaymentDbContext.cs
// LAYER: Infrastructure (Persistence)
// PATH : payment-service/FinVault.PaymentService.Infrastructure/Persistence/
//
// WHAT IS THIS?
// This is the "REMOTE CONTROL" for your SQL Server database.
// It tells EF Core: "Hey, I have a table called 'Payments' and it 
// should look exactly like my 'Payment' class."
//
// It handles all the heavy lifting of:
// - Opening connections
// - Running SQL commands (INSERT, SELECT, UPDATE)
// - Converting SQL rows back into C# objects
// ==================================================================

using FinVault.PaymentService.Domain.Entities;
using FinVault.PaymentService.Infrastructure.Messaging.Sagas;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FinVault.PaymentService.Infrastructure.Persistence;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) 
        : base(options)
    {
    }

    // This is the "Table" in SQL Server
    public DbSet<Payment> Payments { get; set; }
    
    // Billing tables
    public DbSet<Bill> Bills { get; set; }
    public DbSet<PaymentSchedule> PaymentSchedules { get; set; }
    
    // Saga State Persistence
    public DbSet<PaymentState> PaymentStates { get; set; }
    
    // Reward Points
    public DbSet<RewardPoint> RewardPoints { get; set; }

    // This method is where we "Design" the table columns
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the Payment table
        modelBuilder.Entity<Payment>(entity =>
        {
            // Set the primary key
            entity.HasKey(e => e.Id);

            // "Money" columns need special precision in SQL (decimal(18,2))
            // 18 = total digits, 2 = digits after decimal point
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18,2)");

            // Make sure Status is never empty
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20);

            // Add an Index on UserId so we can find user history FAST
            entity.HasIndex(e => e.UserId);

            // Add an Index on CardId
            entity.HasIndex(e => e.CardId);
        });

        // Configure Saga State Table
        modelBuilder.Entity<PaymentState>(entity =>
        {
            entity.HasKey(x => x.CorrelationId);
            entity.Property(x => x.CurrentState).HasMaxLength(64);
            entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        });

        // Configure Bill table
        modelBuilder.Entity<Bill>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MinimumPayment).HasColumnType("decimal(18,2)");
            entity.Property(e => e.PaidAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CardId);
            entity.HasIndex(e => e.DueDate);
        });

        // Configure PaymentSchedule table
        modelBuilder.Entity<PaymentSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.BillId);
            entity.HasIndex(e => e.ScheduledDate);
            
            // Navigation property
            entity.HasOne(e => e.Bill)
                .WithMany()
                .HasForeignKey(e => e.BillId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure RewardPoints table
        modelBuilder.Entity<RewardPoint>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.Type).HasMaxLength(20);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.PaymentId);
            entity.HasIndex(e => e.CardId);
        });
    }
}
