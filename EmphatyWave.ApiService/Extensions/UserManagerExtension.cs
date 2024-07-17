using EmphatyWave.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EmphatyWave.ApiService.Extensions
{
    public static class UserManagerExtension
    {
        public static async Task<User> FindByEmailFromClaimsPrincipal(this UserManager<User> userManager, ClaimsPrincipal user)
        {
            return await userManager.Users.SingleOrDefaultAsync(i => i.Email == user.FindFirstValue(ClaimTypes.Email)) ?? new User { };
        }
    }
}
