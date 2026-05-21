using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;

namespace WaterPurifierTimeAlert.Server.Controllers
{
    using Server.Entity;

    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpGet("whoami")]
        [AllowAnonymous]
        public User Whoami()
        {
            X509Certificate2? clientCertificate = HttpContext.Connection.ClientCertificate;
            if (clientCertificate is not null)
            {
                return new User
                {
                    Email = clientCertificate.GetNameInfo(X509NameType.EmailName, false),
                    NotAfter = clientCertificate.NotAfter.ToMilliseconds(),
                    NotBefore = clientCertificate.NotBefore.ToMilliseconds(),
                    Thumbprint = clientCertificate.Thumbprint
                };
            }

            return new User
            {
                Email = "Anonymous",
                Thumbprint = string.Empty
            };
        }
    }
}
