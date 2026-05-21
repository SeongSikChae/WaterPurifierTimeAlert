using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.RelativeTime.Parser;

namespace WaterPurifierTimeAlert.Server.Controllers
{
    using Server.Context;
    using Server.Context.Entity;
    using Server.Context.Store;
    using Server.Entity;

    [ApiController]
    [Route("api/[controller]")]
    public class FilterTypeController(PurifierContext context, IFilterTypeStore store, IRelativeTimeExpressionParser relativeTimeExpressionParser) : ControllerBase
    {
        [HttpPost("GetFilterTypes")]
        [AllowAnonymous]
        public async Task<IEnumerable<FilterType>> GetAsync([FromBody] SearchQuery query, CancellationToken cancellationToken)
        {
            return await store.GetListWithContextAsync(context, query, cancellationToken);
        }

        [HttpPut("CreateFilterType")]
        [Authorize("Certificate")]
        public async Task CreateAsync([FromBody] FilterType filterType, CancellationToken cancellationToken)
        {
            relativeTimeExpressionParser.Parse(filterType.ExpireTime);
            await store.CreateWithContextAsync(context, filterType, cancellationToken);
        }

        [HttpPut("UpdateFilterType")]
        [Authorize("Certificate")]
        public async Task UpdateAsync([FromBody] FilterType filterType, CancellationToken cancellationToken)
        {
            relativeTimeExpressionParser.Parse(filterType.ExpireTime);
            await store.UpdateWithContextAsync(context, filterType, cancellationToken);
        }

        [HttpPut("DeleteFilterType")]
        [Authorize("Certificate")]
        public async Task DeleteAsync([FromBody] FilterType filterType, CancellationToken cancellationToken)
        {
            await store.DeleteWithContextAsync(context, filterType, cancellationToken);
        }
    }
}
