using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTCallback.Models
{
    public record Amount
    {
        public required decimal Value { get; set; }
        public required string CurrencyCode { get; set; }
    }

    public record ShippingAddress
    {
        public string? AdminArea_2 { get; set; }
        public string? AdminArea_1 { get; set; }
        public required string PostalCode { get; set; }
        public required string CountryCode { get; set; }
    }

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
    }

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
        public required string CurrencyCode { get; set; }
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

    public enum PayPalErrorCode
    {
        ADDRESS_ERROR,
        COUNTRY_ERROR,
        STATE_ERROR,
        ZIP_ERROR,
        METHOD_UNAVAILABLE,
        STORE_UNAVAILABLE
    }

    public record PayPalError
    {
        public PayPalErrorCode Issue { get; set; }
    }

    public record PayPalErrorResponse
    {
        public required string Name { get; set; }
        public required List<PayPalError> Details { get; set; }
    }
}
