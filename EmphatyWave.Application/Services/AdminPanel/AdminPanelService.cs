using EmphatyWave.Domain;
using Microsoft.AspNetCore.Identity;

namespace EmphatyWave.Application.Services.AdminPanel
{
    public class AdminPanelService(UserManager<User> userManager) : IAdminPanelService
    {
        private readonly UserManager<User> _userManager = userManager;

        public async Task<User> GetUserByEmail(string email)
        {
            return await _userManager.FindByEmailAsync(email).ConfigureAwait(false) ?? new User { };
        }

    }
}
