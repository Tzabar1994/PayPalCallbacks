using System.Text.Json;
using System.Text.Json.Serialization;
using BTCallback.Models;
using BTCallback.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace tzabar.braintree;

public class BraintreeShippingCallback
{
    private readonly ILogger<BraintreeShippingCallback> _logger;

    public BraintreeShippingCallback(ILogger<BraintreeShippingCallback> logger)
    {
        _logger = logger;
    }

    [Function("ShippingCallback")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        _logger.LogInformation("Braintree Shipping Callback triggered");

        var braintreeJsonOptions = new JsonSerializerOptions
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
            var blob = JsonSerializer.Deserialize<BraintreeRequest>(rawRequestBody, braintreeJsonOptions)
                ?? throw new JsonException("Could not deserialise input correctly!");

            _logger.LogInformation($"Json De-serialized: {blob}");

            var BTResponse = new BraintreeResponse
            {
                Id = blob.Id,
                Amount = blob.Amount,
                ItemTotal = 0,
                Shipping = 0,
                Handling = 0,
                TaxTotal = 0,
                Insurance = 0,
                ShippingDiscount = 0,
                Discount = 0,
                ShippingOptions = []
            };

            if (blob.ShippingOption == null)
            {
                //If no selected shipping option, then this is a new address?
                //  Update the response with the generic shipping options
                _logger.LogInformation("No shipping options found, generating defaults");

                _logger.LogInformation($"Check if Postal Code is valid (must not contain 'X'): {blob.ShippingAddress.PostalCode}");

                if (blob.ShippingAddress.PostalCode.ToUpper().Contains("X"))
                {
                    _logger.LogInformation("Invalid Postal Code found, rejecting address");

                    var errorCode = new PayPalError { Issue = PayPalErrorCode.ZIP_ERROR };
                    var error = new PayPalErrorResponse
                    {
                        Name = "UNPROCESSABLE_ENTITY",
                        Details = new List<PayPalError> { errorCode }
                    };
                    return new SystemTextJsonResult(error, braintreeJsonOptions, 422);
                }

                //Deal with weirdness in the BT request?
                if (blob.ItemTotal == 0)
                {
                    BTResponse.ItemTotal = blob.Amount.Value;
                }
                else
                {
                    BTResponse.Amount.Value = blob.ItemTotal;
                }

                BTResponse.ShippingOptions = generateOptions();
            }
            else
            {
                //We're responding to a newly selected shipping option?
                _logger.LogInformation("Updating Total price for selected shipping option!");
                var shipping = blob.ShippingOption.Amount.Value;
                var selectedOption = Int32.Parse(blob.ShippingOption.Id) - 1;

                var amount = new Amount
                {
                    CurrencyCode = blob.Amount.CurrencyCode,
                    Value = blob.ItemTotal + shipping
                };

                BTResponse.Amount = amount;
                BTResponse.ItemTotal = blob.ItemTotal;
                BTResponse.Shipping = shipping;
                BTResponse.ShippingOptions = generateOptions(selectedOption);
            }

            _logger.LogInformation($"Returning Result: {BTResponse}");
            return new SystemTextJsonResult(BTResponse, braintreeJsonOptions);

        }
        catch (JsonException)
        {
            _logger.LogError("Failed to de-serialize request from Braintree correctly!");
            _logger.LogError($"Request Body: {rawRequestBody}");
            return new BadRequestObjectResult("Could not deserialize json correctly"); // 400
        }
    }

    public List<BraintreeShippingOption> generateOptions(int selected = 0)
    {
        var free = new Amount { Value = 0, CurrencyCode = "GBP" };
        var medium = new Amount { Value = 5, CurrencyCode = "GBP" };
        var expensive = new Amount { Value = 10, CurrencyCode = "GBP" };

        var shipOptions = new List<BraintreeShippingOption> {
                    new BraintreeShippingOption {
                        Id = "1",
                        Amount = free,
                        Type = "SHIPPING",
                        Description = "Free Shipping",
                        Selected = false
                    },
                    new BraintreeShippingOption {
                        Id = "2",
                        Amount = medium,
                        Type = "SHIPPING",
                        Description = "Medium Shipping",
                        Selected = false
                    },
                    new BraintreeShippingOption {
                        Id = "3",
                        Amount = expensive,
                        Type = "SHIPPING",
                        Description = "Expensive Shipping",
                        Selected = false
                    }
                };

        shipOptions[selected].Selected = true;

        return shipOptions;
    }

    
}
