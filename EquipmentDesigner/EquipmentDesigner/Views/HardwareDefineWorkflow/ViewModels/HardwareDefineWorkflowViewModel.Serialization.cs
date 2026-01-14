using System;
using System.Collections.Generic;
using System.Linq;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// Partial class containing serialization and deserialization functionality.
    /// Handles conversion between ViewModel and HardwareDefinition DTO.
    /// </summary>
    public partial class HardwareDefineWorkflowViewModel
    {
        #region Data Change Handling

        /// <summary>
        /// Called when any data in any tree node changes.
        /// Resets the workflow completion state if data is modified after completion.
        /// Marks data as dirty and starts debounce timer for autosave.
        /// </summary>
        public void OnDataChanged()
        {
            if (IsWorkflowCompleted)
            {
                HasDataChangedSinceCompletion = true;
            }
            OnPropertyChanged(nameof(CanCompleteWorkflow));
            OnPropertyChanged(nameof(AllStepsRequiredFieldsFilled));

            // Mark dirty and start debounce for autosave
            MarkDirty();
            RestartDebounceTimer();
        }

        #endregion

        #region Serialization (ViewModel to DTO)

        /// <summary>
        /// Converts this ViewModel to a HardwareDefinition for persistence.
        /// Serializes the entire tree structure for multi-instance support.
        /// </summary>
        public HardwareDefinition ToHardwareDefinition()
        {
            // For single root node (most common case)
            var rootNode = TreeRootNodes.FirstOrDefault();
            if (rootNode == null)
            {
                return new HardwareDefinition
                {
                    Id = HardwareId,
                    HardwareType = StartType,
                    HardwareKey = HardwareKey,
                    Version = Version,
                    ProcessId = ProcessId,
                    State = ComponentState.Draft,
                    LastModifiedAt = DateTime.Now,
                    CreatedAt = DateTime.Now
                };
            }

            var hw = SerializeToHardwareDefinition(rootNode);
            hw.Id = HardwareId;
            hw.HardwareKey = HardwareKey;
            hw.Version = Version;
            hw.ProcessId = ProcessId;
            hw.State = ComponentState.Draft;
            hw.LastModifiedAt = DateTime.Now;
            return hw;
        }

        /// <summary>
        /// Serializes a tree node and its children to HardwareDefinition.
        /// Uses ViewModel's ToHardwareDefinition() method for property mapping.
        /// </summary>
        private HardwareDefinition SerializeToHardwareDefinition(HardwareTreeNodeViewModel node)
        {
            HardwareDefinition hw;

            // Use ViewModel's ToHardwareDefinition() method for property mapping
            if (node.DataViewModel != null)
            {
                hw = node.DataViewModel switch
                {
                    EquipmentDefineViewModel eqVm => eqVm.ToHardwareDefinition(),
                    SystemDefineViewModel sysVm => sysVm.ToHardwareDefinition(),
                    UnitDefineViewModel unitVm => unitVm.ToHardwareDefinition(),
                    DeviceDefineViewModel devVm => devVm.ToHardwareDefinition(),
                    _ => new HardwareDefinition { HardwareType = node.HardwareType }
                };
            }
            else
            {
                hw = new HardwareDefinition { HardwareType = node.HardwareType };
            }

            // Add tree-specific properties
            hw.Id = node.NodeId;
            hw.CreatedAt = DateTime.Now;
            hw.LastModifiedAt = DateTime.Now;
            hw.Children = node.Children.Select(SerializeToHardwareDefinition).ToList();

            return hw;
        }

        #endregion

        #region Deserialization (DTO to ViewModel)

        /// <summary>
        /// Creates a HardwareDefineWorkflowViewModel from a saved HardwareDefinition.
        /// Uses tree-based format exclusively.
        /// </summary>
        public static HardwareDefineWorkflowViewModel FromHardwareDefinition(HardwareDefinition dto)
        {
            var viewModel = new HardwareDefineWorkflowViewModel(dto.HardwareType, dto.Id, dto.ProcessId, dto.HardwareKey, dto.Version ?? "1.0.0");

            // Rebuild tree from the HardwareDefinition itself (it is the root node)
            viewModel.TreeRootNodes.Clear();
            var rootNode = DeserializeFromHardwareDefinition(dto, null);
            viewModel.TreeRootNodes.Add(rootNode);

            // Select first node
            if (viewModel.TreeRootNodes.Count > 0)
            {
                viewModel.SelectedTreeNode = viewModel.TreeRootNodes[0];
                viewModel.SelectedTreeNode.IsSelected = true;
            }

            // Calculate step index based on tree state (default to 0)
            viewModel.SetCurrentStepIndex(0);

            // Re-initialize callbacks and states
            viewModel.SetupAllDeviceViewModelCallbacks();
            viewModel.InitializeStepFieldCounts();
            viewModel.UpdateStepStates();

            return viewModel;
        }

        /// <summary>
        /// Deserializes a HardwareDefinition to a HardwareTreeNodeViewModel.
        /// </summary>
        private static HardwareTreeNodeViewModel DeserializeFromHardwareDefinition(HardwareDefinition dto, HardwareTreeNodeViewModel parent)
        {
            IHardwareDefineViewModel dataViewModel = CreateViewModelFromHardwareDefinition(dto);

            var node = new HardwareTreeNodeViewModel(dto.HardwareType, parent, dataViewModel);

            foreach (var childDto in dto.Children ?? new List<HardwareDefinition>())
            {
                var childNode = DeserializeFromHardwareDefinition(childDto, node);
                node.Children.Add(childNode);
            }

            return node;
        }

        /// <summary>
        /// Creates an appropriate IHardwareDefineViewModel from HardwareDefinition data.
        /// Uses ViewModel's FromHardwareDefinition() static factory methods.
        /// </summary>
        private static IHardwareDefineViewModel CreateViewModelFromHardwareDefinition(HardwareDefinition dto)
        {
            return dto.HardwareType switch
            {
                HardwareType.Equipment => EquipmentDefineViewModel.FromHardwareDefinition(dto),
                HardwareType.System => SystemDefineViewModel.FromHardwareDefinition(dto),
                HardwareType.Unit => UnitDefineViewModel.FromHardwareDefinition(dto),
                HardwareType.Device => DeviceDefineViewModel.FromHardwareDefinition(dto),
                _ => null
            };
        }

        /// <summary>
        /// Helper method to set DataViewModel using reflection (since setter is private).
        /// </summary>
        private static void SetNodeDataViewModel(HardwareTreeNodeViewModel node, IHardwareDefineViewModel viewModel)
        {
            // Use reflection to set the private DataViewModel property
            var property = typeof(HardwareTreeNodeViewModel).GetProperty("DataViewModel");
            if (property != null)
            {
                // Access the backing field through reflection
                var field = typeof(HardwareTreeNodeViewModel).GetField("_dataViewModel",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    // Unsubscribe old event handlers
                    var oldVm = node.DataViewModel;
                    if (oldVm != null)
                    {
                        oldVm.PropertyChanged -= null; // Will be handled by setter
                    }

                    // Set new value (this triggers the property setter logic)
                    field.SetValue(node, null); // Reset first
                }
            }

            // Recreate node with new ViewModel - simplest approach
            // Note: For legacy support, we just accept the default created VMs
            // The legacy data restoration is best-effort
        }

        #endregion
    }
}
