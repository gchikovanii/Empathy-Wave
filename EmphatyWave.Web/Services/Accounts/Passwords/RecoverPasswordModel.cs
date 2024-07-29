namespace EmphatyWave.Web.Services.Accounts.Passwords
{
    public class RecoverPasswordModel
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public string ResetPasswordToken { get; set; }
    }
}
