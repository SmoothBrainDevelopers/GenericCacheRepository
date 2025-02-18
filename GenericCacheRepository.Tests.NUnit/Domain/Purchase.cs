using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericCacheRepository.Tests.NUnit.Domain
{
    public class Purchase
    {
        [Key, Required]
        public int CustomerId { get; set; }

        [Key, Required]
        public int StoreId { get; set; }

        [Key, Required]
        public int ProductId { get; set; }

        [Key, Required]
        public DateTime PurchaseDate { get; set; }

        public int Quantity { get; set; }
        public bool UsedBOGODiscount { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }

}
