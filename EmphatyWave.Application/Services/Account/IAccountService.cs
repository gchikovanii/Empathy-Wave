using EmphatyWave.Application.Services.Account.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmphatyWave.Application.Services.Account
{
    public interface IAccountService
    {
        Task<string> Register(RegisterDto dto);
        Task<bool> ConfirmEmail(CancellationToken cancellationToken, string token);
        Task<string> Login(LoginDto dto);
        Task<bool> RequestPasswordRecovery(string email);
        Task<bool> ResetPassword(RecoveryDto dto);

    }
}
