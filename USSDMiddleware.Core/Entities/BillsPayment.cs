using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USSDMiddleware.Infrastructure.Entities;

namespace USSDMiddleware.Core.Entities
{
    public class BillsPayment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [StringLength(60)]
        public string CustomerId { get; set; }
        [StringLength(60)]
        public string PhoneNumber { get; set; }
        public string ProviderId { get; set; }
        [StringLength(60)]
        public string itemcode { get; set; }
        [StringLength(60)]
        public string validationref { get; set; }
        [StringLength(60)]
        public string merchantref { get; set; }
        [StringLength(60)]
        public string requeryresponsecode { get; set; }
        [StringLength(60)]
        public string responsecode { get; set; }
        [StringLength(60)]
        public string processorRef { get; set; }
        public decimal Amount { get; set; }
        public decimal Fee { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime UpdatedOn { get; set; } = DateTime.Now;

        public Provider Provider { get; set; }

        public BillsPayment()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
