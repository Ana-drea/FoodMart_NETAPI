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

const Login = () => {
  const [loginEmail, setLoginEmail] = useState("");
  const [loginPassword, setLoginPassword] = useState("");
  const publicKey = process.env.REACT_APP_PUBLIC_KEY;

  const [isChecked, setIsChecked] = useState(false);

  const handleCheckboxChange = () => {
    setIsChecked(!isChecked); // Toggle the checkbox's checked state
  };

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
            className="nav-link active"
            id="tab-login"
            data-bs-toggle="tab"
            href="#pills-login"
            role="tab"
            aria-controls="pills-login"
            aria-selected="true"
          >
            Login
          </a>
        </li>
        <li className="nav-item" role="presentation">
          <a
            className="nav-link"
            id="tab-register"
            data-bs-toggle="tab"
            href="#pills-register"
            role="tab"
            aria-controls="pills-register"
            aria-selected="false"
          >
            Register
          </a>
        </li>
      </ul>

      {/* Pills content */}
      <div className="tab-content">
        {/* Login Tab */}
        <div
          className="tab-pane fade show active"
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
                id="email"
                className="form-control"
                value={loginEmail}
                onChange={(e) => setLoginEmail(e.target.value)}
              />
            </div>
            {/* Password input */}
            <div data-mdb-input-init className="form-outline mb-4">
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
                    checked={isChecked}
                    onChange={handleCheckboxChange}
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
            <MDBBtn
              type="submit"
              data-mdb-button-init
              data-mdb-ripple-init
              className="btn btn-primary btn-block mb-4"
            >
              Sign in
            </MDBBtn>
          </form>
        </div>

        {/* Register Tab */}
        <div
          className="tab-pane fade"
          id="pills-register"
          role="tabpanel"
          aria-labelledby="tab-register"
        >
          <form>
            <div className="mb-3">
              <label htmlFor="registerName" className="form-label">
                Name
              </label>
              <input type="text" id="registerName" className="form-control" />
            </div>
            <div className="mb-3">
              <label htmlFor="registerEmail" className="form-label">
                Email
              </label>
              <input type="email" id="registerEmail" className="form-control" />
            </div>
            <div className="mb-3">
              <label htmlFor="registerPassword" className="form-label">
                Password
              </label>
              <input
                type="password"
                id="registerPassword"
                className="form-control"
              />
            </div>
            <button type="submit" className="btn btn-primary w-100">
              Register
            </button>
          </form>
        </div>
      </div>
    </div>
  );
};
export default Login;
