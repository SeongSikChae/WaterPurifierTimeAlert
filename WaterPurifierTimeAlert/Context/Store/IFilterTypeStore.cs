using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace WaterPurifierTimeAlert.Context.Store
{
	using Entity;

	public interface IFilterTypeStore
	{
		IEnumerable<FilterType> GetList();

		Task CreateAsync(FilterType filterType);

		public sealed class FilterTypeStore(IDbContextFactory<PurifierContext> dbContextFactory, ILogger<FilterTypeStore> logger) : IFilterTypeStore
		{
			public IEnumerable<FilterType> GetList()
			{
				try
				{
					using PurifierContext context = dbContextFactory.CreateDbContext();
					return [.. context.FilterType];
				}
				catch (Exception e)
				{
					logger.Error(e.Message, e);
					throw;
				}
			}

			public async Task CreateAsync(FilterType filterType)
			{
				try
				{
					using PurifierContext context = dbContextFactory.CreateDbContext();
					context.FilterType.Add(filterType);
					await context.SaveChangesAsync();
				}
				catch (Exception e)
				{
					logger.Error(e.Message, e);
					throw;
				}
			}
		}
	}
}
