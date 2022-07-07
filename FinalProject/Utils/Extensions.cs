using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Threading.Tasks;

namespace FinalProject.Utils
{
    public static class Extensions
    {
        

        public async static Task<MailMessage> SendMail(string fromUser,
            string toUser, 
            string link, 
            string subject, 
            string text)
        {
            MailAddress from = new MailAddress(fromUser);
            MailAddress to = new MailAddress(toUser);
            MailMessage message = new MailMessage(from, to);
            message.Subject = subject;
            message.Body = $"<a href={link}>{text}</a>";
            message.IsBodyHtml = true;
            message.BodyEncoding = System.Text.Encoding.UTF8;
            message.SubjectEncoding = System.Text.Encoding.UTF8;


            return (message);
        }
    }
}
