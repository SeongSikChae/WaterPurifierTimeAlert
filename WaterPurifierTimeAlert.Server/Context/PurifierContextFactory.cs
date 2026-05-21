using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WaterPurifierTimeAlert.Server.Context
{
    public class PurifierContextFactory : IDesignTimeDbContextFactory<PurifierContext>
    {
        public PurifierContext CreateDbContext(string[] args)
        {
            DbContextOptionsBuilder<PurifierContext> builder = new DbContextOptionsBuilder<PurifierContext>().UseSqlite($"Data Source={args[0]}");
            return new PurifierContext(builder.Options);
        }
    }
}
