import React, { useEffect, useState } from "react";
import "bootstrap/dist/css/bootstrap.min.css";
import { useNavigate, useSearchParams } from "react-router-dom";

const API_URL = "http://localhost:5134/api/products";

const ProductsPage = () => {
  const [products, setProducts] = useState([]);
  const [searchParams] = useSearchParams();
  const categoryId = searchParams.get("categoryId"); // Get ?categoryId=1

  // Validate if categoryId is int
  const categoryFilter =
    categoryId && !isNaN(parseInt(categoryId))
      ? `?categoryId=${categoryId}`
      : "";
  const apiUrl = `${API_URL}${categoryFilter}`;

  useEffect(() => {
    const token = localStorage.getItem("token");
    fetch(apiUrl, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
    })
      .then((response) => response.json())
      .then((data) => setProducts(data.products))
      .catch((error) => console.error("Error fetching products:", error));
  }, [apiUrl]); // Depends on apiUrl, automatically resend request when categoryId changes

  const handleDelete = (id) => {
    if (window.confirm("Do you want to delete this product?")) {
      fetch(`${API_URL}/${id}`, {
        method: "DELETE",
        credentials: "include",
      })
        .then((response) => {
          if (response.ok) {
            setProducts(products.filter((product) => product.id !== id));
            alert("Product deleted successfully.");
          } else {
            alert("Failed to delete product. Please try again.");
          }
        })
        .catch((error) => console.error("Error deleting product:", error));
    }
  };

  return (
    <div className="container mt-5">
      <h2>Products</h2>
      <ProductsTable products={products} onDelete={handleDelete} />
      <a href="addProduct.html" className="btn btn-primary btn-sm mt-3">
        Add Product
      </a>
    </div>
  );
};

const ProductsTable = ({ products, onDelete }) => {
  return (
    <table className="table">
      <thead>
        <tr>
          <th>Name</th>
          <th>Description</th>
          <th>Category</th>
          <th>Price</th>
          <th>Quantity</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        {products.map((product) => (
          <ProductRow key={product.id} product={product} onDelete={onDelete} />
        ))}
      </tbody>
    </table>
  );
};

const ProductRow = ({ product, onDelete }) => {
  const navigate = useNavigate();
  return (
    <tr
      onClick={() => navigate(`/edit-product/${product.id}`)}
      style={{ cursor: "pointer" }}
    >
      <td>{product.name}</td>
      <td>{product.description}</td>
      <td>
        <a
          className="category-link"
          onClick={() =>
            navigate(`/products?categoryId=${product.category.id}`)
          }
        >
          {product.category.name}
        </a>
      </td>
      <td>{product.price}</td>
      <td>{product.quantityInStock}</td>
      <td>
        <button
          className="btn btn-danger btn-sm"
          onClick={() => onDelete(product.id)}
        >
          Delete
        </button>
      </td>
    </tr>
  );
};

export default ProductsPage;
