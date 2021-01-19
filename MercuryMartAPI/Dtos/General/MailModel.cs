using Microsoft.AspNetCore.Http;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Dtos.General
{
    public class MailModel
    {
        public MailModel(string _from, string fromName, string _to, string _subject, string _body, IFormFileCollection mailAttachments = null)
        {
            From = _from;
            FromName = fromName;
            To = _to;
            Subject = _subject;
            Body = _body;
            MailAttachments = mailAttachments;
        }
        public string From { get; set; }
        public string FromName { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public IFormFileCollection MailAttachments { get; set; }
    }
    public class BulkMailModel
    {
        public BulkMailModel(string _from, string fromName, List<EmailAddress> _to, string _subject, string _body)
        {
            From = _from;
            FromName = fromName;
            To = _to;
            Subject = _subject;
            Body = _body;
        }
        public string From { get; set; }
        public string FromName { get; set; }
        public List<EmailAddress> To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
