using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WaterPurifierTimeAlert.Context
{
	public class PurifierContextFactory : IDesignTimeDbContextFactory<PurifierContext>
	{
		public PurifierContext CreateDbContext(string[] args)
		{
			DbContextOptionsBuilder<PurifierContext> buider = new DbContextOptionsBuilder<PurifierContext>().UseSqlite($"Data Source={args[0]}");
			return new PurifierContext(buider.Options);
		}
	}
}
