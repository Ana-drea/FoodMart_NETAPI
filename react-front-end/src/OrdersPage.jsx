import { useEffect, useState } from "react";
import "bootstrap/dist/css/bootstrap.min.css";
import { useNavigate } from "react-router-dom";

const API_URL = "http://localhost:5134/api/orders"; // API base URL

const OrdersPage = () => {
  const [orders, setOrders] = useState([]);

  // Fetch orders on component mount
  useEffect(() => {
    fetch(API_URL, {
      method: "GET",
      credentials: "include",
    })
      .then((response) => {
        if (!response.ok) {
          throw new Error(`HTTP error! Status: ${response.status}`);
        }
        return response.json();
      })
      .then((data) => setOrders(data))
      .catch((error) => console.error("Error fetching orders: ", error));
  }, []);

  return (
    <div className="col-12 grid-margin">
      <div className="card">
        <div className="card-body">
          <h4 className="card-title">Order Status</h4>
          <div className="table-responsive">
            <OrdersTable orders={orders} />
          </div>
        </div>
      </div>
    </div>
  );
};

const OrdersTable = ({ orders }) => {
  return (
    <table className="table">
      <thead>
        <tr>
          <th> Order No </th>
          <th> Store </th>
          <th> Total Amount </th>
          <th> Start Date </th>
          <th> Order Status </th>
        </tr>
      </thead>
      <tbody>
        {orders.map((order) => (
          <OrderRow key={order.id} order={order} />
        ))}
      </tbody>
    </table>
  );
};

const OrderRow = ({ order }) => {
  const navigate = useNavigate();
  return (
    <tr
      onClick={() => navigate(`/orders/${order.id}`)}
      style={{ cursor: "pointer" }}
    >
      <td>{order.id}</td>
      <td>{order.storeName || "Unknown"}</td>
      <td>${(order.totalAmount || 0).toFixed(2)}</td>
      <td>{new Date(order.orderDate).toLocaleDateString()}</td>
      <td>
        <div
          className={`badge badge-outline-${
            order.isCompleted ? "success" : "warning"
          }`}
        >
          {order.isCompleted ? "Completed" : "Pending"}
        </div>
      </td>
    </tr>
  );
};

export default OrdersPage;
