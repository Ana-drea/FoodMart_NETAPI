import { useState } from "react";
import { encryptPassword } from "./utils/crypto";

const Login = () => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const publicKey = process.env.REACT_APP_PUBLIC_KEY;

  const handleLogin = async (event) => {
    event.preventDefault();
    const API_URL = "http://localhost:5134/api/Account/login";

    if (!email || !password) {
      alert("Please fill in all fields.");
      return;
    }

    try {
      const encryptedPassword = await encryptPassword(password, publicKey);
      const response = await fetch(API_URL, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Accept: "application/json",
        },
        body: JSON.stringify({ email, password: encryptedPassword }),
      });

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || "Login failed.");
      }

      const data = await response.json();
      localStorage.setItem("token", data.token);
      alert("Login successful!");
      // window.location.href = "order.html"; // Redirect after login
    } catch (error) {
      console.error("Error:", error);
      alert(error.message || "An error occurred during login.");
    }
  };

  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-100">
      <div className="w-full max-w-md bg-white p-6 rounded-lg shadow-lg">
        <h3 className="text-2xl font-semibold text-center mb-4">Login</h3>
        <form onSubmit={handleLogin}>
          <div className="mb-4">
            <label className="block text-sm font-medium">Email *</label>
            <input
              type="email"
              className="w-full p-2 border rounded"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>
          <div className="mb-4">
            <label className="block text-sm font-medium">Password *</label>
            <input
              type="password"
              className="w-full p-2 border rounded"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>
          <div className="flex justify-between items-center mb-4">
            <label className="flex items-center">
              <input type="checkbox" className="mr-2" /> Remember me
            </label>
            <a href="#" className="text-blue-500 text-sm">
              Forgot password?
            </a>
          </div>
          <button
            type="submit"
            className="w-full bg-blue-500 text-white p-2 rounded"
          >
            Login
          </button>
        </form>
        <p className="text-center mt-4">
          Don't have an account?{" "}
          <a href="register.html" className="text-blue-500">
            Register
          </a>
        </p>
      </div>
    </div>
  );
};
export default Login;
