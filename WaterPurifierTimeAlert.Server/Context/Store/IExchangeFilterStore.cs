using Microsoft.EntityFrameworkCore;

namespace WaterPurifierTimeAlert.Server.Context.Store
{
    using Server.Entity;
    using Server.Context.Entity;

    public interface IExchangeFilterStore
    {
        Task<List<ExchangeFilter>> GetListAsync(SearchQuery query, CancellationToken cancellationToken);

        Task<List<ExchangeFilter>> GetListWithContextAsync(PurifierContext context, SearchQuery query, CancellationToken cancellationToken);

        Task CreateAsync(ExchangeFilter exchangeFilter, CancellationToken cancellationToken);

        Task CreateWithContextAsync(PurifierContext context, ExchangeFilter exchangeFilter, CancellationToken cancellationToken);

        Task UpdateAsync(ExchangeFilter exchangeFilter, CancellationToken cancellationToken);

        Task UpdateWithContextAsync(PurifierContext context, ExchangeFilter exchangeFilter, CancellationToken cancellationToken);

        Task DeleteAsync(ExchangeFilter exchangeFilter, CancellationToken cancellationToken);

        Task DeleteWithContextAsync(PurifierContext context, ExchangeFilter exchangeFilter, CancellationToken cancellationToken);
    }

    public sealed class ExchangeFilterStore(IDbContextFactory<PurifierContext> dbContextFactory) : IExchangeFilterStore
    {
        public async Task<List<ExchangeFilter>> GetListAsync(SearchQuery query, CancellationToken cancellationToken)
        {
            using PurifierContext context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await GetListWithContextAsync(context, query, cancellationToken);
        }

        public async Task<List<ExchangeFilter>> GetListWithContextAsync(PurifierContext context, SearchQuery query, CancellationToken cancellationToken)
        {
            return await context.ExchangeFilter
                .Where(entity => string.IsNullOrWhiteSpace(query.Query) || entity.FilterName.Contains(query.Query))
                .OrderBy(entity => entity.FilterName)
                .Skip((query.Pagination.CurrentPage - 1) * query.Pagination.ItemSize)
                .Take(query.Pagination.ItemSize)
                .ToListAsync(cancellationToken);
        }

        public async Task CreateAsync(ExchangeFilter exchangeFilter, CancellationToken cancellationToken)
        {
            using PurifierContext context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            await CreateWithContextAsync(context, exchangeFilter, cancellationToken);
        }

        public async Task CreateWithContextAsync(PurifierContext context, ExchangeFilter exchangeFilter, CancellationToken cancellationToken)
        {
            await context.ExchangeFilter.AddAsync(exchangeFilter, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(ExchangeFilter exchangeFilter, CancellationToken cancellationToken)
        {
            using PurifierContext context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            await UpdateWithContextAsync(context, exchangeFilter, cancellationToken);
        }

        public async Task UpdateWithContextAsync(PurifierContext context, ExchangeFilter exchangeFilter, CancellationToken cancellationToken)
        {
            await context.ExchangeFilter
                .Where(entity => entity.FilterName.Equals(exchangeFilter.FilterName))
                .ExecuteUpdateAsync(entity => entity
                    .SetProperty(x => x.LastExchnageDate, exchangeFilter.LastExchnageDate),
                    cancellationToken
                );
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(ExchangeFilter exchangeFilter, CancellationToken cancellationToken)
        {
            using PurifierContext context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            await DeleteWithContextAsync(context, exchangeFilter, cancellationToken);
        }

        public async Task DeleteWithContextAsync(PurifierContext context, ExchangeFilter exchangeFilter, CancellationToken cancellationToken)
        {
            await context.ExchangeFilter
                .Where(entity => entity.FilterName.Equals(exchangeFilter.FilterName))
                .ExecuteDeleteAsync(cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
