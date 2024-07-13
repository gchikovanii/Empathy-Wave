using Microsoft.AspNetCore.Identity;

namespace EmphatyWave.Domain
{
    public class User : IdentityUser
    {
        public ICollection<Order> Orders { get; set; }
    }
}
