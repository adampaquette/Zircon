# Zircon.Test

Testing utilities with AutoFixture and AutoMoq integration for streamlined unit testing using the Fixture/Builder pattern.

## Installation

```bash
dotnet add package Zircon.Test
```

## Features

- `AutoMoqFixture<TSut>` base class for test fixtures
- Automatic dependency mocking with AutoMoq
- Lazy SUT (System Under Test) creation
- Fluent builder pattern for test setup
- Mock retrieval with `GetMock<T>()`

## The Fixture Pattern

The fixture pattern separates test setup from test logic, making tests more readable and maintainable.

### 1. Create a Test Fixture

```csharp
using Zircon.Test;
using Moq;

public class ProductServiceFixture : AutoMoqFixture<ProductService>
{
    // Access mocks for dependencies
    public Mock<IProductRepository> RepositoryMock => GetMock<IProductRepository>();
    public Mock<ILogger<ProductService>> LoggerMock => GetMock<ILogger<ProductService>>();
    
    // Builder method for setting up an existing product
    public ProductServiceFixture WithExistingProduct(Product product)
    {
        RepositoryMock
            .Setup(x => x.GetByIdAsync(product.Id))
            .ReturnsAsync(product);
        return this;
    }
    
    // Builder method for product not found scenario
    public ProductServiceFixture WithProductNotFound(Guid productId)
    {
        RepositoryMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);
        return this;
    }
    
    // Builder method for multiple products
    public ProductServiceFixture WithMultipleProducts(params Product[] products)
    {
        foreach (var product in products)
        {
            WithExistingProduct(product);
        }
        return this;
    }
}
```

### 2. Write Clean Tests

```csharp
public class ProductServiceTests
{
    [Fact]
    public async Task GetProduct_WhenProductExists_ReturnsProduct()
    {
        // Arrange
        var product = new Product { Id = Guid.NewGuid(), Name = "Test Product", Price = 99.99m };
        var fixture = new ProductServiceFixture()
            .WithExistingProduct(product);
        
        // Act
        var result = await fixture.Sut.GetProductAsync(product.Id);
        
        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(product);
    }
    
    [Fact]
    public async Task GetProduct_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var fixture = new ProductServiceFixture()
            .WithProductNotFound(productId);
        
        // Act
        var result = await fixture.Sut.GetProductAsync(productId);
        
        // Assert
        result.Should().BeNull();
    }
}
```

## Complex Example with Multiple Dependencies

```csharp
public class OrderServiceFixture : AutoMoqFixture<OrderService>
{
    public Mock<IOrderRepository> OrderRepoMock => GetMock<IOrderRepository>();
    public Mock<IProductService> ProductServiceMock => GetMock<IProductService>();
    public Mock<IPaymentService> PaymentServiceMock => GetMock<IPaymentService>();
    public Mock<IEmailService> EmailServiceMock => GetMock<IEmailService>();
    
    public OrderServiceFixture WithSuccessfulPayment()
    {
        PaymentServiceMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<decimal>(), It.IsAny<string>()))
            .ReturnsAsync(new PaymentResult { Success = true, TransactionId = "TXN123" });
        return this;
    }
    
    public OrderServiceFixture WithFailedPayment(string reason = "Insufficient funds")
    {
        PaymentServiceMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<decimal>(), It.IsAny<string>()))
            .ReturnsAsync(new PaymentResult { Success = false, Error = reason });
        return this;
    }
    
    public OrderServiceFixture WithProductsInStock(params Product[] products)
    {
        foreach (var product in products)
        {
            ProductServiceMock
                .Setup(x => x.GetProductAsync(product.Id))
                .ReturnsAsync(product);
            ProductServiceMock
                .Setup(x => x.IsInStockAsync(product.Id))
                .ReturnsAsync(true);
        }
        return this;
    }
}

// Using the fixture in tests
[Fact]
public async Task PlaceOrder_WithValidOrder_ProcessesSuccessfully()
{
    // Arrange - Clean, readable setup
    var products = new[]
    {
        new Product { Id = Guid.NewGuid(), Name = "Product 1", Price = 50m },
        new Product { Id = Guid.NewGuid(), Name = "Product 2", Price = 30m }
    };
    
    var fixture = new OrderServiceFixture()
        .WithProductsInStock(products)
        .WithSuccessfulPayment();
    
    var order = new Order
    {
        Items = products.Select(p => new OrderItem { ProductId = p.Id, Quantity = 1 }).ToList()
    };
    
    // Act
    var result = await fixture.Sut.PlaceOrderAsync(order);
    
    // Assert
    result.Success.Should().BeTrue();
    result.OrderId.Should().NotBeEmpty();
    
    // Verify interactions
    fixture.PaymentServiceMock.Verify(
        x => x.ProcessPaymentAsync(80m, It.IsAny<string>()), 
        Times.Once
    );
}
```

## Test Data Builders

Combine fixtures with builders for even cleaner tests:

```csharp
public class ProductBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = "Standard Product";
    private decimal _price = 99.99m;
    
    public ProductBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }
    
    public ProductBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    
    public ProductBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }
    
    public Product Build()
    {
        return new Product { Id = _id, Name = _name, Price = _price };
    }
    
    // Specialized builders for common scenarios
    public Product BuildPremiumProduct()
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = "Premium Gold Package",
            Price = 999.99m,
            Category = "Premium"
        };
    }
}

// Usage
[Fact]
public async Task GetPremiumProduct_ReturnsCorrectDiscount()
{
    // Arrange
    var product = new ProductBuilder().BuildPremiumProduct();
    var fixture = new ProductServiceFixture()
        .WithExistingProduct(product);
    
    // Act
    var result = await fixture.Sut.GetProductWithDiscountAsync(product.Id);
    
    // Assert
    result.DiscountPercentage.Should().Be(20); // Premium products get 20% discount
}
```

## Key Benefits

1. **Separation of Concerns**: Test setup logic is separated from test assertions
2. **Reusability**: Common scenarios can be reused across multiple tests
3. **Readability**: Tests clearly show what's being arranged using fluent syntax
4. **Maintainability**: Changes to setup logic only need to be made in one place
5. **Auto-mocking**: Dependencies are automatically mocked by AutoFixture

## Best Practices

- **Name fixture methods clearly**: `WithValidUser`, `WithExpiredToken`, `WithEmptyCart`
- **Chain multiple setups**: `.WithUser(user).WithProducts(products).WithSuccessfulPayment()`
- **Keep fixture methods focused**: Each method should set up one specific scenario
- **Use `Sut` property**: Access the system under test (lazy-loaded)
- **Verify important interactions**: Use mocks to verify critical method calls

## API Reference

### AutoMoqFixture<TSut>

**Properties:**
- `Fixture`: The AutoFixture instance with AutoMoq customization
- `Sut`: The system under test (automatically created with dependencies)

**Methods:**
- `GetMock<T>()`: Gets a frozen mock for the specified type

## Dependencies

- AutoFixture
- AutoFixture.AutoMoq
- Moq
- xUnit (recommended test framework)