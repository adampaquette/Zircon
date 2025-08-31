# Zircon.Test

Testing utilities and fixtures for unit testing with AutoFixture and AutoMoq, providing convenient base classes for test setup and dependency management.

## Features

- **AutoMoq Integration**: Automatic mock creation and dependency injection
- **AutoFixture Integration**: Automatic test data generation
- **Base Test Classes**: Ready-to-use base classes for test fixtures
- **Consistent Mocking**: Frozen mocks ensure consistency across test methods
- **Simple API**: Minimal setup required for comprehensive testing

## Installation

```bash
dotnet add package Zircon.Test
```

## Usage

### Basic Test Fixture

```csharp
using Xunit;
using Moq;
using Zircon.Test;

public class UserServiceTests : AutoMoqFixture<UserService>
{
    [Fact]
    public void CreateUser_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var userData = new UserCreateRequest { Name = "John Doe", Email = "john@example.com" };
        
        GetMock<IUserRepository>()
            .Setup(x => x.EmailExists(userData.Email))
            .Returns(false);
        
        // Act
        var result = Sut.CreateUser(userData);
        
        // Assert
        Assert.True(result.IsSuccess);
        GetMock<IUserRepository>().Verify(x => x.Save(It.IsAny<User>()), Times.Once);
    }
}
```

### Complex Dependencies

```csharp
public class OrderServiceTests : AutoMoqFixture<OrderService>
{
    [Fact]
    public void ProcessOrder_WithValidOrder_SendsNotification()
    {
        // Arrange
        var order = Fixture.Create<Order>();
        var customer = Fixture.Create<Customer>();
        
        GetMock<ICustomerService>()
            .Setup(x => x.GetById(order.CustomerId))
            .Returns(customer);
            
        GetMock<IPaymentService>()
            .Setup(x => x.ProcessPayment(It.IsAny<PaymentRequest>()))
            .Returns(PaymentResult.Success("txn-123"));
        
        // Act
        var result = Sut.ProcessOrder(order);
        
        // Assert
        Assert.True(result.IsSuccess);
        GetMock<INotificationService>()
            .Verify(x => x.SendOrderConfirmation(customer.Email, order.Id), Times.Once);
    }
}
```

### Testing with Custom Data

```csharp
public class ProductServiceTests : AutoMoqFixture<ProductService>
{
    [Fact]
    public void UpdateProduct_WithPriceChange_UpdatesAuditTrail()
    {
        // Arrange
        var productId = Fixture.Create<int>();
        var existingProduct = Fixture.Build<Product>()
            .With(x => x.Id, productId)
            .With(x => x.Price, 100m)
            .Create();
            
        var updateRequest = Fixture.Build<ProductUpdateRequest>()
            .With(x => x.Id, productId)
            .With(x => x.Price, 150m)
            .Create();
        
        GetMock<IProductRepository>()
            .Setup(x => x.GetById(productId))
            .Returns(existingProduct);
        
        // Act
        Sut.UpdateProduct(updateRequest);
        
        // Assert
        GetMock<IAuditService>()
            .Verify(x => x.LogPriceChange(productId, 100m, 150m), Times.Once);
    }
}
```

### Multiple Mock Configurations

```csharp
public class ShoppingCartTests : AutoMoqFixture<ShoppingCartService>
{
    public ShoppingCartTests()
    {
        // Configure common mock behaviors in constructor
        GetMock<IDiscountService>()
            .Setup(x => x.CalculateDiscount(It.IsAny<decimal>()))
            .Returns<decimal>(amount => amount * 0.1m); // 10% discount
            
        GetMock<ITaxService>()
            .Setup(x => x.CalculateTax(It.IsAny<decimal>()))
            .Returns<decimal>(amount => amount * 0.08m); // 8% tax
    }
    
    [Fact]
    public void CalculateTotal_WithDiscountAndTax_ReturnsCorrectAmount()
    {
        // Arrange
        var items = Fixture.CreateMany<CartItem>(3).ToList();
        var subtotal = items.Sum(x => x.Price * x.Quantity);
        
        // Act
        var total = Sut.CalculateTotal(items);
        
        // Assert
        var expectedTotal = subtotal - (subtotal * 0.1m) + (subtotal * 0.08m);
        Assert.Equal(expectedTotal, total);
    }
}
```

## Key Classes

### AutoMoqFixture<TSut>

Base class for test fixtures that provides automatic mocking and dependency injection.

**Properties:**
- `Fixture`: The AutoFixture instance with AutoMoq customization
- `Sut`: The system under test (automatically created with dependencies)

**Methods:**
- `GetMock<T>()`: Gets a frozen mock for the specified type

**Features:**
- **Automatic SUT Creation**: Creates the system under test with all dependencies mocked
- **Frozen Mocks**: Ensures the same mock instance is used throughout the test
- **AutoFixture Integration**: Leverage AutoFixture's powerful object creation capabilities

## Advanced Usage

### Custom Fixture Configuration

```csharp
public class CustomUserServiceTests : AutoMoqFixture<UserService>
{
    public CustomUserServiceTests()
    {
        // Customize AutoFixture behavior
        Fixture.Customize<User>(composer => composer
            .With(x => x.CreatedDate, DateTime.UtcNow)
            .With(x => x.IsActive, true));
            
        // Configure specific mock behaviors
        GetMock<IEmailService>()
            .Setup(x => x.SendEmail(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult(true));
    }
}
```

### Testing Exception Scenarios

```csharp
[Fact]
public void ProcessPayment_WhenServiceThrows_HandlesGracefully()
{
    // Arrange
    var paymentRequest = Fixture.Create<PaymentRequest>();
    
    GetMock<IPaymentGateway>()
        .Setup(x => x.ProcessPayment(paymentRequest))
        .Throws(new PaymentException("Gateway unavailable"));
    
    // Act & Assert
    var exception = Assert.Throws<PaymentProcessingException>(() => 
        Sut.ProcessPayment(paymentRequest));
    
    Assert.Contains("Gateway unavailable", exception.Message);
}
```

### Verifying Complex Interactions

```csharp
[Fact]
public void CreateOrder_WithInvalidData_LogsErrorAndReturns()
{
    // Arrange
    var invalidOrder = Fixture.Build<CreateOrderRequest>()
        .Without(x => x.CustomerId) // Make it invalid
        .Create();
    
    // Act
    var result = Sut.CreateOrder(invalidOrder);
    
    // Assert
    Assert.False(result.IsSuccess);
    
    GetMock<ILogger<OrderService>>()
        .Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid order data")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
}
```

## Benefits

1. **Reduced Boilerplate**: Eliminates repetitive mock setup and SUT creation
2. **Consistent Testing**: Ensures all dependencies are properly mocked
3. **AutoFixture Power**: Leverage automatic test data generation
4. **Frozen Mocks**: Prevents issues with multiple mock instances
5. **Easy Verification**: Simple access to mocks for verification
6. **Maintainable Tests**: Changes to constructor dependencies don't break existing tests

## Best Practices

1. **Use Meaningful Test Data**: Customize AutoFixture when specific data is important
2. **Configure Common Mocks in Constructor**: Set up shared mock behaviors in the fixture constructor
3. **Verify Important Interactions**: Use `GetMock<T>().Verify()` to ensure critical methods are called
4. **Keep Tests Focused**: Test one behavior per test method
5. **Use Descriptive Test Names**: Follow the pattern `MethodName_Condition_ExpectedResult`

## Integration with xUnit

This package is designed to work seamlessly with xUnit.NET:

```csharp
public class ExampleTests : AutoMoqFixture<ExampleService>
{
    [Theory]
    [InlineData(1, "Test1")]
    [InlineData(2, "Test2")]
    public void ProcessItem_WithDifferentInputs_ReturnsExpectedResult(int id, string name)
    {
        // The fixture works with parameterized tests too
        var item = new Item { Id = id, Name = name };
        
        var result = Sut.ProcessItem(item);
        
        Assert.Equal(name.ToUpper(), result.ProcessedName);
    }
}
```

## Dependencies

- **AutoFixture**: Object creation and test data generation
- **AutoFixture.AutoMoq**: Automatic mock creation and wiring
- **Moq**: Mocking framework for creating test doubles
- **xUnit**: Testing framework integration

## License

This project is licensed under the MIT License - see the LICENSE file for details.