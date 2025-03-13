import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import "bootstrap/dist/css/bootstrap.min.css";

const API_URL = "http://localhost:5134/api/categories";

const EditCategoryPage = () => {
  const { id } = useParams(); // Get category ID from URL
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const navigate = useNavigate();
  
    useEffect(() => {
        if (!id) {
            alert("Category id is missing in the URL!");
            navigate("/categories");
            return;
      }

      fetch(`${API_URL}/${id}`, {
        method: "GET",
        credentials: "include",
      })
        .then((response) => response.json())
          .then((data) => {
              setName(data.name);
              setDescription(data.description);
          })
        .catch((error) => console.error("Error fetching category:", error));
    }, [id]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    const categoryData = { Id:id, Name: name, Description: description };

    try {
      const response = await fetch(`${API_URL}/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        body: JSON.stringify(categoryData),
      });
      
      if (response.ok) {
        alert("Category updated successfully");
        navigate("/categories"); // Redirect to categories page
      } else {
        alert("Failed to update category");
      }
    } catch (error) {
      console.error("Error adding category:", error);
      alert("An error occurred");
    }
  };

  return (
    <div className="container mt-5">
      <h2>Edit Category</h2>
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
          <label htmlFor="Description">Description</label>
          <textarea
            className="form-control"
            id="Description"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          />
        </div>
        <button type="submit" className="btn btn-primary mt-3">Submit</button>
      </form>
      <div style={{ marginTop: "20px" }}></div>
      <button onClick={() => navigate("/categories")} className="btn btn-primary">Go Back</button>
    </div>
  );
};

export default EditCategoryPage;
