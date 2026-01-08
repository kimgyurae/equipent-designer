using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace EquipmentDesigner.Views.ReusableComponents.ContextMenu
{
    /// <summary>
    /// Represents the type of a context menu item.
    /// </summary>
    public enum MenuItemType
    {
        /// <summary>
        /// Standard clickable menu item.
        /// </summary>
        Normal,

        /// <summary>
        /// Destructive action (displayed in danger color).
        /// </summary>
        Destructive,

        /// <summary>
        /// Section divider/separator.
        /// </summary>
        Separator
    }

    /// <summary>
    /// Data model for a context menu item with support for nested sub-menus (max depth 4).
    /// </summary>
    public class CustomContextMenuItem : INotifyPropertyChanged
    {
        private string _header;
        private MenuItemType _itemType = MenuItemType.Normal;
        private ICommand _command;
        private object _commandParameter;
        private bool _isEnabled = true;
        private bool _isSubMenuOpen;
        private string _section;
        private ObservableCollection<CustomContextMenuItem> _children;

        /// <summary>
        /// Gets or sets the display text for this menu item.
        /// </summary>
        public string Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        /// <summary>
        /// Gets or sets the type of menu item (Normal, Destructive, or Separator).
        /// </summary>
        public MenuItemType ItemType
        {
            get => _itemType;
            set => SetProperty(ref _itemType, value);
        }

        /// <summary>
        /// Gets or sets the command to execute when this item is clicked.
        /// </summary>
        public ICommand Command
        {
            get => _command;
            set => SetProperty(ref _command, value);
        }

        /// <summary>
        /// Gets or sets the parameter to pass to the command.
        /// </summary>
        public object CommandParameter
        {
            get => _commandParameter;
            set => SetProperty(ref _commandParameter, value);
        }

        /// <summary>
        /// Gets or sets whether this menu item is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        /// <summary>
        /// Gets or sets whether the sub-menu is currently open.
        /// </summary>
        public bool IsSubMenuOpen
        {
            get => _isSubMenuOpen;
            set => SetProperty(ref _isSubMenuOpen, value);
        }

        /// <summary>
        /// Gets or sets the section identifier. When section changes between items, a divider is drawn.
        /// </summary>
        public string Section
        {
            get => _section;
            set => SetProperty(ref _section, value);
        }

        /// <summary>
        /// Gets or sets the child menu items (sub-menu).
        /// </summary>
        public ObservableCollection<CustomContextMenuItem> Children
        {
            get => _children;
            set => SetProperty(ref _children, value);
        }

        /// <summary>
        /// Gets whether this item has children (sub-menu).
        /// </summary>
        public bool HasChildren => Children != null && Children.Count > 0;

        /// <summary>
        /// Gets whether this item is a separator.
        /// </summary>
        public bool IsSeparator => ItemType == MenuItemType.Separator;

        /// <summary>
        /// Creates a new context menu item.
        /// </summary>
        public CustomContextMenuItem()
        {
            Children = new ObservableCollection<CustomContextMenuItem>();
        }

        /// <summary>
        /// Creates a new context menu item with the specified header.
        /// </summary>
        /// <param name="header">The display text.</param>
        public CustomContextMenuItem(string header) : this()
        {
            Header = header;
        }

        /// <summary>
        /// Creates a new context menu item with header and command.
        /// </summary>
        /// <param name="header">The display text.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="commandParameter">Optional command parameter.</param>
        public CustomContextMenuItem(string header, ICommand command, object commandParameter = null) : this(header)
        {
            Command = command;
            CommandParameter = commandParameter;
        }

        /// <summary>
        /// Creates a separator menu item.
        /// </summary>
        /// <returns>A separator CustomContextMenuItem.</returns>
        public static CustomContextMenuItem CreateSeparator()
        {
            return new CustomContextMenuItem
            {
                ItemType = MenuItemType.Separator
            };
        }

        /// <summary>
        /// Creates a destructive (danger) menu item.
        /// </summary>
        /// <param name="header">The display text.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="commandParameter">Optional command parameter.</param>
        /// <returns>A destructive CustomContextMenuItem.</returns>
        public static CustomContextMenuItem CreateDestructive(string header, ICommand command = null, object commandParameter = null)
        {
            return new CustomContextMenuItem(header, command, commandParameter)
            {
                ItemType = MenuItemType.Destructive
            };
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}
