using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USSDMiddleware.Infrastructure.Entities;

namespace USSDMiddleware.Core.Entities
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string PhoneNumber { get; set; }
        public string ProviderId { get; set; }
        public string CustomerName { get; set; }
        public string? Address { get; set; }
        public string Email { get; set; }
        public string BankVerificationNumber { get; set; }
        public string DateOfBirth { get; set; }
        public string? Salt { get; set; }
        public string TransactionPin { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime UpdatedOn { get; set; } = DateTime.Now;
        public Provider Provider { get; set; }

        public User()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
