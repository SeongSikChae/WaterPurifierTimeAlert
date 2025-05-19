using Microsoft.EntityFrameworkCore;

namespace WaterPurifierTimeAlert.Context
{
	using Entity;

	public class PurifierContext(DbContextOptions<PurifierContext> options) : DbContext(options)
	{
		public virtual DbSet<FilterType> FilterType { get; set; }

		public virtual DbSet<ExchangeFilter> ExchangeFilter { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			base.OnConfiguring(optionsBuilder);
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<FilterType>().HasKey(entity => entity.Name);
			modelBuilder.Entity<FilterType>().Property(property => property.Name).HasMaxLength(20);
			modelBuilder.Entity<FilterType>().Property(property => property.ExpireTime).HasMaxLength(10).IsRequired();

			modelBuilder.Entity<ExchangeFilter>().HasKey(entity => entity.FilterName);
			modelBuilder.Entity<ExchangeFilter>().Property(property => property.FilterName).HasMaxLength(20).IsRequired();
			modelBuilder.Entity<ExchangeFilter>().Property(property => property.LastExchnageDate).IsRequired().HasConversion(v => v.ToString("yyyy-MM-dd"), v => DateTime.Parse(v));

			base.OnModelCreating(modelBuilder);
		}
	}
}
