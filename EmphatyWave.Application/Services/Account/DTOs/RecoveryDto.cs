namespace EmphatyWave.Application.Services.Account.DTOs
{
    public class RecoveryDto
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public string ResetPasswordToken { get; set; }

    }
}
