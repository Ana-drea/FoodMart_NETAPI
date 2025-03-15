import { useState } from "react";
import { encryptPassword } from "./utils/crypto";
import "bootstrap/dist/css/bootstrap.min.css";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faFacebookF,
  faGoogle,
  faTwitter,
  faGithub,
} from "@fortawesome/free-brands-svg-icons";
import { MDBBtn, MDBRipple, MDBInput } from "mdb-react-ui-kit";
import { use } from "react";

const AuthPage = () => {
  const publicKey = process.env.REACT_APP_PUBLIC_KEY;

  const [loginEmail, setLoginEmail] = useState("");
  const [loginPassword, setLoginPassword] = useState("");
  const [registerEmail, setRegisterEmail] = useState("");
  const [registerPassword, setRegisterPassword] = useState("");
  const [repeatRegisterPassword, setRepeatRegisterPassword] = useState("");

  const [isLoginChecked, setIsLoginChecked] = useState(false);
  const [isRegisterChecked, setIsRegisterChecked] = useState(false);

  const [activeTab, setActiveTab] = useState("login");

  const handleLogin = async (event) => {
    event.preventDefault();
    const API_URL = "http://localhost:5134/api/Account/login";

    if (!loginEmail || !loginPassword) {
      alert("Please fill in all fields.");
      return;
    }

    try {
      const encryptedPassword = await encryptPassword(loginPassword, publicKey);
      const response = await fetch(API_URL, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Accept: "application/json",
        },
        body: JSON.stringify({
          email: loginEmail,
          password: encryptedPassword,
        }),
      });

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || "Login failed.");
      }

      const data = await response.json();
      localStorage.setItem("token", data.token);
      alert("Login successful!");
    } catch (error) {
      console.error("Error:", error);
      alert(error.message || "An error occurred during login.");
    }
  };

  const handleRegister = async (event) => {
    event.preventDefault();
    const API_URL = "http://localhost:5134/api/Account/register";
    // Validate inputs
    if (!registerEmail || !registerPassword || !repeatRegisterPassword) {
      alert("Please fill in all fields.");
      return;
    }
    if (registerPassword !== repeatRegisterPassword) {
      alert("Passwords do not match.");
      return;
    }
    if (!isRegisterChecked) {
      alert("Please agree to the terms.");
      return;
    }

    // Load public key and encrypt password
    const encryptedPassword = await encryptPassword(
      registerPassword,
      publicKey
    );

    // Submit to backend
    fetch(API_URL, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        email: registerEmail,
        password: encryptedPassword,
      }),
    })
      .then(async (response) => {
        if (response.ok) {
          //return response.json();
          console.log("ok");
        } else {
          const errorData = await response.json();
          const errorMessages = errorData.map((e) => e.description).join("\n");
          throw new Error(
            errorMessages || "Failed to register. Please try again."
          );
        }
      })
      .then((data) => {
        alert("Registration successful! \nPlease confirm your email");
      })
      .catch((error) => {
        console.error("Error:", error);
        alert(`Error: ${error.message}`);
      });
  };

  return (
    <div className="container mt-5">
      {/* Pills navs */}
      <ul
        className="nav nav-pills nav-justified mb-3"
        id="authTabs"
        role="tablist"
      >
        <li className="nav-item" role="presentation">
          <a
            className={`nav-link ${activeTab === "login" ? "active" : ""}`}
            onClick={() => setActiveTab("login")}
            id="tab-login"
            role="tab"
            aria-controls="pills-login"
            aria-selected={activeTab === "login"}
          >
            Login
          </a>
        </li>
        <li className="nav-item" role="presentation">
          <a
            className={`nav-link ${activeTab === "register" ? "active" : ""}`}
            onClick={() => setActiveTab("register")}
            id="tab-register"
            role="tab"
            aria-controls="pills-register"
            aria-selected={activeTab === "register"}
          >
            Register
          </a>
        </li>
      </ul>

      {/* Pills content */}
      <div className="tab-content">
        {/* Login Tab */}
        <div
          className={`tab-pane fade ${
            activeTab === "login" ? "show active" : ""
          }`}
          id="pills-login"
          role="tabpanel"
          aria-labelledby="tab-login"
        >
          <form onSubmit={handleLogin}>
            <div className="text-center mb-3">
              <p>Sign in with:</p>
              <MDBRipple>
                <button
                  type="button"
                  className="btn btn-link btn-floating mx-1"
                >
                  <FontAwesomeIcon icon={faFacebookF} />
                </button>
              </MDBRipple>
              <MDBRipple>
                <button
                  type="button"
                  className="btn btn-link btn-floating mx-1"
                >
                  <FontAwesomeIcon icon={faGoogle} />
                </button>
              </MDBRipple>
              <MDBRipple>
                <button
                  type="button"
                  className="btn btn-link btn-floating mx-1"
                >
                  <FontAwesomeIcon icon={faTwitter} />
                </button>
              </MDBRipple>
              <MDBRipple>
                <button
                  type="button"
                  className="btn btn-link btn-floating mx-1"
                >
                  <FontAwesomeIcon icon={faGithub} />
                </button>
              </MDBRipple>
            </div>
            <p className="text-center">or:</p>
            {/* Email input */}
            <div className="form-outline mb-4">
              <MDBInput
                label="Email"
                type="email"
                id="loginEmail"
                className="form-control"
                value={loginEmail}
                onChange={(e) => setLoginEmail(e.target.value)}
              />
            </div>
            {/* Password input */}
            <div className="form-outline mb-4">
              <MDBInput
                label="Password"
                type="password"
                id="loginPassword"
                className="form-control"
                value={loginPassword}
                onChange={(e) => setLoginPassword(e.target.value)}
              />
            </div>
            {/* 2 column grid layout */}
            <div className="row mb-4">
              <div className="col-md-6 d-flex justify-content-center">
                {/* Checkbox */}
                <div className="form-check mb-3 mb-md-0">
                  <input
                    className="form-check-input"
                    type="checkbox"
                    value=""
                    id="loginCheck"
                    checked={isLoginChecked}
                    onChange={() => setIsLoginChecked(!isLoginChecked)}
                  />
                  <label className="form-check-label" htmlFor="loginCheck">
                    {" "}
                    Remember me{" "}
                  </label>
                </div>
              </div>

              <div className="col-md-6 d-flex justify-content-center">
                {/* Simple link */}
                <a href="#!">Forgot password?</a>
              </div>
            </div>
            <MDBBtn type="submit" className="btn btn-primary btn-block mb-4">
              Sign in
            </MDBBtn>
            {/* Register buttons*/}
            <div className="text-center">
              <p>
                Not a member?{" "}
                <a
                  href="#!"
                  onClick={(e) => {
                    e.preventDefault();
                    setActiveTab("register");
                  }}
                >
                  Register
                </a>
              </p>
            </div>
          </form>
        </div>

        {/* Register Tab */}
        <div
          className={`tab-pane fade ${
            activeTab === "register" ? "show active" : ""
          }`}
          id="pills-register"
          role="tabpanel"
          aria-labelledby="tab-register"
        >
          <form onSubmit={handleRegister}>
            <div class="text-center mb-3">
              <p>Sign up with:</p>
              <MDBRipple>
                <button
                  type="button"
                  className="btn btn-link btn-floating mx-1"
                >
                  <FontAwesomeIcon icon={faFacebookF} />
                </button>
              </MDBRipple>
              <MDBRipple>
                <button
                  type="button"
                  className="btn btn-link btn-floating mx-1"
                >
                  <FontAwesomeIcon icon={faGoogle} />
                </button>
              </MDBRipple>
              <MDBRipple>
                <button
                  type="button"
                  className="btn btn-link btn-floating mx-1"
                >
                  <FontAwesomeIcon icon={faTwitter} />
                </button>
              </MDBRipple>
              <MDBRipple>
                <button
                  type="button"
                  className="btn btn-link btn-floating mx-1"
                >
                  <FontAwesomeIcon icon={faGithub} />
                </button>
              </MDBRipple>
            </div>

            <p class="text-center">or:</p>
            {/* Email input */}
            <div className="form-outline mb-4">
              <MDBInput
                label="Email"
                type="email"
                id="registerEmail"
                className="form-control"
                value={registerEmail}
                onChange={(e) => setRegisterEmail(e.target.value)}
              />
            </div>
            {/* Password input */}
            <div className="form-outline mb-4">
              <MDBInput
                label="Password"
                type="password"
                id="registerPassword"
                className="form-control"
                value={registerPassword}
                onChange={(e) => setRegisterPassword(e.target.value)}
              />
            </div>
            {/* Repeat password input */}
            <div className="form-outline mb-4">
              <MDBInput
                label="Password"
                type="password"
                id="repeatRegisterPassword"
                className="form-control"
                value={repeatRegisterPassword}
                onChange={(e) => setRepeatRegisterPassword(e.target.value)}
              />
            </div>
            {/* Checkbox */}
            <div class="form-check d-flex justify-content-center mb-4">
              <input
                className="form-check-input me-2"
                type="checkbox"
                value=""
                id="registerCheck"
                checked={isRegisterChecked}
                onChange={() => setIsRegisterChecked(!isRegisterChecked)}
                aria-describedby="registerCheckHelpText"
              />
              <label className="form-check-label" htmlFor="registerCheck">
                I have read and agree to the terms
              </label>
            </div>
            <MDBBtn type="submit" className="btn btn-primary btn-block mb-4">
              Register
            </MDBBtn>
            {/* Login buttons*/}
            <div className="text-center">
              <p>
                Already have an Account?{" "}
                <a
                  href="#!"
                  onClick={(e) => {
                    e.preventDefault();
                    setActiveTab("login");
                  }}
                >
                  Sign In
                </a>
              </p>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};
export default AuthPage;
