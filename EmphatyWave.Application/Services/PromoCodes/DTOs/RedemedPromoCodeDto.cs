namespace EmphatyWave.Application.Services.PromoCodes.DTOs
{
    public class RedemedPromoCodeDto
    {
        public DateTimeOffset RedeemedAt { get; set; }
        public bool Redeemed { get; set; }
    }
}
