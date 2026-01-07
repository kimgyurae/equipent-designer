using System;
using FluentAssertions;
using Xunit;
using EquipmentDesigner.Models.Storage;
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
        public void RegisterSingleton_WithTypedRepository_RegistersServiceInstance()
        {
            // Arrange
            var repository = new MemoryTypedRepository<IncompleteWorkflowDataStore>();

            // Act
            ServiceLocator.RegisterSingleton<ITypedDataRepository<IncompleteWorkflowDataStore>>(repository);
            var resolved = ServiceLocator.GetService<ITypedDataRepository<IncompleteWorkflowDataStore>>();

            // Assert
            resolved.Should().BeSameAs(repository);
        }

        [Fact]
        public void RegisterSingleton_ReturnsSameInstanceOnMultipleCalls()
        {
            // Arrange
            var repository = new MemoryTypedRepository<IncompleteWorkflowDataStore>();
            ServiceLocator.RegisterSingleton<ITypedDataRepository<IncompleteWorkflowDataStore>>(repository);

            // Act
            var first = ServiceLocator.GetService<ITypedDataRepository<IncompleteWorkflowDataStore>>();
            var second = ServiceLocator.GetService<ITypedDataRepository<IncompleteWorkflowDataStore>>();

            // Assert
            first.Should().BeSameAs(second);
        }

        [Fact]
        public void RegisterFactory_WithTypedRepository_RegistersFactoryMethod()
        {
            // Arrange & Act
            ServiceLocator.RegisterFactory<ITypedDataRepository<IncompleteWorkflowDataStore>>(() => new MemoryTypedRepository<IncompleteWorkflowDataStore>());
            var resolved = ServiceLocator.GetService<ITypedDataRepository<IncompleteWorkflowDataStore>>();

            // Assert
            resolved.Should().NotBeNull();
            resolved.Should().BeOfType<MemoryTypedRepository<IncompleteWorkflowDataStore>>();
        }

        [Fact]
        public void RegisterFactory_CreatesNewInstanceEachCall()
        {
            // Arrange
            ServiceLocator.RegisterFactory<ITypedDataRepository<IncompleteWorkflowDataStore>>(() => new MemoryTypedRepository<IncompleteWorkflowDataStore>());

            // Act
            var first = ServiceLocator.GetService<ITypedDataRepository<IncompleteWorkflowDataStore>>();
            var second = ServiceLocator.GetService<ITypedDataRepository<IncompleteWorkflowDataStore>>();

            // Assert
            first.Should().NotBeSameAs(second);
        }

        #endregion

        #region Resolution Tests

        [Fact]
        public void GetService_WhenNotRegistered_ThrowsInvalidOperationException()
        {
            // Act & Assert
            Action action = () => ServiceLocator.GetService<ITypedDataRepository<IncompleteWorkflowDataStore>>();
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*ITypedDataRepository*not registered*");
        }

        [Fact]
        public void TryGetService_WhenRegistered_ReturnsTrue()
        {
            // Arrange
            ServiceLocator.RegisterSingleton<ITypedDataRepository<IncompleteWorkflowDataStore>>(new MemoryTypedRepository<IncompleteWorkflowDataStore>());

            // Act
            var result = ServiceLocator.TryGetService<ITypedDataRepository<IncompleteWorkflowDataStore>>(out var service);

            // Assert
            result.Should().BeTrue();
            service.Should().NotBeNull();
        }

        [Fact]
        public void TryGetService_WhenNotRegistered_ReturnsFalse()
        {
            // Act
            var result = ServiceLocator.TryGetService<ITypedDataRepository<IncompleteWorkflowDataStore>>(out var service);

            // Assert
            result.Should().BeFalse();
            service.Should().BeNull();
        }

        [Fact]
        public void IsRegistered_WhenRegistered_ReturnsTrue()
        {
            // Arrange
            ServiceLocator.RegisterSingleton<ITypedDataRepository<IncompleteWorkflowDataStore>>(new MemoryTypedRepository<IncompleteWorkflowDataStore>());

            // Act & Assert
            ServiceLocator.IsRegistered<ITypedDataRepository<IncompleteWorkflowDataStore>>().Should().BeTrue();
        }

        [Fact]
        public void IsRegistered_WhenNotRegistered_ReturnsFalse()
        {
            // Act & Assert
            ServiceLocator.IsRegistered<ITypedDataRepository<IncompleteWorkflowDataStore>>().Should().BeFalse();
        }

        #endregion

        #region Reset Tests

        [Fact]
        public void Reset_ClearsAllRegistrations()
        {
            // Arrange
            ServiceLocator.RegisterSingleton<ITypedDataRepository<IncompleteWorkflowDataStore>>(new MemoryTypedRepository<IncompleteWorkflowDataStore>());

            // Act
            ServiceLocator.Reset();

            // Assert
            ServiceLocator.IsRegistered<ITypedDataRepository<IncompleteWorkflowDataStore>>().Should().BeFalse();
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void ConfigureForProduction_RegistersRequiredServices()
        {
            // Act
            ServiceLocator.ConfigureForProduction();

            // Assert
            ServiceLocator.IsRegistered<ITypedDataRepository<IncompleteWorkflowDataStore>>().Should().BeTrue();
            ServiceLocator.IsRegistered<ITypedDataRepository<UploadedHardwareDataStore>>().Should().BeTrue();
            ServiceLocator.IsRegistered<IEquipmentApiService>().Should().BeTrue();

            var workflowRepository = ServiceLocator.GetService<ITypedDataRepository<IncompleteWorkflowDataStore>>();
            workflowRepository.Should().BeOfType<WorkflowRepository>();

            var uploadedHardwareRepository = ServiceLocator.GetService<ITypedDataRepository<UploadedHardwareDataStore>>();
            uploadedHardwareRepository.Should().BeOfType<UploadedHardwareRepository>();

            var apiService = ServiceLocator.GetService<IEquipmentApiService>();
            apiService.Should().BeOfType<MockEquipmentApiService>();
        }

        [Fact]
        public void ConfigureForTesting_RegistersMemoryTypedRepositories()
        {
            // Act
            ServiceLocator.ConfigureForTesting();

            // Assert
            var workflowRepository = ServiceLocator.GetService<ITypedDataRepository<IncompleteWorkflowDataStore>>();
            workflowRepository.Should().BeOfType<MemoryTypedRepository<IncompleteWorkflowDataStore>>();

            var uploadedHardwareRepository = ServiceLocator.GetService<ITypedDataRepository<UploadedHardwareDataStore>>();
            uploadedHardwareRepository.Should().BeOfType<MemoryTypedRepository<UploadedHardwareDataStore>>();

            var apiService = ServiceLocator.GetService<IEquipmentApiService>();
            apiService.Should().BeOfType<MockEquipmentApiService>();
        }

        #endregion
    }
}
