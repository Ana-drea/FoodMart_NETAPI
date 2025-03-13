import React, { useEffect, useState } from "react";
import "bootstrap/dist/css/bootstrap.min.css";
import { useNavigate } from "react-router-dom";

const API_URL = "http://localhost:5134/api/categories";

const CategoriesPage = () => {
  const [categories, setCategories] = useState([]);
  const navigate = useNavigate();

  useEffect(() => {
    fetch(API_URL, {
      method: "GET",
      credentials: "include",
    })
      .then((response) => response.json())
      .then((data) => setCategories(data))
      .catch((error) => console.error("Error fetching categories:", error));
  }, []);

  const handleDelete = (id) => {
    if (window.confirm("Do you want to delete this category?")) {
      fetch(`${API_URL}/${id}`, {
        method: "DELETE",
        credentials: "include",
      })
        .then((response) => {
          if (response.ok) {
            setCategories(categories.filter((category) => category.id !== id));
            alert("Category deleted successfully.");
          } else {
            alert("Failed to delete category. Please try again.");
          }
        })
        .catch((error) => console.error("Error deleting category:", error));
    }
  };

  return (
    <div className="container mt-5">
      <h2>Categories</h2>
      <CategoriesTable categories={categories} onDelete={handleDelete} />
      <a
        onClick={() => navigate("/add-category")}
        className="btn btn-primary btn-sm mt-3"
      >
        Add Category
      </a>
    </div>
  );
};

const CategoriesTable = ({ categories, onDelete }) => {
  return (
    <table className="table">
      <thead>
        <tr>
          <th>Name</th>
          <th>Description</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        {categories.map((category) => (
          <CategoryRow
            key={category.id}
            category={category}
            onDelete={onDelete}
          />
        ))}
      </tbody>
    </table>
  );
};

const CategoryRow = ({ category, onDelete }) => {
  const navigate = useNavigate();
  return (
    <tr>
      <td>
        <a
          className="category-link"
          onClick={() => navigate(`/products?categoryId=${category.id}`)}
          style={{ cursor: "pointer" }}
        >
          {category.name}
        </a>
      </td>
      <td
        onClick={() => navigate(`/edit-category/${category.id}`)}
        style={{ cursor: "pointer" }}
      >
        {category.description}
      </td>
      <td>
        <button
          className="btn btn-danger btn-sm"
          onClick={() => onDelete(category.id)}
        >
          Delete
        </button>
      </td>
    </tr>
  );
};

export default CategoriesPage;
