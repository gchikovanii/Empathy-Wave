using System.ComponentModel.DataAnnotations.Schema;

namespace EmphatyWave.Domain
{
    [ComplexType]
    public class ShippingDetail
    {
        public string Address { get; set; }
        public string ZipCode { get; set; }
        public string CountryCode { get; set; }
        public string PhoneNumber { get; set; }
    }
}
