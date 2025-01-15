using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Siasystem.Models
{
    public class DeliveryHistory
    {
        public int HistoryID { get; set; }
        public string ProductImage { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string Supplier { get; set; }
        public DateTime? DateDelivered { get; set; }
    }
}