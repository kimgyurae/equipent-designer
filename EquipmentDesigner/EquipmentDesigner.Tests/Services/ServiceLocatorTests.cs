using System;
using FluentAssertions;
using Xunit;
using EquipmentDesigner.Services;

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
        public void RegisterSingleton_WithTypedRepository_RegistersServiceInstance()
        {
            // Arrange
            var repository = new MemoryWorkflowRepository();

            // Act
            ServiceLocator.RegisterSingleton<IWorkflowRepository>(repository);
            var resolved = ServiceLocator.GetService<IWorkflowRepository>();

            // Assert
            resolved.Should().BeSameAs(repository);
        }

        [Fact]
        public void RegisterSingleton_ReturnsSameInstanceOnMultipleCalls()
        {
            // Arrange
            var repository = new MemoryWorkflowRepository();
            ServiceLocator.RegisterSingleton<IWorkflowRepository>(repository);

            // Act
            var first = ServiceLocator.GetService<IWorkflowRepository>();
            var second = ServiceLocator.GetService<IWorkflowRepository>();

            // Assert
            first.Should().BeSameAs(second);
        }

        [Fact]
        public void RegisterFactory_WithTypedRepository_RegistersFactoryMethod()
        {
            // Arrange & Act
            ServiceLocator.RegisterFactory<IWorkflowRepository>(() => new MemoryWorkflowRepository());
            var resolved = ServiceLocator.GetService<IWorkflowRepository>();

            // Assert
            resolved.Should().NotBeNull();
            resolved.Should().BeOfType<MemoryWorkflowRepository>();
        }

        [Fact]
        public void RegisterFactory_CreatesNewInstanceEachCall()
        {
            // Arrange
            ServiceLocator.RegisterFactory<IWorkflowRepository>(() => new MemoryWorkflowRepository());

            // Act
            var first = ServiceLocator.GetService<IWorkflowRepository>();
            var second = ServiceLocator.GetService<IWorkflowRepository>();

            // Assert
            first.Should().NotBeSameAs(second);
        }

        #endregion

        #region Resolution Tests

        [Fact]
        public void GetService_WhenNotRegistered_ThrowsInvalidOperationException()
        {
            // Act & Assert
            Action action = () => ServiceLocator.GetService<IWorkflowRepository>();
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*IWorkflowRepository*not registered*");
        }

        [Fact]
        public void TryGetService_WhenRegistered_ReturnsTrue()
        {
            // Arrange
            ServiceLocator.RegisterSingleton<IWorkflowRepository>(new MemoryWorkflowRepository());

            // Act
            var result = ServiceLocator.TryGetService<IWorkflowRepository>(out var service);

            // Assert
            result.Should().BeTrue();
            service.Should().NotBeNull();
        }

        [Fact]
        public void TryGetService_WhenNotRegistered_ReturnsFalse()
        {
            // Act
            var result = ServiceLocator.TryGetService<IWorkflowRepository>(out var service);

            // Assert
            result.Should().BeFalse();
            service.Should().BeNull();
        }

        [Fact]
        public void IsRegistered_WhenRegistered_ReturnsTrue()
        {
            // Arrange
            ServiceLocator.RegisterSingleton<IWorkflowRepository>(new MemoryWorkflowRepository());

            // Act & Assert
            ServiceLocator.IsRegistered<IWorkflowRepository>().Should().BeTrue();
        }

        [Fact]
        public void IsRegistered_WhenNotRegistered_ReturnsFalse()
        {
            // Act & Assert
            ServiceLocator.IsRegistered<IWorkflowRepository>().Should().BeFalse();
        }

        #endregion

        #region Reset Tests

        [Fact]
        public void Reset_ClearsAllRegistrations()
        {
            // Arrange
            ServiceLocator.RegisterSingleton<IWorkflowRepository>(new MemoryWorkflowRepository());

            // Act
            ServiceLocator.Reset();

            // Assert
            ServiceLocator.IsRegistered<IWorkflowRepository>().Should().BeFalse();
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void ConfigureForProduction_RegistersRequiredServices()
        {
            // Act
            ServiceLocator.ConfigureForProduction();

            // Assert
            ServiceLocator.IsRegistered<IWorkflowRepository>().Should().BeTrue();
            ServiceLocator.IsRegistered<IUploadedWorkflowRepository>().Should().BeTrue();

            var workflowRepository = ServiceLocator.GetService<IWorkflowRepository>();
            workflowRepository.Should().BeOfType<WorkflowRepository>();

            var uploadedWorkflowRepository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            uploadedWorkflowRepository.Should().BeOfType<UploadedWorkflowRepository>();
        }

        [Fact]
        public void ConfigureForTesting_RegistersMemoryTypedRepositories()
        {
            // Act
            ServiceLocator.ConfigureForTesting();

            // Assert
            var workflowRepository = ServiceLocator.GetService<IWorkflowRepository>();
            workflowRepository.Should().BeOfType<MemoryWorkflowRepository>();

            var uploadedWorkflowRepository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            uploadedWorkflowRepository.Should().BeOfType<MemoryUploadedWorkflowRepository>();
        }

        #endregion
    }
}
