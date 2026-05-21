using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.RelativeTime;
using System.RelativeTime.Ast;
using System.RelativeTime.Parser;

namespace WaterPurifierTimeAlert.Server.Controllers
{
    using Server.Context;
    using Server.Context.Entity;
    using Server.Context.Store;
    using Server.Entity;

    [Route("api/[controller]")]
    [ApiController]
    public class ExchangeFilterController(PurifierContext context, IFilterTypeStore filterTypeStore, IExchangeFilterStore exchangeFilterStore, IRelativeTimeExpressionParser relativeTimeExpressionParser, IRelativeTimeToDateTimeParser relativeTimeToDateTimeParser) : ControllerBase
    {
        [HttpPost("GetExchangeFilters")]
        [AllowAnonymous]
        public async Task<IEnumerable<ExchangeFilter>> GetAsync([FromBody] SearchQuery query, CancellationToken cancellationToken)
        {
            List<ExchangeFilter> exchanges = await exchangeFilterStore.GetListWithContextAsync(context, query, cancellationToken);
            foreach (ExchangeFilter exchangeFilter in exchanges)
            {
                FilterType? filterType = await filterTypeStore.FindAsync(exchangeFilter.FilterName, cancellationToken);
                if (filterType is not null)
                {
                    IRelativeTimeExpression expression = relativeTimeExpressionParser.Parse(filterType.ExpireTime);
                    exchangeFilter.NextExchnageDate = relativeTimeToDateTimeParser.Parse(expression, exchangeFilter.LastExchnageDate);
                }
            }
            return exchanges;
        }

        [HttpPut("CreateExchangeFilter")]
        [Authorize("Certificate")]
        public async Task CreateAsync([FromBody] ExchangeFilter exchangeFilter, CancellationToken cancellationToken)
        {
            await exchangeFilterStore.CreateWithContextAsync(context, exchangeFilter, cancellationToken);
        }

        [HttpPut("UpdateExchangeFilter")]
        [Authorize("Certificate")]
        public async Task UpdateAsync([FromBody] ExchangeFilter exchangeFilter, CancellationToken cancellationToken)
        {
            await exchangeFilterStore.UpdateWithContextAsync(context, exchangeFilter, cancellationToken);
        }

        [HttpPut("DeleteExchangeFilter")]
        [Authorize("Certificate")]
        public async Task DeleteAsync([FromBody] ExchangeFilter exchangeFilter, CancellationToken cancellationToken)
        {
            await exchangeFilterStore.DeleteWithContextAsync(context, exchangeFilter, cancellationToken);
        }
    }
}
