using System.ComponentModel.DataAnnotations;

namespace EmphatyWave.Web.Services.Accounts.Logins
{
    public class LoginModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
