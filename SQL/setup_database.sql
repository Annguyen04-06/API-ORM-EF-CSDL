IF DB_ID('OrderManagementDB') IS NOT NULL
BEGIN
    ALTER DATABASE OrderManagementDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE OrderManagementDB;
END
GO

CREATE DATABASE OrderManagementDB;
GO

USE OrderManagementDB;
GO
CREATE TABLE Customers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Phone NVARCHAR(20)
);
CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ProductName NVARCHAR(150) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    StockQuantity INT NOT NULL CHECK (StockQuantity >= 0)
);
CREATE TABLE Orders (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NOT NULL,
    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
);
CREATE TABLE OrderDetails (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL CHECK (Quantity > 0),
    UnitPrice DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(Id),
    FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT UQ_Order_Product UNIQUE (OrderId, ProductId)
);
INSERT INTO Customers (FullName, Email, Phone)
VALUES
(N'Nguyen Van An', 'an.nguyen@gmail.com', '0905123456'),
(N'Tran Thi Binh', 'binh.tran@gmail.com', '0912345678'),
(N'Le Minh Cuong', 'cuong.le@gmail.com', '0987654321'),
(N'Pham Thu Dung', 'dung.pham@gmail.com', '0933445566'),
(N'Hoang Gia Huy', 'huy.hoang@gmail.com', '0977889900');
INSERT INTO Products (ProductName, Price, StockQuantity)
VALUES
(N'Laptop Dell Inspiron 15', 18500000, 20),
(N'Chuot Logitech G102', 350000, 100),
(N'Ban phim co Keychron K6', 2100000, 50),
(N'Man hinh LG 24 inch', 3200000, 30),
(N'Tai nghe Sony WH-1000XM4', 6500000, 15);
INSERT INTO Orders (CustomerId)
VALUES
(1),
(2),
(3),
(1),
(4);
INSERT INTO OrderDetails (OrderId, ProductId, Quantity, UnitPrice)
VALUES
(1, 1, 1, 18500000),
(1, 2, 2, 350000),
(2, 3, 1, 2100000),
(2, 2, 1, 350000),
(3, 4, 2, 3200000),
(4, 5, 1, 6500000),
(4, 2, 1, 350000),
(5, 1, 1, 18500000);
UPDATE O
SET O.TotalAmount = (
    SELECT SUM(OD.Quantity * OD.UnitPrice)
    FROM OrderDetails OD
    WHERE OD.OrderId = O.Id
)
FROM Orders O;
UPDATE P
SET P.StockQuantity = P.StockQuantity - OD.TotalQty
FROM Products P
JOIN (
    SELECT ProductId, SUM(Quantity) AS TotalQty
    FROM OrderDetails
    GROUP BY ProductId
) OD ON P.Id = OD.ProductId;

IF OBJECT_ID('dbo.sp_AddOrderItem', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_AddOrderItem;
GO

CREATE PROCEDURE dbo.sp_AddOrderItem
    @OrderId INT,
    @ProductId INT,
    @Quantity INT,
    @ResultCode INT OUTPUT,
    @Message NVARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM Orders WHERE Id = @OrderId)
        BEGIN
            SET @ResultCode = -1;
            SET @Message = N'Don hang khong ton tai';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        DECLARE @UnitPrice DECIMAL(18,2);
        DECLARE @CurrentStock INT;

        SELECT @UnitPrice = Price, @CurrentStock = StockQuantity
        FROM Products
        WHERE Id = @ProductId;

        IF @UnitPrice IS NULL
        BEGIN
            SET @ResultCode = -2;
            SET @Message = N'San pham khong ton tai';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        IF EXISTS (
            SELECT 1
            FROM OrderDetails
            WHERE OrderId = @OrderId AND ProductId = @ProductId
        )
        BEGIN
            SET @ResultCode = -3;
            SET @Message = N'San pham da ton tai trong don hang nay';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        IF @CurrentStock < @Quantity
        BEGIN
            SET @ResultCode = -4;
            SET @Message = N'So luong ton kho khong du';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        INSERT INTO OrderDetails (OrderId, ProductId, Quantity, UnitPrice)
        VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice);
        UPDATE Products
        SET StockQuantity = StockQuantity - @Quantity
        WHERE Id = @ProductId;
        UPDATE Orders
        SET TotalAmount = ISNULL((
            SELECT SUM(Quantity * UnitPrice)
            FROM OrderDetails
            WHERE OrderId = @OrderId
        ), 0)
        WHERE Id = @OrderId;

        COMMIT TRANSACTION;

        SET @ResultCode = 0;
        SET @Message = N'Them san pham thanh cong';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SET @ResultCode = -99;
        SET @Message = ERROR_MESSAGE();
    END CATCH
END
GO

IF OBJECT_ID('dbo.sp_CalculateOrderTotal', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_CalculateOrderTotal;
GO

CREATE PROCEDURE dbo.sp_CalculateOrderTotal
    @OrderId INT,
    @TotalAmount DECIMAL(18,2) OUTPUT,
    @ResultCode INT OUTPUT,
    @Message NVARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Orders WHERE Id = @OrderId)
    BEGIN
        SET @ResultCode = -1;
        SET @Message = N'Don hang khong ton tai';
        SET @TotalAmount = 0;
        RETURN;
    END

    SELECT @TotalAmount = ISNULL(SUM(Quantity * UnitPrice), 0)
    FROM OrderDetails
    WHERE OrderId = @OrderId;

    UPDATE Orders
    SET TotalAmount = @TotalAmount
    WHERE Id = @OrderId;

    SET @ResultCode = 0;
    SET @Message = N'Tinh tong tien thanh cong';
END
GO
SELECT * FROM Customers;
SELECT * FROM Products;
SELECT * FROM Orders;
SELECT * FROM OrderDetails;

