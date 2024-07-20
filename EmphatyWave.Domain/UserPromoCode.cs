namespace EmphatyWave.Domain
{
    public class UserPromoCode
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public Guid PromoCodeId { get; set; }
        public PromoCode PromoCode { get; set; }
        public DateTime? RedeemedAt { get; set; }
    }
}
