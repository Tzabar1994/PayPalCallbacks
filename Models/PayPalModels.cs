using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShippingCallbacks.Models
{
    public record PayPalShippingOption
    {
        public required string Id { get; set; }
        public required Amount Amount { get; set; }
        public required string Type { get; set; }
        public required string Label { get; set; }
        public bool? Selected { get; set; }

    }

    public record PayPalAmount
    {
        public required decimal Value { get; set; }
        public required string Currency_code { get; set; }
        public PayPalPurchaseUnitBreakdown? Breakdown { get; set; }
    }

    public record PayPalPurchaseUnit
    {
        public required string ReferenceId { get; set; }
        public required PayPalAmount Amount { get; set; }

    }

    public record PayPalPurchaseUnitBreakdown
    {
        public required Amount ItemTotal { get; set; }
        public required Amount TaxTotal { get; set; }
        public required Amount Shipping { get; set; }
    }

    public record PayPalRequest
    {
        public required string Id { get; set; }
        public ShippingAddress? ShippingAddress { get; set; }
        public PayPalShippingOption? ShippingOption { get; set; }
        public required List<PayPalPurchaseUnit> PurchaseUnits { get; set; }

    }

    public record PayPalResponse
    {
        public required string Id { get; set; }
        public required List<PayPalPurchaseUnit> PurchaseUnits { get; set; }
        public required List<PayPalShippingOption> ShippingOptions { get; set; }
    }

   
}
