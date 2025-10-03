using Hangfire.Common;
using Hangfire.Server;

namespace HangFireApi
{
    public class DisableConcurrentExecutionWithArgAttribute : JobFilterAttribute, IServerFilter
    {
        private readonly int _timeoutInSeconds;
        private readonly int _argPos;
        public DisableConcurrentExecutionWithArgAttribute(int timeoutInSeconds, int argPos)
        {
            if (timeoutInSeconds < 0) throw new ArgumentException("Timeout argument value should be greater that zero.");
            _timeoutInSeconds = timeoutInSeconds;
            _argPos = argPos;
        }
        public void OnPerforming(PerformingContext filterContext)
        {
            var resource = GetResource(filterContext.BackgroundJob.Job);

            var timeout = TimeSpan.FromSeconds(_timeoutInSeconds);

            var distributedLock = filterContext.Connection.AcquireDistributedLock(resource, timeout);
            filterContext.Items["DistributedLock"] = distributedLock;
        }
        public void OnPerformed(PerformedContext filterContext)
        {
            if (!filterContext.Items.TryGetValue("DistributedLock", out var value) || value is null)
            {
                throw new InvalidOperationException("Can not release a distributed lock: it was not acquired.");
            }
            var distributedLock = (IDisposable)value;
            distributedLock.Dispose();
        }
        private static string GetResource(Job job)
        {
            return $"{job.Type}.{job.Method.Name}";
        }
    }
}
