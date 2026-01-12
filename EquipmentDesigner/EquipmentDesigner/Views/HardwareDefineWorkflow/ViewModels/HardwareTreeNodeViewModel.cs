using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Input;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.ViewModels
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
        private IHardwareDefineViewModel _dataViewModel;

        public HardwareTreeNodeViewModel(HardwareType hardwareType, HardwareTreeNodeViewModel parent = null)
            : this(hardwareType, parent, CreateDefaultViewModel(hardwareType))
        {
        }

        public HardwareTreeNodeViewModel(HardwareType hardwareType, HardwareTreeNodeViewModel parent, IHardwareDefineViewModel dataViewModel)
        {
            HardwareType = hardwareType;
            Parent = parent;
            Children = new ObservableCollection<HardwareTreeNodeViewModel>();
            NodeId = Guid.NewGuid().ToString();
            DataViewModel = dataViewModel;
        }

        /// <summary>
        /// Creates a default ViewModel instance based on the hardware layer type.
        /// </summary>
        private static IHardwareDefineViewModel CreateDefaultViewModel(HardwareType layer)
        {
            return layer switch
            {
                HardwareType.Equipment => new EquipmentDefineViewModel(),
                HardwareType.System => new SystemDefineViewModel(),
                HardwareType.Unit => new UnitDefineViewModel(),
                HardwareType.Device => new DeviceDefineViewModel(),
                _ => null
            };
        }

        /// <summary>
        /// Unique identifier for this node.
        /// </summary>
        public string NodeId { get; }

        /// <summary>
        /// The hardware layer type (Equipment, System, Unit, Device).
        /// </summary>
        public HardwareType HardwareType { get; }

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
        /// Display name for the node: shows ComponentName if set, otherwise "New {HardwareType}".
        /// </summary>
        public string DisplayName => string.IsNullOrWhiteSpace(ComponentName)
            ? $"New {HardwareType}"
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
        /// The data ViewModel associated with this tree node.
        /// Each node has its own independent ViewModel instance.
        /// </summary>
        public IHardwareDefineViewModel DataViewModel
        {
            get => _dataViewModel;
            private set
            {
                if (_dataViewModel != null)
                {
                    _dataViewModel.PropertyChanged -= OnDataViewModelPropertyChanged;
                }

                if (SetProperty(ref _dataViewModel, value))
                {
                    if (_dataViewModel != null)
                    {
                        _dataViewModel.PropertyChanged += OnDataViewModelPropertyChanged;
                        SyncFromDataViewModel();
                    }
                }
            }
        }

        /// <summary>
        /// Handles property changes from the associated data ViewModel.
        /// Syncs relevant properties to the tree node.
        /// </summary>
        private void OnDataViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IHardwareDefineViewModel.Name):
                    ComponentName = DataViewModel?.Name ?? string.Empty;
                    break;
                case nameof(IHardwareDefineViewModel.FilledFieldCount):
                    FilledFieldCount = DataViewModel?.FilledFieldCount ?? 0;
                    break;
                case nameof(IHardwareDefineViewModel.TotalFieldCount):
                    TotalFieldCount = DataViewModel?.TotalFieldCount ?? 0;
                    break;
                case nameof(IHardwareDefineViewModel.CanProceedToNext):
                    OnPropertyChanged(nameof(IsCompleted));
                    break;
            }
        }

        /// <summary>
        /// Synchronizes all properties from the DataViewModel to the tree node.
        /// </summary>
        private void SyncFromDataViewModel()
        {
            if (DataViewModel != null)
            {
                ComponentName = DataViewModel.Name ?? string.Empty;
                FilledFieldCount = DataViewModel.FilledFieldCount;
                TotalFieldCount = DataViewModel.TotalFieldCount;
            }
        }

        /// <summary>
        /// Whether all required fields are completed.
        /// </summary>
        public bool IsCompleted => DataViewModel?.CanProceedToNext ?? (TotalFieldCount > 0 && FilledFieldCount >= TotalFieldCount);

        /// <summary>
        /// Whether this node can have children (Equipment, System, Unit can have children).
        /// </summary>
        public bool CanHaveChildren => HardwareType != HardwareType.Device;

        /// <summary>
        /// Whether this node allows adding new child items.
        /// Equipment level doesn't allow adding (only 1 Equipment per workflow).
        /// </summary>
        public bool CanAddChild => HardwareType != HardwareType.Device;

        /// <summary>
        /// Text to display on the add child button (e.g., "New System", "New Unit", "New Device").
        /// </summary>
        public string AddChildButtonText => ChildHardwareType.HasValue
            ? $"New {ChildHardwareType.Value}"
            : string.Empty;

        /// <summary>
        /// Gets the child hardware layer type.
        /// </summary>
        public HardwareType? ChildHardwareType
        {
            get
            {
                return HardwareType switch
                {
                    HardwareType.Equipment => Models.HardwareType.System,
                    HardwareType.System => Models.HardwareType.Unit,
                    HardwareType.Unit => Models.HardwareType.Device,
                    HardwareType.Device => null,
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
            if (!CanHaveChildren || ChildHardwareType == null)
                return null;

            // Create new ViewModel for the child
            var childViewModel = CreateDefaultViewModel(ChildHardwareType.Value);
            var child = new HardwareTreeNodeViewModel(ChildHardwareType.Value, this, childViewModel);
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
            if (!CanHaveChildren || ChildHardwareType == null)
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

        #region Copy and Delete Support Methods

        /// <summary>
        /// Gets all descendant nodes (children, grandchildren, etc.) in a flattened list.
        /// </summary>
        /// <returns>A flat list of all descendant nodes, not including the node itself.</returns>
        public List<HardwareTreeNodeViewModel> GetAllDescendants()
        {
            var descendants = new List<HardwareTreeNodeViewModel>();
            CollectDescendants(this, descendants);
            return descendants;
        }

        private static void CollectDescendants(HardwareTreeNodeViewModel node, List<HardwareTreeNodeViewModel> list)
        {
            foreach (var child in node.Children)
            {
                list.Add(child);
                CollectDescendants(child, list);
            }
        }

        /// <summary>
        /// Generates a unique copy name by appending "- copy" suffix.
        /// If the name already has a copy suffix, increments the number.
        /// </summary>
        /// <param name="originalName">The original name to generate a copy name from.</param>
        /// <returns>A new name with appropriate copy suffix.</returns>
        public static string GenerateCopyName(string originalName)
        {
            if (string.IsNullOrWhiteSpace(originalName))
                return "copy";

            // Pattern to match " - copy" or " - copy N" at the end
            var copyPattern = new Regex(@" - copy( (\d+))?$");
            var match = copyPattern.Match(originalName);

            if (match.Success)
            {
                // Already has copy suffix
                if (match.Groups[2].Success)
                {
                    // Has a number, increment it
                    int currentNum = int.Parse(match.Groups[2].Value);
                    return copyPattern.Replace(originalName, $" - copy {currentNum + 1}");
                }
                else
                {
                    // Just " - copy", add " 2"
                    return originalName + " 2";
                }
            }

            // No copy suffix, add " - copy"
            return originalName + " - copy";
        }

        /// <summary>
        /// Creates a deep copy of this node and all its descendants.
        /// </summary>
        /// <param name="newParent">The parent for the copied node.</param>
        /// <returns>A new node with copied data and recursively copied children.</returns>
        public HardwareTreeNodeViewModel DeepCopy(HardwareTreeNodeViewModel newParent)
        {
            // Clone the DataViewModel
            var clonedDataViewModel = CloneDataViewModel(this.DataViewModel);
            
            // Apply copy suffix to the name
            if (clonedDataViewModel != null)
            {
                clonedDataViewModel.Name = GenerateCopyName(clonedDataViewModel.Name);
            }

            // Create new node with cloned data
            var copiedNode = new HardwareTreeNodeViewModel(this.HardwareType, newParent, clonedDataViewModel);

            // Recursively copy all children
            foreach (var child in this.Children)
            {
                var copiedChild = child.DeepCopy(copiedNode);
                copiedNode.Children.Add(copiedChild);
            }

            return copiedNode;
        }

        /// <summary>
        /// Creates a clone of the data ViewModel using the ToDto/FromDto pattern.
        /// </summary>
        private static IHardwareDefineViewModel CloneDataViewModel(IHardwareDefineViewModel source)
        {
            if (source == null)
                return null;

            return source switch
            {
                EquipmentDefineViewModel vm => EquipmentDefineViewModel.FromDto(vm.ToDto()),
                SystemDefineViewModel vm => SystemDefineViewModel.FromDto(vm.ToDto()),
                UnitDefineViewModel vm => UnitDefineViewModel.FromDto(vm.ToDto()),
                DeviceDefineViewModel vm => DeviceDefineViewModel.FromDto(vm.ToDto()),
                _ => null
            };
        }

        #endregion
    }
}