using EmphatyWave.ApiService.Infrastructure.Extensions;
using EmphatyWave.Application.Services.Account;
using EmphatyWave.Application.Services.Account.DTOs;
using EmphatyWave.Application.Services.PromoCodes.Abstraction;
using EmphatyWave.Domain;
using Humanizer;
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
            var result = await _accountService.Login(dto).ConfigureAwait(false);
            if(result.IsSuccess == false)
                return BadRequest(result);
            return Ok(result);
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _accountService.Register(dto).ConfigureAwait(false);
            if (result.IsSuccess == false)
                return BadRequest(result);
            return Ok(result);
        }
        [HttpPost("confirmEmail")]
        public async Task<IActionResult> ConfirmEmail(CancellationToken cancellationToken, string token)
        {
            var result = await _accountService.ConfirmEmail(cancellationToken, token).ConfigureAwait(false);
            if (result.IsSuccess == false)
                return BadRequest(result);
            return Ok(result);
        }
        [HttpPost("requestPasswordRecovery")]
        public async Task<IActionResult> RequestPasswordRecovery(string email)
        {
            var result = await _accountService.RequestPasswordRecovery(email).ConfigureAwait(false);
            if (result.IsSuccess == false)
                return BadRequest(result);
            return Ok(result);
        }
        [HttpPost("recoverPassword")]
        public async Task<IActionResult> RecoverPassword(RecoveryDto dto)
        {
            var result = await _accountService.ResetPassword(dto).ConfigureAwait(false);
            if (result.IsSuccess == false)
                return BadRequest(result);
            return Ok(result);
        }
    }
}
