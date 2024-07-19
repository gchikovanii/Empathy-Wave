namespace EmphatyWave.Application.Services.PromoCodes.DTOs
{
    public class PromoCodeDto
    {
        public Guid Id { get; set; }
        public DateTimeOffset ExpirationDate { get; set; }
        public decimal DiscountPercentage { get; set; }
        public bool IsActive { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
