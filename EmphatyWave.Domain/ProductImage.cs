using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmphatyWave.Domain
{
    public class ProductImage
    {
        public Guid Id { get; set; }
        public string PublicId { get; set; }
        public string Url { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; }

    }
}
