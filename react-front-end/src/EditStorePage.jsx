import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import "bootstrap/dist/css/bootstrap.min.css";

const API_URL = "http://localhost:5134/api/stores";

const EditStorePage = () => {
  const { id } = useParams(); // Get store ID from URL
  const [name, setName] = useState("");
  const [address, setAddress] = useState("");
  const [isActive, setIsActive] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    if (!id) {
      alert("Store id is missing in the URL!");
      navigate("/stores");
      return;
    }

    fetch(`${API_URL}/${id}`, {
      method: "GET",
      credentials: "include",
    })
      .then((response) => response.json())
      .then((data) => {
        setName(data.name);
        setAddress(data.address);
        setIsActive(data.isActive);
      })
      .catch((error) => console.error("Error fetching store:", error));
  }, [id]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    const storeData = {
      Id: id,
      Name: name,
      Address: address,
      IsActive: isActive,
    };

    try {
      const response = await fetch(`${API_URL}/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        body: JSON.stringify(storeData),
      });

      if (response.ok) {
        alert("Store updated successfully");
        navigate("/stores"); // Redirect to stores page
      } else {
        alert("Failed to update store");
      }
    } catch (error) {
      console.error("Error update store:", error);
      alert("An error occurred");
    }
  };

  return (
    <div className="container mt-5">
      <h2>Edit Store</h2>
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
          <label className="form-check-label" htmlFor="IsActive">
            {isActive ? "Active" : "Disabled"}
          </label>
          <input
            type="checkbox"
            className="form-check-input" // use form-check-input for checkbox
            id="IsActive"
            checked={isActive} // use checked rather than value
            onChange={(e) => setIsActive(e.target.checked)} // use e.target.checked rather than e.target.value for bool
          />
        </div>
        <button type="submit" className="btn btn-primary mt-3">
          Submit
        </button>
      </form>
      <div style={{ marginTop: "20px" }}></div>
      <button onClick={() => navigate("/stores")} className="btn btn-primary">
        Go Back
      </button>
    </div>
  );
};

export default EditStorePage;
