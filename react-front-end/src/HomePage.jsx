import { useEffect, useState } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faTrash,
  faHeart,
  faMinus,
  faPlus,
} from "@fortawesome/free-solid-svg-icons";
import { MDBBtn, MDBInput } from "mdb-react-ui-kit";

const HomePage = () => {
  const [activeCategory, setActiveCategory] = useState(-1); // Set All tab as default active tab
  const [categories, setCategories] = useState([]);
  const [products, setProducts] = useState([]);
  // State to control products and quantities to be added to cart
  // {"1":1, "2":5}
  const [cartQuantities, setCartQuantities] = useState({});
  const API_URL = "http://localhost:5134/api";
  const token = localStorage.getItem("token");

  useEffect(() => {
    const fetchCategoryData = async () => {
      try {
        const response = await fetch(`${API_URL}/categories`, {
          headers: { "Content-Type": "application/json" },
        });
        if (!response.ok) {
          throw new Error("Failed to fetch categories data.");
        }
        const data = await response.json();
        setCategories([
          // Add "All" to categories
          { id: -1, name: "All", description: "All products go here." },
          ...data,
        ]);
      } catch (error) {
        console.error("Error: ", error);
      }
    };

    const fetchProductData = async () => {
      try {
        const response = await fetch(`${API_URL}/products`, {
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`,
          },
        });
        if (!response.ok) {
          throw new Error("Failed to fetch products data.");
        }
        const data = await response.json();
        setProducts(data.products);
      } catch (error) {
        console.error("Error: ", error);
      }
    };

    fetchCategoryData();
    fetchProductData();
  }, []);

  const handleInputChange = (productId, newQuantity) => {
    if (newQuantity === "") {
      setCartQuantities((prev) => ({
        ...prev,
        [productId]: "",
      }));
      return;
    }

    const quantity = Math.max(1, parseInt(newQuantity, 10) || 1);
    setCartQuantities((prev) => ({
      ...prev,
      [productId]: quantity,
    }));
  };

  const handleAdjustQuantity = (productId, delta) => {
    setCartQuantities((prev) => ({
      ...prev,
      [productId]: Math.max(1, (prev[productId] || 1) + delta),
    }));
  };

  return (
    <div>
      {/* Tabs header */}
      <div className="tabs-header d-flex justify-content-between border-bottom my-5">
        <h3>Trending Products</h3>
        <nav>
          <div className="nav nav-tabs" role="tablist">
            {categories.map((category) => (
              <button
                key={category.id}
                className={`nav-link text-uppercase fs-6 ${
                  activeCategory === category.id ? "active" : ""
                }`}
                onClick={() => setActiveCategory(category.id)}
              >
                {category.name}
              </button>
            ))}
          </div>
        </nav>
      </div>

      {/* Tab content */}
      <div className="tab-content">
        {categories.map((category) => (
          <div
            key={category.id}
            className={`tab-pane fade ${
              activeCategory === category.id ? "show active" : ""
            }`}
            role="tabpanel"
            aria-labelledby={`nav-${category.name}-tab`}
          >
            {/* Filter out products under current category, populate product grid */}
            <ProductGrid
              products={
                activeCategory === -1
                  ? products
                  : products.filter(
                      (product) => product.categoryId === activeCategory
                    )
              }
              cartQuantities={cartQuantities}
              onAdjustQuantity={handleAdjustQuantity}
              onInputChange={handleInputChange}
            />
          </div>
        ))}
      </div>
    </div>
  );
};

const ProductGrid = ({
  products,
  cartQuantities,
  onAdjustQuantity,
  onInputChange,
}) => (
  <div className="product-grid row row-cols-1 row-cols-sm-2 row-cols-md-3 row-cols-lg-4 row-cols-xl-5">
    {products.map((product) => (
      <ProductItem
        key={product.id}
        product={product}
        cartQuantities={cartQuantities}
        onAdjustQuantity={onAdjustQuantity}
        onInputChange={onInputChange}
      />
    ))}
  </div>
);

const ProductItem = ({
  product,
  cartQuantities,
  onAdjustQuantity,
  onInputChange,
}) => (
  <div className="col">
    <div className="product-item">
      {product.discount
        ? `<span className="badge bg-success position-absolute m-3">${product.discount}%</span>`
        : ""}
      <MDBBtn className="btn btn-danger btn-sm mb-2">
        <FontAwesomeIcon icon={faHeart} />
      </MDBBtn>
      <figure>
        <div className="bg-image hover-overlay hover-zoom ripple rounded">
          <img
            src={
              product.imageUrl ||
              "https://res.cloudinary.com/dq4ljtojj/image/upload/v1740955722/vmpadykmuofhnlqiw5mb.png"
            }
            className="tab-image w-100"
          />
          <a href="#!">
            <div
              className="mask"
              style={{ backgroundColor: "rgba(251, 251, 251, 0.2)" }}
            ></div>
          </a>
        </div>
      </figure>
      <h3>{product.name}</h3>
      <p className="qty">{product.quantityInStock} Unit</p>
      {product.rating
        ? `      <span className="rating">
        <svg width="24" height="24" class="text-primary">
          <use href="#star-solid"></use>
        </svg>
        ${product.rating}
      </span>`
        : ""}
      <span className="price">${product.price}</span>
      <div className="d-flex align-items-center justify-content-between">
        <div className="input-group product-qty">
          <div className="d-flex mb-4" style={{ maxWidth: "300px" }}>
            <MDBBtn
              className="btn btn-primary px-3 me-2"
              onClick={() => onAdjustQuantity(product.id, -1)}
            >
              <FontAwesomeIcon icon={faMinus} />
            </MDBBtn>

            <div className="form-outline">
              <MDBInput
                label="Quantity"
                type="number"
                className="form-control"
                value={cartQuantities[product.id] || 1}
                onChange={(e) =>
                  onInputChange(product.id, parseInt(e.target.value))
                }
              />
            </div>

            <MDBBtn
              className="btn btn-primary px-3 ms-2"
              onClick={() => onAdjustQuantity(product.id, 1)}
            >
              <FontAwesomeIcon icon={faPlus} />
            </MDBBtn>
          </div>
        </div>
        <a href="#" className="nav-link">
          Add to Cart <iconify-icon icon="uil:shopping-cart" />
        </a>
      </div>
    </div>
  </div>
);

export default HomePage;
