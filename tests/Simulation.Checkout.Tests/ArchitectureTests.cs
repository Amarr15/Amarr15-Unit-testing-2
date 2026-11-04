using FluentAssertions;
using NetArchTest.Rules;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Infrastructure;

namespace Simulation.Checkout.Tests;

public class ArchitectureTests
{
    private static readonly System.Reflection.Assembly DomainAssembly = typeof(Order).Assembly;
    private static readonly System.Reflection.Assembly ApplicationAssembly = typeof(CheckoutService).Assembly;
    private static readonly System.Reflection.Assembly InfrastructureAssembly = typeof(InMemoryOrderRepository).Assembly;

    [Fact]
    public void DomainLayer_WhenChecked_ShouldNotDependOnApplicationOrInfrastructure()
    {
        // Arrange
        var domainTypes = Types.InAssembly(DomainAssembly);

        // Act
        var resultAgainstApp = domainTypes
            .ShouldNot()
            .HaveDependencyOn(ApplicationAssembly.GetName().Name)
            .GetResult();

        var resultAgainstInfra = domainTypes
            .ShouldNot()
            .HaveDependencyOn(InfrastructureAssembly.GetName().Name)
            .GetResult();

        // Assert
        resultAgainstApp.IsSuccessful.Should().BeTrue();
        resultAgainstInfra.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void ApplicationLayer_WhenChecked_ShouldNotDependOnInfrastructure()
    {
        // Arrange
        var appTypes = Types.InAssembly(ApplicationAssembly);

        // Act
        var result = appTypes
            .ShouldNot()
            .HaveDependencyOn(InfrastructureAssembly.GetName().Name)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void AllInterfaces_WhenChecked_ShouldStartWithI()
    {
        // Arrange
        var allTypes = Types.InAssemblies(new[] { DomainAssembly, ApplicationAssembly, InfrastructureAssembly });

        // Act
        var result = allTypes
            .That()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void OrderRepositoryClasses_WhenImplementingIOrderRepository_ShouldEndWithRepository()
    {
        // Arrange
        var allTypes = Types.InAssemblies(new[] { DomainAssembly, ApplicationAssembly, InfrastructureAssembly });

        // Act
        var result = allTypes
            .That()
            .ImplementInterface(typeof(IOrderRepository))
            .Should()
            .HaveNameEndingWith("Repository")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }
}
