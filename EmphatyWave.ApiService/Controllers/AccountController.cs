using EmphatyWave.ApiService.Infrastructure.Extensions;
using EmphatyWave.Application.Services.Account;
using EmphatyWave.Application.Services.Account.DTOs;
using EmphatyWave.Application.Services.PromoCodes.Abstraction;
using EmphatyWave.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EmphatyWave.ApiService.Controllers
{
    public class AccountController(IAccountService accountService, IPromoCodeService promoCodeService,
        UserManager<User> userManager) : BaseController
    {
        private readonly IAccountService _accountService = accountService;
        private readonly IPromoCodeService _promoCodeService = promoCodeService;
        private readonly UserManager<User> _userManager = userManager;


        [HttpGet("GetUsersPromoCodes")]
        public async Task<IActionResult> GetUsersPromoCodes(CancellationToken token)
        {
            var user = await _userManager.FindByEmailFromClaimsPrincipal(User);
            return Ok(await _promoCodeService.GetAvaliablePromoCodes(token,user.Id));
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            return Ok(await _accountService.Login(dto).ConfigureAwait(false));
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            return Ok(await _accountService.Register(dto).ConfigureAwait(false));
        }
        [HttpPost("confirmEmail")]
        public async Task<IActionResult> ConfirmEmail(CancellationToken token, string email)
        {
            return Ok(await _accountService.ConfirmEmail(token,email).ConfigureAwait(false));
        }
        [HttpPost("requestPasswordRecovery")]
        public async Task<IActionResult> RequestPasswordRecovery(string email)
        {
            return Ok(await _accountService.RequestPasswordRecovery(email).ConfigureAwait(false));
        }
        [HttpPost("recoverPassword")]
        public async Task<IActionResult> RecoverPassword(RecoveryDto dto)
        {
            return Ok(await _accountService.ResetPassword(dto).ConfigureAwait(false));
        }
    }
}
