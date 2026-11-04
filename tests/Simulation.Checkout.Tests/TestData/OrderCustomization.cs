using AutoFixture;
using Simulation.Checkout.Domain;

namespace Simulation.Checkout.Tests.TestData;

public sealed class ValidOrderCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<OrderItem>(composer => composer
            .With(i => i.ProductId, Guid.NewGuid())
            .With(i => i.ProductName, "AutoFixture Product")
            .With(i => i.Quantity, 2)
            .With(i => i.UnitPrice, 150m));

        fixture.Customize<Order>(composer => composer
            .With(o => o.Id, Guid.NewGuid())
            .With(o => o.CustomerName, "Valid Customer")
            .With(o => o.CustomerEmail, "valid.customer@example.com")
            .With(o => o.Items, () => new List<OrderItem>
            {
                fixture.Create<OrderItem>(),
                fixture.Create<OrderItem>()
            }));
    }
}
