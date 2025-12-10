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
        public required List<BraintreeShippingOption> ShippingOptions { get; set; }
    }

    public record BraintreeRequest : BraintreeBaseResponse
    {
        public required ShippingAddress ShippingAddress { get; set; }
        public BraintreeShippingOption? ShippingOption { get; set; }

    }

    public record BraintreeBaseResponse
    {
        public required string Id { get; set; }
        public required Amount Amount { get; set; }
        public decimal ItemTotal { get; set; }
        public decimal Shipping { get; set; }
        public decimal Handling { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal Insurance { get; set; }
        public decimal ShippingDiscount { get; set; }
        public decimal Discount { get; set; }
        public List<BraintreeLineItem>? LineItems { get; set; }
    }

    public record BraintreeLineItem
    {
        public required string Name { get; set; }
        public required int Quantity { get; set; }
        public required Amount UnitAmount { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
        public string? ProductCode { get; set; }
        public Amount? UnitTaxAmount { get; set; }
        public string? Url { get; set; }
        public string? ImageUrl { get; set; }
    }
}
