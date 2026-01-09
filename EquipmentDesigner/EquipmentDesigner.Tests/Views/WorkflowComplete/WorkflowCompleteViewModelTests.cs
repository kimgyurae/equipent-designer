using System;
using System.Collections.Generic;
using System.Linq;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models;
using EquipmentDesigner.ViewModels;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Views.WorkflowComplete
{
    public class WorkflowCompleteViewModelTests
    {
        #region Test Helpers

        private WorkflowSessionDto CreateValidSessionDto()
        {
            return new WorkflowSessionDto
            {
                WorkflowId = "test-workflow-id",
                HardwareType = HardwareLayer.Equipment,
                State = ComponentState.Draft,
                HardwareLayer = HardwareLayer.Equipment,
                LastModifiedAt = DateTime.Now,
                TreeNodes = new List<TreeNodeDataDto>
                {
                    new TreeNodeDataDto
                    {
                        NodeId = "equipment-1",
                        HardwareLayer = HardwareLayer.Equipment,
                        EquipmentData = new EquipmentDto { Name = "Test Equipment", Description = "Equipment Description" },
                        Children = new List<TreeNodeDataDto>
                        {
                            new TreeNodeDataDto
                            {
                                NodeId = "system-1",
                                HardwareLayer = HardwareLayer.System,
                                SystemData = new SystemDto { Name = "Test System", Description = "System Description" },
                                Children = new List<TreeNodeDataDto>
                                {
                                    new TreeNodeDataDto
                                    {
                                        NodeId = "unit-1",
                                        HardwareLayer = HardwareLayer.Unit,
                                        UnitData = new UnitDto { Name = "Test Unit", Description = "Unit Description" },
                                        Children = new List<TreeNodeDataDto>
                                        {
                                            new TreeNodeDataDto
                                            {
                                                NodeId = "device-1",
                                                HardwareLayer = HardwareLayer.Device,
                                                DeviceData = new DeviceDto { Name = "Test Device", Description = "Device Description" },
                                                Children = new List<TreeNodeDataDto>()
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        #endregion

        #region Initialization and State Management

        [Fact]
        public void Constructor_WithValidSessionDto_StoresSessionData()
        {
            // Arrange
            var sessionDto = CreateValidSessionDto();

            // Act
            var viewModel = new WorkflowCompleteViewModel(sessionDto);

            // Assert
            viewModel.WorkflowId.Should().Be("test-workflow-id");
        }

        [Fact]
        public void Constructor_WithSessionDto_SetsStateToReady()
        {
            // Arrange
            var sessionDto = CreateValidSessionDto();
            sessionDto.State = ComponentState.Draft;

            // Act
            var viewModel = new WorkflowCompleteViewModel(sessionDto);

            // Assert
            viewModel.SessionState.Should().Be(ComponentState.Ready);
        }

        [Fact]
        public void Constructor_WithSessionDto_ExposesWorkflowId()
        {
            // Arrange
            var sessionDto = CreateValidSessionDto();
            sessionDto.WorkflowId = "unique-workflow-123";

            // Act
            var viewModel = new WorkflowCompleteViewModel(sessionDto);

            // Assert
            viewModel.WorkflowId.Should().Be("unique-workflow-123");
        }

        [Fact]
        public void Constructor_WithSessionDto_ExposesStartType()
        {
            // Arrange
            var sessionDto = CreateValidSessionDto();
            sessionDto.HardwareType = HardwareLayer.System;

            // Act
            var viewModel = new WorkflowCompleteViewModel(sessionDto);

            // Assert
            viewModel.StartType.Should().Be(HardwareLayer.System);
        }

        [Fact]
        public void Constructor_WithNullSessionDto_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => new WorkflowCompleteViewModel(null);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("sessionDto");
        }

        [Fact]
        public void Constructor_WithNullTreeNodes_InitializesEmptyHierarchy()
        {
            // Arrange
            var sessionDto = CreateValidSessionDto();
            sessionDto.TreeNodes = null;

            // Act
            var viewModel = new WorkflowCompleteViewModel(sessionDto);

            // Assert
            viewModel.TreeNodes.Should().NotBeNull();
            viewModel.TreeNodes.Should().BeEmpty();
        }

        #endregion

        #region Hierarchical Data Exposure

        [Fact]
        public void TreeNodes_WithValidSession_PopulatesFromSessionTreeNodes()
        {
            // Arrange
            var sessionDto = CreateValidSessionDto();

            // Act
            var viewModel = new WorkflowCompleteViewModel(sessionDto);

            // Assert
            viewModel.TreeNodes.Should().NotBeNull();
            viewModel.TreeNodes.Should().HaveCount(1);
        }

        [Fact]
        public void TreeNodes_WithEquipmentRoot_ExposesEquipmentAtRootLevel()
        {
            // Arrange
            var sessionDto = CreateValidSessionDto();

            // Act
            var viewModel = new WorkflowCompleteViewModel(sessionDto);

            // Assert
            viewModel.TreeNodes.First().HardwareLayer.Should().Be(HardwareLayer.Equipment);
        }

        [Fact]
        public void TreeNodes_WithNestedHierarchy_SystemNodesNestedUnderEquipment()
        {
            // Arrange
            var sessionDto = CreateValidSessionDto();

            // Act
            var viewModel = new WorkflowCompleteViewModel(sessionDto);

            // Assert
            var equipmentNode = viewModel.TreeNodes.First();
            equipmentNode.Children.Should().HaveCount(1);
            equipmentNode.Children.First().HardwareLayer.Should().Be(HardwareLayer.System);
        }

        [Fact]
        public void TreeNodes_WithNestedHierarchy_UnitNodesNestedUnderSystem()
        {
            // Arrange
            var sessionDto = CreateValidSessionDto();

            // Act
            var viewModel = new WorkflowCompleteViewModel(sessionDto);

            // Assert
            var systemNode = viewModel.TreeNodes.First().Children.First();
            systemNode.Children.Should().HaveCount(1);
            systemNode.Children.First().HardwareLayer.Should().Be(HardwareLayer.Unit);
        }

        [Fact]
        public void TreeNodes_WithNestedHierarchy_DeviceNodesNestedUnderUnit()
        {
            // Arrange
            var sessionDto = CreateValidSessionDto();

            // Act
            var viewModel = new WorkflowCompleteViewModel(sessionDto);

            // Assert
            var unitNode = viewModel.TreeNodes.First().Children.First().Children.First();
            unitNode.Children.Should().HaveCount(1);
            unitNode.Children.First().HardwareLayer.Should().Be(HardwareLayer.Device);
        }

        [Fact]
        public void TreeNodes_WithEmptyTreeNodes_ReturnsEmptyCollection()
        {
            // Arrange
            var sessionDto = CreateValidSessionDto();
            sessionDto.TreeNodes = new List<TreeNodeDataDto>();

            // Act
            var viewModel = new WorkflowCompleteViewModel(sessionDto);

            // Assert
            viewModel.TreeNodes.Should().BeEmpty();
        }

        [Fact]
        public void TreeNodes_EachNodeExposesName()
        {
            // Arrange
            var sessionDto = CreateValidSessionDto();

            // Act
            var viewModel = new WorkflowCompleteViewModel(sessionDto);

            // Assert
            viewModel.TreeNodes.First().Name.Should().Be("Test Equipment");
        }

        [Fact]
        public void TreeNodes_EachNodeExposesDescription()
        {
            // Arrange
            var sessionDto = CreateValidSessionDto();

            // Act
            var viewModel = new WorkflowCompleteViewModel(sessionDto);

            // Assert
            viewModel.TreeNodes.First().Description.Should().Be("Equipment Description");
        }

        [Fact]
        public void TreeNodes_EachNodeExposesHardwareLayer()
        {
            // Arrange
            var sessionDto = CreateValidSessionDto();

            // Act
            var viewModel = new WorkflowCompleteViewModel(sessionDto);

            // Assert
            viewModel.TreeNodes.First().HardwareLayer.Should().Be(HardwareLayer.Equipment);
        }

        #endregion

        #region Upload Command

        [Fact]
        public void UploadCommand_OnInitialization_IsNotNull()
        {
            // Arrange
            var sessionDto = CreateValidSessionDto();

            // Act
            var viewModel = new WorkflowCompleteViewModel(sessionDto);

            // Assert
            viewModel.UploadCommand.Should().NotBeNull();
        }

        [Fact]
        public void UploadCommand_CanExecute_WithValidSession_ReturnsTrue()
        {
            // Arrange
            var sessionDto = CreateValidSessionDto();
            var viewModel = new WorkflowCompleteViewModel(sessionDto);

            // Act
            var canExecute = viewModel.UploadCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeTrue();
        }

        #endregion

        #region Continue Editing Command

        [Fact]
        public void ContinueEditingCommand_OnInitialization_IsNotNull()
        {
            // Arrange
            var sessionDto = CreateValidSessionDto();

            // Act
            var viewModel = new WorkflowCompleteViewModel(sessionDto);

            // Assert
            viewModel.ContinueEditingCommand.Should().NotBeNull();
        }

        [Fact]
        public void ContinueEditingCommand_CanExecute_ReturnsTrue()
        {
            // Arrange
            var sessionDto = CreateValidSessionDto();
            var viewModel = new WorkflowCompleteViewModel(sessionDto);

            // Act
            var canExecute = viewModel.ContinueEditingCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeTrue();
        }

        #endregion

        #region Upload Later Command

        [Fact]
        public void UploadLaterCommand_OnInitialization_IsNotNull()
        {
            // Arrange
            var sessionDto = CreateValidSessionDto();

            // Act
            var viewModel = new WorkflowCompleteViewModel(sessionDto);

            // Assert
            viewModel.UploadLaterCommand.Should().NotBeNull();
        }

        [Fact]
        public void UploadLaterCommand_CanExecute_ReturnsTrue()
        {
            // Arrange
            var sessionDto = CreateValidSessionDto();
            var viewModel = new WorkflowCompleteViewModel(sessionDto);

            // Act
            var canExecute = viewModel.UploadLaterCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeTrue();
        }

        #endregion

        #region Card Click Command

        [Fact]
        public void CardClickCommand_OnInitialization_IsNotNull()
        {
            // Arrange
            var sessionDto = CreateValidSessionDto();

            // Act
            var viewModel = new WorkflowCompleteViewModel(sessionDto);

            // Assert
            viewModel.CardClickCommand.Should().NotBeNull();
        }

        #endregion

        #region UI Text Properties

        [Fact]
        public void Title_ReturnsExpectedValue()
        {
            // Arrange
            var sessionDto = CreateValidSessionDto();

            // Act
            var viewModel = new WorkflowCompleteViewModel(sessionDto);

            // Assert
            viewModel.Title.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Subtitle_ReturnsExpectedValue()
        {
            // Arrange
            var sessionDto = CreateValidSessionDto();

            // Act
            var viewModel = new WorkflowCompleteViewModel(sessionDto);

            // Assert
            viewModel.Subtitle.Should().NotBeNullOrEmpty();
        }

        #endregion
    }
}
