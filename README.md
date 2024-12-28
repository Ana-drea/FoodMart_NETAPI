### How to update database after change in models:
1. Empty database and all tables
```bash
DROP DATABASE FoodMartDb;
CREATE DATABASE FoodMartDb;
```
2. migrate to database
```bash
dotnet ef migrations add Initial
dotnet ef database update
```
3. insert seed data
```bash
INSERT INTO Categories (Id, Name, Description) VALUES
(1, 'Electronics', 'Devices and gadgets like phones, laptops, and cameras'),
(2, 'Groceries', 'Everyday essential food items and supplies');

INSERT INTO Products (Id, Name, Description, CategoryId, Price, QuantityInStock) VALUES
(1, 'Smartphone', 'A high-end smartphone with 128GB storage', 1, 699.99, 50),
(2, 'Laptop', 'Lightweight laptop with 16GB RAM and 512GB SSD', 1, 1199.99, 30),
(3, 'Camera', 'Digital camera with 20MP resolution and 4K video', 1, 499.99, 15),
(4, 'Apple', 'Fresh organic apples, 1kg', 2, 3.99, 100),
(5, 'Milk', '1-liter full cream milk', 2, 1.49, 200),
(6, 'Bread', 'Whole grain bread, 400g', 2, 2.49, 150);

INSERT INTO Stores (Id, Name, Address, IsActive)
VALUES
    (1, 'Store 1', '123 Main St, City, Country', TRUE),
    (2, 'Store 2', '456 Elm St, City, Country', TRUE),
    (3, 'Store 3', '789 Oak St, City, Country', FALSE);
```


