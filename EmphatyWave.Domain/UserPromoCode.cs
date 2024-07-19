namespace EmphatyWave.Domain
{
    public class UserPromoCode
    {
        public string UserId { get; set; }
        public User User { get; set; }
        public Guid PromoCodeId { get; set; }
        public PromoCode PromoCode { get; set; }
        public DateTime? RedeemedAt { get; set; }
    }
}
