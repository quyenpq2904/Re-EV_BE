using Microsoft.EntityFrameworkCore;
using ReEV.Service.Marketplace.Models;

namespace ReEV.Service.Marketplace.Repositories
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Bid> Bids { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<Listing> Listings { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasMany(u => u.ReviewsGiven)
                      .WithOne(r => r.Reviewer)
                      .HasForeignKey(r => r.ReviewerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.ReviewsReceived)
                      .WithOne(r => r.Reviewee)
                      .HasForeignKey(r => r.RevieweeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.ComplaintsMade)
                      .WithOne(c => c.Reporter)
                      .HasForeignKey(c => c.ReporterId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.ComplaintsReceived)
                      .WithOne(c => c.ComplainedUser)
                      .HasForeignKey(c => c.ComplainedUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.Listing)    
                .WithMany()                
                .HasForeignKey(c => c.ListingId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Bid>(entity =>
            {
                entity.HasOne(b => b.Bidder)
                      .WithMany(u => u.Bids)
                      .HasForeignKey(b => b.BidderId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Listing)
                      .WithMany(l => l.Bids)
                      .HasForeignKey(b => b.ListingId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserFavorite>(entity =>
            {
                entity.HasOne(uf => uf.User)
                      .WithMany(u => u.UserFavorites)
                      .HasForeignKey(uf => uf.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(uf => uf.Listing)
                      .WithMany(u => u.UserFavorites)
                      .HasForeignKey(uf => uf.ListingId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
