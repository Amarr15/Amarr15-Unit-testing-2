using Bogus;
using Simulation.Checkout.Domain;

namespace Simulation.Checkout.Tests.TestData;

public static class CustomerFaker
{
    public const int DefaultSeed = 42;

    public static Faker<Order> CreateOrderFaker(int seed = DefaultSeed)
    {
        Randomizer.Seed = new Random(seed);

        var itemFaker = new Faker<OrderItem>()
            .CustomInstantiator(f => new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = f.Commerce.ProductName(),
                UnitPrice = decimal.Parse(f.Commerce.Price(10, 500)),
                Quantity = f.Random.Int(1, 5)
            });

        return new Faker<Order>()
            .CustomInstantiator(f => new Order
            {
                Id = Guid.NewGuid(),
                CustomerName = f.Person.FullName,
                CustomerEmail = f.Person.Email,
                Items = itemFaker.Generate(2)
            });
    }
}
