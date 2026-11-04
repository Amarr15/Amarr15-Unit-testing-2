using FluentAssertions;
using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Domain.Exceptions;

namespace Simulation.Checkout.Tests;

public class CheckoutServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IInventoryService> _inventoryServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly CheckoutService _sut;

    public CheckoutServiceTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _inventoryServiceMock = new Mock<IInventoryService>();
        _notificationServiceMock = new Mock<INotificationService>();

        _inventoryServiceMock
            .Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _sut = new CheckoutService(
            _orderRepositoryMock.Object,
            _inventoryServiceMock.Object,
            _notificationServiceMock.Object);
    }

    // ── Validation & Business Rules ──

    [Fact]
    public async Task CheckoutAsync_WhenOrderHasNoItems_ShouldThrowEmptyOrderException()
    {
        // Arrange
        var order = CreateOrderWithItems();

        // Act
        var act = () => _sut.CheckoutAsync(order);

        // Assert
        await act.Should().ThrowAsync<EmptyOrderException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CheckoutAsync_WhenItemQuantityIsZeroOrNegative_ShouldThrowInvalidQuantityException(int quantity)
    {
        // Arrange
        const string productName = "Mechanical Keyboard";
        var item = CreateItem(productName, unitPrice: 150m, quantity: quantity);
        var order = CreateOrderWithItems(item);

        // Act
        var act = () => _sut.CheckoutAsync(order);

        // Assert
        var exceptionAssertion = await act.Should().ThrowAsync<InvalidQuantityException>();
        exceptionAssertion.WithMessage($"*{productName}*");
    }

    [Fact]
    public async Task CheckoutAsync_WhenItemPriceIsNegative_ShouldThrowInvalidPriceException()
    {
        // Arrange
        const string productName = "Wireless Mouse";
        var item = CreateItem(productName, unitPrice: -0.01m, quantity: 1);
        var order = CreateOrderWithItems(item);

        // Act
        var act = () => _sut.CheckoutAsync(order);

        // Assert
        await act.Should().ThrowAsync<InvalidPriceException>();
    }

    [Fact]
    public async Task CheckoutAsync_WhenItemPriceIsZero_ShouldSucceed()
    {
        // Arrange
        var item = CreateItem("Free Promotional Sticker", unitPrice: 0m, quantity: 1);
        var order = CreateOrderWithItems(item);

        // Act
        var result = await _sut.CheckoutAsync(order);

        // Assert
        result.Should().NotBeNull();
        result.Subtotal.Should().Be(0m);
        result.Discount.Should().Be(0m);
        result.Total.Should().Be(0m);
    }

    [Theory]
    [InlineData(999.99, 0)]
    [InlineData(1000.00, 100)]
    [InlineData(1500.00, 100)]
    public async Task CheckoutAsync_WhenSubtotalReachesThreshold_ShouldCalculateExpectedDiscount(decimal subtotal, decimal expectedDiscount)
    {
        // Arrange
        var item = CreateItem("Test Item", unitPrice: subtotal, quantity: 1);
        var order = CreateOrderWithItems(item);

        // Act
        var result = await _sut.CheckoutAsync(order);

        // Assert
        result.Discount.Should().Be(expectedDiscount);
    }

    [Fact]
    public async Task CheckoutAsync_WhenGivenExampleA_ShouldCalculateCorrectSubtotalDiscountAndTotal()
    {
        // Arrange — 2×150 + 1×400 = 700
        var item1 = CreateItem("Item A1", unitPrice: 150m, quantity: 2);
        var item2 = CreateItem("Item A2", unitPrice: 400m, quantity: 1);
        var order = CreateOrderWithItems(item1, item2);

        // Act
        var result = await _sut.CheckoutAsync(order);

        // Assert
        result.Subtotal.Should().Be(700m);
        result.Discount.Should().Be(0m);
        result.Total.Should().Be(700m);
    }

    [Fact]
    public async Task CheckoutAsync_WhenGivenExampleC_ShouldCalculateCorrectSubtotalDiscountAndTotal()
    {
        // Arrange — 3×500 = 1500
        var item = CreateItem("Item C", unitPrice: 500m, quantity: 3);
        var order = CreateOrderWithItems(item);

        // Act
        var result = await _sut.CheckoutAsync(order);

        // Assert
        result.Subtotal.Should().Be(1500m);
        result.Discount.Should().Be(100m);
        result.Total.Should().Be(1400m);
    }

    // ── Mocking & Interaction Verification ──

    [Fact]
    public async Task CheckoutAsync_WhenOrderIsValidAndAllItemsInStock_ShouldSaveOrderAndSendConfirmationOnce()
    {
        // Arrange
        var item = CreateItem("Headphones", unitPrice: 250m, quantity: 2);
        var order = CreateOrderWithItems(item);

        // Act
        var result = await _sut.CheckoutAsync(order);

        // Assert
        result.Should().NotBeNull();
        _orderRepositoryMock.Verify(x => x.SaveAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        _notificationServiceMock.Verify(x => x.SendOrderConfirmationAsync(It.Is<CheckoutReceipt>(r => r.OrderId == order.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckoutAsync_WhenOneItemIsOutOfStock_ShouldThrowInsufficientInventoryExceptionAndNeverSaveOrSend()
    {
        // Arrange
        var availableItem = CreateItem("Available Keyboard", unitPrice: 100m, quantity: 1);
        var outOfStockItem = CreateItem("OutOfStock Monitor", unitPrice: 300m, quantity: 1);
        var order = CreateOrderWithItems(availableItem, outOfStockItem);

        _inventoryServiceMock
            .Setup(x => x.IsAvailableAsync(outOfStockItem.ProductId, outOfStockItem.Quantity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var act = () => _sut.CheckoutAsync(order);

        // Assert
        await act.Should().ThrowAsync<InsufficientInventoryException>();
        _orderRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        _notificationServiceMock.Verify(x => x.SendOrderConfirmationAsync(It.IsAny<CheckoutReceipt>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckoutAsync_WhenOrderFailsValidation_ShouldNeverCallInventorySaveOrNotification()
    {
        // Arrange
        var item = CreateItem("Invalid Item", unitPrice: 50m, quantity: 0);
        var order = CreateOrderWithItems(item);

        // Act
        var act = () => _sut.CheckoutAsync(order);

        // Assert
        await act.Should().ThrowAsync<InvalidQuantityException>();
        _inventoryServiceMock.Verify(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _orderRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        _notificationServiceMock.Verify(x => x.SendOrderConfirmationAsync(It.IsAny<CheckoutReceipt>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── Helpers ──

    private static Order CreateOrderWithItems(params OrderItem[] items)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = "Ahmed Ali",
            CustomerEmail = "ahmed.ali@example.com",
            Items = items.ToList()
        };
    }

    private static OrderItem CreateItem(string productName, decimal unitPrice, int quantity)
    {
        return new OrderItem
        {
            ProductId = Guid.NewGuid(),
            ProductName = productName,
            UnitPrice = unitPrice,
            Quantity = quantity
        };
    }
}
