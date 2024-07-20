using EmphatyWave.Application.Services.AdminPanel;
using EmphatyWave.Application.Services.PromoCodes.Abstraction;
using EmphatyWave.Application.Services.PromoCodes.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmphatyWave.ApiService.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminPanelController(IAdminPanelService adminPanelService, IPromoCodeService promoCodeSerivice
        ) : BaseController
    {
        private readonly IAdminPanelService _adminPanelService = adminPanelService;
        private readonly IPromoCodeService _promoCodeSerivice = promoCodeSerivice;

        
        [HttpGet("GetByEmail")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            return Ok(await _adminPanelService.GetUserByEmail(email).ConfigureAwait(false));
        }

        [HttpGet("CheckAllPromoCodesForUser")]
        public async Task<IActionResult> CheckAllPromoCodesForUser(CancellationToken token,string email)
        {
            var user = await _adminPanelService.GetUserByEmail(email).ConfigureAwait(false);
            if (user == null)
                return NotFound();
            return Ok(await _promoCodeSerivice.GetCurrentUserPromoCodes(token,user.Id));
        }

        [HttpPost("IssuePromoCode")]
        public async Task<IActionResult> IssuePromoCode(CancellationToken token, PromoCodeDto promoCodeDto)
        {
            return Ok(await _promoCodeSerivice.IssuePromoCode(token,promoCodeDto));
        }

        [HttpPost("ApplyPromoCodeToUsers")]
        public async Task<IActionResult> ApplyPromoCodeToUsers(CancellationToken token, Guid promoCodeId,DateTimeOffset? dateTime)
        {
            return Ok(await _promoCodeSerivice.ApplyPromoCodes(token, promoCodeId,dateTime));
        }
        [HttpPost("ApplyToConcreteUser")]
        public async Task<IActionResult> ApplyToConcreteUser(CancellationToken token,ApplyPromoCodeDto promoCodeDto)
        {
            return Ok(await _promoCodeSerivice.ApplyPromoCode(token, promoCodeDto));
        }
        [HttpDelete("DeletePromoCode")]

        public async Task<IActionResult> DeletePromoCode(CancellationToken token, string promoCode)
        {
            return Ok(await _promoCodeSerivice.DeletePromoCode(token, promoCode));
        }
    }
}
