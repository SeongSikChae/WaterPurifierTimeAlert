using System.Configuration.Annotation;

namespace WaterPurifierTimeAlert
{
    public sealed class Configuration : IValidatableConfiguration
    {
		[Property(PropertyType.STRING, required: true)]
		public string DbPath { get; set; } = null!;

		[Property(PropertyType.STRING, required: true)]
		public string CronExpression { get; set; } = null!;

		[Property(PropertyType.STRING, required: true)]
		public string TelegramBotToken { get; set; } = null!;

		public List<string> Receivers { get; set; } = null!;

		public void Validate()
		{
			if (Receivers is null)
				throw new Exception($"config field '{nameof(Receivers)}' must be provided");
		}
	}
}
