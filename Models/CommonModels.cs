using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShippingCallbacks.Models
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

    public enum AddressErrorCode
    {
        ADDRESS_ERROR,
        COUNTRY_ERROR,
        STATE_ERROR,
        ZIP_ERROR,
        METHOD_UNAVAILABLE,
        STORE_UNAVAILABLE
    }

    public record CallbackError
    {
        public AddressErrorCode Issue { get; set; }
    }

    public record CallbackErrorResponse
    {
        public required string Name { get; set; }
        public required List<CallbackError> Details { get; set; }
    }


}
