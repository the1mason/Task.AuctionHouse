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

    public DbSet<Role> Roles { get; set; } = null!;

    public DbSet<AccountTransaction> AccountTransactions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasOne(a => a.Role)
                .WithMany(r => r!.Accounts)
                .HasForeignKey(a => a.RoleId);
        });

        modelBuilder.Entity<Lot>(entity =>
        {
            entity.HasOne(l => l.Seller).WithMany().HasForeignKey(l => l.SellerId);
            entity.HasOne(l => l.Winner).WithMany().HasForeignKey(l => l.WinnerId);
        });

        modelBuilder.Entity<Bid>(entity =>
        {
            entity.HasOne(b => b.Lot).WithMany(l => l!.Bids).HasForeignKey(b => b.LotId);
            entity.HasOne(b => b.User).WithMany().HasForeignKey(b => b.UserId);
            entity.HasOne(b => b.AccountTransaction).WithOne().HasForeignKey<Bid>(b => b.TransactionId);
        });

        modelBuilder.Entity<AccountTransaction>(entity =>
        {
            entity.HasOne(at => at.Account).WithMany().HasForeignKey(at => at.UserId);
        });
            
    }
}
