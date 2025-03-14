import React from "react";
import { BrowserRouter as Router, Route, Routes } from "react-router-dom";
import Login from "./Login";
import CategoriesPage from "./CategoriesPage";
import AddCategoryPage from "./AddCategoryPage";
import EditCategoryPage from "./EditCategoryPage";
import ProductsPage from "./ProductsPage";
import AddProductPage from "./AddProductPage";
import EditProductPage from "./EditProductPage";
import StoresPage from "./StoresPage";
import AddStorePage from "./AddStorePage";
import EditStorePage from "./EditStorePage";
import OrdersPage from "./OrdersPage";
import OrderDetailPage from "./OrderDetailPage";
import AccountSettings from "./AccountSettings";

function HomePage() {
  return <h1>Home Page</h1>;
}

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/login" element={<Login />} />
        <Route path="/categories" element={<CategoriesPage />} />
        <Route path="/add-category" element={<AddCategoryPage />} />
        <Route path="/edit-category/:id" element={<EditCategoryPage />} />
        <Route path="/products" element={<ProductsPage />} />
        {/* categoryId may be passed as search params */}
        <Route path="/add-product" element={<AddProductPage />} />
        <Route path="/edit-product/:id" element={<EditProductPage />} />
        <Route path="/stores" element={<StoresPage />} />
        <Route path="/add-store" element={<AddStorePage />} />
        <Route path="/edit-store/:id" element={<EditStorePage />} />
        <Route path="/orders" element={<OrdersPage />} />
        <Route path="/orders/:id" element={<OrderDetailPage />} />
        <Route path="/account" element={<AccountSettings />} />
      </Routes>
    </Router>
  );
}

export default App;
