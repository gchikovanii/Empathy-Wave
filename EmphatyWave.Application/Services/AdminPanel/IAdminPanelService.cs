using EmphatyWave.Domain;

namespace EmphatyWave.Application.Services.AdminPanel
{
    public interface IAdminPanelService
    {
        Task<User> GetUserByEmail(string email);
    }
}
