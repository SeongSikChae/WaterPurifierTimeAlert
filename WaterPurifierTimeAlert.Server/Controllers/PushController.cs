using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;

namespace WaterPurifierTimeAlert.Server.Controllers
{
    using Server.Context.Entity;
    using Server.Context.Store;
    using Server.Services;

    public sealed class PushSubscriptionKeysDto
    {
        public string P256dh { get; set; } = null!;
        public string Auth { get; set; } = null!;
    }

    public sealed class PushSubscriptionDto
    {
        public string Endpoint { get; set; } = null!;
        public PushSubscriptionKeysDto Keys { get; set; } = null!;
    }

    [Route("api/[controller]")]
    [ApiController]
    public class PushController(IPushSubscriptionStore store, WebPushSender sender) : ControllerBase
    {
        [HttpGet("public-key")]
        [AllowAnonymous]
        public ActionResult<string> GetPublicKey()
        {
            if (string.IsNullOrWhiteSpace(sender.PublicKey))
                return NotFound();
            return Ok(sender.PublicKey);
        }

        [HttpPost("subscribe")]
        [AllowAnonymous]
        public async Task<IActionResult> Subscribe([FromBody] PushSubscriptionDto dto, CancellationToken cancellationToken)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Endpoint) || dto.Keys is null)
                return BadRequest();

            X509Certificate2? clientCertificate = HttpContext.Connection.ClientCertificate;
            string email = clientCertificate is not null
                ? clientCertificate.GetNameInfo(X509NameType.EmailName, false)
                : "Anonymous";

            await store.UpsertAsync(new PushSubscription
            {
                Endpoint = dto.Endpoint,
                P256dh = dto.Keys.P256dh,
                Auth = dto.Keys.Auth,
                UserEmail = email,
            }, cancellationToken);

            return NoContent();
        }

        [HttpPost("unsubscribe")]
        [AllowAnonymous]
        public async Task<IActionResult> Unsubscribe([FromBody] PushSubscriptionDto dto, CancellationToken cancellationToken)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Endpoint))
                return BadRequest();
            await store.DeleteAsync(dto.Endpoint, cancellationToken);
            return NoContent();
        }
    }
}
