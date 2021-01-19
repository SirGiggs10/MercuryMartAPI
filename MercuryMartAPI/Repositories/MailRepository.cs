using MercuryMartAPI.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;
using MercuryMartAPI.Interfaces;
using MercuryMartAPI.Dtos.General;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Castle.DynamicProxy.Generators;
using System.Net.Http;
using System.Net;

namespace MercuryMartAPI.Repositories
{
    public class MailRepository : IMailRepository
    {
        private readonly IConfiguration _configuration;

        public MailRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<Response> SendMail(MailModel _mailModel)
        {
            var mailClient = new SendGridClient(_configuration.GetValue<string>("ApiKey"));
            var mailObject = MailHelper.CreateSingleEmail(new EmailAddress(_mailModel.From, _mailModel.FromName), new EmailAddress(_mailModel.To), _mailModel.Subject, _mailModel.Body, _mailModel.Body);       
            if(_mailModel.MailAttachments != null)
            {
                if(_mailModel.MailAttachments.Any())
                {
                    //var attachmentsToSend = new List<Attachment>();
                    foreach (var file in _mailModel.MailAttachments)
                    {
                        if (file.Length > 0 && file.Length < (100 * 1024 * 1024))  //MAXIMUM FILE SIZE - 100MB
                        {
                            using (var stream = file.OpenReadStream())
                            {
                                await mailObject.AddAttachmentAsync(file.FileName, stream);
                            }
                        }
                        else
                        {
                            return new Response(HttpStatusCode.BadRequest, null, null);
                        }
                    }
                }
            }

            var mailResponse = await mailClient.SendEmailAsync(mailObject);
            return mailResponse;
        }

        public async Task<Response> SendBulkMail(BulkMailModel _bulkmailModel)
        {
            var mailClient = new SendGridClient(_configuration.GetValue<string>("ApiKey"));
            var mailObject = MailHelper.CreateSingleEmailToMultipleRecipients(new EmailAddress(_bulkmailModel.From, _bulkmailModel.FromName), _bulkmailModel.To, _bulkmailModel.Subject, _bulkmailModel.Body, _bulkmailModel.Body);
            var mailResponse = await mailClient.SendEmailAsync(mailObject);
            return mailResponse;
        }

        public async Task<Response> SendMail(MailModel _mailModel, List<string> emailAddresses)
        {
            List<EmailAddress> allEmail = new List<EmailAddress>();
            foreach (var f in emailAddresses)
            {
                allEmail.Add(new EmailAddress(f));
            }

            var mailClient = new SendGridClient(_configuration.GetValue<string>("ApiKey"));
            var mailObject = MailHelper.CreateSingleEmailToMultipleRecipients(new EmailAddress(_mailModel.From, _mailModel.FromName), allEmail, _mailModel.Subject, _mailModel.Body, _mailModel.Body);
            if (_mailModel.MailAttachments != null)
            {
                if (_mailModel.MailAttachments.Any())
                {
                    //var attachmentsToSend = new List<Attachment>();
                    foreach (var file in _mailModel.MailAttachments)
                    {
                        if (file.Length > 0 && file.Length < (100 * 1024 * 1024))  //MAXIMUM FILE SIZE - 100MB
                        {
                            using (var stream = file.OpenReadStream())
                            {
                                await mailObject.AddAttachmentAsync(file.FileName, stream);
                            }
                        }
                        else
                        {
                            return new Response(HttpStatusCode.BadRequest, null, null);
                        }
                    }
                }
            }

            var mailResponse = await mailClient.SendEmailAsync(mailObject);
            return mailResponse;
        }
    }
}
