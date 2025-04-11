using WaterPurifierTimeAlert.Context.Entity;

namespace WaterPurifierTimeAlert.Context.Store
{
	public interface IExchangeFilterStore
	{
		IEnumerable<ExchangeFilter> GetList();

		Task AddOrUpdateAsync(ExchangeFilter exchangeFilter);

		public sealed class ExchangeFilterStore(PurifierContext context) : IExchangeFilterStore
		{
			public IEnumerable<ExchangeFilter> GetList()
			{
				return [.. context.ExchangeFilter];
			}

			public Task AddOrUpdateAsync(ExchangeFilter exchangeFilter)
			{
				if (context.ExchangeFilter.Where(e => e.FilterName.Equals(exchangeFilter.FilterName)).Any())
					context.ExchangeFilter.Update(exchangeFilter);
				else
					context.ExchangeFilter.Add(exchangeFilter);
				return context.SaveChangesAsync();
			}
		}
	}
}
