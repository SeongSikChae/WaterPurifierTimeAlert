using CommandLine;
using System.Collections.Concurrent;
using System.CommandLine.Parsing;
using System.RelativeTime;
using System.RelativeTime.Ast;
using System.RelativeTime.Parser;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using WaterPurifierTimeAlert.Context.Entity;
using WaterPurifierTimeAlert.Context.Store;

namespace WaterPurifierTimeAlert
{
	public sealed class AlertTask : SyncTask
	{
		public const string TASK_ID = "ALERT_TASK";

		private readonly IFilterTypeStore filterTypeStore;
		private readonly IExchangeFilterStore exchangeFilterStore;
		private readonly ITaskScheduler taskScheduler;
		private readonly IRelativeTimeExpressionParser relativeTimeExpressionParser;
		private readonly IRelativeTimeToDateTimeParser relativeTimeToDateTimeParser;
		private readonly Configuration configuration;
		private readonly TelegramBotClient client;
		private readonly CommandLine.Parser parser;
		private readonly ConcurrentQueue<long> recevierQueue;
		private readonly TelegramMessageSender messageSender;

		public AlertTask(IFilterTypeStore filterTypeStore, IExchangeFilterStore exchangeFilterStore, ITaskScheduler taskScheduler, IRelativeTimeExpressionParser relativeTimeExpressionParser, IRelativeTimeToDateTimeParser relativeTimeToDateTimeParser, Configuration configuration)
		{
			this.filterTypeStore = filterTypeStore;
			this.exchangeFilterStore = exchangeFilterStore;
			this.taskScheduler = taskScheduler;
			this.relativeTimeExpressionParser = relativeTimeExpressionParser;
			this.relativeTimeToDateTimeParser = relativeTimeToDateTimeParser;
			this.configuration = configuration;
			client = new TelegramBotClient(configuration.TelegramBotToken);
			client.OnMessage += Client_OnMessage;
			recevierQueue = new ConcurrentQueue<long>();
			messageSender = new TelegramMessageSender(recevierQueue, client);
			// CommandLine.Parser.Default.Settings.HelpWriter = messageSender;
			parser = new CommandLine.Parser(configure =>
			{
				configure.HelpWriter = messageSender;
			});
		}

		[Verb("listFilterType", HelpText = "Get List Filter Type")]
		internal sealed class ListFilterTypeCommand
		{			
		}

		[Verb("addFilterType", HelpText = "Add Filter Type")]
		internal sealed class AddFilterTypeCommand
		{
			[Option("filterName", Required = true)]
			public string FilterName { get; set; } = null!;

			[Option("expireTime", Required = true)]
			public string ExpireTime { get; set; } = null!;
		}

		[Verb("exchageFilter", HelpText = "Exchage Filter")]
		internal sealed class ExchangeFilterCommand
		{
			[Option("filterName", Required = true)]
			public string FilterName { get; set; } = null!;

			[Option("lastExchnageDate")]
			public string? LastExchnageDate { get; set; }
		}

		private Task Client_OnMessage(Message message, Telegram.Bot.Types.Enums.UpdateType type)
		{
			recevierQueue.Enqueue(message.Chat.Id);

			if (message.Text is null)
				return Task.CompletedTask;

			ParserResult<object> result = parser.ParseArguments<ListFilterTypeCommand, AddFilterTypeCommand, ExchangeFilterCommand>(CommandLineStringSplitter.Instance.Split(message.Text));
			return result.MapResult((ListFilterTypeCommand cmd) =>
			{
				foreach (FilterType filterType in filterTypeStore.GetList())
					client.SendMessage(message.Chat.Id, $"필터명: {filterType.Name}, 유효기간: {filterType.ExpireTime}");
				return Task.CompletedTask;
			},
			(AddFilterTypeCommand cmd) =>
			{
				return filterTypeStore.CreateAsync(new FilterType
				{
					Name = cmd.FilterName,
					ExpireTime = cmd.ExpireTime
				});
			},
			(ExchangeFilterCommand cmd) =>
			{
				return exchangeFilterStore.AddOrUpdateAsync(new ExchangeFilter
				{
					FilterName = cmd.FilterName,
					LastExchnageDate = cmd.LastExchnageDate is null ? DateTime.Now : DateTime.ParseExact(cmd.LastExchnageDate, "yyyy-MM-dd", System.Globalization.CultureInfo.CurrentCulture)
				});
			},
			(errors) =>
			{
				if (errors.IsHelp())
					return Task.CompletedTask;

				StringBuilder builder = new StringBuilder();
				foreach (Error err in errors)
				{
					switch (err)
					{
						case MissingRequiredOptionError missingRequiredOptionError:
							builder.AppendLine($"Required option '{missingRequiredOptionError.NameInfo.NameText}' is missing");
							break;
						case BadVerbSelectedError badVerbSelectedError:
							builder.AppendLine($"{badVerbSelectedError.Tag}: Verb '{badVerbSelectedError.Token} is not recognized.");
							break;
						default:
							builder.AppendLine($"{err.Tag}: {err}");
							break;
					}
				}
				client.SendMessage(message.Chat.Id, builder.ToString());
				return Task.CompletedTask;
			});
		}

		public override void Run(CancellationToken cancellationToken)
		{
			IEnumerable<FilterType> filterTypeList = filterTypeStore.GetList();
			foreach (ExchangeFilter exchangeFilter in exchangeFilterStore.GetList())
			{
				if (cancellationToken.IsCancellationRequested)
					break;

				FilterType? filterType = filterTypeList.Where(filterType => filterType.Name.Equals(exchangeFilter.FilterName)).FirstOrDefault();
				ArgumentNullException.ThrowIfNull(filterType);

				IRelativeTimeExpression expression = relativeTimeExpressionParser.Parse(filterType.ExpireTime);
				DateTime expireTime = relativeTimeToDateTimeParser.Parse(expression, exchangeFilter.LastExchnageDate);

				foreach (string chatId in configuration.Receivers) 
				{
					StringBuilder builder = new StringBuilder($"필터명: {exchangeFilter.FilterName}")
						.AppendLine()
						.AppendLine($"마지막 교환 시간: {exchangeFilter.LastExchnageDate.ToString("yyyy-MM-dd")}")
						.AppendLine($"다음 교환 시간: {expireTime.ToString("yyyy-MM-dd")}");

					client.SendMessage(new ChatId(chatId), builder.ToString(), cancellationToken: cancellationToken);
				}
			}
		}

		private bool disposedValue;

		public override void Dispose()
		{
			if (!disposedValue)
			{
				taskScheduler.RemoveTask(TASK_ID);
				disposedValue = true;
			}
		}
	}
}
