using Microsoft.EntityFrameworkCore;

namespace WaterPurifierTimeAlert.Server.Context
{
    using Server.Context.Entity;

    public class PurifierContext(DbContextOptions<PurifierContext> options) : DbContext(options)
    {
        public virtual DbSet<FilterType> FilterType { get; set; }

        public virtual DbSet<ExchangeFilter> ExchangeFilter { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
