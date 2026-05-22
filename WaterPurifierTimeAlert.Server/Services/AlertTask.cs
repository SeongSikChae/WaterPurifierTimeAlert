using Quartz;
using System.RelativeTime;
using System.RelativeTime.Ast;
using System.RelativeTime.Parser;

namespace WaterPurifierTimeAlert.Server.Services
{
    using Server.Context.Entity;
    using Server.Context.Store;

    public sealed class AlertTask : AsyncTask
    {
        private const string TASK_ID = "AlertTask";

        private readonly WebPushSender pushSender;
        private readonly IFilterTypeStore filterTypeStore;
        private readonly IExchangeFilterStore exchangeFilterStore;
        private readonly IRelativeTimeExpressionParser relativeTimeExpressionParser;
        private readonly IRelativeTimeToDateTimeParser relativeTimeToDateTimeParser;
        private readonly ITaskScheduler taskScheduler;

        public AlertTask(WebPushSender pushSender, IFilterTypeStore filterTypeStore, IExchangeFilterStore exchangeFilterStore, IRelativeTimeExpressionParser relativeTimeExpressionParser, IRelativeTimeToDateTimeParser relativeTimeToDateTimeParser, ITaskScheduler taskScheduler)
        {
            this.pushSender = pushSender;
            this.filterTypeStore = filterTypeStore;
            this.exchangeFilterStore = exchangeFilterStore;
            this.relativeTimeExpressionParser = relativeTimeExpressionParser;
            this.relativeTimeToDateTimeParser = relativeTimeToDateTimeParser;
            this.taskScheduler = taskScheduler;

            // taskScheduler.AddTask(TASK_ID, this, new CronExpression("0 0 9 * * ?"));
            taskScheduler.AddTask(TASK_ID, this, new CronExpression("0 * * * * ?"));
        }

        private volatile bool disposed = false;

        public override async Task RunAsync(CancellationToken cancellationToken)
        {
            List<ExchangeFilter> list = await exchangeFilterStore.GetListAsync(new Entity.SearchQuery
            {
                Pagination = new Entity.Pagination
                {
                    CurrentPage = 1,
                    ItemSize = int.MaxValue
                },
            }, cancellationToken);
            foreach (ExchangeFilter filter in list)
            {
                FilterType? filterType = await filterTypeStore.FindAsync(filter.FilterName, cancellationToken);
                if (filterType is not null)
                {
                    IRelativeTimeExpression expression = relativeTimeExpressionParser.Parse(filterType.ExpireTime);
                    filter.NextExchnageDate = relativeTimeToDateTimeParser.Parse(expression, filter.LastExchnageDate);
                }
            }

            foreach (ExchangeFilter filter in list)
            {
                await pushSender.SendToAllAsync(new
                {
                    title = "필터 교체 알림",
                    body = $"필터: {filter.FilterName}\n다음 교체일: {filter.NextExchnageDate:yyyy-MM-dd}",
                    tag = filter.FilterName,
                    filter,
                }, cancellationToken);
            }
        }

        public override void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                taskScheduler.RemoveTask(TASK_ID);
            }
        }
    }
}
