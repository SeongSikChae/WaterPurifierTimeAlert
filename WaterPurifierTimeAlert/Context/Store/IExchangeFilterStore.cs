using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace WaterPurifierTimeAlert.Context.Store
{
	using Entity;

	public interface IExchangeFilterStore
	{
		IEnumerable<ExchangeFilter> GetList();

		Task AddOrUpdateAsync(ExchangeFilter exchangeFilter);

		public sealed class ExchangeFilterStore(IDbContextFactory<PurifierContext> dbContextFactory, ILogger<ExchangeFilterStore> logger) : IExchangeFilterStore
		{
			public IEnumerable<ExchangeFilter> GetList()
			{
				try
				{
					using PurifierContext context = dbContextFactory.CreateDbContext();
					return [.. context.ExchangeFilter];
				}
				catch (Exception e)
				{
					logger.Error(e.Message, e);
					throw;
				}
			}

			public async Task AddOrUpdateAsync(ExchangeFilter exchangeFilter)
			{
				try
				{
					using PurifierContext context = await dbContextFactory.CreateDbContextAsync();
					ExchangeFilter? filter = await context.ExchangeFilter.Where(e => e.FilterName.Equals(exchangeFilter.FilterName)).SingleOrDefaultAsync();
					if (filter is not null)
						context.Entry(filter).CurrentValues.SetValues(exchangeFilter);
					else
						await context.ExchangeFilter.AddAsync(exchangeFilter);
					await context.SaveChangesAsync();
				}
				catch (Exception e)
				{
					logger.Error(e.Message, e);
					await Task.FromException(e);
				}
			}
		}
	}
}
