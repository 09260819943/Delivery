using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Siasystem.Models
{
    public class Delivery
    {
        public int DeliveredID { get; set; }
        public string ProductImage { get; set; }
        public string ProductName { get; set; }
        public int OrderQuantity { get; set; }
        public string Supplier { get; set; }
        public string Status { get; set; } // "Pending", "Confirmed", etc.
    }

}