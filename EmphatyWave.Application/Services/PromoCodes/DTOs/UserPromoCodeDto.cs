using EmphatyWave.Domain;

namespace EmphatyWave.Application.Services.PromoCodes.DTOs
{
    public class UserPromoCodeDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public Guid PromoCodeId { get; set; }
        public DateTime? RedeemedAt { get; set; }
    }
}
