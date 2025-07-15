using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace portfolioAPI.Controllers
{
    public static class EmailService
    {
        public static async Task<bool> SendEmailAsync(string to, string subject, string body, IFormFile file = null)
        {
            try
            {
                var fromAddress = new MailAddress("akashkce123@gmail.com", "Portfolio API");
                var toAddress = new MailAddress(to);
                const string fromPassword = "eqqg lfuu qvkl cqyo";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    MemoryStream ms = null;
                    try
                    {
                        if (file != null && file.Length > 0)
                        {
                            ms = new MemoryStream();
                            await file.CopyToAsync(ms);
                            ms.Position = 0;
                            message.Attachments.Add(new Attachment(ms, file.FileName, file.ContentType));
                        }
                        await smtp.SendMailAsync(message);
                    }
                    finally
                    {
                        ms?.Dispose();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email send failed: {ex?.Message}\n{ex?.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}");
                }
                System.Diagnostics.Debug.WriteLine($"Email send failed: {ex?.Message}\n{ex?.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}");
                }
                throw;
            }
        }
    }
} 