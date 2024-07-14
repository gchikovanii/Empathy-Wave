using EmphatyWave.Application.Services.AdminPanel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmphatyWave.ApiService.Controllers
{
    public class AdminPanelController(IAdminPanelService adminPanelService) : BaseController
    {
        private readonly IAdminPanelService _adminPanelService = adminPanelService;

        [Authorize(Roles = "Admin")]
        [HttpGet("GetByEmail")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            return Ok(await _adminPanelService.GetUserByEmail(email).ConfigureAwait(false));
        }
    }
}
