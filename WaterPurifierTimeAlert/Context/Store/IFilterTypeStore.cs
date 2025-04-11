using WaterPurifierTimeAlert.Context.Entity;

namespace WaterPurifierTimeAlert.Context.Store
{
	public interface IFilterTypeStore
	{
		IEnumerable<FilterType> GetList();

		Task CreateAsync(FilterType filterType);

		public sealed class FilterTypeStore(PurifierContext context) : IFilterTypeStore
		{
			public IEnumerable<FilterType> GetList()
			{
				return [.. context.FilterType];
			}

			public async Task CreateAsync(FilterType filterType)
			{
				context.FilterType.Add(filterType);
				await context.SaveChangesAsync();
			}
		}
	}
}
