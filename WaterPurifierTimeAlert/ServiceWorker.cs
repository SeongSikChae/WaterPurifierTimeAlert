using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;

namespace WaterPurifierTimeAlert
{
	internal sealed class ServiceWorker(IServiceProvider serviceProvider) : IHostedService, IHostedLifecycleService
	{
		public Task StartingAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		public Task StartedAsync(CancellationToken cancellationToken)
		{
			Configuration configuration = serviceProvider.GetRequiredService<Configuration>();
			ITaskScheduler taskScheduler = serviceProvider.GetRequiredService<ITaskScheduler>();
			AlertTask alertTask = serviceProvider.GetRequiredService<AlertTask>();
			taskScheduler.AddTask(AlertTask.TASK_ID, alertTask, new CronExpression(configuration.CronExpression));
			return Task.CompletedTask;
		}

		public Task StoppingAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		public Task StoppedAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
