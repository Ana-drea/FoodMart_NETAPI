import React, { useState, useEffect, useRef } from "react";
import { useNavigate } from "react-router-dom";

const API_URL = "http://localhost:5134/api"; // API base URL

const AddProductPage = () => {
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [categories, setCategories] = useState([]);
  const [categoryId, setCategoryId] = useState("");
  const [price, setPrice] = useState("");
  const [quantity, setQuantity] = useState("");
  const [imageUrl, setImageUrl] = useState("");
  const navigate = useNavigate();
  const fileInputRef = useRef(null); // Create ref to control image upload input

  // Fetch categories on component mount
  useEffect(() => {
    fetch(`${API_URL}/categories`, { credentials: "include" })
      .then((res) => res.json())
      .then((data) => setCategories(data))
      .catch((err) => console.error("Error fetching categories:", err));
  }, []);

  // Handle image upload preview
  const handleImageUpload = async (e) => {
    const file = e.target.files[0];
    if (!file) return;

    // Create local preview using base64
    const reader = new FileReader();
    reader.onloadend = () => setImageUrl(reader.result);
    reader.readAsDataURL(file);

    // Upload to backend and get url
    let formData = new FormData();
    formData.append("file", file);

    try {
      const response = await fetch(`${API_URL}/images`, {
        method: "POST",
        body: formData, // Send file data
      });

      if (!response.ok) {
        throw new Error(`HTTP error! Status: ${response.status}`);
      }

      const result = await response.json();
      setImageUrl(result.link); // Replace base64 preview with URL returned from backend
    } catch (error) {
      console.error("Error uploading image:", error);
      alert("Failed to upload image. Please try again.");
    }
  };
  // Handle image cancelling
  const handleCancel = () => {
    setImageUrl(null); // Clear out imageUrl
    if (fileInputRef.current) {
      fileInputRef.current.value = ""; // Clear out content of image input
    }
  };

  // Handle form submission
  const handleSubmit = (e) => {
    e.preventDefault();
    const productData = {
      name,
      description,
      categoryId,
      price,
      quantityInStock: quantity,
      imageUrl,
    };

    fetch(`${API_URL}/products`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      credentials: "include",
      body: JSON.stringify(productData),
    })
      .then((res) => {
        if (res.ok) {
          alert("Product added successfully");
          navigate("/products");
        } else {
          alert("Failed to add product");
        }
      })
      .catch((err) => console.error("Error adding product:", err));
  };

  return (
    <div className="container mt-5">
      <h2>Add Product</h2>
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="Name">Name</label>
          <input
            type="text"
            className="form-control"
            id="Name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="Description">Description</label>
          <textarea
            className="form-control"
            id="Description"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          />
        </div>

        <div className="form-group">
          <label htmlFor="CategoryId">Category</label>
          <select
            className="form-control"
            id="CategoryId"
            value={categoryId}
            onChange={(e) => setCategoryId(e.target.value)}
            required
          >
            <option value="" disabled>
              Select a category
            </option>
            {categories.map((cat) => (
              <option key={cat.id} value={cat.id}>
                {cat.name}
              </option>
            ))}
          </select>
        </div>
        <div
          className="form-group"
          style={{ marginTop: "10px", marginBottom: "10px" }}
        >
          <label htmlFor="Image">Image Upload</label>

          {imageUrl && (
            <div
              style={{
                position: "relative",
                display: "inline-block",
                marginBottom: "10px",
              }}
            >
              <img
                src={imageUrl}
                alt="Product"
                style={{ width: "200px", display: "block" }}
              />
              {/* Cancel button */}
              <button
                type="button"
                onClick={handleCancel} // Release memory
                className="btn-close"
                aria-label="Close"
                style={{
                  position: "absolute",
                  top: "10px", // Adjust the distance from the top
                  right: "10px", // Adjust the distance from the right
                  backgroundColor: "transparent", // Optional: Make the background transparent
                  border: "none", // Optional: Remove button borders
                  padding: "0", // Optional: Remove padding
                }}
              ></button>
            </div>
          )}

          <input
            type="file"
            className="form-control"
            id="Image"
            onChange={handleImageUpload}
            ref={fileInputRef}
          />
        </div>

        <div className="form-group">
          <label htmlFor="Price">Price</label>
          <input
            type="number"
            className="form-control"
            id="Price"
            value={price}
            onChange={(e) => setPrice(e.target.value)}
            step="0.01"
            min="0"
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="Quantity">Quantity In Stock</label>
          <input
            type="number"
            className="form-control"
            id="Quantity"
            value={quantity}
            onChange={(e) => setQuantity(e.target.value)}
            step="1"
            min="0"
            required
          />
        </div>

        <div style={{ marginTop: "20px" }}>
          <button type="submit" className="btn btn-primary">
            Submit
          </button>
        </div>
      </form>

      <div style={{ marginTop: "20px" }}>
        <button
          className="btn btn-primary"
          onClick={() => navigate("/products")}
        >
          Go Back
        </button>
      </div>
    </div>
  );
};

export default AddProductPage;
