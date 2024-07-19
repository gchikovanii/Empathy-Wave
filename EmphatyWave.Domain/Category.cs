using EmphatyWave.Domain.Localization;
using System.ComponentModel.DataAnnotations;

namespace EmphatyWave.Domain
{
    public class Category
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = nameof(ErrorMessages.FieldIsRequired))]
        [MaxLength(15,ErrorMessage = nameof(ErrorMessages.MaxLengthIs) + "15")]
        public string Name { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
