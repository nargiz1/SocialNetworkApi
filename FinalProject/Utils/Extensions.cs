using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace bauen.Utils
{
    public static class Extensions
    {
        public static bool IsImage(this IFormFile file)
        {
            return file.ContentType.Contains("image/");
        }
        public static bool IsvalidSize(this IFormFile file, int kb)
        {
            return file.Length / 1024 < kb;
        }
        public async static Task<string> Upload(this IFormFile file, string folder)
        {
            string fileName = Guid.NewGuid().ToString()+"-" + file.FileName;
            string finalPath = Path.Combine( folder, fileName);
            FileStream fileStream = new FileStream(finalPath, FileMode.Create);
            await file.CopyToAsync(fileStream);
            return finalPath;
        }

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
