using FluentScheduler;

namespace MailQueueManager
{
    public class ProcessMailJob : IJob
    {
        public void Execute()
        {
            Manager.ProcessQueue();
        }
    }
}