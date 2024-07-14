using EmphatyWave.Application.Services.Account.DTOs;


namespace EmphatyWave.Application.Services.Account
{
    public class AccountService : IAccountService
    {
        public Task<bool> ConfirmEmail(string token)
        {
            throw new NotImplementedException();
        }

        public Task<string> Login(LoginDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RecoverPassword(RecoveryDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<string> Register(RegisterDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RequestPasswordRecovery(string email)
        {
            throw new NotImplementedException();
        }
    }
}
