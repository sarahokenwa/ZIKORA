using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USSDMiddleware.Core.Models.Bills
{
    public class BillersResponse
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public Biller[] Data { get; set; }
    }

    public class Biller
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CyberPayBillerCategoryId { get; set; }
        public string BillerCode { get; set; }
        public string CustomerIdHint { get; set; }
        public string LogoUrl { get; set; }
    }

}
