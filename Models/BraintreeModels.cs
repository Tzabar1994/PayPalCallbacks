using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShippingCallbacks.Models
{
    public record BraintreeShippingOption
    {
        public required string Id { get; set; }
        public required Amount Amount { get; set; }
        public required string Type { get; set; }
        public required string Description { get; set; }
        public bool? Selected { get; set; }

    }

    public record BraintreeResponse : BraintreeBaseResponse
    {
        public required List<BraintreeShippingOption> Shipping_options { get; set; }
    }

    public record BraintreeRequest : BraintreeBaseResponse
    {
        public required ShippingAddress Shipping_address { get; set; }
        public BraintreeShippingOption? Shipping_option { get; set; }

    }

    public record BraintreeBaseResponse
    {
        public required string Id { get; set; }
        public required Amount Amount { get; set; }
        public decimal Item_total { get; set; }
        public decimal Shipping { get; set; }
        public decimal Handling { get; set; }
        public decimal Tax_total { get; set; }
        public decimal Insurance { get; set; }
        public decimal Shipping_discount { get; set; }
        public decimal Discount { get; set; }
    }
}
