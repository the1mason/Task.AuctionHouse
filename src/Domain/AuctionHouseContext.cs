using Microsoft.EntityFrameworkCore;

using Domain.Models;

namespace Domain;
public class AuctionHouseContext : DbContext
{
    public AuctionHouseContext(DbContextOptions<AuctionHouseContext> options)
        : base(options)
    {
    }

    public DbSet<Lot> Lots { get; set; } = null!;

    public DbSet<Account> Accounts { get; set; } = null!;

    public DbSet<Bid> Bids { get; set; } = null!;

    public DbSet<AccountTransaction> AccountTransactions { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Account>(entity =>
        {
            entity.Property(a => a.Role).HasConversion<int>();
            entity.HasIndex(a => a.Login).IsUnique();
            entity.Property(a => a.Login).HasMaxLength(32);
        });

        modelBuilder.Entity<Lot>(entity =>
        {
            entity.HasOne(l => l.Seller).WithMany().HasForeignKey(l => l.SellerId);
            entity.HasOne(l => l.Winner).WithMany().HasForeignKey(l => l.WinnerId);
            entity.Property(l => l.Title).HasMaxLength(64);
            entity.Property(l => l.Description).HasMaxLength(4096);
        });

        modelBuilder.Entity<Bid>(entity =>
        {
            entity.HasOne(b => b.Lot).WithMany(l => l!.Bids).HasForeignKey(b => b.LotId);
            entity.HasOne(b => b.Account).WithMany().HasForeignKey(b => b.AccountId);
            entity.HasOne(b => b.AccountTransaction).WithOne().HasForeignKey<Bid>(b => b.TransactionId);
        });

        modelBuilder.Entity<AccountTransaction>(entity =>
        {
            entity.HasOne(at => at.Sender).WithMany().HasForeignKey(at => at.SenderId);
            entity.HasOne(at => at.Recipient).WithMany().HasForeignKey(at => at.RecipientId);
            entity.Property(a => a.Type).HasConversion<int>();
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasOne(rt => rt.Account).WithMany().HasForeignKey(rt => rt.AccountId);
        });
            
    }
}
