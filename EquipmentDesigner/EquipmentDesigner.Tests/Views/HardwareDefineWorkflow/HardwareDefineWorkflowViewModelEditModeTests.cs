using System;
using System.Collections.Generic;
using System.Linq;
using EquipmentDesigner.Models;
using EquipmentDesigner.ViewModels;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Views.HardwareDefineWorkflow
{
    /// <summary>
    /// Tests for EditModeSelectionDialog options: DirectEdit and CreateCopy.
    /// </summary>
    public class HardwareDefineWorkflowViewModelEditModeTests
    {
        #region RegenerateNodeIds Tests

        [Fact]
        public void RegenerateNodeIds_GeneratesNewGuidForTargetNode()
        {
            // Arrange
            var originalId = Guid.NewGuid().ToString();
            var nodeDto = new HardwareDefinition
            {
                Id = originalId,
                HardwareType = HardwareType.Equipment
            };

            // Act
            HardwareDefineWorkflowViewModel.RegenerateNodeIds(nodeDto);

            // Assert
            nodeDto.Id.Should().NotBe(originalId);
            Guid.TryParse(nodeDto.Id, out _).Should().BeTrue("Id should be a valid GUID");
        }

        [Fact]
        public void RegenerateNodeIds_RecursivelyGeneratesNewIdsForAllChildren()
        {
            // Arrange
            var rootId = "root-id";
            var childId = "child-id";
            var grandchildId = "grandchild-id";

            var nodeDto = new HardwareDefinition
            {
                Id = rootId,
                HardwareType = HardwareType.Equipment,
                Children = new List<HardwareDefinition>
                {
                    new HardwareDefinition
                    {
                        Id = childId,
                        HardwareType = HardwareType.System,
                        Children = new List<HardwareDefinition>
                        {
                            new HardwareDefinition
                            {
                                Id = grandchildId,
                                HardwareType = HardwareType.Unit
                            }
                        }
                    }
                }
            };

            // Act
            HardwareDefineWorkflowViewModel.RegenerateNodeIds(nodeDto);

            // Assert
            nodeDto.Id.Should().NotBe(rootId);
            nodeDto.Children[0].Id.Should().NotBe(childId);
            nodeDto.Children[0].Children[0].Id.Should().NotBe(grandchildId);
        }

        [Fact]
        public void RegenerateNodeIds_AllGeneratedIdsAreUnique()
        {
            // Arrange
            var nodeDto = new HardwareDefinition
            {
                Id = "id1",
                HardwareType = HardwareType.Equipment,
                Children = new List<HardwareDefinition>
                {
                    new HardwareDefinition { Id = "id2", HardwareType = HardwareType.System },
                    new HardwareDefinition { Id = "id3", HardwareType = HardwareType.System }
                }
            };

            // Act
            HardwareDefineWorkflowViewModel.RegenerateNodeIds(nodeDto);

            // Assert
            var allIds = new List<string> { nodeDto.Id, nodeDto.Children[0].Id, nodeDto.Children[1].Id };
            allIds.Should().OnlyHaveUniqueItems();
        }

        [Fact]
        public void RegenerateNodeIds_HandlesEmptyChildrenCollection()
        {
            // Arrange
            var nodeDto = new HardwareDefinition
            {
                Id = "original-id",
                HardwareType = HardwareType.Device,
                Children = new List<HardwareDefinition>()
            };

            // Act & Assert - should not throw
            HardwareDefineWorkflowViewModel.RegenerateNodeIds(nodeDto);
            nodeDto.Id.Should().NotBe("original-id");
        }

        #endregion

        #region ApplyCopySuffixToNode Tests

        [Fact]
        public void ApplyCopySuffixToNode_Equipment_AppliesCopySuffix()
        {
            // Arrange
            var nodeDto = new HardwareDefinition
            {
                HardwareType = HardwareType.Equipment,
                Name = "MyEquipment"
            };

            // Act
            HardwareDefineWorkflowViewModel.ApplyCopySuffixToNode(nodeDto);

            // Assert
            nodeDto.Name.Should().Be("MyEquipment - copy");
        }

        [Fact]
        public void ApplyCopySuffixToNode_System_AppliesCopySuffix()
        {
            // Arrange
            var nodeDto = new HardwareDefinition
            {
                HardwareType = HardwareType.System,
                Name = "MySystem"
            };

            // Act
            HardwareDefineWorkflowViewModel.ApplyCopySuffixToNode(nodeDto);

            // Assert
            nodeDto.Name.Should().Be("MySystem - copy");
        }

        [Fact]
        public void ApplyCopySuffixToNode_Unit_AppliesCopySuffix()
        {
            // Arrange
            var nodeDto = new HardwareDefinition
            {
                HardwareType = HardwareType.Unit,
                Name = "MyUnit"
            };

            // Act
            HardwareDefineWorkflowViewModel.ApplyCopySuffixToNode(nodeDto);

            // Assert
            nodeDto.Name.Should().Be("MyUnit - copy");
        }

        [Fact]
        public void ApplyCopySuffixToNode_Device_AppliesCopySuffix()
        {
            // Arrange
            var nodeDto = new HardwareDefinition
            {
                HardwareType = HardwareType.Device,
                Name = "MyDevice"
            };

            // Act
            HardwareDefineWorkflowViewModel.ApplyCopySuffixToNode(nodeDto);

            // Assert
            nodeDto.Name.Should().Be("MyDevice - copy");
        }

        [Fact]
        public void ApplyCopySuffixToNode_IncrementsCopySuffixWhenAlreadyHasSuffix()
        {
            // Arrange
            var nodeDto = new HardwareDefinition
            {
                HardwareType = HardwareType.Equipment,
                Name = "MyEquipment - copy"
            };

            // Act
            HardwareDefineWorkflowViewModel.ApplyCopySuffixToNode(nodeDto);

            // Assert
            nodeDto.Name.Should().Be("MyEquipment - copy 2");
        }

        [Fact]
        public void ApplyCopySuffixToNode_HandlesNullNameGracefully()
        {
            // Arrange
            var nodeDto = new HardwareDefinition
            {
                HardwareType = HardwareType.Equipment,
                Name = null
            };

            // Act & Assert - should not throw
            HardwareDefineWorkflowViewModel.ApplyCopySuffixToNode(nodeDto);
        }

        #endregion

        #region DirectEdit Behavior Tests

        [Fact]
        public void DirectEdit_SetsIsReadOnlyToFalse()
        {
            // Arrange
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            // Force read-only mode (simulating a loaded component)
            viewModel.GetType().GetProperty("IsReadOnly").SetValue(viewModel, true);
            viewModel.IsReadOnly.Should().BeTrue("Precondition: should start in read-only mode");

            // Act - Call EnableEditModeDirectly via reflection (private method)
            var enableEditMethod = viewModel.GetType().GetMethod("EnableEditModeDirectly", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            enableEditMethod.Invoke(viewModel, null);

            // Assert
            viewModel.IsReadOnly.Should().BeFalse();
        }

        [Fact]
        public void DirectEdit_PreservesWorkflowId()
        {
            // Arrange
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            var originalWorkflowId = viewModel.WorkflowId;

            // Act
            var enableEditMethod = viewModel.GetType().GetMethod("EnableEditModeDirectly", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            enableEditMethod.Invoke(viewModel, null);

            // Assert
            viewModel.WorkflowId.Should().Be(originalWorkflowId);
        }

        #endregion

        #region CreateCopy Behavior Tests

        [Fact]
        public void CreateCopySession_CreatesNewWorkflowId()
        {
            // Arrange
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            var originalWorkflowId = viewModel.WorkflowId;

            // Act
            var sessionDto = viewModel.ToHardwareDefinition();
            var newSessionDto = HardwareDefineWorkflowViewModel.CreateCopySession(sessionDto);

            // Assert
            newSessionDto.Id.Should().NotBe(originalWorkflowId);
            Guid.TryParse(newSessionDto.Id, out _).Should().BeTrue();
        }

        [Fact]
        public void CreateCopySession_SetsDraftState()
        {
            // Arrange
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            var sessionDto = viewModel.ToHardwareDefinition();
            sessionDto.State = ComponentState.Uploaded;

            // Act
            var newSessionDto = HardwareDefineWorkflowViewModel.CreateCopySession(sessionDto);

            // Assert
            newSessionDto.State.Should().Be(ComponentState.Draft);
        }

        [Fact]
        public void CreateCopySession_RegeneratesAllTreeNodeIds()
        {
            // Arrange
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            var sessionDto = viewModel.ToHardwareDefinition();
            var originalRootNodeId = sessionDto.Id;

            // Act
            var newSessionDto = HardwareDefineWorkflowViewModel.CreateCopySession(sessionDto);

            // Assert
            newSessionDto.Id.Should().NotBe(originalRootNodeId);
        }

        [Fact]
        public void CreateCopySession_AppliesCopySuffixToRootNodeName()
        {
            // Arrange
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            
            // Set the equipment name
            if (viewModel.TreeRootNodes.FirstOrDefault()?.DataViewModel is EquipmentDefineViewModel equipmentVm)
            {
                equipmentVm.Name = "TestEquipment";
            }
            
            var sessionDto = viewModel.ToHardwareDefinition();

            // Act
            var newSessionDto = HardwareDefineWorkflowViewModel.CreateCopySession(sessionDto);

            // Assert
            newSessionDto.Name.Should().Be("TestEquipment - copy");
        }

        #endregion
    }
}