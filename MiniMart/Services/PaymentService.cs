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
            // 设置 StripeConfiguration 的 ApiKey
            StripeConfiguration.ApiKey = _stripeOptions.SecretKey;
        }

        public async Task<string> CreatePaymentIntentAsync(long amount, string currency = "cad")
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
                }
            };

            var paymentIntent = await _paymentIntentService.CreateAsync(options);
            return paymentIntent.ClientSecret;
        }

        public string GetPublishableKey()
        {
            return _stripeOptions.PublishableKey;
        }
    }
}
