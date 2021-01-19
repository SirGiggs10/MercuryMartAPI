using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MercuryMartAPI.Dtos.General;
using Microsoft.AspNetCore.Http;
using SendGrid;

namespace MercuryMartAPI.Interfaces
{
    public interface IMailRepository
    {
        Task<Response> SendMail(MailModel _mailModel);
        Task<Response> SendBulkMail(BulkMailModel _bulkmailModel);
        Task<Response> SendMail(MailModel _mailModel, List<string> emailAddresses);
    }
}
