import React, { useEffect, useState } from "react";
import "bootstrap/dist/css/bootstrap.min.css";
import { useNavigate } from "react-router-dom";

const API_URL = "http://localhost:5134/api/stores";

const StoresPage = () => {
  const [stores, setStores] = useState([]);
  const navigate = useNavigate();

  useEffect(() => {
    fetch(API_URL, {
      method: "GET",
      credentials: "include",
    })
      .then((response) => response.json())
      .then((data) => setStores(data))
      .catch((error) => console.error("Error fetching stores:", error));
  }, []);

  const handleDelete = (id) => {
    if (window.confirm("Do you want to delete this store?")) {
      fetch(`${API_URL}/${id}`, {
        method: "DELETE",
        credentials: "include",
      })
        .then((response) => {
          if (response.ok) {
            setStores(stores.filter((store) => store.id !== id));
            alert("Store deleted successfully.");
          } else {
            alert("Failed to delete store. Please try again.");
          }
        })
        .catch((error) => console.error("Error deleting store:", error));
    }
  };

  return (
    <div className="container mt-5">
      <h2>Stores</h2>
      <StoresTable stores={stores} onDelete={handleDelete} />
      <a
        onClick={() => navigate("/add-store")}
        className="btn btn-primary btn-sm mt-3"
      >
        Add Store
      </a>
    </div>
  );
};

const StoresTable = ({ stores, onDelete }) => {
  return (
    <table className="table">
      <thead>
        <tr>
          <th>Name</th>
          <th>Address</th>
          <th>State</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        {stores.map((store) => (
          <StoreRow key={store.id} store={store} onDelete={onDelete} />
        ))}
      </tbody>
    </table>
  );
};

const StoreRow = ({ store, onDelete }) => {
  const navigate = useNavigate();

  return (
    <tr
      onClick={() => navigate(`/edit-store/${store.id}`)}
      style={{ cursor: "pointer" }}
    >
      <td>{store.name}</td>
      <td>{store.address}</td>
      <td>{store.isActive ? "active" : "disabled"}</td>
      <td>
        <button
          className="btn btn-danger btn-sm"
          onClick={() => onDelete(store.id)}
        >
          Delete
        </button>
      </td>
    </tr>
  );
};

export default StoresPage;
