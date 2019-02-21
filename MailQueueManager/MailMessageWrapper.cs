using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;

namespace MailQueueManager
{
    public class MailMessageWrapper
    {
        public MailMessageWrapper()
        {
            Exceptions = Array.Empty<string>();
            Attachments = Array.Empty<AttachmentWrapper>();
        }

        public Guid Id { get; set; }

        public string Subject { get; set; }

        public string[] To { get; set; }

        public string From { get; set; }

        public string Body { get; set; }

        public AttachmentWrapper[] Attachments { get; set; }

        public string[] Exceptions { get; set; }

        public MailMessage Get()
        {
            var msg = new MailMessage
            {
                IsBodyHtml = true,
                Body = Body,
                Subject = Subject,
                From = new MailAddress(From)
            };

            foreach (var to in To)
            {
                msg.To.Add(to);
            }

            if (Attachments != null && Attachments.Length > 0)
            {
                foreach (var at in Attachments)
                {
                    var attachment =
                        new Attachment(new MemoryStream(at.Data),
                            new ContentType(at.ContentType));
                    msg.Attachments.Add(attachment);
                }
            }

            return msg;
        }

        public void Set(MailMessage msg)
        {
            From = msg.From.Address;

            if (msg.To.Count <= 0)
            {
                throw new Exception("TO address missing");
            }

            To = msg.To.Select(x => x.Address).ToArray();
            Subject = msg.Subject;
            Body = msg.Body;

            var list = new List<AttachmentWrapper>();
            foreach (var at in msg.Attachments)
            {
                var attachment = new AttachmentWrapper { ContentType = at.ContentType.Name };
                using (var ms = new MemoryStream())
                {
                    at.ContentStream.CopyTo(ms);
                    attachment.Data = ms.ToArray();
                }

                list.Add(attachment);
            }

            Attachments = list.ToArray();
        }
    }
}