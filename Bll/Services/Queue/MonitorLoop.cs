using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bll.Services.Queue
{
    public sealed class MonitorLoop(IBackgroundTaskQueue taskQueue,ILogger<MonitorLoop> logger,IHostApplicationLifetime applicationLifetime)
    {
        private readonly CancellationToken _cancellationToken = applicationLifetime.ApplicationStopping;
        public void StartMonitorLoop()
        {
            logger.LogInformation($"{nameof(MonitorAsync)} loop is starting.");

            Task.Run(async () => await MonitorAsync());
        }

        // Demo code (Microsoft sample): reads keys from an interactive console. It is not registered in DI;
        // inside a container/service there is no console, so it exits instead of throwing.
        private async ValueTask MonitorAsync()
        {
            if (Console.IsInputRedirected || !Environment.UserInteractive)
            {
                logger.LogInformation("{Name} disabled: no interactive console.", nameof(MonitorLoop));
                return;
            }
            while (!_cancellationToken.IsCancellationRequested)
            {
                var keyStroke = Console.ReadKey();
                if (keyStroke.Key == ConsoleKey.W)
                {
                    await taskQueue.QueueBackgroundWorkItemAsync(BuildWorkItemAsync);
                }
            }
        }
        private async ValueTask BuildWorkItemAsync(CancellationToken token)
        {
            int delayLoop = 0;
            var guid = Guid.NewGuid();

            logger.LogInformation("Queued work item {Guid} is starting.", guid);

            while (!token.IsCancellationRequested && delayLoop < 3)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), token);
                }
                catch (OperationCanceledException)
                {
                    // Prevent throwing if the Delay is cancelled
                }

                ++delayLoop;

                logger.LogInformation("Queued work item {Guid} is running. {DelayLoop}/3", guid, delayLoop);
            }

            if (delayLoop is 3)
            {
                logger.LogInformation("Queued Background Task {Guid} is complete.", guid);
            }
            else
            {
                logger.LogInformation("Queued Background Task {Guid} was cancelled.", guid);
            }
        }
    }
}
