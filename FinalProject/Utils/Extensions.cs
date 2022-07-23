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
            string linkText,
            string text)
        {
            string htmlBody = @"
            <html lang=""en"">
            <head>
                <meta charset=""UTF-8"">
                <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>Document</title>
                <style>
                    .main{
                        padding: 20px;
                        text-align: center;
                    }
                    .img-wrapper{
                        display: block;
                    }
                    img{
                        width: 150px;
                    }
                    .text-wrapper{
                        display: inline-block;
                        text-align: start;
                    }
                    h2{
                        margin-bottom: 15px;
                    }
                    p{
                        margin-bottom: 20px;
                    }
                    .btn{
                        background-color: #5850EC;
                        color: white!important;
                        text-decoration: none;
                        padding: 10px 20px;
                        border-radius: 8px;
                    }
                </style>
            </head>
            <body>
    
                <div class=""main"">
                    <div class=""img-wrapper"">
                        <a href=""https://imgbb.com/""><img src=""https://i.ibb.co/DtJ5chy/undraw-join-re-w1lh.png"" alt=""undraw-join-re-w1lh"" border=""0""></a>
                    </div>
                    <div class=""text-wrapper"">
                        <h2>Dear user,</h2>
                        <p>" + text + @"</p>
                        <a class=""btn"" href=" + link + @">" +linkText+ @"</a>
                    </div>
                </div>
            </body>
            </html>
            ";

            MailAddress from = new MailAddress(fromUser);
            MailAddress to = new MailAddress(toUser);
            MailMessage message = new MailMessage(from, to);
            message.Subject = subject;
            message.Body = htmlBody;
            message.IsBodyHtml = true;
            message.BodyEncoding = System.Text.Encoding.UTF8;
            message.SubjectEncoding = System.Text.Encoding.UTF8;


            return (message);
        }
    }
}
