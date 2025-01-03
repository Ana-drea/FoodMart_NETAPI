document.addEventListener("DOMContentLoaded", async () => {
  // Fetch the publishable key from the server
  const { publishableKey } = await fetch("https://localhost:7221/config", {
    method: "GET",
    credentials: "include", // Include user credentials
    headers: {
      "Content-Type": "application/json",
    },
  }).then((r) => r.json());
  const stripe = Stripe(publishableKey);
  // Retrieve the payment intent and render
  const params = new URLSearchParams(window.location.href);
  const clientSecret = params.get("payment_intent_client_secret");
  const { paymentIntent } = await stripe.retrievePaymentIntent(clientSecret);

  const paymentIntentPre = document.getElementById("payment-intent");
  paymentIntentPre.innerText = JSON.stringify(paymentIntent, null, 2);
});
