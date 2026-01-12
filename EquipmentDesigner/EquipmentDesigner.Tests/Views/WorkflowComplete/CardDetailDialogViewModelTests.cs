using System;
using System.Collections.Generic;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models;
using EquipmentDesigner.ViewModels;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Views.WorkflowComplete
{
    public class CardDetailDialogViewModelTests
    {
        #region Test Helpers

        private TreeNodeDataDto CreateEquipmentNodeDto()
        {
            return new TreeNodeDataDto
            {
                Id = "equipment-1",
                HardwareType = HardwareType.Equipment,
                EquipmentData = new EquipmentDto
                {
                    Id = "eq-001",
                    Name = "Test Equipment",
                    DisplayName = "Primary Reactor A1",
                    Description = "Main production equipment",
                    EquipmentType = "Reactor",
                    Customer = "ACME Corporation",
                    Process = "Chemical Processing",
                    AttachedDocuments = new List<string>
                    {
                        "Design_Specification_v2.pdf",
                        "Installation_Manual.pdf"
                    }
                }
            };
        }

        private TreeNodeDataDto CreateSystemNodeDto()
        {
            return new TreeNodeDataDto
            {
                Id = "system-1",
                HardwareType = HardwareType.System,
                SystemData = new SystemDto
                {
                    Id = "sys-001",
                    Name = "Test System",
                    DisplayName = "Control System",
                    Description = "Main control system",
                    ProcessInfo = "Temperature Control Process",
                    Commands = new List<CommandDto>
                    {
                        new CommandDto { Name = "Start", Description = "Start the system" }
                    }
                }
            };
        }

        private TreeNodeDataDto CreateUnitNodeDto()
        {
            return new TreeNodeDataDto
            {
                Id = "unit-1",
                HardwareType = HardwareType.Unit,
                UnitData = new UnitDto
                {
                    Id = "unit-001",
                    Name = "Test Unit",
                    DisplayName = "Processing Unit",
                    Description = "Main processing unit",
                    ProcessInfo = "Batch Processing",
                    Commands = new List<CommandDto>
                    {
                        new CommandDto { Name = "Initialize", Description = "Initialize unit" }
                    }
                }
            };
        }

        private TreeNodeDataDto CreateDeviceNodeDto()
        {
            return new TreeNodeDataDto
            {
                Id = "device-1",
                HardwareType = HardwareType.Device,
                DeviceData = new DeviceDto
                {
                    Id = "dev-001",
                    Name = "Test Device",
                    DisplayName = "Temperature Sensor",
                    Description = "Main temperature sensor",
                    DeviceType = "Sensor",
                    IoInfo = new List<IoInfoDto>
                    {
                        new IoInfoDto { Name = "TempInput", IoType = "AnalogInput", Address = "0x0010" }
                    },
                    Commands = new List<CommandDto>
                    {
                        new CommandDto { Name = "Calibrate", Description = "Calibrate sensor" }
                    }
                }
            };
        }

        #endregion

        #region Initialization and Constructor

        [Fact]
        public void Constructor_WithTreeNodeDataDto_StoresDataCorrectly()
        {
            // Arrange
            var nodeDto = CreateEquipmentNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.HardwareType.Should().Be(HardwareType.Equipment);
        }

        [Fact]
        public void Constructor_WithNullTreeNodeDataDto_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => new CardDetailDialogViewModel(null);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("treeNodeData");
        }

        [Fact]
        public void Constructor_ExtractsHardwareTypeFromTreeNodeDataDto()
        {
            // Arrange
            var nodeDto = CreateSystemNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.HardwareType.Should().Be(HardwareType.System);
        }

        [Fact]
        public void ParameterlessConstructor_InitializesEmptyStateForDesignTime()
        {
            // Act
            var viewModel = new CardDetailDialogViewModel();

            // Assert
            viewModel.Name.Should().BeEmpty();
            viewModel.HardwareType.Should().Be(HardwareType.Equipment);
        }

        #endregion

        #region Hardware Layer Detection

        [Fact]
        public void HardwareType_ReturnsEquipment_WhenTreeNodeDataDtoHasEquipmentLayer()
        {
            // Arrange
            var nodeDto = CreateEquipmentNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.HardwareType.Should().Be(HardwareType.Equipment);
        }

        [Fact]
        public void HardwareType_ReturnsSystem_WhenTreeNodeDataDtoHasSystemLayer()
        {
            // Arrange
            var nodeDto = CreateSystemNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.HardwareType.Should().Be(HardwareType.System);
        }

        [Fact]
        public void HardwareType_ReturnsUnit_WhenTreeNodeDataDtoHasUnitLayer()
        {
            // Arrange
            var nodeDto = CreateUnitNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.HardwareType.Should().Be(HardwareType.Unit);
        }

        [Fact]
        public void HardwareType_ReturnsDevice_WhenTreeNodeDataDtoHasDeviceLayer()
        {
            // Arrange
            var nodeDto = CreateDeviceNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.HardwareType.Should().Be(HardwareType.Device);
        }

        #endregion

        #region Common Properties Extraction

        [Fact]
        public void Name_ExtractsFromEquipmentData_WhenLayerIsEquipment()
        {
            // Arrange
            var nodeDto = CreateEquipmentNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.Name.Should().Be("Test Equipment");
        }

        [Fact]
        public void Name_ExtractsFromSystemData_WhenLayerIsSystem()
        {
            // Arrange
            var nodeDto = CreateSystemNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.Name.Should().Be("Test System");
        }

        [Fact]
        public void Name_ExtractsFromUnitData_WhenLayerIsUnit()
        {
            // Arrange
            var nodeDto = CreateUnitNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.Name.Should().Be("Test Unit");
        }

        [Fact]
        public void Name_ExtractsFromDeviceData_WhenLayerIsDevice()
        {
            // Arrange
            var nodeDto = CreateDeviceNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.Name.Should().Be("Test Device");
        }

        [Fact]
        public void DisplayName_ExtractsFromAppropriateLayerDto()
        {
            // Arrange
            var nodeDto = CreateEquipmentNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.DisplayName.Should().Be("Primary Reactor A1");
        }

        [Fact]
        public void Description_ExtractsFromAppropriateLayerDto()
        {
            // Arrange
            var nodeDto = CreateEquipmentNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.Description.Should().Be("Main production equipment");
        }

        [Fact]
        public void Properties_ReturnEmptyString_WhenSourceDataIsNull()
        {
            // Arrange
            var nodeDto = new TreeNodeDataDto
            {
                Id = "empty-1",
                HardwareType = HardwareType.Equipment,
                EquipmentData = null
            };

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.Name.Should().BeEmpty();
            viewModel.DisplayName.Should().BeEmpty();
            viewModel.Description.Should().BeEmpty();
        }

        #endregion

        #region Equipment-Specific Properties

        [Fact]
        public void EquipmentType_ReturnsValueFromEquipmentData()
        {
            // Arrange
            var nodeDto = CreateEquipmentNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.EquipmentType.Should().Be("Reactor");
        }

        [Fact]
        public void Customer_ReturnsValueFromEquipmentData()
        {
            // Arrange
            var nodeDto = CreateEquipmentNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.Customer.Should().Be("ACME Corporation");
        }

        [Fact]
        public void Process_ReturnsValueFromEquipmentData()
        {
            // Arrange
            var nodeDto = CreateEquipmentNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.Process.Should().Be("Chemical Processing");
        }

        [Fact]
        public void AttachedDocuments_ReturnsListFromEquipmentData()
        {
            // Arrange
            var nodeDto = CreateEquipmentNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.AttachedDocuments.Should().HaveCount(2);
            viewModel.AttachedDocuments.Should().Contain("Design_Specification_v2.pdf");
        }

        [Fact]
        public void AttachedDocuments_ReturnsEmptyList_WhenNoDocumentsAttached()
        {
            // Arrange
            var nodeDto = CreateEquipmentNodeDto();
            nodeDto.EquipmentData.AttachedDocuments = null;

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.AttachedDocuments.Should().BeEmpty();
        }

        [Fact]
        public void EquipmentSpecificProperties_ReturnNullOrEmpty_ForNonEquipmentLayers()
        {
            // Arrange
            var nodeDto = CreateSystemNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.EquipmentType.Should().BeEmpty();
            viewModel.Customer.Should().BeEmpty();
            viewModel.AttachedDocuments.Should().BeEmpty();
        }

        #endregion

        #region System-Specific Properties

        [Fact]
        public void ProcessInfo_ReturnsValueFromSystemData_WhenLayerIsSystem()
        {
            // Arrange
            var nodeDto = CreateSystemNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.ProcessInfo.Should().Be("Temperature Control Process");
        }

        [Fact]
        public void Commands_ReturnsListFromSystemData_WhenLayerIsSystem()
        {
            // Arrange
            var nodeDto = CreateSystemNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.Commands.Should().HaveCount(1);
            viewModel.Commands[0].Name.Should().Be("Start");
        }

        [Fact]
        public void SystemSpecificProperties_ReturnNullOrEmpty_ForNonSystemLayers()
        {
            // Arrange
            var nodeDto = CreateEquipmentNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.ProcessInfo.Should().BeEmpty();
        }

        #endregion

        #region Unit-Specific Properties

        [Fact]
        public void ProcessInfo_ReturnsValueFromUnitData_WhenLayerIsUnit()
        {
            // Arrange
            var nodeDto = CreateUnitNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.ProcessInfo.Should().Be("Batch Processing");
        }

        [Fact]
        public void Commands_ReturnsListFromUnitData_WhenLayerIsUnit()
        {
            // Arrange
            var nodeDto = CreateUnitNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.Commands.Should().HaveCount(1);
            viewModel.Commands[0].Name.Should().Be("Initialize");
        }

        #endregion

        #region Device-Specific Properties

        [Fact]
        public void DeviceType_ReturnsValueFromDeviceData_WhenLayerIsDevice()
        {
            // Arrange
            var nodeDto = CreateDeviceNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.DeviceType.Should().Be("Sensor");
        }

        [Fact]
        public void IoInfo_ReturnsListFromDeviceData_WhenLayerIsDevice()
        {
            // Arrange
            var nodeDto = CreateDeviceNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.IoInfo.Should().HaveCount(1);
            viewModel.IoInfo[0].Name.Should().Be("TempInput");
        }

        [Fact]
        public void IoInfo_ReturnsEmptyList_WhenNoIoDefined()
        {
            // Arrange
            var nodeDto = CreateDeviceNodeDto();
            nodeDto.DeviceData.IoInfo = null;

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.IoInfo.Should().BeEmpty();
        }

        [Fact]
        public void Commands_ReturnsListFromDeviceData_WhenLayerIsDevice()
        {
            // Arrange
            var nodeDto = CreateDeviceNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.Commands.Should().HaveCount(1);
            viewModel.Commands[0].Name.Should().Be("Calibrate");
        }

        [Fact]
        public void DeviceSpecificProperties_ReturnNullOrEmpty_ForNonDeviceLayers()
        {
            // Arrange
            var nodeDto = CreateSystemNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.DeviceType.Should().BeEmpty();
            viewModel.IoInfo.Should().BeEmpty();
        }

        #endregion

        #region Visibility Helpers

        [Fact]
        public void HasEquipmentType_ReturnsTrue_OnlyForEquipmentLayerWithNonEmptyType()
        {
            // Arrange
            var nodeDto = CreateEquipmentNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.HasEquipmentType.Should().BeTrue();
        }

        [Fact]
        public void HasEquipmentType_ReturnsFalse_ForNonEquipmentLayer()
        {
            // Arrange
            var nodeDto = CreateSystemNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.HasEquipmentType.Should().BeFalse();
        }

        [Fact]
        public void HasCustomer_ReturnsTrue_OnlyForEquipmentLayerWithNonEmptyCustomer()
        {
            // Arrange
            var nodeDto = CreateEquipmentNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.HasCustomer.Should().BeTrue();
        }

        [Fact]
        public void HasProcess_ReturnsTrue_WhenProcessInfoOrProcessIsNonEmpty()
        {
            // Arrange - Equipment with Process
            var equipmentDto = CreateEquipmentNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(equipmentDto);

            // Assert
            viewModel.HasProcess.Should().BeTrue();
        }

        [Fact]
        public void HasAttachedDocuments_ReturnsTrue_OnlyWhenDocumentsListIsNotEmpty()
        {
            // Arrange
            var nodeDto = CreateEquipmentNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.HasAttachedDocuments.Should().BeTrue();
        }

        [Fact]
        public void HasAttachedDocuments_ReturnsFalse_WhenDocumentsListIsEmpty()
        {
            // Arrange
            var nodeDto = CreateEquipmentNodeDto();
            nodeDto.EquipmentData.AttachedDocuments = new List<string>();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.HasAttachedDocuments.Should().BeFalse();
        }

        [Fact]
        public void HasCommands_ReturnsTrue_OnlyWhenCommandsListIsNotEmpty()
        {
            // Arrange
            var nodeDto = CreateSystemNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.HasCommands.Should().BeTrue();
        }

        [Fact]
        public void HasIoInfo_ReturnsTrue_OnlyForDeviceLayerWithNonEmptyIoList()
        {
            // Arrange
            var nodeDto = CreateDeviceNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.HasIoInfo.Should().BeTrue();
        }

        [Fact]
        public void HasDeviceType_ReturnsTrue_OnlyForDeviceLayerWithNonEmptyType()
        {
            // Arrange
            var nodeDto = CreateDeviceNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.HasDeviceType.Should().BeTrue();
        }

        #endregion

        #region Close Command

        [Fact]
        public void CloseCommand_IsInitializedAndNotNull()
        {
            // Arrange
            var nodeDto = CreateEquipmentNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.CloseCommand.Should().NotBeNull();
        }

        [Fact]
        public void CloseCommand_CanExecute_ReturnsTrue()
        {
            // Arrange
            var nodeDto = CreateEquipmentNodeDto();
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Act
            var canExecute = viewModel.CloseCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeTrue();
        }

        [Fact]
        public void CloseCommand_RaisesRequestCloseEvent_WhenExecuted()
        {
            // Arrange
            var nodeDto = CreateEquipmentNodeDto();
            var viewModel = new CardDetailDialogViewModel(nodeDto);
            var eventRaised = false;
            viewModel.RequestClose += (s, e) => eventRaised = true;

            // Act
            viewModel.CloseCommand.Execute(null);

            // Assert
            eventRaised.Should().BeTrue();
        }

        [Fact]
        public void RequestClose_ProvidesDialogResultForClosing()
        {
            // Arrange
            var nodeDto = CreateEquipmentNodeDto();
            var viewModel = new CardDetailDialogViewModel(nodeDto);
            bool? dialogResult = null;
            viewModel.RequestClose += (s, e) => dialogResult = e.DialogResult;

            // Act
            viewModel.CloseCommand.Execute(null);

            // Assert
            dialogResult.Should().BeFalse();
        }

        #endregion

        #region Display Text Properties

        [Fact]
        public void HardwareTypeDisplayText_ReturnsLocalizedText_ForEquipmentLayer()
        {
            // Arrange
            var nodeDto = CreateEquipmentNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.HardwareTypeDisplayText.Should().Be("Equipment");
        }

        [Fact]
        public void HardwareTypeDisplayText_ReturnsLocalizedText_ForSystemLayer()
        {
            // Arrange
            var nodeDto = CreateSystemNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.HardwareTypeDisplayText.Should().Be("System");
        }

        [Fact]
        public void DialogTitle_ReturnsNamePropertyValue()
        {
            // Arrange
            var nodeDto = CreateEquipmentNodeDto();

            // Act
            var viewModel = new CardDetailDialogViewModel(nodeDto);

            // Assert
            viewModel.DialogTitle.Should().Be("Test Equipment");
        }

        #endregion
    }
}
