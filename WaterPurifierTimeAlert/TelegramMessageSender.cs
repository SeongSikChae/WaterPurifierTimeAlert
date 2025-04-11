using System.Collections.Concurrent;
using System.Text;
using Telegram.Bot;

namespace WaterPurifierTimeAlert
{
	internal sealed class TelegramMessageSender(ConcurrentQueue<long> recevierQueue, TelegramBotClient client) : TextWriter
	{
		public override Encoding Encoding => Encoding.UTF8;

		public override void Write(string? value)
		{
			if (recevierQueue.TryDequeue(out long id) && value is not null)
				client.SendMessage(id, value);
		}
	}
}
