using EmphatyWave.Application.Services.Account.DTOs;
using EmphatyWave.Application.Services.Account.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmphatyWave.Application.Services.Account
{
    public interface IAccountService
    {
        Task<ResultOrValue<string>> Register(RegisterDto dto);
        Task<bool> ConfirmEmail(CancellationToken cancellationToken, string token);
        Task<ResultOrValue<string>> Login(LoginDto dto);
        Task<ResultOrValue<string>> RequestPasswordRecovery(string email);
        Task<ResultOrValue<string>> ResetPassword(RecoveryDto dto);
        Task RemoveExpiredTokensAsync(CancellationToken token, string option);
    }
}
