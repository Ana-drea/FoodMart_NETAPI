document.addEventListener("DOMContentLoaded", async () => {
  // Fetch the publishable key from the server
  const { publishableKey } = await fetch(`${window.config.apiUrl}config`, {
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

  // Check the payment status and update the h1 title
  const paymentStatus = paymentIntent.status;
  const h1Element = document.querySelector("h1");
  if (paymentStatus === "succeeded") {
    h1Element.innerText = "Payment successful!";
  } else {
    h1Element.innerText = "Payment failed or pending.";
  }
  // const paymentIntentPre = document.getElementById("payment-intent");
  // paymentIntentPre.innerText = JSON.stringify(paymentIntent, null, 2);
});
