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
  const API_URL = "http://localhost:5134/api";
  // Item data state
  const [items, setItems] = useState([]);
  const [stores, setStores] = useState([]);

  useEffect(() => {
    const fetchCartData = async () => {
      try {
        const response = await fetch(`${API_URL}/carts`, {
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`,
          },
        });
        if (response.status === 401) {
          return;
        }

        if (!response.ok) throw new Error("Failed to fetch cart data");

        const data = await response.json();
        setItems(data?.items || []);
      } catch (error) {
        console.error("Error:", error);
      }
    };

    const fetchStoreData = async () => {
      try {
        const response = await fetch(`${API_URL}/stores`, {
          headers: {
            "Content-Type": "application/json",
          },
        });
        if (!response.ok) throw new Error("Failed to fetch stores data.");
        const data = await response.json();
        setStores(data || []);
      } catch (error) {
        console.error("Error:", error);
      }
    };

    fetchCartData();
    fetchStoreData();
  }, [token]);

  // Calculate total price
  const total = items.reduce(
    (sum, item) => sum + item.productPrice * item.quantity,
    0
  );

  // Handle item quantity change
  const handleQuantityChange = async (productId, delta) => {
    // Store previous data for possible rollback
    const prevItems = [...items];

    const currentItem = items.find((item) => item.productId === productId);
    if (!currentItem) return;
    if (Math.max(1, currentItem.quantity + delta) === currentItem.quantity)
      return;

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
      const response = await fetch(`${API_URL}/carts`, {
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

  const handleInputChange = (productId, newQuantity) => {
    if (newQuantity === "") {
      setItems((prev) =>
        prev.map((item) =>
          item.productId === productId ? { ...item, quantity: "" } : item
        )
      );
      return; // If input is null, return
    }
    setItems((prev) =>
      prev.map((item) =>
        item.productId === productId ? { ...item, quantity: newQuantity } : item
      )
    );
  };

  const handleInputBlur = async (productId, quantity) => {
    if (isNaN(quantity) || quantity < 1) return;
    // Store previous data for possible rollback
    const prevItems = [...items];

    setItems((prev) =>
      prev.map((item) =>
        item.productId === productId
          ? {
              ...item,
              quantity: quantity === null ? 1 : quantity,
              // Indicates that the item is being updated
              _pending: true,
            }
          : item
      )
    );
    // Update on backend
    const payload = {
      productId: productId,
      quantity: quantity,
    };

    try {
      // Send POST request to backend
      const response = await fetch(`${API_URL}/carts`, {
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
      alert(`${error} Failed to update cart, please try again`);
    } finally {
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
      const response = await fetch(`${API_URL}/carts/${productId}`, {
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
                    onInputChange={handleInputChange}
                    onInputBlur={handleInputBlur}
                  />
                ))}
              </div>
            </div>

            <PickupInfo stores={stores} />
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
const CartItem = ({
  item,
  onQuantityChange,
  onRemove,
  onMoveToWishlist,
  onInputChange,
  onInputBlur,
}) => (
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
          <MDBBtn
            className="btn btn-danger btn-sm mb-2"
            onClick={() => onMoveToWishlist(item.productId)}
          >
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
              value={item.quantity === "" ? "" : item.quantity} // Allow input value to be null
              onChange={(e) => {
                onInputChange(
                  item.productId,
                  e.target.value === "" ? "" : parseInt(e.target.value, 10) || 0
                );
              }}
              onBlur={() => onInputBlur(item.productId, item.quantity)}
              onKeyDown={(e) => {
                if (e.key === "Enter") {
                  e.target.blur();
                }
              }}
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
      <MDBBtn className="btn btn-primary btn-lg btn-block">Checkout</MDBBtn>
    </div>
  </div>
);

// Pickup info component
const PickupInfo = ({ stores }) => {
  const [selectedStore, setSelectedStore] = useState("");
  return (
    <div className="card mb-4">
      <div className="card-header py-3">
        <h5 className="mb-0">Pickup info</h5>
      </div>
      <div className="card-body">
        <select
          className="form-select mb-0"
          aria-label="Select pickup store"
          value={selectedStore}
          onChange={(e) => setSelectedStore(e.target.value)}
        >
          <option value="" disabled>
            Select pickup store
          </option>
          {stores
            .filter((store) => store.isActive)
            .map((store) => (
              <option key={store.id} value={store.id}>
                {store.name}
              </option>
            ))}
        </select>
      </div>
    </div>
  );
};

export default CheckoutPage;
