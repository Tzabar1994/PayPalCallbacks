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
        public required string Currency_code { get; set; }
    }

    public record ShippingAddress
    {
        public string? Admin_area_2 { get; set; }
        public string? Admin_area_1 { get; set; }
        public required string Postal_code { get; set; }
        public required string Country_code { get; set; }
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
