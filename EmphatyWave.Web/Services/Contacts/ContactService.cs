namespace EmphatyWave.Web.Services.Contacts
{
    public class ContactService(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task SendToEmail(ContactForm contactForm)
        {
            await _httpClient.PostAsJsonAsync("https://localhost:7481/api/Contact", contactForm);
        }
    }
}
