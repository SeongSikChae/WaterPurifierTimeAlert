using Microsoft.EntityFrameworkCore;

namespace WaterPurifierTimeAlert.Server.Context.Store
{
    using Server.Entity;
    using Server.Context.Entity;

    public interface IFilterTypeStore
    {
        Task<List<FilterType>> GetListAsync(SearchQuery query, CancellationToken cancellationToken);

        Task<List<FilterType>> GetListWithContextAsync(PurifierContext context, SearchQuery query, CancellationToken cancellationToken);

        Task<FilterType?> FindAsync(string name, CancellationToken cancellationToken);

        Task<FilterType?> FindWithContextAsync(PurifierContext context, string name, CancellationToken cancellationToken);

        Task CreateAsync(FilterType filterType, CancellationToken cancellationToken);

        Task CreateWithContextAsync(PurifierContext context, FilterType filterType, CancellationToken cancellationToken);

        Task UpdateAsync(FilterType filterType, CancellationToken cancellationToken);

        Task UpdateWithContextAsync(PurifierContext context, FilterType filterType, CancellationToken cancellationToken);

        Task DeleteAsync(FilterType filterType, CancellationToken cancellationToken);

        Task DeleteWithContextAsync(PurifierContext context, FilterType filterType, CancellationToken cancellationToken);
    }

    public sealed class FilterTypeStore(IDbContextFactory<PurifierContext> dbContextFactory) : IFilterTypeStore
    {
        public async Task<List<FilterType>> GetListAsync(SearchQuery query, CancellationToken cancellationToken)
        {
            using PurifierContext context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await GetListWithContextAsync(context, query, cancellationToken);
        }

        public async Task<List<FilterType>> GetListWithContextAsync(PurifierContext context, SearchQuery query, CancellationToken cancellationToken)
        {
            return await context.FilterType
                .Where(entity => string.IsNullOrWhiteSpace(query.Query) || entity.Name.Contains(query.Query))
                .OrderBy(entity => entity.Name)
                .Skip((query.Pagination.CurrentPage - 1) * query.Pagination.ItemSize)
                .Take(query.Pagination.ItemSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<FilterType?> FindAsync(string name, CancellationToken cancellationToken)
        {
            using PurifierContext context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await FindWithContextAsync(context, name, cancellationToken);
        }

        public async Task<FilterType?> FindWithContextAsync(PurifierContext context, string name, CancellationToken cancellationToken)
        {
            return await context.FilterType.Where(entity => entity.Name.Equals(name)).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task CreateAsync(FilterType filterType, CancellationToken cancellationToken)
        {
            using PurifierContext context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            await CreateWithContextAsync(context, filterType, cancellationToken);
        }

        public async Task CreateWithContextAsync(PurifierContext context, FilterType filterType, CancellationToken cancellationToken)
        {
            await context.FilterType.AddAsync(filterType, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(FilterType filterType, CancellationToken cancellationToken)
        {
            using PurifierContext context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            await UpdateWithContextAsync(context, filterType, cancellationToken);
        }

        public async Task UpdateWithContextAsync(PurifierContext context, FilterType filterType, CancellationToken cancellationToken)
        {
            await context.FilterType
                .Where(entity => entity.Name.Equals(filterType.Name))
                .ExecuteUpdateAsync(entity => entity
                    .SetProperty(x => x.ExpireTime, filterType.ExpireTime),
                    cancellationToken
                );
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(FilterType filterType, CancellationToken cancellationToken)
        {
            using PurifierContext context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            await DeleteWithContextAsync(context, filterType, cancellationToken);
        }

        public async Task DeleteWithContextAsync(PurifierContext context, FilterType filterType, CancellationToken cancellationToken)
        {
            await context.FilterType
                .Where(entity => entity.Name.Equals(filterType.Name))
                .ExecuteDeleteAsync(cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
