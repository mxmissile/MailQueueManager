using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using FluentScheduler;
using Newtonsoft.Json;

namespace MailQueueManager
{
    public static class Manager
    {
        static Manager()
        {
            Settings = new ManagerSettings();
            Initialize();
        }

        public static void Initialize()
        {
            JobManager.Initialize();
            JobManager.AddJob(ProcessQueue, s => s.ToRunEvery(Settings.SendIntervalMinutes).Minutes());
        }

        public static ManagerSettings Settings { get; set; }

        public static void Add(MailMessage msg)
        {
            var wrapped = new MailMessageWrapper();
            wrapped.Set(msg);

           Add(wrapped);
        }

        public static void Add(MailMessageWrapper msg)
        {
            msg.Id = Guid.NewGuid();
            var json = JsonConvert.SerializeObject(msg, Formatting.Indented);

            var path = Path.Combine(Settings.FileStore, $"{msg.Id}.json");

            File.WriteAllText(path, json);
        }


        public static MailMessageWrapper[] Queue
        {
            get
            {
                var files = Directory.GetFiles(Settings.FileStore, "*.json");

                var list = new List<MailMessageWrapper>();
                foreach (var file in files)
                {
                    var wrapper = JsonConvert.DeserializeObject<MailMessageWrapper>(File.ReadAllText(file));
                    list.Add(wrapper);
                }

                return list.ToArray();
            }
        }

        private static void Process(MailMessageWrapper wrapper)
        {
            if (string.IsNullOrWhiteSpace(wrapper.From))
            {
                wrapper.From = "noreply@fusion-imaging.com";
            }

            var msg = wrapper.Get();

            try
            {

                using (var client = new SmtpClient())
                {
#if (DEBUG)
                    msg.To.Clear();
                    msg.To.Add("travish@fusion-imaging.com");
                    msg.Subject = "[DEBUG] " + msg.Subject;
                    client.Send(msg);
#else
                            client.Send(msg);
#endif
                }

                var path = Path.Combine(Settings.FileStore, $"{wrapper.Id}.json");
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // log and swallow so we can try again later
            }
        }

        public static void ProcessQueue()
        {
            foreach (var msg in Queue)
            {
                Process(msg);
            }
        }
    }
}