using AutoFixture;
using FluentAssertions;
using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;

namespace Simulation.Checkout.Tests.TestData;

public class TestDataTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IInventoryService> _inventoryServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly CheckoutService _sut;

    public TestDataTests()
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

    [Fact]
    public async Task CheckoutAsync_WhenOrderCreatedWithAutoFixture_ShouldSucceed()
    {
        // Arrange
        var fixture = new Fixture();
        fixture.Customize(new ValidOrderCustomization());
        var order = fixture.Create<Order>();

        // Act
        var result = await _sut.CheckoutAsync(order);

        // Assert
        result.Should().NotBeNull();
        result.OrderId.Should().Be(order.Id);
        result.Subtotal.Should().BeGreaterThan(0m);
        result.Total.Should().BeGreaterThan(0m);
    }

    [Fact]
    public async Task CheckoutAsync_WhenCustomerGeneratedWithBogusFixedSeed_ReceiptShouldHaveMatchingNameAndEmail()
    {
        // Arrange
        var orderFaker = CustomerFaker.CreateOrderFaker(CustomerFaker.DefaultSeed);
        var order = orderFaker.Generate();

        // Act
        var result = await _sut.CheckoutAsync(order);

        // Assert
        result.Should().NotBeNull();
        result.CustomerName.Should().Be(order.CustomerName);
        result.CustomerEmail.Should().Be(order.CustomerEmail);
        result.CustomerName.Should().NotBeNullOrWhiteSpace();
        result.CustomerEmail.Should().Contain("@");
    }
}
