using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;

namespace Simulation.Checkout.Tests;

public class ReceiptSnapshotTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IInventoryService> _inventoryServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly CheckoutService _sut;

    public ReceiptSnapshotTests()
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
    public async Task CheckoutAsync_WhenGivenExampleC_ReceiptShouldMatchSnapshot()
    {
        // Arrange
        var order = new Order
        {
            Id = Guid.Parse("c0000000-0000-0000-0000-000000000001"),
            CustomerName = "Sara Mostafa",
            CustomerEmail = "sara.mostafa@example.com",
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductId = Guid.Parse("c0000000-0000-0000-0000-000000000002"),
                    ProductName = "Premium Mechanical Keyboard",
                    UnitPrice = 500m,
                    Quantity = 3
                }
            }
        };

        // Act
        var receipt = await _sut.CheckoutAsync(order);

        // Assert
        await Verify(receipt);
    }
}
