using EmphatyWave.ApiService.Infrastructure.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

namespace EmphatyWave.ApiService.Controllers
{
    public class ContactController : BaseController
    {
        private readonly GoogleAppSettings _googleAppSettings;
        public ContactController(IOptions<GoogleAppSettings> googleAppSettings)
        {
            _googleAppSettings = googleAppSettings.Value;
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ContactForm formData)
        {
            try
            {
                string googleAppPassword = _googleAppSettings.GoogleAppPassword;
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(formData.Email);
                mailMessage.To.Add("empathywave.shop@gmail.com");
                mailMessage.Subject = formData.Subject;
                mailMessage.Body = $"Name: {formData.FullName}\nEmail: {formData.Email}\nPhone Number: {formData.Phone}\nSubject: {formData.Subject}\nMessage: {formData.Message}";
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");
                smtpClient.Port = 587;
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential("empathywave.shop@gmail.com", googleAppPassword);
                smtpClient.Send(mailMessage);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
