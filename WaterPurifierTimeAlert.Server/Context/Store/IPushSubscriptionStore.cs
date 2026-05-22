using Microsoft.EntityFrameworkCore;

namespace WaterPurifierTimeAlert.Server.Context.Store
{
    using Server.Context.Entity;

    public interface IPushSubscriptionStore
    {
        Task<List<PushSubscription>> GetAllAsync(CancellationToken cancellationToken);

        Task<List<PushSubscription>> GetByUserAsync(string userEmail, CancellationToken cancellationToken);

        Task UpsertAsync(PushSubscription subscription, CancellationToken cancellationToken);

        Task DeleteAsync(string endpoint, CancellationToken cancellationToken);
    }

    public sealed class PushSubscriptionStore(IDbContextFactory<PurifierContext> dbContextFactory) : IPushSubscriptionStore
    {
        public async Task<List<PushSubscription>> GetAllAsync(CancellationToken cancellationToken)
        {
            using PurifierContext context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await context.PushSubscription.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<List<PushSubscription>> GetByUserAsync(string userEmail, CancellationToken cancellationToken)
        {
            using PurifierContext context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await context.PushSubscription.AsNoTracking()
                .Where(x => x.UserEmail == userEmail)
                .ToListAsync(cancellationToken);
        }

        public async Task UpsertAsync(PushSubscription subscription, CancellationToken cancellationToken)
        {
            using PurifierContext context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            PushSubscription? existing = await context.PushSubscription
                .FirstOrDefaultAsync(x => x.Endpoint == subscription.Endpoint, cancellationToken);
            if (existing is null)
            {
                await context.PushSubscription.AddAsync(subscription, cancellationToken);
            }
            else
            {
                existing.P256dh = subscription.P256dh;
                existing.Auth = subscription.Auth;
                existing.UserEmail = subscription.UserEmail;
            }
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(string endpoint, CancellationToken cancellationToken)
        {
            using PurifierContext context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            await context.PushSubscription
                .Where(x => x.Endpoint == endpoint)
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
