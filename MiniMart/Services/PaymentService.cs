using Microsoft.Extensions.Options;
using Stripe;

namespace MiniMart.Services
{
    public class PaymentService
    {
        private readonly PaymentIntentService _paymentIntentService;
        private readonly StripeOptions _stripeOptions;


        public PaymentService(PaymentIntentService paymentIntentService, IOptions<StripeOptions> stripeOptions)
        {
            _paymentIntentService = paymentIntentService;
            _stripeOptions = stripeOptions.Value;
            // Set ApiKey FOR StripeConfiguration 
            StripeConfiguration.ApiKey = _stripeOptions.SecretKey;
        }

        public async Task<PaymentIntent> CreatePaymentIntentAsync(long amount, string currency = "cad", string orderId = null)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than zero.");
            }

            var options = new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = currency,
                AutomaticPaymentMethods = new()
                {
                    Enabled = true
                },
                Metadata = new Dictionary<string, string>
                {
                    { "orderId", orderId } // Save orderHistory.Id as metadata
                }
            };

            // Create and return the PaymentIntent object
            return await _paymentIntentService.CreateAsync(options);
        }

        public string GetPublishableKey()
        {
            return _stripeOptions.PublishableKey;
        }
    }
}
