namespace MailQueueManager
{
    public class ManagerSettings
    {
        public ManagerSettings()
        {
            SendIntervalMinutes = 1;
        }

        public string FileStore { get; set; }

        public int SendIntervalMinutes { get; set; }

        public string DefaultEmailAddress { get; set; }
    }
}