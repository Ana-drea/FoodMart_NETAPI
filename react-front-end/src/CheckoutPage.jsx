import { useEffect, useState } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faTrash,
  faHeart,
  faMinus,
  faPlus,
} from "@fortawesome/free-solid-svg-icons";
import { MDBBtn, MDBInput } from "mdb-react-ui-kit";

const CheckoutPage = () => {
  const token = localStorage.getItem("token");
  const API_URL = "http://localhost:5134/api/carts";
  // Item data state
  const [items, setItems] = useState([]);

  useEffect(() => {
    const fetchCartData = async () => {
      try {
        const response = await fetch(API_URL, {
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`,
          },
        });
        if (response.status === 401) {
          // clearCartAndPromptLogin();
          return;
        }

        if (!response.ok) throw new Error("Failed to fetch cart data");

        const data = await response.json();
        setItems(data?.items || []);
      } catch (error) {
        console.error("Error:", error);
      }
    };

    fetchCartData();
  }, [token]);

  // Calculate total price
  const total = items.reduce(
    (sum, item) => sum + item.productPrice * item.quantity,
    0
  );

  // Handle item quantity change
  const handleQuantityChange = async (productId, delta) => {
    // Store previous data for possible rollback
    const prevItems = items;

    // Optimistic update on frontend
    setItems((prev) =>
      prev.map((item) =>
        item.productId === productId
          ? {
              ...item,
              quantity: Math.max(1, item.quantity + delta),
              // Indicates that the item is being updated
              _pending: true,
            }
          : item
      )
    );

    // Update on backend
    const payload = {
      productId: productId,
      change: delta,
    };
    try {
      // Send POST request to backend
      const response = await fetch(API_URL, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(payload),
      });
      if (!response.ok) {
        throw new Error("Failed to update cart");
      }
    } catch (error) {
      // Rollback when request failed
      setItems(prevItems);
      alert("Failed to update cart, please try again");
    } finally {
      // Remove loading status
      setItems((prev) =>
        prev.map((item) =>
          item.productId === productId ? { ...item, _pending: false } : item
        )
      );
    }
  };

  // Delete item
  const handleRemoveItem = async (productId) => {
    // Store previous data for possible rollback
    const prevItems = items;

    // Remove from frontend
    setItems((prev) => prev.filter((item) => item.productId !== productId));

    try {
      const response = await fetch(`${API_URL}/${productId}`, {
        method: "DELETE",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
      });

      if (!response.ok) {
        throw new Error("Failed to update cart");
      }
    } catch (error) {
      // Rollback when request failed
      setItems(prevItems);
      alert("Failed to update cart, please try again");
    }
  };

  // Move to wish list
  const handleMoveToWishlist = (id) => {
    console.log("Move to wishlist:", id);
  };

  return (
    <section className="h-100 gradient-custom">
      <div className="container py-5">
        <div className="row d-flex justify-content-center my-4">
          <div className="col-md-8">
            <div className="card mb-4">
              <div className="card-header py-3">
                <h5 className="mb-0">Cart - {items.length} items</h5>
              </div>
              <div className="card-body">
                {items.map((item) => (
                  <CartItem
                    key={item.productId}
                    item={item}
                    onQuantityChange={handleQuantityChange}
                    onRemove={handleRemoveItem}
                    onMoveToWishlist={handleMoveToWishlist}
                  />
                ))}
              </div>
            </div>

            <PickupInfo />
          </div>

          <div className="col-md-4">
            <OrderSummary total={total} />
          </div>
        </div>
      </div>
    </section>
  );
};

// Cart item component
const CartItem = ({ item, onQuantityChange, onRemove, onMoveToWishlist }) => (
  <>
    <div className="row mb-4">
      <div className="col-lg-3 col-md-12 mb-4 mb-lg-0">
        <div className="bg-image hover-overlay hover-zoom ripple rounded">
          <img
            src={item.productImage || ""}
            className="w-100"
            alt={item.productName}
          />
          <a href="#!">
            <div
              className="mask"
              style={{ backgroundColor: "rgba(251, 251, 251, 0.2)" }}
            ></div>
          </a>
        </div>
      </div>

      <div className="col-lg-5 col-md-6 mb-4 mb-lg-0">
        <p>
          <strong>{item.productName}</strong>
        </p>
        <div className="d-flex">
          <MDBBtn
            className="btn btn-primary btn-sm me-1 mb-2"
            onClick={() => onRemove(item.productId)}
          >
            <FontAwesomeIcon icon={faTrash} />
          </MDBBtn>
          <MDBBtn className="btn btn-danger btn-sm mb-2">
            <FontAwesomeIcon icon={faHeart} />
          </MDBBtn>
        </div>
      </div>

      <div className="col-lg-4 col-md-6 mb-4 mb-lg-0">
        <div className="d-flex mb-4" style={{ maxWidth: "300px" }}>
          <MDBBtn
            className="btn btn-primary px-3 me-2"
            onClick={() => onQuantityChange(item.productId, -1)}
          >
            <FontAwesomeIcon icon={faMinus} />
          </MDBBtn>

          <div className="form-outline">
            <MDBInput
              label="Quantity"
              type="number"
              className="form-control"
              value={item.quantity}
              readOnly
            />
          </div>

          <MDBBtn
            className="btn btn-primary px-3 ms-2"
            onClick={() => onQuantityChange(item.productId, +1)}
          >
            <FontAwesomeIcon icon={faPlus} />
          </MDBBtn>
        </div>
        <p className="text-start text-md-center">
          <strong>${(item.productPrice * item.quantity).toFixed(2)}</strong>
        </p>
      </div>
    </div>
    <hr className="my-4" />
  </>
);

// Order summary component
const OrderSummary = ({ total }) => (
  <div className="card mb-4">
    <div className="card-header py-3">
      <h5 className="mb-0">Summary</h5>
    </div>
    <div className="card-body">
      <ul className="list-group list-group-flush">
        <li className="list-group-item d-flex justify-content-between align-items-center border-0 px-0 pb-0">
          Products
          <span>${total.toFixed(2)}</span>
        </li>
        <li className="list-group-item d-flex justify-content-between align-items-center px-0">
          Shipping
          <span>Gratis</span>
        </li>
        <li className="list-group-item d-flex justify-content-between align-items-center border-0 px-0 mb-3">
          <div>
            <strong>Total amount</strong>
          </div>
          <span>
            <strong>${total.toFixed(2)}</strong>
          </span>
        </li>
      </ul>
      <MDBBtn className="btn btn-primary btn-lg btn-block">
        Go to checkout
      </MDBBtn>
    </div>
  </div>
);

// Pickup info component
const PickupInfo = () => (
  <div className="card mb-4">
    <div className="card-body">
      <p>
        <strong>Expected pickup location</strong>
      </p>
      <p className="mb-0">12.10.2020 - 14.10.2020</p>
    </div>
  </div>
);

export default CheckoutPage;
