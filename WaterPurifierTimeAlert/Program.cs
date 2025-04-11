using CommandLine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Configuration;
using System.Configuration;
using System.RelativeTime;
using System.RelativeTime.Parser;
using System.Revision;
using WaterPurifierTimeAlert.Context;
using WaterPurifierTimeAlert.Context.Store;

namespace WaterPurifierTimeAlert
{
    internal class Program
    {
        public sealed class CmdMain
        {
			[Option("config", Required = true, HelpText = "config file path")]
			public string ConfigFilePath { get; set; } = null!;

			[Option("log", Required = true, HelpText = "log dir path")]
			public string LogDirPath { get; set; } = null!;
		}

        static async Task Main(string[] args)
        {
            ParserResult<CmdMain> result = await Parser.Default.ParseArguments<CmdMain>(args).WithParsedAsync(async cmdMain =>
            {
				HostApplicationBuilder builder = CreateApplicationHostBuilder(cmdMain, args);
				IHost host = builder.Build();
				await host.RunAsync();
			});

			await result.WithNotParsedAsync(async errors =>
			{
				if (errors.IsVersion())
					errors.Output().WriteLine(RevisionUtil.GetRevision<RevisionAttribute>());

				await Task.CompletedTask;
			});
		}

		static HostApplicationBuilder CreateApplicationHostBuilder(CmdMain cmdMain, string[] args)
		{
			YamlDotNet.Serialization.Deserializer deserializer = new YamlDotNet.Serialization.Deserializer();
			Configuration configuration = deserializer.Deserialize<Configuration>(File.ReadAllText(cmdMain.ConfigFilePath));
			ConfigurationValidator.Validate(configuration);
			return CreateApplicationHostBuilder(cmdMain, configuration, args);
		}

		static HostApplicationBuilder CreateApplicationHostBuilder(CmdMain cmdMain, Configuration configuration, string[] args)
		{
			HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);


			builder.Logging.Services.AddSerilog(configure =>
			{
				configure.Enrich.WithCaller().WriteTo.File(new DirectoryInfo(cmdMain.LogDirPath).FullName, "waterPurifierTimeAlert.log", Serilog.Events.LogEventLevel.Information, CallerEnricherOutputTemplate.Default, rollingInterval: RollingInterval.Month, retainedFileCountLimit: 12);
			});

			builder.Services.AddSystemd();
			builder.Services.AddWindowsService();
			builder.Services.AddDbContextPool<PurifierContext>(builder =>
			{
				DirectoryInfo? directory = new FileInfo(configuration.DbPath).Directory;
				if (directory is not null && !directory.Exists)
					directory.Create();
				builder.UseSqlite($"Data Source={configuration.DbPath}");
				using PurifierContext context = new PurifierContext((DbContextOptions<PurifierContext>)builder.Options);
				context.Database.Migrate();
			});
			builder.Services.AddSingleton(RelativeTimeExpressionParserFactory.Create());
			builder.Services.AddSingleton(RelativeTimeExpressionToDateTimeParserFactory.Create());
			builder.Services.AddSingleton(configuration);
			builder.Services.AddSingleton<IFilterTypeStore, IFilterTypeStore.FilterTypeStore>();
			builder.Services.AddSingleton<IExchangeFilterStore, IExchangeFilterStore.ExchangeFilterStore>();
			builder.Services.AddSingleton<ITaskScheduler, DefaultTaskScheduler>();
			builder.Services.AddSingleton<AlertTask>();
			builder.Services.AddHostedService<ServiceWorker>();
			return builder;
		}
	}
}
