import React, { useState, useEffect } from "react";
import { Tab, Nav, Form, Button, Container, Row, Col } from "react-bootstrap";
import { encryptPassword } from "./utils/crypto";

const AccountSettings = () => {
  const API_URL = "http://localhost:5134/api";
  const token = localStorage.getItem("token");
  const [currentEmail, setCurrentEmail] = useState("");
  const [newEmail, setNewEmail] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");

  useEffect(() => {
    loadCurrentInfo();
  }, []);

  const loadCurrentInfo = async () => {
    try {
      const response = await fetch(`${API_URL}/account/current-info`, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
      });

      if (!response.ok) {
        if (response.status === 401) {
          alert("User is not logged in.");
          window.location.href = "/login";
        } else {
          throw new Error("Failed to fetch user info");
        }
      }

      const data = await response.json();
      setCurrentEmail(data.email);
      setPhoneNumber(data.phoneNumber);
    } catch (error) {
      console.error("Error fetching user info", error);
    }
  };

  const handleSavePhone = async (e) => {
    e.preventDefault();
    const phoneRegex = /^\d{10}$/;
    if (!phoneRegex.test(phoneNumber)) {
      alert("Phone number must be exactly 10 digits.");
      return;
    }

    try {
      const response = await fetch(`${API_URL}/account/add-phone-number`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ phoneNumber: phoneNumber }),
      });

      const data = await response.json();
      if (!response.ok)
        throw new Error(data.message || "Error saving phone number");
      alert(data.message || "Phone number saved successfully!");
    } catch (error) {
      console.error("Error:", error);
      alert(error.message);
    }
  };

  const handleChangeEmail = async (e) => {
    e.preventDefault();

    // Validate the new email
    if (!newEmail || !newEmail.includes("@")) {
      alert("Please enter a valid email address.");
      return;
    }

    try {
      const response = await fetch(`${API_URL}/account/change-email`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ newEmail: newEmail }),
      })
        .then(async (response) => {
          if (!response.ok) {
            // If the response status is not 2xx，throw an error
            const errorData = await response.json();
            throw new Error(
              errorData.message || "An error occurred while changing the email."
            );
          }
          return response.json();
        })
        .then((data) => {
          // Handle successful response
          alert(data.message || "Email changed successfully!");
        });
    } catch (error) {
      console.error("Error:", error);
      alert(error.message || "An error occurred while changing the email.");
    }
  };

  const handleChangePassword = async (e) => {
    const token = localStorage.getItem("token");
    const publicKey = process.env.REACT_APP_PUBLIC_KEY;
    const API_URL = "http://localhost:5134/api";
    e.preventDefault();

    // Validate the new password
    if (!currentPassword || !newPassword) {
      alert("Password cannot be empty.");
      return;
    }
    // Load public key and encrypt password
    const currentEncryptedPassword = await encryptPassword(
      currentPassword,
      publicKey
    );
    const newEncryptedPassword = await encryptPassword(newPassword, publicKey);

    // Send POST request to the backend
    fetch(`${API_URL}/account/change-password`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({
        currentPassword: currentEncryptedPassword,
        newPassword: newEncryptedPassword,
      }),
    })
      .then(async (response) => {
        if (!response.ok) {
          // If the response status is not 2xx，throw an error
          const errorData = await response.json();
          throw new Error(
            errorData.message ||
              "An error occurred while changing the password."
          );
        }
        return response.json();
      })
      .then((data) => {
        // Handle successful response
        alert(data.message || "Password changed successfully!");
      })
      .catch((error) => {
        console.error("Error:", error);
        alert(
          error.message || "An error occurred while changing the password."
        );
      });
  };

  return (
    <Container>
      <h2>Manage your account</h2>
      <p>Change your account settings</p>
      <Tab.Container defaultActiveKey="profile">
        <Row className="account-settings">
          <Col md={3}>
            <Nav variant="pills" className="flex-column">
              <Nav.Item>
                <Nav.Link eventKey="profile">Profile</Nav.Link>
              </Nav.Item>
              <Nav.Item>
                <Nav.Link eventKey="email">Email</Nav.Link>
              </Nav.Item>
              <Nav.Item>
                <Nav.Link eventKey="password">Password</Nav.Link>
              </Nav.Item>
            </Nav>
          </Col>
          <Col md={9}>
            <Tab.Content>
              <Tab.Pane eventKey="profile">
                <h3>Manage Profile</h3>
                <Form>
                  <Form.Group controlId="username">
                    <Form.Label>Username</Form.Label>
                    <Form.Control
                      type="text"
                      value={currentEmail}
                      readOnly
                      disabled
                    />
                  </Form.Group>
                  <Form.Group controlId="phone-number">
                    <Form.Label>Phone number</Form.Label>
                    <Form.Control
                      type="tel"
                      value={phoneNumber || ""}
                      onChange={(e) => setPhoneNumber(e.target.value)}
                    />
                  </Form.Group>
                  <Button onClick={handleSavePhone}>Save</Button>
                </Form>
              </Tab.Pane>
              <Tab.Pane eventKey="email">
                <h3>Manage Email</h3>
                <Form>
                  <Form.Group controlId="current-email">
                    <Form.Label>Current Email</Form.Label>
                    <Form.Control
                      type="email"
                      value={currentEmail}
                      readOnly
                      disabled
                    />
                  </Form.Group>
                  <Form.Group controlId="new-email">
                    <Form.Label>New email</Form.Label>
                    <Form.Control
                      type="email"
                      placeholder="Enter new email"
                      onChange={(e) => setNewEmail(e.target.value)}
                    />
                  </Form.Group>
                  <Button onClick={handleChangeEmail}>Change Email</Button>
                </Form>
              </Tab.Pane>
              <Tab.Pane eventKey="password">
                <h3>Manage Password</h3>
                <Form>
                  <Form.Group controlId="current-password">
                    <Form.Label>Current password</Form.Label>
                    <Form.Control
                      type="password"
                      placeholder="Enter current password"
                      onChange={(e) => setCurrentPassword(e.target.value)}
                    />
                  </Form.Group>
                  <Form.Group controlId="new-password">
                    <Form.Label>New password</Form.Label>
                    <Form.Control
                      type="password"
                      placeholder="Enter new password"
                      onChange={(e) => setNewPassword(e.target.value)}
                    />
                  </Form.Group>
                  <Button onClick={handleChangePassword}>
                    Change Password
                  </Button>
                </Form>
              </Tab.Pane>
            </Tab.Content>
          </Col>
        </Row>
      </Tab.Container>
    </Container>
  );
};

export default AccountSettings;
