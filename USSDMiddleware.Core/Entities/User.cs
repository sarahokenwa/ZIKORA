using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime UpdatedOn { get; set; } = DateTime.Now;

        public ICollection<Account> Accounts { get; set; }

        public User()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
