using Microsoft.EntityFrameworkCore;

namespace WaterPurifierTimeAlert.Server.Context
{
    using Server.Context.Entity;

    public class PurifierContext(DbContextOptions<PurifierContext> options) : DbContext(options)
    {
        public virtual DbSet<FilterType> FilterType { get; set; }

        public virtual DbSet<ExchangeFilter> ExchangeFilter { get; set; }

        public virtual DbSet<PushSubscription> PushSubscription { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PushSubscription>().HasKey(entity => entity.Endpoint);
            modelBuilder.Entity<PushSubscription>().Property(property => property.Endpoint).HasMaxLength(500).IsRequired();
            modelBuilder.Entity<PushSubscription>().Property(property => property.P256dh).HasMaxLength(200).IsRequired();
            modelBuilder.Entity<PushSubscription>().Property(property => property.Auth).HasMaxLength(100).IsRequired();
            modelBuilder.Entity<PushSubscription>().Property(property => property.UserEmail).HasMaxLength(256).IsRequired();
            modelBuilder.Entity<PushSubscription>().HasIndex(entity => entity.UserEmail);

            modelBuilder.Entity<FilterType>().HasKey(entity => entity.Name);
            modelBuilder.Entity<FilterType>().Property(property => property.Name).HasMaxLength(20);
            modelBuilder.Entity<FilterType>().Property(property => property.ExpireTime).HasMaxLength(10).IsRequired();

            modelBuilder.Entity<ExchangeFilter>().HasKey(entity => entity.FilterName);
            modelBuilder.Entity<ExchangeFilter>().Property(property => property.FilterName).HasMaxLength(20).IsRequired();
            modelBuilder.Entity<ExchangeFilter>().Property(property => property.LastExchnageDate).IsRequired().HasConversion(v => v.ToString("yyyy-MM-dd"), v => DateTime.Parse(v));
            modelBuilder.Entity<ExchangeFilter>().Ignore(property => property.NextExchnageDate);

            base.OnModelCreating(modelBuilder);
        }
    }
}
