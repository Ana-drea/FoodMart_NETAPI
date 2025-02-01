document.addEventListener("DOMContentLoaded", async () => {
  // Fetch the publishable key from the server
  const { publishableKey } = await fetch(`${window.config.apiUrl}config`).then(
    (r) => r.json()
  );
  const stripe = Stripe(publishableKey);
  // Get the client secret from URL
  const url = new URL(window.location.href);
  const clientSecret = url.searchParams.get("clientSecret");
  if (!clientSecret) {
    alert("Payment clientSecret not found in URL. Please try again.");
  }

  //Mount our elements
  const elements = stripe.elements({ clientSecret });
  const paymentElement = elements.create("payment");
  paymentElement.mount("#payment-element");

  // Add event listener to submit payment to Stripe
  const form = document.getElementById("payment-form");
  form.addEventListener("submit", async (e) => {
    e.preventDefault();

    const { error } = stripe.confirmPayment({
      elements,
      confirmParams: {
        return_url: window.location.href.split("payment")[0] + "complete.html",
      },
    });
    if (error) {
      const errorMessages = document.getElementById("error-messages");
      errorMessages.innerText = error.message;
    }
  });
});
