using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace WaterPurifierTimeAlert.Server.Services
{
    using Server.Context.Entity;

    public sealed class WebSocketProcessor(NotificationHubs hubs)
    {
        public async Task ProcessAsync(WebSocket webSocket, X509Certificate2? clientCertificate)
        {
            Task receiveTask = ReceiveAsync(webSocket);
            Task sendTask = clientCertificate is null
                ? Task.Delay(Timeout.Infinite)
                : SendAsync(webSocket, clientCertificate);

            await Task.WhenAny(receiveTask, sendTask);

            try
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
            }
            catch { }
        }

        private async Task ReceiveAsync(WebSocket webSocket)
        {
            byte[] buffer = new byte[1024];

            while (webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                    break;
            }
        }

        private async Task SendAsync(WebSocket webSocket, X509Certificate2 clientCertificate)
        {
            string emailName = clientCertificate.GetNameInfo(X509NameType.EmailName, false);

            while (webSocket.State == WebSocketState.Open)
            {
                BlockingCollection<ExchangeFilter> hub = hubs.GetHub(emailName);
                while (hub.TryTake(out ExchangeFilter? filter, TimeSpan.FromSeconds(1)))
                {
                    string json = JsonSerializer.Serialize(filter, JsonSerializerOptions.Web);
                    await webSocket.SendAsync(Encoding.UTF8.GetBytes(json), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}
