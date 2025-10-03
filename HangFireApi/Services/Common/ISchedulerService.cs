namespace HangFireApi.Services.Common
{
    public interface ISchedulerService
    {
        [DisableConcurrentExecutionWithArg(6000, 0)]
        Task HangFireScheduler();
    }

    public class SchedulerService: ISchedulerService
    {
        public SchedulerService()
        {
                
        }
        public Task HangFireScheduler()
        {
            throw new NotImplementedException();
        }
    }
}
