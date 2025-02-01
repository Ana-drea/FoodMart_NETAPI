async function logoutUser() {
  try {
    const response = await fetch(`${window.config.apiUrl}api/account/logout`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        accept: "application/json",
      },
      credentials: "include", // Include cookies for authentication
    });

    if (!response.ok) {
      throw new Error("Logout failed.");
    }

    const data = await response.json();
    alert(data.message || "Logout successful!");
    // Optionally redirect to the login page or homepage
    window.location.href = "/login.html";
  } catch (error) {
    console.error("Error during logout:", error);
    alert("An error occurred while logging out.");
  }
}

// Dynamically load header to webpage when needed
document.addEventListener("DOMContentLoaded", function () {
  fetch("header.html")
    .then((response) => response.text())
    .then((data) => {
      document.getElementById("header-container").innerHTML = data;
      // Bind the logout event after header is loaded
      const logoutOption = document.querySelector(
        '#user-nav a[href="#logout"]'
      );
      if (logoutOption) {
        logoutOption.addEventListener("click", logoutUser);
      } else {
        console.log("Logout link not found in the header!");
      }
    })
    .catch((error) => console.error("Error loading header:", error));
});

// Load styles.css
let stylesLink = document.createElement("link");
stylesLink.rel = "stylesheet";
stylesLink.href = "styles.css";
document.head.appendChild(stylesLink);

// Load Bootstrap
let bootstrapLink = document.createElement("link");
bootstrapLink.rel = "stylesheet";
bootstrapLink.href =
  "https://cdn.jsdelivr.net/npm/bootstrap@5.3.0-alpha1/dist/css/bootstrap.min.css";
document.head.appendChild(bootstrapLink);
