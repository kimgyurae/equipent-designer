using System;
using FluentAssertions;
using Xunit;
using EquipmentDesigner.Services;
using EquipmentDesigner.Services.Api;
using EquipmentDesigner.Services.Storage;

namespace EquipmentDesigner.Tests.Services
{
    public class ServiceLocatorTests : IDisposable
    {
        public ServiceLocatorTests()
        {
            // Reset ServiceLocator before each test
            ServiceLocator.Reset();
        }

        public void Dispose()
        {
            ServiceLocator.Reset();
        }

        #region Registration Tests

        [Fact]
        public void RegisterSingleton_RegistersServiceInstance()
        {
            // Arrange
            var repository = new MemoryRepository();

            // Act
            ServiceLocator.RegisterSingleton<IDataRepository>(repository);
            var resolved = ServiceLocator.GetService<IDataRepository>();

            // Assert
            resolved.Should().BeSameAs(repository);
        }

        [Fact]
        public void RegisterSingleton_ReturnsSameInstanceOnMultipleCalls()
        {
            // Arrange
            var repository = new MemoryRepository();
            ServiceLocator.RegisterSingleton<IDataRepository>(repository);

            // Act
            var first = ServiceLocator.GetService<IDataRepository>();
            var second = ServiceLocator.GetService<IDataRepository>();

            // Assert
            first.Should().BeSameAs(second);
        }

        [Fact]
        public void RegisterFactory_RegistersFactoryMethod()
        {
            // Arrange & Act
            ServiceLocator.RegisterFactory<IDataRepository>(() => new MemoryRepository());
            var resolved = ServiceLocator.GetService<IDataRepository>();

            // Assert
            resolved.Should().NotBeNull();
            resolved.Should().BeOfType<MemoryRepository>();
        }

        [Fact]
        public void RegisterFactory_CreatesNewInstanceEachCall()
        {
            // Arrange
            ServiceLocator.RegisterFactory<IDataRepository>(() => new MemoryRepository());

            // Act
            var first = ServiceLocator.GetService<IDataRepository>();
            var second = ServiceLocator.GetService<IDataRepository>();

            // Assert
            first.Should().NotBeSameAs(second);
        }

        #endregion

        #region Resolution Tests

        [Fact]
        public void GetService_WhenNotRegistered_ThrowsInvalidOperationException()
        {
            // Act & Assert
            Action action = () => ServiceLocator.GetService<IDataRepository>();
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*IDataRepository*not registered*");
        }

        [Fact]
        public void TryGetService_WhenRegistered_ReturnsTrue()
        {
            // Arrange
            ServiceLocator.RegisterSingleton<IDataRepository>(new MemoryRepository());

            // Act
            var result = ServiceLocator.TryGetService<IDataRepository>(out var service);

            // Assert
            result.Should().BeTrue();
            service.Should().NotBeNull();
        }

        [Fact]
        public void TryGetService_WhenNotRegistered_ReturnsFalse()
        {
            // Act
            var result = ServiceLocator.TryGetService<IDataRepository>(out var service);

            // Assert
            result.Should().BeFalse();
            service.Should().BeNull();
        }

        [Fact]
        public void IsRegistered_WhenRegistered_ReturnsTrue()
        {
            // Arrange
            ServiceLocator.RegisterSingleton<IDataRepository>(new MemoryRepository());

            // Act & Assert
            ServiceLocator.IsRegistered<IDataRepository>().Should().BeTrue();
        }

        [Fact]
        public void IsRegistered_WhenNotRegistered_ReturnsFalse()
        {
            // Act & Assert
            ServiceLocator.IsRegistered<IDataRepository>().Should().BeFalse();
        }

        #endregion

        #region Reset Tests

        [Fact]
        public void Reset_ClearsAllRegistrations()
        {
            // Arrange
            ServiceLocator.RegisterSingleton<IDataRepository>(new MemoryRepository());

            // Act
            ServiceLocator.Reset();

            // Assert
            ServiceLocator.IsRegistered<IDataRepository>().Should().BeFalse();
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void ConfigureForProduction_RegistersRequiredServices()
        {
            // Act
            ServiceLocator.ConfigureForProduction();

            // Assert
            ServiceLocator.IsRegistered<IDataRepository>().Should().BeTrue();
            ServiceLocator.IsRegistered<IEquipmentApiService>().Should().BeTrue();

            var repository = ServiceLocator.GetService<IDataRepository>();
            repository.Should().BeOfType<JsonFileRepository>();

            var apiService = ServiceLocator.GetService<IEquipmentApiService>();
            apiService.Should().BeOfType<MockEquipmentApiService>();
        }

        [Fact]
        public void ConfigureForTesting_RegistersMemoryRepository()
        {
            // Act
            ServiceLocator.ConfigureForTesting();

            // Assert
            var repository = ServiceLocator.GetService<IDataRepository>();
            repository.Should().BeOfType<MemoryRepository>();

            var apiService = ServiceLocator.GetService<IEquipmentApiService>();
            apiService.Should().BeOfType<MockEquipmentApiService>();
        }

        #endregion
    }
}