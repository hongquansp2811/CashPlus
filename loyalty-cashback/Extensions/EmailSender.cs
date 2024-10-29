using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using LOYALTY.Models;
using System.Collections.Generic;
using LOYALTY.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace LOYALTY.Extensions
{
    public class EmailSender : IEmailSender
    {
        public EmailSender(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        private static readonly HttpClient client = new HttpClient();
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var host = Configuration["Email:host"];
            var user = Configuration["Email:user"];
            var password = Configuration["Email:password"];

            var email_send = new MimeMessage();
            email_send.From.Add(MailboxAddress.Parse(user));
            email_send.To.Add(MailboxAddress.Parse(email));
            email_send.Subject = subject;
            email_send.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = message };

            // send email
            using var smtp = new SmtpClient();
            smtp.Connect(host, 25, SecureSocketOptions.StartTlsWhenAvailable);
            smtp.Authenticate(user, password);
            smtp.Send(email_send);
            smtp.Disconnect(true);
            return Task.CompletedTask;
        }

        public Task SendListEmailAsync(List<string> emails, string subject, string message)
        {
            var host = Configuration["Email:host"];
            var user = Configuration["Email:user"];
            var password = Configuration["Email:password"];

            if (emails.Count == 0)
            {
                return Task.CompletedTask;
            }
            var email_send = new MimeMessage();
            email_send.From.Add(MailboxAddress.Parse(user));
            for (int i = 0; i < emails.Count; i++)
            {
                email_send.To.Add(MailboxAddress.Parse(emails[i]));
            }
            email_send.Subject = subject;
            email_send.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = message };

            // send email
            using var smtp = new SmtpClient();
            smtp.Connect(host, 25, SecureSocketOptions.StartTls);
            smtp.Authenticate(user, password);
            smtp.Send(email_send);
            smtp.Disconnect(true);
            return Task.CompletedTask;
        }

        public async Task SendSms(string phone_number, string message)
        {
            try
            {
                var url_string = "http://rest.esms.vn/MainService.svc/json/SendMultipleMessage_V4_get?Phone=";
                url_string += phone_number;
                url_string += "&Content=";
                url_string += message;
                url_string += "&SecretKey=";
                url_string += Consts.ESMS_SECRET_KEY;
                url_string += "&ApiKey=";
                url_string += Consts.ESMS_API_KEY;
                url_string += "&SmsType=2&Brandname=";
                url_string += Consts.ESMS_BRAND_NAME;
                var responseSend = await client.GetAsync(url_string);
                responseSend.EnsureSuccessStatusCode();

                var content2 = await responseSend.Content.ReadAsStringAsync();
                JObject dataResponse = (JObject)JsonConvert.DeserializeObject(content2);

                TokenOTPResponse dataResponse2 = dataResponse.ToObject<TokenOTPResponse>();

                var data = new SMSHistory
                {
                    id = Guid.NewGuid(),
                    CodeResult = dataResponse2.CodeResult,
                    SMSID = dataResponse2.SMSID,
                    ErrorMessage = dataResponse2.ErrorMessage,
                    phone = phone_number,
                    message = message,
                    date_created = DateTime.Now
                };

                var services = new ServiceCollection();

                services.AddDbContext<LOYALTYContext>(options =>
                {
                    options.UseSqlServer(Configuration["ConnectionStrings:CoreDB"]);
                });

                var serviceProvider = services.BuildServiceProvider();

                using (var context = serviceProvider.GetService<LOYALTYContext>())
                {
                    context.SMSHistories.Add(data);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        private class TokenOTPResponse
        {
            public string? CodeResult;
            public string? CountRegenerate;
            public string? SMSID;
            public string? ErrorMessage;
        }

        public Task SendEmailAsyncTempalte(string email, string subject, string message)
        {
            var email_send = new MimeMessage();

            var host = Configuration["Email:host"];
            var user = Configuration["Email:user"];
            var password = Configuration["Email:password"];

            email_send.From.Add(MailboxAddress.Parse(user));
            email_send.To.Add(MailboxAddress.Parse(email));
            email_send.Subject = subject;
            //String sBody = System.IO.File.ReadAllText("./TemplatePrint/templateEmail.html");
            //sBody = String.Format(sBody, subject, message);
            //email_send.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = sBody };

            email_send.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = message };

            // send email
            using var smtp = new SmtpClient();
            smtp.Connect(host, 25, SecureSocketOptions.StartTlsWhenAvailable);
            smtp.Authenticate(user, password);
            smtp.Send(email_send);
            smtp.Disconnect(true);
            return Task.CompletedTask;
        }
    }
}
