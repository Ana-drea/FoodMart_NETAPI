using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using MiniMart.Data; // Update with your actual namespace
using MiniMart.Models; 

namespace MiniMart.Services
{
    public class StripeWebhookService
    {
        private readonly AppDbContext _context;
        private readonly StripeOptions _stripeOptions;
        private readonly ILogger<StripeWebhookService> _logger;

        public StripeWebhookService(AppDbContext context, IOptions<StripeOptions> options, ILogger<StripeWebhookService> logger)
        {
            _context = context;
            _stripeOptions = options.Value;
            _logger = logger;
        }

        public async Task<IResult> HandleWebhookAsync(HttpRequest req)
        {
            var json = await new StreamReader(req.Body).ReadToEndAsync();
            Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    req.Headers["Stripe-Signature"],
                    _stripeOptions.WebhookSecret,
                    throwOnApiVersionMismatch: false
                );
                _logger.LogInformation($"Webhook notification with type: {stripeEvent.Type} found for {stripeEvent.Id}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Something failed => {e.Message}");
                return Results.BadRequest();
            }

            if (stripeEvent.Type == Events.PaymentIntentSucceeded)
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                if (paymentIntent != null && paymentIntent.Metadata.TryGetValue("orderId", out var orderId))
                {
                    _logger.LogInformation($"PaymentIntent ID: {paymentIntent.Id} succeeded for Order ID: {orderId}");

                    if (int.TryParse(orderId, out int orderIdInt))
                    {
                        var orderHistory = await _context.OrderHistories.FindAsync(orderIdInt);
                        if (orderHistory != null && paymentIntent.Id == orderHistory.StripePI)
                        {
                            orderHistory.IsCompleted = true;
                            await _context.SaveChangesAsync();
                            _logger.LogInformation($"Order {orderId} marked as completed.");
                        }
                        else
                        {
                            return Results.BadRequest("Payment intent ID doesn't match.");
                        }
                    }
                    else
                    {
                        return Results.BadRequest("Invalid order ID format.");
                    }
                }
                else
                {
                    _logger.LogWarning("Payment intent or orderId not found.");
                }
            }

            return Results.Ok();
        }
    }
}
