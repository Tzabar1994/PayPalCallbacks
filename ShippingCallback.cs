using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Identity.Client;
using System;
using System.Security.AccessControl;

namespace tzabar.braintree;

public class ShippingCallback
{
    private readonly ILogger<ShippingCallback> _logger;

    public ShippingCallback(ILogger<ShippingCallback> logger)
    {
        _logger = logger;
    }

    [Function("ShippingCallback")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        req.EnableBuffering();
        req.Body.Position = 0;

        var rawRequestBody = await new StreamReader(req.Body).ReadToEndAsync();

        _logger.LogInformation($"Request Body: {rawRequestBody}");

        try
        {
            var blob = JsonSerializer.Deserialize<BraintreeRequest>(rawRequestBody, options)
                ?? throw new JsonException("Could not deserialise input correctly!");

            _logger.LogInformation($"Json De-serialized: {blob}");

            var BTResponse = new BraintreeResponse
            {
                Id = blob.Id,
                Amount = blob.Amount,
                Item_total = 0,
                Shipping = 0,
                Handling = 0,
                Tax_total = 0,
                Insurance = 0,
                Shipping_discount = 0,
                Discount = 0,
                Shipping_options = []
            };

            if (blob.Shipping_option == null)
            {
                //If no selected shipping option, then this is a new address?
                //  Update the response with the generic shipping options
                _logger.LogInformation("No option provided, generate default options");

                if (blob.Item_total == 0)
                {
                    BTResponse.Item_total = blob.Amount.Value;
                }
                else
                {
                    BTResponse.Amount.Value = blob.Item_total;
                }

                BTResponse.Shipping_options = generateOptions(0);
            }
            else
            {
                //We're responding to a newly selected shipping option?
                _logger.LogInformation("Updating Total price for selected shipping option!");
                var shipping = blob?.Shipping_option.Amount.Value;
                var total = blob?.Item_total + shipping ?? 0;
                var selectedOption = Int32.Parse(blob.Shipping_option.Id) - 1;

                var amount = new Amount
                {
                    Currency_code = blob.Amount.Currency_code,
                    Value = total
                };

                BTResponse.Amount = amount;
                BTResponse.Item_total = blob.Item_total;
                BTResponse.Shipping = shipping ?? 0;
                BTResponse.Shipping_options = generateOptions(selectedOption);
            }

            _logger.LogInformation($"Returning Result: {BTResponse}");
            return new SystemTextJsonResult(BTResponse, options);

        }
        catch (JsonException)
        {
            return new BadRequestObjectResult("Could not deserialize json correctly"); // 400
        }
    }

    public List<Shipping_option> generateOptions(int selected)
    {
        var free = new Amount { Value = 0, Currency_code = "GBP" };
        var medium = new Amount { Value = 5, Currency_code = "GBP" };
        var expensive = new Amount { Value = 10, Currency_code = "GBP" };

        var ship_options = new List<Shipping_option> {
                    new Shipping_option {
                        Id = "1",
                        Amount = free,
                        Type = "SHIPPING",
                        Description = "Free Shipping",
                        Selected = false
                    },
                    new Shipping_option {
                        Id = "2",
                        Amount = medium,
                        Type = "SHIPPING",
                        Description = "Medium Shipping",
                        Selected = false
                    },
                    new Shipping_option {
                        Id = "3",
                        Amount = expensive,
                        Type = "SHIPPING",
                        Description = "Expensive Shipping",
                        Selected = false
                    }
                };

        ship_options[selected].Selected = true;

        return ship_options;
    }

    public class SystemTextJsonResult : ContentResult
    {
        private const string ContentTypeApplicationJson = "application/json";

        public SystemTextJsonResult(object value, JsonSerializerOptions options)
        {
            ContentType = ContentTypeApplicationJson;
            Content = JsonSerializer.Serialize(value, options);
        }
    }

    public record Amount
    {
        public required decimal Value { get; set; }
        public required string Currency_code { get; set; }
    }

    public record Shipping_address
    {
        public string? Admin_area_2 { get; set; }
        public string? Admin_area_1 { get; set; }
        public required string Postal_code { get; set; }
        public required string Country_code { get; set; }
    }

    public record Shipping_option
    {
        public required string Id { get; set; }
        public required Amount Amount { get; set; }
        public required string Type { get; set; }
        public required string Description { get; set; }
        public bool? Selected { get; set; }

    }

    public record BraintreeResponse : BraintreeBase
    {
        public required List<Shipping_option> Shipping_options { get; set; }
    }

    public record BraintreeRequest : BraintreeBase
    {
        public required Shipping_address Shipping_address { get; set; }
        public Shipping_option? Shipping_option { get; set; }

    }


    public record BraintreeBase
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