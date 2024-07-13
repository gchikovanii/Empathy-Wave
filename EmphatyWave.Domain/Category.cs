using System.ComponentModel.DataAnnotations;

namespace EmphatyWave.Domain
{
    public class Category
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Name is Neccessery!")]
        [MaxLength(15,ErrorMessage = "Max Length is 15")]
        public string Name { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
