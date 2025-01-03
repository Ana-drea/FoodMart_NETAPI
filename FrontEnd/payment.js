document.addEventListener("DOMContentLoaded", async () => {
  // Fetch the publishable key from the server
  const { publishableKey } = await fetch("/config").then((r) => r.json());
  const stripe = Stripe(publishableKey);
  // Create the payment intent on the server
  const { clientSecret } = await fetch("/create-payment-intent", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
  }).then((r) => r.json());

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
        return_url: window.location.href.split("?")[0] + "complete.html",
      },
    });
    if (error) {
      const errorMessages = document.getElementById("error-messages");
      errorMessages.innerText = error.message;
    }
  });
});
