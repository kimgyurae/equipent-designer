using System;
using System.Collections.Generic;
using System.Windows.Input;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// Event arguments for dialog close request.
    /// </summary>
    public class DialogCloseEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the dialog result to set when closing.
        /// </summary>
        public bool? DialogResult { get; }

        public DialogCloseEventArgs(bool? dialogResult)
        {
            DialogResult = dialogResult;
        }
    }

    /// <summary>
    /// ViewModel for the CardDetailDialog that displays all properties of a hardware node.
    /// Supports Equipment, System, Unit, and Device layers with their specific fields.
    /// </summary>
    public class CardDetailDialogViewModel : ViewModelBase
    {
        private readonly TreeNodeDataDto _treeNodeData;

        /// <summary>
        /// Event raised when the dialog should be closed.
        /// </summary>
        public event EventHandler<DialogCloseEventArgs> RequestClose;

        #region Constructors

        /// <summary>
        /// Creates a new CardDetailDialogViewModel with the given tree node data.
        /// </summary>
        /// <param name="treeNodeData">The tree node data containing hardware information.</param>
        /// <exception cref="ArgumentNullException">Thrown when treeNodeData is null.</exception>
        public CardDetailDialogViewModel(TreeNodeDataDto treeNodeData)
        {
            _treeNodeData = treeNodeData ?? throw new ArgumentNullException(nameof(treeNodeData));
            InitializeCommands();
        }

        /// <summary>
        /// Parameterless constructor for design-time support.
        /// </summary>
        public CardDetailDialogViewModel()
        {
            _treeNodeData = null;
            InitializeCommands();
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to close the dialog.
        /// </summary>
        public ICommand CloseCommand { get; private set; }

        private void InitializeCommands()
        {
            CloseCommand = new RelayCommand(ExecuteClose);
        }

        private void ExecuteClose()
        {
            RequestClose?.Invoke(this, new DialogCloseEventArgs(false));
        }

        #endregion

        #region Hardware Layer

        /// <summary>
        /// Gets the hardware layer type.
        /// </summary>
        public HardwareLayer HardwareLayer => _treeNodeData?.HardwareLayer ?? HardwareLayer.Equipment;

        /// <summary>
        /// Gets the display text for the hardware layer.
        /// </summary>
        public string HardwareLayerDisplayText => HardwareLayer.ToString();

        #endregion

        #region Common Properties

        /// <summary>
        /// Gets the name of the hardware component.
        /// </summary>
        public string Name => GetCommonProperty(
            e => e?.Name,
            s => s?.Name,
            u => u?.Name,
            d => d?.Name
        );

        /// <summary>
        /// Gets the display name of the hardware component.
        /// </summary>
        public string DisplayName => GetCommonProperty(
            e => e?.DisplayName,
            s => s?.DisplayName,
            u => u?.DisplayName,
            d => d?.DisplayName
        );

        /// <summary>
        /// Gets the subname of the hardware component.
        /// </summary>
        public string Subname => GetCommonProperty(
            e => e?.Subname,
            s => s?.Subname,
            u => u?.Subname,
            d => d?.Subname
        );

        /// <summary>
        /// Gets the description of the hardware component.
        /// </summary>
        public string Description => GetCommonProperty(
            e => e?.Description,
            s => s?.Description,
            u => u?.Description,
            d => d?.Description
        );

        /// <summary>
        /// Gets the dialog title (same as Name).
        /// </summary>
        public string DialogTitle => Name;

        private string GetCommonProperty(
            Func<EquipmentDto, string> equipmentSelector,
            Func<SystemDto, string> systemSelector,
            Func<UnitDto, string> unitSelector,
            Func<DeviceDto, string> deviceSelector)
        {
            if (_treeNodeData == null) return string.Empty;

            return HardwareLayer switch
            {
                HardwareLayer.Equipment => equipmentSelector(_treeNodeData.EquipmentData) ?? string.Empty,
                HardwareLayer.System => systemSelector(_treeNodeData.SystemData) ?? string.Empty,
                HardwareLayer.Unit => unitSelector(_treeNodeData.UnitData) ?? string.Empty,
                HardwareLayer.Device => deviceSelector(_treeNodeData.DeviceData) ?? string.Empty,
                _ => string.Empty
            };
        }

        #endregion

        #region Equipment-Specific Properties

        /// <summary>
        /// Gets the equipment type (Equipment layer only).
        /// </summary>
        public string EquipmentType =>
            HardwareLayer == HardwareLayer.Equipment
                ? _treeNodeData?.EquipmentData?.EquipmentType ?? string.Empty
                : string.Empty;

        /// <summary>
        /// Gets the customer name (Equipment layer only).
        /// </summary>
        public string Customer =>
            HardwareLayer == HardwareLayer.Equipment
                ? _treeNodeData?.EquipmentData?.Customer ?? string.Empty
                : string.Empty;

        /// <summary>
        /// Gets the process name (Equipment layer only).
        /// </summary>
        public string Process =>
            HardwareLayer == HardwareLayer.Equipment
                ? _treeNodeData?.EquipmentData?.Process ?? string.Empty
                : string.Empty;

        /// <summary>
        /// Gets the list of attached documents (Equipment layer only).
        /// </summary>
        public IReadOnlyList<string> AttachedDocuments =>
            HardwareLayer == HardwareLayer.Equipment
                ? _treeNodeData?.EquipmentData?.AttachedDocuments ?? new List<string>()
                : new List<string>();

        #endregion

        #region System/Unit/Device Properties

        /// <summary>
        /// Gets the process info (System/Unit layers).
        /// </summary>
        public string ProcessInfo
        {
            get
            {
                if (_treeNodeData == null) return string.Empty;

                return HardwareLayer switch
                {
                    HardwareLayer.System => _treeNodeData.SystemData?.ProcessInfo ?? string.Empty,
                    HardwareLayer.Unit => _treeNodeData.UnitData?.ProcessInfo ?? string.Empty,
                    _ => string.Empty
                };
            }
        }

        /// <summary>
        /// Gets the commands list (System/Unit/Device layers).
        /// </summary>
        public IReadOnlyList<CommandDto> Commands
        {
            get
            {
                if (_treeNodeData == null) return new List<CommandDto>();

                return HardwareLayer switch
                {
                    HardwareLayer.System => _treeNodeData.SystemData?.Commands ?? new List<CommandDto>(),
                    HardwareLayer.Unit => _treeNodeData.UnitData?.Commands ?? new List<CommandDto>(),
                    HardwareLayer.Device => _treeNodeData.DeviceData?.Commands ?? new List<CommandDto>(),
                    _ => new List<CommandDto>()
                };
            }
        }

        #endregion

        #region Device-Specific Properties

        /// <summary>
        /// Gets the device type (Device layer only).
        /// </summary>
        public string DeviceType =>
            HardwareLayer == HardwareLayer.Device
                ? _treeNodeData?.DeviceData?.DeviceType ?? string.Empty
                : string.Empty;

        /// <summary>
        /// Gets the IO information list (Device layer only).
        /// </summary>
        public IReadOnlyList<IoInfoDto> IoInfo =>
            HardwareLayer == HardwareLayer.Device
                ? _treeNodeData?.DeviceData?.IoInfo ?? new List<IoInfoDto>()
                : new List<IoInfoDto>();

        #endregion

        #region Visibility Helpers

        /// <summary>
        /// Gets whether equipment type should be displayed.
        /// </summary>
        public bool HasEquipmentType =>
            HardwareLayer == HardwareLayer.Equipment && !string.IsNullOrEmpty(EquipmentType);

        /// <summary>
        /// Gets whether customer should be displayed.
        /// </summary>
        public bool HasCustomer =>
            HardwareLayer == HardwareLayer.Equipment && !string.IsNullOrEmpty(Customer);

        /// <summary>
        /// Gets whether process information should be displayed.
        /// </summary>
        public bool HasProcess =>
            !string.IsNullOrEmpty(Process) || !string.IsNullOrEmpty(ProcessInfo);

        /// <summary>
        /// Gets whether attached documents should be displayed.
        /// </summary>
        public bool HasAttachedDocuments =>
            AttachedDocuments.Count > 0;

        /// <summary>
        /// Gets whether commands should be displayed.
        /// </summary>
        public bool HasCommands =>
            Commands.Count > 0;

        /// <summary>
        /// Gets whether IO info should be displayed.
        /// </summary>
        public bool HasIoInfo =>
            HardwareLayer == HardwareLayer.Device && IoInfo.Count > 0;

        /// <summary>
        /// Gets whether device type should be displayed.
        /// </summary>
        public bool HasDeviceType =>
            HardwareLayer == HardwareLayer.Device && !string.IsNullOrEmpty(DeviceType);

        #endregion
    }
}
