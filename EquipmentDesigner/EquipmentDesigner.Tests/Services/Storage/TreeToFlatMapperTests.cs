using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models.Dtos;
using EquipmentDesigner.Models.Storage;
using EquipmentDesigner.Services.Storage;

namespace EquipmentDesigner.Tests.Services.Storage
{
    public class TreeToFlatMapperTests
    {
        private readonly TreeToFlatMapper _mapper;

        public TreeToFlatMapperTests()
        {
            _mapper = new TreeToFlatMapper();
        }

        #region Equipment Extraction

        [Fact]
        public void MapTreeToFlat_ExtractsEquipmentDto_FromEquipmentLayerNode()
        {
            // Arrange
            var treeNodes = new List<TreeNodeDataDto>
            {
                new TreeNodeDataDto
                {
                    NodeId = "node-1",
                    HardwareLayer = HardwareLayer.Equipment,
                    EquipmentData = new EquipmentDto
                    {
                        Name = "TestEquipment",
                        EquipmentType = "Manufacturing"
                    }
                }
            };

            // Act
            var result = _mapper.MapTreeToFlat(treeNodes);

            // Assert
            result.Equipments.Should().HaveCount(1);
            result.Equipments[0].Name.Should().Be("TestEquipment");
            result.Equipments[0].EquipmentType.Should().Be("Manufacturing");
        }

        #endregion

        #region System Extraction

        [Fact]
        public void MapTreeToFlat_ExtractsSystemDto_WithCorrectEquipmentId()
        {
            // Arrange
            var treeNodes = new List<TreeNodeDataDto>
            {
                new TreeNodeDataDto
                {
                    NodeId = "eq-node",
                    HardwareLayer = HardwareLayer.Equipment,
                    EquipmentData = new EquipmentDto { Name = "Equipment1" },
                    Children = new List<TreeNodeDataDto>
                    {
                        new TreeNodeDataDto
                        {
                            NodeId = "sys-node",
                            HardwareLayer = HardwareLayer.System,
                            SystemData = new SystemDto { Name = "System1" }
                        }
                    }
                }
            };

            // Act
            var result = _mapper.MapTreeToFlat(treeNodes);

            // Assert
            result.Systems.Should().HaveCount(1);
            result.Systems[0].Name.Should().Be("System1");
            result.Systems[0].EquipmentId.Should().Be(result.Equipments[0].Id);
        }

        #endregion

        #region Unit Extraction

        [Fact]
        public void MapTreeToFlat_ExtractsUnitDto_WithCorrectSystemId()
        {
            // Arrange
            var treeNodes = new List<TreeNodeDataDto>
            {
                new TreeNodeDataDto
                {
                    NodeId = "sys-node",
                    HardwareLayer = HardwareLayer.System,
                    SystemData = new SystemDto { Name = "System1" },
                    Children = new List<TreeNodeDataDto>
                    {
                        new TreeNodeDataDto
                        {
                            NodeId = "unit-node",
                            HardwareLayer = HardwareLayer.Unit,
                            UnitData = new UnitDto { Name = "Unit1" }
                        }
                    }
                }
            };

            // Act
            var result = _mapper.MapTreeToFlat(treeNodes);

            // Assert
            result.Units.Should().HaveCount(1);
            result.Units[0].Name.Should().Be("Unit1");
            result.Units[0].SystemId.Should().Be(result.Systems[0].Id);
        }

        #endregion

        #region Device Extraction

        [Fact]
        public void MapTreeToFlat_ExtractsDeviceDto_WithCorrectUnitId()
        {
            // Arrange
            var treeNodes = new List<TreeNodeDataDto>
            {
                new TreeNodeDataDto
                {
                    NodeId = "unit-node",
                    HardwareLayer = HardwareLayer.Unit,
                    UnitData = new UnitDto { Name = "Unit1" },
                    Children = new List<TreeNodeDataDto>
                    {
                        new TreeNodeDataDto
                        {
                            NodeId = "dev-node",
                            HardwareLayer = HardwareLayer.Device,
                            DeviceData = new DeviceDto { Name = "Device1" }
                        }
                    }
                }
            };

            // Act
            var result = _mapper.MapTreeToFlat(treeNodes);

            // Assert
            result.Devices.Should().HaveCount(1);
            result.Devices[0].Name.Should().Be("Device1");
            result.Devices[0].UnitId.Should().Be(result.Units[0].Id);
        }

        #endregion

        #region Property Preservation

        [Fact]
        public void MapTreeToFlat_PreservesAllDtoProperties()
        {
            // Arrange
            var treeNodes = new List<TreeNodeDataDto>
            {
                new TreeNodeDataDto
                {
                    NodeId = "eq-node",
                    HardwareLayer = HardwareLayer.Equipment,
                    EquipmentData = new EquipmentDto
                    {
                        Name = "TestEquipment",
                        DisplayName = "Test Display",
                        Description = "Test Description",
                        Customer = "TestCustomer",
                        Process = "TestProcess"
                    }
                }
            };

            // Act
            var result = _mapper.MapTreeToFlat(treeNodes);

            // Assert
            var equipment = result.Equipments[0];
            equipment.Name.Should().Be("TestEquipment");
            equipment.DisplayName.Should().Be("Test Display");
            equipment.Description.Should().Be("Test Description");
            equipment.Customer.Should().Be("TestCustomer");
            equipment.Process.Should().Be("TestProcess");
        }

        #endregion

        #region ID Generation

        [Fact]
        public void MapTreeToFlat_GeneratesUniqueIds_ForAllExtractedComponents()
        {
            // Arrange
            var treeNodes = new List<TreeNodeDataDto>
            {
                new TreeNodeDataDto
                {
                    NodeId = "eq-node",
                    HardwareLayer = HardwareLayer.Equipment,
                    EquipmentData = new EquipmentDto { Name = "Equipment1" },
                    Children = new List<TreeNodeDataDto>
                    {
                        new TreeNodeDataDto
                        {
                            NodeId = "sys-node",
                            HardwareLayer = HardwareLayer.System,
                            SystemData = new SystemDto { Name = "System1" },
                            Children = new List<TreeNodeDataDto>
                            {
                                new TreeNodeDataDto
                                {
                                    NodeId = "unit-node",
                                    HardwareLayer = HardwareLayer.Unit,
                                    UnitData = new UnitDto { Name = "Unit1" },
                                    Children = new List<TreeNodeDataDto>
                                    {
                                        new TreeNodeDataDto
                                        {
                                            NodeId = "dev-node",
                                            HardwareLayer = HardwareLayer.Device,
                                            DeviceData = new DeviceDto { Name = "Device1" }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Act
            var result = _mapper.MapTreeToFlat(treeNodes);

            // Assert
            var allIds = new List<string>
            {
                result.Equipments[0].Id,
                result.Systems[0].Id,
                result.Units[0].Id,
                result.Devices[0].Id
            };

            allIds.Should().OnlyHaveUniqueItems();
            allIds.Should().NotContainNulls();
        }

        #endregion

        #region State Setting

        [Fact]
        public void MapTreeToFlat_SetsState_ToUploadedForAllComponents()
        {
            // Arrange
            var treeNodes = new List<TreeNodeDataDto>
            {
                new TreeNodeDataDto
                {
                    NodeId = "eq-node",
                    HardwareLayer = HardwareLayer.Equipment,
                    EquipmentData = new EquipmentDto { Name = "E1", State = ComponentState.Defined },
                    Children = new List<TreeNodeDataDto>
                    {
                        new TreeNodeDataDto
                        {
                            NodeId = "sys-node",
                            HardwareLayer = HardwareLayer.System,
                            SystemData = new SystemDto { Name = "S1" }
                        }
                    }
                }
            };

            // Act
            var result = _mapper.MapTreeToFlat(treeNodes);

            // Assert
            result.Equipments[0].State.Should().Be(ComponentState.Uploaded);
            result.Systems[0].State.Should().Be(ComponentState.Uploaded);
        }

        #endregion

        #region Timestamp Setting

        [Fact]
        public void MapTreeToFlat_SetsTimestamps_ForAllExtractedComponents()
        {
            // Arrange
            var treeNodes = new List<TreeNodeDataDto>
            {
                new TreeNodeDataDto
                {
                    NodeId = "dev-node",
                    HardwareLayer = HardwareLayer.Device,
                    DeviceData = new DeviceDto { Name = "Device1" }
                }
            };

            // Act
            var result = _mapper.MapTreeToFlat(treeNodes);

            // Assert
            result.Devices[0].CreatedAt.Should().BeCloseTo(System.DateTime.Now, System.TimeSpan.FromSeconds(5));
            result.Devices[0].UpdatedAt.Should().BeCloseTo(System.DateTime.Now, System.TimeSpan.FromSeconds(5));
        }

        #endregion

        #region Multiple Children

        [Fact]
        public void MapTreeToFlat_ExtractsMultipleSystems_UnderOneEquipment_WithSameEquipmentId()
        {
            // Arrange
            var treeNodes = new List<TreeNodeDataDto>
            {
                new TreeNodeDataDto
                {
                    NodeId = "eq-node",
                    HardwareLayer = HardwareLayer.Equipment,
                    EquipmentData = new EquipmentDto { Name = "Equipment1" },
                    Children = new List<TreeNodeDataDto>
                    {
                        new TreeNodeDataDto
                        {
                            NodeId = "sys-1",
                            HardwareLayer = HardwareLayer.System,
                            SystemData = new SystemDto { Name = "System1" }
                        },
                        new TreeNodeDataDto
                        {
                            NodeId = "sys-2",
                            HardwareLayer = HardwareLayer.System,
                            SystemData = new SystemDto { Name = "System2" }
                        }
                    }
                }
            };

            // Act
            var result = _mapper.MapTreeToFlat(treeNodes);

            // Assert
            result.Systems.Should().HaveCount(2);
            var equipmentId = result.Equipments[0].Id;
            result.Systems[0].EquipmentId.Should().Be(equipmentId);
            result.Systems[1].EquipmentId.Should().Be(equipmentId);
        }

        [Fact]
        public void MapTreeToFlat_ExtractsMultipleUnits_UnderOneSystem_WithSameSystemId()
        {
            // Arrange
            var treeNodes = new List<TreeNodeDataDto>
            {
                new TreeNodeDataDto
                {
                    NodeId = "sys-node",
                    HardwareLayer = HardwareLayer.System,
                    SystemData = new SystemDto { Name = "System1" },
                    Children = new List<TreeNodeDataDto>
                    {
                        new TreeNodeDataDto
                        {
                            NodeId = "unit-1",
                            HardwareLayer = HardwareLayer.Unit,
                            UnitData = new UnitDto { Name = "Unit1" }
                        },
                        new TreeNodeDataDto
                        {
                            NodeId = "unit-2",
                            HardwareLayer = HardwareLayer.Unit,
                            UnitData = new UnitDto { Name = "Unit2" }
                        }
                    }
                }
            };

            // Act
            var result = _mapper.MapTreeToFlat(treeNodes);

            // Assert
            result.Units.Should().HaveCount(2);
            var systemId = result.Systems[0].Id;
            result.Units[0].SystemId.Should().Be(systemId);
            result.Units[1].SystemId.Should().Be(systemId);
        }

        [Fact]
        public void MapTreeToFlat_ExtractsMultipleDevices_UnderOneUnit_WithSameUnitId()
        {
            // Arrange
            var treeNodes = new List<TreeNodeDataDto>
            {
                new TreeNodeDataDto
                {
                    NodeId = "unit-node",
                    HardwareLayer = HardwareLayer.Unit,
                    UnitData = new UnitDto { Name = "Unit1" },
                    Children = new List<TreeNodeDataDto>
                    {
                        new TreeNodeDataDto
                        {
                            NodeId = "dev-1",
                            HardwareLayer = HardwareLayer.Device,
                            DeviceData = new DeviceDto { Name = "Device1" }
                        },
                        new TreeNodeDataDto
                        {
                            NodeId = "dev-2",
                            HardwareLayer = HardwareLayer.Device,
                            DeviceData = new DeviceDto { Name = "Device2" }
                        }
                    }
                }
            };

            // Act
            var result = _mapper.MapTreeToFlat(treeNodes);

            // Assert
            result.Devices.Should().HaveCount(2);
            var unitId = result.Units[0].Id;
            result.Devices[0].UnitId.Should().Be(unitId);
            result.Devices[1].UnitId.Should().Be(unitId);
        }

        #endregion

        #region Single Device (StartType.Device)

        [Fact]
        public void MapTreeToFlat_WithSingleDeviceNode_MapsCorrectlyToSingleDeviceDto()
        {
            // Arrange - StartType.Device scenario with no parent hierarchy
            var treeNodes = new List<TreeNodeDataDto>
            {
                new TreeNodeDataDto
                {
                    NodeId = "dev-only",
                    HardwareLayer = HardwareLayer.Device,
                    DeviceData = new DeviceDto
                    {
                        Name = "StandaloneDevice",
                        DeviceType = "Sensor"
                    }
                }
            };

            // Act
            var result = _mapper.MapTreeToFlat(treeNodes);

            // Assert
            result.Equipments.Should().BeEmpty();
            result.Systems.Should().BeEmpty();
            result.Units.Should().BeEmpty();
            result.Devices.Should().HaveCount(1);
            result.Devices[0].Name.Should().Be("StandaloneDevice");
            result.Devices[0].UnitId.Should().BeNull(); // No parent
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void MapTreeToFlat_WithEmptyTreeNodesList_ReturnsEmptyResultWithoutException()
        {
            // Arrange
            var treeNodes = new List<TreeNodeDataDto>();

            // Act
            var result = _mapper.MapTreeToFlat(treeNodes);

            // Assert
            result.Should().NotBeNull();
            result.Equipments.Should().BeEmpty();
            result.Systems.Should().BeEmpty();
            result.Units.Should().BeEmpty();
            result.Devices.Should().BeEmpty();
        }

        [Fact]
        public void MapTreeToFlat_WithNullChildren_HandlesGracefully()
        {
            // Arrange - Node with null Children property
            var treeNodes = new List<TreeNodeDataDto>
            {
                new TreeNodeDataDto
                {
                    NodeId = "node-1",
                    HardwareLayer = HardwareLayer.Device,
                    DeviceData = new DeviceDto { Name = "Device1" },
                    Children = null
                }
            };

            // Act
            var result = _mapper.MapTreeToFlat(treeNodes);

            // Assert
            result.Devices.Should().HaveCount(1);
            result.Devices[0].Name.Should().Be("Device1");
        }

        #endregion
    }
}