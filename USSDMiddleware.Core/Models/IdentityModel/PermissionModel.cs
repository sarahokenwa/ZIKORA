using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USSDMiddleware.Core.Models.IdentityModel
{
    public class PermissionModel
    {
        [Range(int.MinValue, int.MaxValue)]
        public string Id { get; set; }

        [StringLength(50)]
        public string PermissionName { get; set; }
        [StringLength(150)]
        public string PermissionDescription { get; set; }

        [StringLength(50)]
        public string ClientId { get; set; }


        [StringLength(50)]
        public string Type { get; set; }
    }
}
