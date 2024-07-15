using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace USSDMiddleware.Core.Entities;

public class ValidationLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; }
    public string ValidationReference { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public string? OtherNames { get; set; }
    public string Bvn { get; set; }
    public string Dob { get; set; }
    public string PhoneNumber { get; set; }
    public bool Valid { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;

    public ValidationLog()
    {
        Id = Guid.NewGuid().ToString();
    }
}