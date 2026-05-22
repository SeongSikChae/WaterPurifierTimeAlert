using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace WaterPurifierTimeAlert.Server.Services
{
    using Server.Context.Store;
    using EntityPushSubscription = Server.Context.Entity.PushSubscription;
    using WebPushSubscription = Lib.Net.Http.WebPush.PushSubscription;

    public sealed class WebPushSender
    {
        private readonly Configuration configuration;
        private readonly IPushSubscriptionStore subscriptionStore;
        private readonly ILogger<WebPushSender> logger;
        private readonly PushServiceClient pushServiceClient;
        private readonly bool isConfigured;

        public WebPushSender(Configuration configuration, IPushSubscriptionStore subscriptionStore, ILogger<WebPushSender> logger)
        {
            this.configuration = configuration;
            this.subscriptionStore = subscriptionStore;
            this.logger = logger;

            AppDomain.CurrentDomain.FirstChanceException += (_, args) =>
            {
                if (args.Exception is PushServiceClientException ex)
                {
                    logger.LogWarning("[first-chance] {Type} status={Status} msg={Message}",
                        ex.GetType().Name, ex.StatusCode, ex.Message);
                }
            };
            HttpClient httpClient = new HttpClient(new WnsResponseLoggingHandler(logger)
            {
                InnerHandler = new HttpClientHandler()
            });
            this.pushServiceClient = new PushServiceClient(httpClient);

            if (!string.IsNullOrWhiteSpace(configuration.VapidPublicKey)
             && !string.IsNullOrWhiteSpace(configuration.VapidPrivateKey)
             && !string.IsNullOrWhiteSpace(configuration.VapidSubject))
            {
                pushServiceClient.DefaultAuthentication = new VapidAuthentication(
                    configuration.VapidPublicKey,
                    configuration.VapidPrivateKey)
                {
                    Subject = configuration.VapidSubject
                };
                isConfigured = true;
            }
            else
            {
                isConfigured = false;
            }
        }

        public bool IsConfigured => isConfigured;

        public string? PublicKey => configuration.VapidPublicKey;

        public async Task SendAsync(EntityPushSubscription entity, object payload, CancellationToken cancellationToken)
        {
            if (!isConfigured)
            {
                logger.LogWarning("VAPID 키가 설정되지 않아 푸시를 보낼 수 없습니다.");
                return;
            }

            WebPushSubscription subscription = new WebPushSubscription
            {
                Endpoint = entity.Endpoint,
            };
            subscription.SetKey(PushEncryptionKeyName.P256DH, entity.P256dh);
            subscription.SetKey(PushEncryptionKeyName.Auth, entity.Auth);

            string json = JsonSerializer.Serialize(payload, JsonSerializerOptions.Web);
            PushMessage message = new PushMessage(json)
            {
                Urgency = PushMessageUrgency.High,
                TimeToLive = 60 * 60 * 24
            };

            try
            {
                await pushServiceClient.RequestPushMessageDeliveryAsync(subscription, message, cancellationToken);
                logger.LogInformation("푸시 발송 성공: {Host}", new Uri(entity.Endpoint).Host);
            }
            catch (PushServiceClientException ex) when (ex.StatusCode == HttpStatusCode.Gone || ex.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogInformation("만료된 구독 제거: {Endpoint}", entity.Endpoint);
                await subscriptionStore.DeleteAsync(entity.Endpoint, cancellationToken);
            }
            catch (PushServiceClientException ex)
            {
                logger.LogError(ex, "푸시 발송 실패: {Endpoint} status={StatusCode} body={Body}",
                    entity.Endpoint, ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "푸시 발송 실패: {Endpoint}", entity.Endpoint);
            }
        }

        public async Task SendToAllAsync(object payload, CancellationToken cancellationToken)
        {
            List<EntityPushSubscription> all = await subscriptionStore.GetAllAsync(cancellationToken);
            foreach (EntityPushSubscription sub in all)
                await SendAsync(sub, payload, cancellationToken);
        }

        public static (string PublicKey, string PrivateKey) GenerateVapidKeys()
        {
            using ECDsa ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            ECParameters parameters = ecdsa.ExportParameters(true);

            byte[] publicKey = new byte[65];
            publicKey[0] = 0x04;
            Buffer.BlockCopy(parameters.Q.X!, 0, publicKey, 1, 32);
            Buffer.BlockCopy(parameters.Q.Y!, 0, publicKey, 33, 32);

            return (Base64UrlEncode(publicKey), Base64UrlEncode(parameters.D!));
        }

        private static string Base64UrlEncode(byte[] data)
        {
            return Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }

    internal sealed class WnsResponseLoggingHandler : DelegatingHandler
    {
        private readonly ILogger logger;

        public WnsResponseLoggingHandler(ILogger logger)
        {
            this.logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string host = request.RequestUri?.Host ?? "?";
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            if (host.Contains("notify.windows.com"))
            {
                string wnsStatus = response.Headers.TryGetValues("X-WNS-Status", out var s1) ? string.Join(",", s1) : "-";
                string wnsNotifStatus = response.Headers.TryGetValues("X-WNS-NotificationStatus", out var s2) ? string.Join(",", s2) : "-";
                string wnsDeviceStatus = response.Headers.TryGetValues("X-WNS-DeviceConnectionStatus", out var s3) ? string.Join(",", s3) : "-";
                string wnsMsgId = response.Headers.TryGetValues("X-WNS-Msg-ID", out var s4) ? string.Join(",", s4) : "-";
                string wnsError = response.Headers.TryGetValues("X-WNS-Error-Description", out var s5) ? string.Join(",", s5) : "-";

                string body = string.Empty;
                if (response.Content is not null)
                {
                    try { body = await response.Content.ReadAsStringAsync(cancellationToken); }
                    catch { }
                }

                logger.LogInformation(
                    "[WNS] {Host} http={Http} X-WNS-Status={Status} NotificationStatus={NotifStatus} DeviceStatus={DeviceStatus} MsgId={MsgId} Error={Err} Body={Body}",
                    host, (int)response.StatusCode, wnsStatus, wnsNotifStatus, wnsDeviceStatus, wnsMsgId, wnsError, body);
            }

            return response;
        }
    }
}
