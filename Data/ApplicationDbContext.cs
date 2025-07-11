using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ZaiEats.Models;

namespace ZaiEats.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<KitchenStaff> KitchenStaffs { get; set; }
        public DbSet<MenuCategory> MenuCategories { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<MenuOptionGroup> MenuOptionGroups { get; set; }
        public DbSet<MenuOptionItem> MenuOptionItems { get; set; }
        public DbSet<QuotationRequest> QuotationRequests { get; set; }
        public DbSet<NewsEvent> NewsEvents { get; set; }
        public DbSet<PodcastEpisode> PodcastEpisodes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }

        // New linking table
        public DbSet<RestaurantManager> RestaurantManagers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Prevent multiple cascade paths
            modelBuilder.Entity<MenuItem>()
                .HasOne(mi => mi.Restaurant)
                .WithMany()
                .HasForeignKey(mi => mi.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MenuCategory>()
                .HasOne(mc => mc.Restaurant)
                .WithMany()
                .HasForeignKey(mc => mc.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-Many: Restaurant <-> ApplicationUser (as Manager)
            modelBuilder.Entity<RestaurantManager>()
                .HasKey(rm => rm.RestaurantManagerId);

            modelBuilder.Entity<RestaurantManager>()
                .HasOne(rm => rm.Restaurant)
                .WithMany(r => r.RestaurantManagers)
                .HasForeignKey(rm => rm.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RestaurantManager>()
                .HasOne(rm => rm.Manager)
                .WithMany(u => u.RestaurantManagers)
                .HasForeignKey(rm => rm.ManagerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
