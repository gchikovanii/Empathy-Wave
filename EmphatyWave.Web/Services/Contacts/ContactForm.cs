using System.ComponentModel.DataAnnotations;

namespace EmphatyWave.Web.Services.Contacts
{
    public record ContactForm
    {
        [Required]
        public string FullName { get; set; }
        [Required]

        public string Phone { get; set; }
        [Required]

        public string Email { get; set; }
        [Required]

        public string Subject { get; set; }
        [Required]

        public string Message { get; set; }


    }
}
