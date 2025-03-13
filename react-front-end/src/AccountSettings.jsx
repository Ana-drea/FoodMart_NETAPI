import React, { useState, useEffect } from "react";
import { Tab, Nav, Form, Button, Container, Row, Col } from "react-bootstrap";

const AccountSettings = () => {
  const API_URL = "http://localhost:5134/api";
  const token = localStorage.getItem("token");
  const [userInfo, setUserInfo] = useState({
    email: "",
    phoneNumber: "",
  });

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
      setUserInfo({ email: data.email, phoneNumber: data.phoneNumber });
    } catch (error) {
      console.error("Error fetching user info", error);
    }
  };

  const handleSavePhone = async () => {
    const phoneRegex = /^\d{10}$/;
    if (!phoneRegex.test(userInfo.phoneNumber)) {
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
        body: JSON.stringify({ phoneNumber: userInfo.phoneNumber }),
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
                      value={userInfo.email}
                      readOnly
                      disabled
                    />
                  </Form.Group>
                  <Form.Group controlId="phone-number">
                    <Form.Label>Phone number</Form.Label>
                    <Form.Control
                      type="tel"
                      value={userInfo.phoneNumber || ""}
                      onChange={(e) =>
                        setUserInfo({
                          ...userInfo,
                          phoneNumber: e.target.value,
                        })
                      }
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
                      value={userInfo.email}
                      readOnly
                      disabled
                    />
                  </Form.Group>
                  <Form.Group controlId="new-email">
                    <Form.Label>New email</Form.Label>
                    <Form.Control type="email" placeholder="Enter new email" />
                  </Form.Group>
                  <Button>Change Email</Button>
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
                    />
                  </Form.Group>
                  <Form.Group controlId="new-password">
                    <Form.Label>New password</Form.Label>
                    <Form.Control
                      type="password"
                      placeholder="Enter new password"
                    />
                  </Form.Group>
                  <Button>Change Password</Button>
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
