using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using EquipmentDesigner.Services.Storage;
using EquipmentDesigner.Models.Storage;

namespace EquipmentDesigner.Tests.Services.Storage
{
    /// <summary>
    /// IDataRepository 인터페이스 정의 테스트
    /// 인터페이스가 올바른 메서드와 속성을 정의하는지 확인
    /// </summary>
    public class IDataRepositoryInterfaceTests
    {
        [Fact]
        public void IDataRepository_DefinesLoadAsyncMethod()
        {
            // Arrange & Act - Interface method exists
            var methodInfo = typeof(IDataRepository).GetMethod("LoadAsync");

            // Assert
            methodInfo.Should().NotBeNull();
            methodInfo.ReturnType.Should().Be(typeof(Task<SharedMemoryDataStore>));
        }

        [Fact]
        public void IDataRepository_DefinesSaveAsyncMethod()
        {
            // Arrange & Act
            var methodInfo = typeof(IDataRepository).GetMethod("SaveAsync");

            // Assert
            methodInfo.Should().NotBeNull();
            methodInfo.ReturnType.Should().Be(typeof(Task));
            methodInfo.GetParameters().Should().HaveCount(1);
            methodInfo.GetParameters()[0].ParameterType.Should().Be(typeof(SharedMemoryDataStore));
        }

        [Fact]
        public void IDataRepository_DefinesEnableAutoSaveMethod()
        {
            // Arrange & Act
            var methodInfo = typeof(IDataRepository).GetMethod("EnableAutoSave");

            // Assert
            methodInfo.Should().NotBeNull();
            methodInfo.GetParameters().Should().HaveCount(1);
            methodInfo.GetParameters()[0].ParameterType.Should().Be(typeof(TimeSpan));
        }

        [Fact]
        public void IDataRepository_DefinesDisableAutoSaveMethod()
        {
            // Arrange & Act
            var methodInfo = typeof(IDataRepository).GetMethod("DisableAutoSave");

            // Assert
            methodInfo.Should().NotBeNull();
        }

        [Fact]
        public void IDataRepository_DefinesMarkDirtyMethod()
        {
            // Arrange & Act
            var methodInfo = typeof(IDataRepository).GetMethod("MarkDirty");

            // Assert
            methodInfo.Should().NotBeNull();
        }

        [Fact]
        public void IDataRepository_DefinesIsDirtyProperty()
        {
            // Arrange & Act
            var propertyInfo = typeof(IDataRepository).GetProperty("IsDirty");

            // Assert
            propertyInfo.Should().NotBeNull();
            propertyInfo.PropertyType.Should().Be(typeof(bool));
        }
    }
}
