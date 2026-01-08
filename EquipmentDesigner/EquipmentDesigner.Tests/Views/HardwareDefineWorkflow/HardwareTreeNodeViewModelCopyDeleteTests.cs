using System.Collections.Generic;
using System.Linq;
using EquipmentDesigner.Models;
using EquipmentDesigner.ViewModels;
using EquipmentDesigner.ViewModels;
using EquipmentDesigner.ViewModels;
using EquipmentDesigner.ViewModels;
using EquipmentDesigner.ViewModels;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Views.HardwareDefineWorkflow
{
    public class HardwareTreeNodeViewModelCopyDeleteTests
    {
        #region GetAllDescendants Tests

        [Fact]
        public void GetAllDescendants_WhenNodeHasNoChildren_ReturnsEmptyList()
        {
            // Arrange
            var node = new HardwareTreeNodeViewModel(HardwareLayer.Device);

            // Act
            var descendants = node.GetAllDescendants();

            // Assert
            descendants.Should().BeEmpty();
        }

        [Fact]
        public void GetAllDescendants_WhenNodeHasDirectChildrenOnly_ReturnsDirectChildren()
        {
            // Arrange
            var parent = new HardwareTreeNodeViewModel(HardwareLayer.Unit);
            var child1 = new HardwareTreeNodeViewModel(HardwareLayer.Device, parent);
            var child2 = new HardwareTreeNodeViewModel(HardwareLayer.Device, parent);
            parent.Children.Add(child1);
            parent.Children.Add(child2);

            // Act
            var descendants = parent.GetAllDescendants();

            // Assert
            descendants.Should().HaveCount(2);
            descendants.Should().Contain(child1);
            descendants.Should().Contain(child2);
        }

        [Fact]
        public void GetAllDescendants_WhenNodeHasNestedChildren_ReturnsFlatListOfAllDescendants()
        {
            // Arrange
            var equipment = new HardwareTreeNodeViewModel(HardwareLayer.Equipment);
            var system = new HardwareTreeNodeViewModel(HardwareLayer.System, equipment);
            var unit = new HardwareTreeNodeViewModel(HardwareLayer.Unit, system);
            var device = new HardwareTreeNodeViewModel(HardwareLayer.Device, unit);

            equipment.Children.Add(system);
            system.Children.Add(unit);
            unit.Children.Add(device);

            // Act
            var descendants = equipment.GetAllDescendants();

            // Assert
            descendants.Should().HaveCount(3);
            descendants.Should().Contain(system);
            descendants.Should().Contain(unit);
            descendants.Should().Contain(device);
        }

        [Fact]
        public void GetAllDescendants_DoesNotIncludeTheNodeItself_OnlyDescendants()
        {
            // Arrange
            var parent = new HardwareTreeNodeViewModel(HardwareLayer.Equipment);
            var child = new HardwareTreeNodeViewModel(HardwareLayer.System, parent);
            parent.Children.Add(child);

            // Act
            var descendants = parent.GetAllDescendants();

            // Assert
            descendants.Should().NotContain(parent);
            descendants.Should().Contain(child);
        }

        #endregion

        #region GenerateCopyName Tests

        [Fact]
        public void GenerateCopyName_WhenOriginalName_AppendsCopySuffix()
        {
            // Act
            var result = HardwareTreeNodeViewModel.GenerateCopyName("MyEquipment");

            // Assert
            result.Should().Be("MyEquipment - copy");
        }

        [Fact]
        public void GenerateCopyName_WhenNameAlreadyHasCopySuffix_AppendsCopy2()
        {
            // Act
            var result = HardwareTreeNodeViewModel.GenerateCopyName("MyEquipment - copy");

            // Assert
            result.Should().Be("MyEquipment - copy 2");
        }

        [Fact]
        public void GenerateCopyName_WhenNameHasCopy2Suffix_IncrementsToCopy3()
        {
            // Act
            var result = HardwareTreeNodeViewModel.GenerateCopyName("MyEquipment - copy 2");

            // Assert
            result.Should().Be("MyEquipment - copy 3");
        }

        [Fact]
        public void GenerateCopyName_WhenNameHasCopyNSuffix_IncrementsToNextNumber()
        {
            // Act
            var result = HardwareTreeNodeViewModel.GenerateCopyName("MyEquipment - copy 99");

            // Assert
            result.Should().Be("MyEquipment - copy 100");
        }

        [Fact]
        public void GenerateCopyName_WhenNameIsEmpty_ReturnsCopy()
        {
            // Act
            var result = HardwareTreeNodeViewModel.GenerateCopyName("");

            // Assert
            result.Should().Be("copy");
        }

        [Fact]
        public void GenerateCopyName_WhenNameIsNull_ReturnsCopy()
        {
            // Act
            var result = HardwareTreeNodeViewModel.GenerateCopyName(null);

            // Assert
            result.Should().Be("copy");
        }

        #endregion

        #region DeepCopy Tests

        [Fact]
        public void DeepCopy_CreatesNewNodeWithSameHardwareLayer()
        {
            // Arrange
            var original = new HardwareTreeNodeViewModel(HardwareLayer.System);

            // Act
            var copy = original.DeepCopy(null);

            // Assert
            copy.HardwareLayer.Should().Be(HardwareLayer.System);
        }

        [Fact]
        public void DeepCopy_GeneratesNewUniqueNodeId()
        {
            // Arrange
            var original = new HardwareTreeNodeViewModel(HardwareLayer.System);

            // Act
            var copy = original.DeepCopy(null);

            // Assert
            copy.NodeId.Should().NotBe(original.NodeId);
        }

        [Fact]
        public void DeepCopy_SetsCorrectParentReference()
        {
            // Arrange
            var parent = new HardwareTreeNodeViewModel(HardwareLayer.Equipment);
            var original = new HardwareTreeNodeViewModel(HardwareLayer.System, parent);

            // Act
            var copy = original.DeepCopy(parent);

            // Assert
            copy.Parent.Should().Be(parent);
        }

        [Fact]
        public void DeepCopy_AppliesCopySuffixToDataViewModelName()
        {
            // Arrange
            var original = new HardwareTreeNodeViewModel(HardwareLayer.System);
            original.DataViewModel.Name = "TestSystem";

            // Act
            var copy = original.DeepCopy(null);

            // Assert
            copy.DataViewModel.Name.Should().Be("TestSystem - copy");
        }

        [Fact]
        public void DeepCopy_WhenNodeHasNoChildren_ReturnsNodeWithEmptyChildren()
        {
            // Arrange
            var original = new HardwareTreeNodeViewModel(HardwareLayer.Device);

            // Act
            var copy = original.DeepCopy(null);

            // Assert
            copy.Children.Should().BeEmpty();
        }

        [Fact]
        public void DeepCopy_WhenNodeHasChildren_RecursivelyCopiesAllChildren()
        {
            // Arrange
            var parent = new HardwareTreeNodeViewModel(HardwareLayer.Unit);
            var child1 = new HardwareTreeNodeViewModel(HardwareLayer.Device, parent);
            var child2 = new HardwareTreeNodeViewModel(HardwareLayer.Device, parent);
            parent.Children.Add(child1);
            parent.Children.Add(child2);

            // Act
            var copy = parent.DeepCopy(null);

            // Assert
            copy.Children.Should().HaveCount(2);
            copy.Children.Should().NotContain(child1);
            copy.Children.Should().NotContain(child2);
        }

        [Fact]
        public void DeepCopy_CopiedChildrenHaveCorrectParentReferences()
        {
            // Arrange
            var parent = new HardwareTreeNodeViewModel(HardwareLayer.Unit);
            var child = new HardwareTreeNodeViewModel(HardwareLayer.Device, parent);
            parent.Children.Add(child);

            // Act
            var copiedParent = parent.DeepCopy(null);

            // Assert
            copiedParent.Children.First().Parent.Should().Be(copiedParent);
        }

        [Fact]
        public void DeepCopy_AllDescendantsHaveNewUniqueNodeIds()
        {
            // Arrange
            var equipment = new HardwareTreeNodeViewModel(HardwareLayer.Equipment);
            var system = new HardwareTreeNodeViewModel(HardwareLayer.System, equipment);
            var unit = new HardwareTreeNodeViewModel(HardwareLayer.Unit, system);
            equipment.Children.Add(system);
            system.Children.Add(unit);

            var originalIds = new HashSet<string> { equipment.NodeId, system.NodeId, unit.NodeId };

            // Act
            var copy = equipment.DeepCopy(null);
            var copyDescendants = copy.GetAllDescendants();
            var copyIds = new HashSet<string> { copy.NodeId };
            foreach (var desc in copyDescendants)
            {
                copyIds.Add(desc.NodeId);
            }

            // Assert
            copyIds.Should().NotIntersectWith(originalIds);
        }

        #endregion
    }
}
