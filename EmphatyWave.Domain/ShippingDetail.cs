using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmphatyWave.Domain
{
    [ComplexType]
    public class ShippingDetail
    {
        [Required(ErrorMessage = "Address is required!")]
        [MinLength(5, ErrorMessage = "Address must be at least 5 characters")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Zip Code is required!")]
        [MinLength(3, ErrorMessage = "Zip Code must be at least 3 characters")]
        public string ZipCode { get; set; }
        [Required(ErrorMessage = "Country Code required!")]
        public string CountryCode { get; set; }
        [Required(ErrorMessage = "Phone number is required!")]
        [MaxLength(20,ErrorMessage = "Max Length of phone number must be 20")]
        public string PhoneNumber { get; set; }
    }
}
