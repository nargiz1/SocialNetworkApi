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
        public static bool IsImage(this IFormFile file)
        {
            return file.ContentType.Contains("image/");
        }
        public static bool IsVideo(this IFormFile file)
        {
            return file.ContentType.Contains("video/");
        }
        public static bool IsvalidSize(this IFormFile file, int kb)
        {
            return file.Length / 1024 < kb;
        }
        public async static Task<string> Upload(this IFormFile image, string folder)
        {
                var file = image;
                var folderName = Path.Combine("Resources", folder);
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                if (file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(folderName, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    return fileName;
                }
            return "invalid";
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
