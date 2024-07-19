using Microsoft.AspNetCore.Identity;

namespace EmphatyWave.Domain
{
    public class User : IdentityUser
    {
        public string? VerificationToken { get; set; }
        public DateTimeOffset? VerificationTokenExp { get; set; }
        public DateTimeOffset? VerifiedAt { get; set; }
        public string? ResetPasswordToken { get; set; }
        public DateTimeOffset? ResetTokenExp { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<UserPromoCode> UserPromoCodes { get; set; }

    }
}
