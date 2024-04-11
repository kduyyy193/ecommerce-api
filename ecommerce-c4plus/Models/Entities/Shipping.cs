using System;
namespace ecommerce_c4plus.Models
{

    public class ShippingDTO
    {
        public int ShippingId { get; set; }
        public int OrderId { get; set; }
        public string ShippingMethod { get; set; }
        public string TrackingNumber { get; set; }
        public DateTime ShippingDate { get; set; }
    }
}

