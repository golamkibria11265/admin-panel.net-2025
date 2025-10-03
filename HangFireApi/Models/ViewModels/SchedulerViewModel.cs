namespace HangFireApi.Models.ViewModels
{
    public class SchedulerViewModel
    {
        public List<Scheduler> Schedulers { get; set; } = [];
        public class Scheduler
        {
            public string? Name { get; set; }
            public string? CronExpression { get; set; }
            public bool Enabled { get; set; }
        }
    }
}
