using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using System.Text.Json;
using ShippingCallbacks.Results;
using ShippingCallbacks.Models;

namespace BTCallback;

public class PayPalShippingCallback
{
    private readonly ILogger<PayPalShippingCallback> _logger;

    public PayPalShippingCallback(ILogger<PayPalShippingCallback> logger)
    {
        _logger = logger;
    }

    [Function("PayPalShippingCallback")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        _logger.LogInformation("PayPal Shipping Callback triggered");

        var payPalJsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper)
            }
        };

        req.EnableBuffering();
        req.Body.Position = 0;

        var rawRequestBody = await new StreamReader(req.Body).ReadToEndAsync();

        _logger.LogInformation($"Request Body: {rawRequestBody}");

        try
        {
            var blob = JsonSerializer.Deserialize<PayPalRequest>(rawRequestBody, payPalJsonOptions)
                ?? throw new JsonException("Could not deserialise input correctly!");

            _logger.LogInformation($"Json De-serialized: {blob}");

            var ppResponse = new PayPalResponse
            {
                Id = blob.Id,
                PurchaseUnits = blob.PurchaseUnits,
                ShippingOptions = new List<PayPalShippingOption>()
            };

            var orderCurrency = blob.PurchaseUnits[0].Amount.CurrencyCode;
            var orderTotal = blob.PurchaseUnits[0].Amount.Value;

            if (blob.ShippingOption is null)
            {
                ppResponse.ShippingOptions = GenerateOptions(0, orderCurrency ?? "");
                var breakdown = new PayPalPurchaseUnitBreakdown
                {
                    ItemTotal = new Amount { Value = orderTotal, CurrencyCode = orderCurrency ?? "" },
                    TaxTotal = new Amount { Value = 0m, CurrencyCode = orderCurrency ?? "" },
                    Shipping = new Amount { Value = 0, CurrencyCode = orderCurrency ?? "" },
                };
                
                ppResponse.PurchaseUnits[0].Amount.Breakdown = breakdown;

            }

            _logger.LogInformation($"Returning Result: {ppResponse}");

            return new SystemTextJsonResult(ppResponse, payPalJsonOptions);
            
        }
        catch (JsonException e)
        {
            _logger.LogError("Failed to de-serialize request from PayPal correctly!");
            _logger.LogError(e.ToString());
            _logger.LogError($"Request Body: {rawRequestBody}");
            return new BadRequestObjectResult("Could not deserialize json correctly"); // 400
        }
            
    }

    public List<PayPalShippingOption> GenerateOptions(int selected = 0, string currency = "GBP")
    {
        var free = new Amount { Value = 0, CurrencyCode = currency };
        var medium = new Amount { Value = 5, CurrencyCode = currency };
        var expensive = new Amount { Value = 10, CurrencyCode = currency };

        var ship_options = new List<PayPalShippingOption> {
                    new PayPalShippingOption {
                        Id = "1",
                        Amount = free,
                        Type = "SHIPPING",
                        Label = "Free Shipping",
                        Selected = false
                    },
                    new PayPalShippingOption {
                        Id = "2",
                        Amount = medium,
                        Type = "SHIPPING",
                        Label = "Medium Shipping",
                        Selected = false
                    },
                    new PayPalShippingOption {
                        Id = "3",
                        Amount = expensive,
                        Type = "SHIPPING",
                        Label = "Expensive Shipping",
                        Selected = false
                    }
                };

        ship_options[selected].Selected = true;

        return ship_options;
    }
}

