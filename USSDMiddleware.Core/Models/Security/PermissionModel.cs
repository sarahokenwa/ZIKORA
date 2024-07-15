using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USSDMiddleware.Core.Models.Security
{
    public class PermissionModel
    {
        [Range(int.MinValue, int.MaxValue)]
        public string Id { get; set; }

        [MaxLength(50)]
        public string PermissionName { get; set; }
        [MaxLength(150)]
        public string PermissionDescription { get; set; }

        [MaxLength(50)]
        public string ClientId { get; set; }


        [MaxLength(50)]
        public string Type { get; set; }
    }
}
