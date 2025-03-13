import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import "bootstrap/dist/css/bootstrap.min.css";

const API_URL = "http://localhost:5134/api/orders";
const OrderDetailPage = () => {
  const { id } = useParams();
  const [order, setOrder] = useState({});
  const navigate = useNavigate();

  useEffect(() => {
    if (!id) {
      alert("Order id is missing in the URL!");
      navigate("/orders");
      return;
    }

    fetch(`${API_URL}/${id}`, {
      method: "GET",
      credentials: "include",
    })
      .then((response) => {
        if (!response.ok) {
          throw new Error(`HTTP error! Status: ${response.status}`);
        }
        return response.json();
      })
      .then((data) => setOrder(data))
      .catch((error) => console.error("Error fetching order: ", error));
  }, [id]);

  return (
    <div className="container mt-5">
      <h2>Order No: {id}</h2>
      <OrderDetailTable order={order} />
      <div style={{ marginTop: "20px" }}></div>
      <a className="btn btn-primary" onClick={() => navigate("/orders")}>
        Go Back
      </a>
    </div>
  );
};

const OrderDetailTable = ({ order }) => {
  return (
    <table className="table">
      <thead>
        <tr>
          <th style={{ display: "flex", gap: "10px" }}>
            <span>{new Date(order.orderDate).toLocaleDateString()}</span>
            <span>{order.isCompleted ? "Completed" : "Pending"}</span>
          </th>
        </tr>
        <tr>
          <th>Item</th>
          <th>Price</th>
          <th>Quantity</th>
          <th>Total Amount</th>
        </tr>
      </thead>
      <tbody>
        {(order.items || []).map((item) => (
          <ItemRow key={item.productId} item={item} />
        ))}
      </tbody>
      <tfoot>
        <tr>
          <td colSpan="3" className="text-right"></td>
          <td>
            <span
              style={{
                fontWeight: "bold",
                fontSize: "1.2em",
                color: "#FF5733",
              }}
            >
              ${order.totalAmount}
            </span>
          </td>
        </tr>
      </tfoot>
    </table>
  );
};

const ItemRow = ({ item }) => {
  return (
    <tr>
      <td>{item.productName}</td>
      <td>{item.unitPrice}</td>
      <td>{item.quantity}</td>
      <td>{item.totalPrice}</td>
    </tr>
  );
};

export default OrderDetailPage;
