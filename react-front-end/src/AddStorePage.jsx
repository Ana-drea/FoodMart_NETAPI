import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import "bootstrap/dist/css/bootstrap.min.css";

const API_URL = "http://localhost:5134/api/stores";

const AddStorePage = () => {
  const [name, setName] = useState("");
  const [address, setAddress] = useState("");
  const [isActive, setIsActive] = useState(true);
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    const storeData = { Name: name, Address: address, IsActive:isActive };

    try {
      const response = await fetch(API_URL, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        body: JSON.stringify(storeData),
      });
      
      if (response.ok) {
        alert("Store added successfully");
        navigate("/stores"); // Redirect to stores page
      } else {
        alert("Failed to add store");
      }
    } catch (error) {
      console.error("Error adding store:", error);
      alert("An error occurred");
    }
  };

  return (
    <div className="container mt-5">
      <h2>Add Store</h2>
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="Name">Name</label>
          <input
            type="text"
            className="form-control"
            id="Name"
            value={name}
            onChange={(e) => setName(e.target.value)}
          />
        </div>
        <div className="form-group">
          <label htmlFor="Address">Address</label>
          <textarea
            className="form-control"
            id="Address"
            value={address}
            onChange={(e) => setAddress(e.target.value)}
          />
        </div>
        <div className="form-group form-check form-switch">
          <label className="form-check-label" htmlFor="IsActive">{isActive?"Active":"Disabled"}</label>
          <input
            type="checkbox"
            className="form-check-input" // use form-check-input for checkbox
            id="IsActive"
            checked={isActive} // use checked rather than value
            onChange={(e) => setIsActive(e.target.checked)} // use e.target.checked rather than e.target.value for bool
          />
        </div>
        <button type="submit" className="btn btn-primary mt-3">Submit</button>
      </form>
      <div style={{ marginTop: "20px" }}></div>
      <button onClick={() => navigate("/stores")} className="btn btn-primary">Go Back</button>
    </div>
  );
};

export default AddStorePage;
