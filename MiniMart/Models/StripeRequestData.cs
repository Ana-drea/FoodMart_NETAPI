namespace MiniMart.Models
{
    public class StripeRequestData
    {
        public long Amount { get; set; }  // Amount should be in the smallest currency unit (e.g., cents)
    }
}
