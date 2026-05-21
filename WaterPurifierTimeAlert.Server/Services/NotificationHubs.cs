using System.Collections.Concurrent;

namespace WaterPurifierTimeAlert.Server.Services
{
    using Server.Context.Entity;

    public sealed class NotificationHubs
    {
        private readonly ConcurrentDictionary<string, BlockingCollection<ExchangeFilter>> hubs = new ConcurrentDictionary<string, BlockingCollection<ExchangeFilter>>();

        public BlockingCollection<ExchangeFilter> GetHub(string name)
        {
            return hubs.GetOrAdd(name, new BlockingCollection<ExchangeFilter>());
        }

        public IEnumerable<string> GetHubNames()
        {
            return hubs.Keys;
        }
    }
}
