using System;
using System.Collections.Generic;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models.Dtos;
using EquipmentDesigner.Models.Storage;

namespace EquipmentDesigner.Services.Storage
{
    /// <summary>
    /// Maps hierarchical tree node structure to flat hardware component lists.
    /// Used by UploadToServerCommand to extract DTOs from workflow tree.
    /// </summary>
    public class TreeToFlatMapper
    {
        /// <summary>
        /// Result of mapping tree nodes to flat hardware lists.
        /// </summary>
        public class MappingResult
        {
            public List<EquipmentDto> Equipments { get; } = new List<EquipmentDto>();
            public List<SystemDto> Systems { get; } = new List<SystemDto>();
            public List<UnitDto> Units { get; } = new List<UnitDto>();
            public List<DeviceDto> Devices { get; } = new List<DeviceDto>();
        }

        /// <summary>
        /// Maps tree nodes to flat hardware component lists with proper parent-child relationships.
        /// </summary>
        /// <param name="rootNodes">The root tree nodes to map.</param>
        /// <returns>MappingResult containing all extracted hardware components.</returns>
        public MappingResult MapTreeToFlat(IEnumerable<TreeNodeDataDto> rootNodes)
        {
            var result = new MappingResult();
            var timestamp = DateTime.Now;

            foreach (var rootNode in rootNodes)
            {
                MapNodeRecursively(rootNode, null, result, timestamp);
            }

            return result;
        }

        /// <summary>
        /// Recursively maps a tree node and its children to flat DTOs.
        /// </summary>
        private string MapNodeRecursively(
            TreeNodeDataDto node,
            string parentId,
            MappingResult result,
            DateTime timestamp)
        {
            string nodeId = Guid.NewGuid().ToString();

            switch (node.HardwareLayer)
            {
                case HardwareLayer.Equipment:
                    if (node.EquipmentData != null)
                    {
                        var equipment = CloneEquipmentDto(node.EquipmentData);
                        equipment.Id = nodeId;
                        equipment.State = ComponentState.Uploaded;
                        equipment.CreatedAt = timestamp;
                        equipment.UpdatedAt = timestamp;
                        result.Equipments.Add(equipment);
                    }
                    break;

                case HardwareLayer.System:
                    if (node.SystemData != null)
                    {
                        var system = CloneSystemDto(node.SystemData);
                        system.Id = nodeId;
                        system.EquipmentId = parentId;
                        system.State = ComponentState.Uploaded;
                        system.CreatedAt = timestamp;
                        system.UpdatedAt = timestamp;
                        result.Systems.Add(system);
                    }
                    break;

                case HardwareLayer.Unit:
                    if (node.UnitData != null)
                    {
                        var unit = CloneUnitDto(node.UnitData);
                        unit.Id = nodeId;
                        unit.SystemId = parentId;
                        unit.State = ComponentState.Uploaded;
                        unit.CreatedAt = timestamp;
                        unit.UpdatedAt = timestamp;
                        result.Units.Add(unit);
                    }
                    break;

                case HardwareLayer.Device:
                    if (node.DeviceData != null)
                    {
                        var device = CloneDeviceDto(node.DeviceData);
                        device.Id = nodeId;
                        device.UnitId = parentId;
                        device.State = ComponentState.Uploaded;
                        device.CreatedAt = timestamp;
                        device.UpdatedAt = timestamp;
                        result.Devices.Add(device);
                    }
                    break;
            }

            // Recursively map children
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    MapNodeRecursively(child, nodeId, result, timestamp);
                }
            }

            return nodeId;
        }

        private EquipmentDto CloneEquipmentDto(EquipmentDto source)
        {
            return new EquipmentDto
            {
                Name = source.Name,
                EquipmentType = source.EquipmentType,
                DisplayName = source.DisplayName,
                Subname = source.Subname,
                Description = source.Description,
                Customer = source.Customer,
                Process = source.Process,
                AttachedDocuments = source.AttachedDocuments != null
                    ? new List<string>(source.AttachedDocuments)
                    : new List<string>(),
                ProgramRoot = source.ProgramRoot
            };
        }

        private SystemDto CloneSystemDto(SystemDto source)
        {
            return new SystemDto
            {
                Name = source.Name,
                DisplayName = source.DisplayName,
                Subname = source.Subname,
                Description = source.Description,
                Commands = source.Commands != null
                    ? new List<CommandDto>(source.Commands)
                    : new List<CommandDto>(),
                ImplementationInstructions = source.ImplementationInstructions != null
                    ? new List<string>(source.ImplementationInstructions)
                    : new List<string>()
            };
        }

        private UnitDto CloneUnitDto(UnitDto source)
        {
            return new UnitDto
            {
                Name = source.Name,
                DisplayName = source.DisplayName,
                Subname = source.Subname,
                Description = source.Description,
                Commands = source.Commands != null
                    ? new List<CommandDto>(source.Commands)
                    : new List<CommandDto>(),
                ImplementationInstructions = source.ImplementationInstructions != null
                    ? new List<string>(source.ImplementationInstructions)
                    : new List<string>()
            };
        }

        private DeviceDto CloneDeviceDto(DeviceDto source)
        {
            return new DeviceDto
            {
                Name = source.Name,
                DisplayName = source.DisplayName,
                Subname = source.Subname,
                Description = source.Description,
                DeviceType = source.DeviceType,
                Commands = source.Commands != null
                    ? new List<CommandDto>(source.Commands)
                    : new List<CommandDto>(),
                IoInfo = source.IoInfo != null
                    ? new List<IoInfoDto>(source.IoInfo)
                    : new List<IoInfoDto>(),
                ImplementationInstructions = source.ImplementationInstructions != null
                    ? new List<string>(source.ImplementationInstructions)
                    : new List<string>()
            };
        }
    }
}
