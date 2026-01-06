using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Views.HardwareDefineWorkflow
{
    /// <summary>
    /// ViewModel representing a node in the hardware hierarchy tree.
    /// Supports Equipment → System → Unit → Device structure.
    /// </summary>
    public class HardwareTreeNodeViewModel : ViewModelBase
    {
        private bool _isExpanded = true;
        private bool _isSelected;
        private int _filledFieldCount;
        private int _totalFieldCount;
        private string _componentName = string.Empty;

        public HardwareTreeNodeViewModel(HardwareLayer hardwareLayer, HardwareTreeNodeViewModel parent = null)
        {
            HardwareLayer = hardwareLayer;
            Parent = parent;
            Children = new ObservableCollection<HardwareTreeNodeViewModel>();
            NodeId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Unique identifier for this node.
        /// </summary>
        public string NodeId { get; }

        /// <summary>
        /// The hardware layer type (Equipment, System, Unit, Device).
        /// </summary>
        public HardwareLayer HardwareLayer { get; }

        /// <summary>
        /// Parent node in the hierarchy (null for root Equipment node).
        /// </summary>
        public HardwareTreeNodeViewModel Parent { get; }

        /// <summary>
        /// Child nodes in the hierarchy.
        /// </summary>
        public ObservableCollection<HardwareTreeNodeViewModel> Children { get; }

        /// <summary>
        /// The component name from the associated ViewModel.
        /// </summary>
        public string ComponentName
        {
            get => _componentName;
            set
            {
                if (SetProperty(ref _componentName, value))
                {
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        /// <summary>
        /// Display name for the node: shows ComponentName if set, otherwise "New {HardwareLayer}".
        /// </summary>
        public string DisplayName => string.IsNullOrWhiteSpace(ComponentName)
            ? $"New {HardwareLayer}"
            : ComponentName;

        /// <summary>
        /// Whether this node is expanded in the tree.
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        /// <summary>
        /// Whether this node is selected.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        /// <summary>
        /// Number of fields that have been filled.
        /// </summary>
        public int FilledFieldCount
        {
            get => _filledFieldCount;
            set
            {
                if (SetProperty(ref _filledFieldCount, value))
                {
                    OnPropertyChanged(nameof(FieldProgressText));
                    OnPropertyChanged(nameof(IsCompleted));
                }
            }
        }

        /// <summary>
        /// Total number of fields in this node.
        /// </summary>
        public int TotalFieldCount
        {
            get => _totalFieldCount;
            set
            {
                if (SetProperty(ref _totalFieldCount, value))
                {
                    OnPropertyChanged(nameof(FieldProgressText));
                    OnPropertyChanged(nameof(IsCompleted));
                }
            }
        }

        /// <summary>
        /// Display text showing field progress (e.g., "0/2").
        /// </summary>
        public string FieldProgressText => $"{FilledFieldCount}/{TotalFieldCount}";

        /// <summary>
        /// Whether all required fields are completed.
        /// </summary>
        public bool IsCompleted => TotalFieldCount > 0 && FilledFieldCount >= TotalFieldCount;

        /// <summary>
        /// Whether this node can have children (Equipment, System, Unit can have children).
        /// </summary>
        public bool CanHaveChildren => HardwareLayer != HardwareLayer.Device;

        /// <summary>
        /// Whether this node allows adding new child items.
        /// Equipment level doesn't allow adding (only 1 Equipment per workflow).
        /// </summary>
        public bool CanAddChild => HardwareLayer != HardwareLayer.Device;

        /// <summary>
        /// Text to display on the add child button (e.g., "New System", "New Unit", "New Device").
        /// </summary>
        public string AddChildButtonText => ChildHardwareLayer.HasValue
            ? $"New {ChildHardwareLayer.Value}"
            : string.Empty;

        /// <summary>
        /// Gets the child hardware layer type.
        /// </summary>
        public HardwareLayer? ChildHardwareLayer
        {
            get
            {
                return HardwareLayer switch
                {
                    HardwareLayer.Equipment => Models.HardwareLayer.System,
                    HardwareLayer.System => Models.HardwareLayer.Unit,
                    HardwareLayer.Unit => Models.HardwareLayer.Device,
                    HardwareLayer.Device => null,
                    _ => null
                };
            }
        }

        /// <summary>
        /// The indentation level based on hierarchy depth.
        /// </summary>
        public int IndentLevel
        {
            get
            {
                int level = 0;
                var current = Parent;
                while (current != null)
                {
                    level++;
                    current = current.Parent;
                }
                return level;
            }
        }

        /// <summary>
        /// Adds a new child node of the appropriate type.
        /// </summary>
        public HardwareTreeNodeViewModel AddChild()
        {
            if (!CanHaveChildren || ChildHardwareLayer == null)
                return null;

            var child = new HardwareTreeNodeViewModel(ChildHardwareLayer.Value, this);
            Children.Add(child);
            IsExpanded = true;
            return child;
        }

        /// <summary>
        /// Adds a new child node with its full hierarchy down to Device level.
        /// For example, adding a System will also create Unit and Device underneath.
        /// </summary>
        /// <returns>The newly created child node.</returns>
        public HardwareTreeNodeViewModel AddChildWithFullHierarchy()
        {
            if (!CanHaveChildren || ChildHardwareLayer == null)
                return null;

            var child = AddChild();
            if (child == null)
                return null;

            // Recursively add children down to Device level
            var current = child;
            while (current.CanHaveChildren)
            {
                current = current.AddChild();
                if (current == null)
                    break;
            }

            return child;
        }

        /// <summary>
        /// Removes a child node.
        /// </summary>
        public bool RemoveChild(HardwareTreeNodeViewModel child)
        {
            return Children.Remove(child);
        }
    }
}