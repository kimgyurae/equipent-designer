using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace EquipmentDesigner.Controls
{
    /// <summary>
    /// Service for easily creating and showing context menus throughout the application.
    /// Provides a fluent API for building menu structures.
    /// </summary>
    public class ContextMenuService
    {
        private static readonly Lazy<ContextMenuService> _instance =
            new Lazy<ContextMenuService>(() => new ContextMenuService());

        /// <summary>
        /// Gets the singleton instance of the ContextMenuService.
        /// </summary>
        public static ContextMenuService Instance => _instance.Value;

        private CustomContextMenu _currentMenu;
        private object _currentOwner;
        private bool _isApplicationExitRegistered;
        private bool _isWindowEventHandlersRegistered;

        private ContextMenuService()
        {
            RegisterApplicationExitHandler();
        }

        #region Application Lifecycle

        private void RegisterApplicationExitHandler()
        {
            if (_isApplicationExitRegistered) return;

            if (Application.Current != null)
            {
                Application.Current.Exit += OnApplicationExit;
                Application.Current.Dispatcher.ShutdownStarted += OnDispatcherShutdown;
                _isApplicationExitRegistered = true;
            }
        }

        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            Close();
        }

        private void OnDispatcherShutdown(object sender, EventArgs e)
        {
            Close();
        }

        #endregion

        #region Window Event Handling

        /// <summary>
        /// Registers for window events to close menus when window state changes.
        /// Includes: deactivation, state change (minimize/maximize), location change, size change.
        /// </summary>
        private void RegisterWindowEventHandlers()
        {
            if (_isWindowEventHandlersRegistered) return;

            var mainWindow = Application.Current?.MainWindow;
            if (mainWindow != null)
            {
                mainWindow.Deactivated += OnWindowDeactivated;
                mainWindow.StateChanged += OnWindowStateChanged;
                mainWindow.LocationChanged += OnWindowLocationChanged;
                mainWindow.SizeChanged += OnWindowSizeChanged;
                _isWindowEventHandlersRegistered = true;
            }
        }

        /// <summary>
        /// Unregisters from window events.
        /// </summary>
        private void UnregisterWindowEventHandlers()
        {
            if (!_isWindowEventHandlersRegistered) return;

            var mainWindow = Application.Current?.MainWindow;
            if (mainWindow != null)
            {
                mainWindow.Deactivated -= OnWindowDeactivated;
                mainWindow.StateChanged -= OnWindowStateChanged;
                mainWindow.LocationChanged -= OnWindowLocationChanged;
                mainWindow.SizeChanged -= OnWindowSizeChanged;
            }
            _isWindowEventHandlersRegistered = false;
        }

        /// <summary>
        /// Handles window deactivation (e.g., Alt+Tab to another application).
        /// Closes all open context menus.
        /// </summary>
        private void OnWindowDeactivated(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handles window state changes (minimize, maximize, restore).
        /// Closes all open context menus.
        /// </summary>
        private void OnWindowStateChanged(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handles window location changes (moving the window).
        /// Closes all open context menus.
        /// </summary>
        private void OnWindowLocationChanged(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handles window size changes (resizing the window).
        /// Closes all open context menus.
        /// </summary>
        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Close();
        }

        #endregion

        /// <summary>
        /// Gets whether a context menu is currently open.
        /// </summary>
        public bool IsOpen => _currentMenu != null;

        /// <summary>
        /// Checks if the menu opened by the specified owner is currently open.
        /// </summary>
        /// <param name="owner">The owner element that opened the menu.</param>
        /// <returns>True if the menu from this owner is open.</returns>
        public bool IsOpenFrom(object owner)
        {
            return _currentMenu != null && _currentOwner == owner;
        }

        /// <summary>
        /// Creates a new context menu builder.
        /// </summary>
        /// <returns>A new ContextMenuBuilder instance.</returns>
        public ContextMenuBuilder Create()
        {
            return new ContextMenuBuilder(this);
        }

        /// <summary>
        /// Shows a context menu with the specified items at the mouse position.
        /// </summary>
        /// <param name="items">The menu items to display.</param>
        /// <param name="onItemClicked">Optional callback when an item is clicked.</param>
        public void Show(
            ObservableCollection<CustomContextMenuItem> items,
            Action<CustomContextMenuItem> onItemClicked = null)
        {
            var mousePos = GetMouseScreenPosition();
            Show(items, mousePos, onItemClicked);
        }

        /// <summary>
        /// Shows a context menu with the specified items at a specific position.
        /// </summary>
        /// <param name="items">The menu items to display.</param>
        /// <param name="screenPosition">Position in screen coordinates.</param>
        /// <param name="onItemClicked">Optional callback when an item is clicked.</param>
        /// <param name="owner">Optional owner element for toggle support.</param>
        public void Show(
            ObservableCollection<CustomContextMenuItem> items,
            Point screenPosition,
            Action<CustomContextMenuItem> onItemClicked = null,
            object owner = null)
        {
            // If the same owner is requesting and menu is already open, close it (toggle behavior)
            if (owner != null && IsOpenFrom(owner))
            {
                Close();
                return;
            }

            // Close existing menu
            Close();

            _currentMenu = new CustomContextMenu
            {
                Items = items
            };
            _currentOwner = owner;

            if (onItemClicked != null)
            {
                _currentMenu.ItemClicked += (s, item) => onItemClicked(item);
            }

            // Register for window deactivation to close menu when window loses focus
            RegisterWindowEventHandlers();

            _currentMenu.Open(screenPosition);
        }

        /// <summary>
        /// Closes the currently open context menu.
        /// </summary>
        public void Close()
        {
            // Unregister window deactivation handler
            UnregisterWindowEventHandlers();

            _currentMenu?.Close();
            _currentMenu = null;
            _currentOwner = null;
        }

        private Point GetMouseScreenPosition()
        {
            // Get physical screen pixels from WinForms
            var physicalPoint = System.Windows.Forms.Control.MousePosition;

            // Convert physical pixels to WPF logical pixels (DIU)
            // WPF Popup with PlacementMode.Absolute expects logical pixels
            var source = PresentationSource.FromVisual(Application.Current.MainWindow);
            if (source?.CompositionTarget != null)
            {
                var dpiX = source.CompositionTarget.TransformToDevice.M11;
                var dpiY = source.CompositionTarget.TransformToDevice.M22;

                return new Point(physicalPoint.X / dpiX, physicalPoint.Y / dpiY);
            }

            return new Point(physicalPoint.X, physicalPoint.Y);
        }
    }

    /// <summary>
    /// Fluent builder for creating context menus.
    /// </summary>
    public class ContextMenuBuilder
    {
        private readonly ContextMenuService _service;
        private readonly ObservableCollection<CustomContextMenuItem> _items;
        private string _currentSection;
        private Action<CustomContextMenuItem> _onItemClicked;

        internal ContextMenuBuilder(ContextMenuService service)
        {
            _service = service;
            _items = new ObservableCollection<CustomContextMenuItem>();
        }

        /// <summary>
        /// Adds a menu item.
        /// </summary>
        /// <param name="header">The display text.</param>
        /// <param name="command">Optional command to execute.</param>
        /// <param name="commandParameter">Optional command parameter.</param>
        /// <param name="isEnabled">Whether the menu item is enabled. Default is true.</param>
        /// <returns>This builder for chaining.</returns>
        public ContextMenuBuilder AddItem(string header, ICommand command = null, object commandParameter = null, bool isEnabled = true)
        {
            var item = new CustomContextMenuItem(header, command, commandParameter)
            {
                Section = _currentSection,
                IsEnabled = isEnabled
            };
            _items.Add(item);
            return this;
        }

        /// <summary>
        /// Adds a menu item with a click action.
        /// </summary>
        /// <param name="header">The display text.</param>
        /// <param name="onClick">Action to execute on click.</param>
        /// <param name="isEnabled">Whether the menu item is enabled. Default is true.</param>
        /// <returns>This builder for chaining.</returns>
        public ContextMenuBuilder AddItem(string header, Action onClick, bool isEnabled = true)
        {
            var command = new RelayCommand(_ => onClick());
            return AddItem(header, command, null, isEnabled);
        }

        /// <summary>
        /// Adds a menu item with a click action and keyboard shortcut display.
        /// </summary>
        /// <param name="header">The display text.</param>
        /// <param name="onClick">Action to execute on click.</param>
        /// <param name="shortcut">Keyboard shortcut to display (e.g., "Ctrl+C"). Display only.</param>
        /// <param name="isEnabled">Whether the menu item is enabled. Default is true.</param>
        /// <returns>This builder for chaining.</returns>
        public ContextMenuBuilder AddItem(string header, Action onClick, string shortcut, bool isEnabled = true)
        {
            var command = new RelayCommand(_ => onClick());
            var item = new CustomContextMenuItem(header, command, null)
            {
                Section = _currentSection,
                IsEnabled = isEnabled,
                Shortcut = shortcut
            };
            _items.Add(item);
            return this;
        }

        /// <summary>
        /// Adds a menu item with a parameterized click action.
        /// </summary>
        /// <param name="header">The display text.</param>
        /// <param name="onClick">Action to execute on click with parameter.</param>
        /// <param name="parameter">Parameter to pass to the action.</param>
        /// <param name="isEnabled">Whether the menu item is enabled. Default is true.</param>
        /// <returns>This builder for chaining.</returns>
        public ContextMenuBuilder AddItem<T>(string header, Action<T> onClick, T parameter, bool isEnabled = true)
        {
            var command = new RelayCommand(_ => onClick(parameter));
            return AddItem(header, command, null, isEnabled);
        }

        /// <summary>
        /// Adds a destructive (danger) menu item.
        /// </summary>
        /// <param name="header">The display text.</param>
        /// <param name="command">Optional command to execute.</param>
        /// <param name="commandParameter">Optional command parameter.</param>
        /// <param name="isEnabled">Whether the menu item is enabled. Default is true.</param>
        /// <returns>This builder for chaining.</returns>
        public ContextMenuBuilder AddDestructiveItem(string header, ICommand command = null, object commandParameter = null, bool isEnabled = true)
        {
            var item = CustomContextMenuItem.CreateDestructive(header, command, commandParameter);
            item.Section = _currentSection;
            item.IsEnabled = isEnabled;
            _items.Add(item);
            return this;
        }

        /// <summary>
        /// Adds a destructive (danger) menu item with a click action.
        /// </summary>
        /// <param name="header">The display text.</param>
        /// <param name="onClick">Action to execute on click.</param>
        /// <param name="isEnabled">Whether the menu item is enabled. Default is true.</param>
        /// <returns>This builder for chaining.</returns>
        public ContextMenuBuilder AddDestructiveItem(string header, Action onClick, bool isEnabled = true)
        {
            var command = new RelayCommand(_ => onClick());
            return AddDestructiveItem(header, command, null, isEnabled);
        }

        /// <summary>
        /// Adds a destructive (danger) menu item with a click action and keyboard shortcut display.
        /// </summary>
        /// <param name="header">The display text.</param>
        /// <param name="onClick">Action to execute on click.</param>
        /// <param name="shortcut">Keyboard shortcut to display (e.g., "Del"). Display only.</param>
        /// <param name="isEnabled">Whether the menu item is enabled. Default is true.</param>
        /// <returns>This builder for chaining.</returns>
        public ContextMenuBuilder AddDestructiveItem(string header, Action onClick, string shortcut, bool isEnabled = true)
        {
            var command = new RelayCommand(_ => onClick());
            var item = CustomContextMenuItem.CreateDestructive(header, command, null);
            item.Section = _currentSection;
            item.IsEnabled = isEnabled;
            item.Shortcut = shortcut;
            _items.Add(item);
            return this;
        }

        /// <summary>
        /// Adds a separator.
        /// </summary>
        /// <returns>This builder for chaining.</returns>
        public ContextMenuBuilder AddSeparator()
        {
            _items.Add(CustomContextMenuItem.CreateSeparator());
            return this;
        }

        /// <summary>
        /// Starts a new section. Items added after this will be in the new section.
        /// A divider is automatically drawn between sections.
        /// </summary>
        /// <param name="sectionName">Name identifier for the section.</param>
        /// <returns>This builder for chaining.</returns>
        public ContextMenuBuilder BeginSection(string sectionName = null)
        {
            _currentSection = sectionName ?? Guid.NewGuid().ToString();
            return this;
        }

        /// <summary>
        /// Adds a sub-menu.
        /// </summary>
        /// <param name="header">The display text for the parent item.</param>
        /// <param name="configure">Action to configure the sub-menu items.</param>
        /// <returns>This builder for chaining.</returns>
        public ContextMenuBuilder AddSubMenu(string header, Action<SubMenuBuilder> configure)
        {
            var item = new CustomContextMenuItem(header)
            {
                Section = _currentSection
            };

            var subBuilder = new SubMenuBuilder(item);
            configure(subBuilder);

            _items.Add(item);
            return this;
        }

        /// <summary>
        /// Sets a callback for when any item is clicked.
        /// </summary>
        /// <param name="callback">The callback action.</param>
        /// <returns>This builder for chaining.</returns>
        public ContextMenuBuilder OnItemClicked(Action<CustomContextMenuItem> callback)
        {
            _onItemClicked = callback;
            return this;
        }

        /// <summary>
        /// Shows the context menu at the current mouse position.
        /// </summary>
        public void Show()
        {
            _service.Show(_items, _onItemClicked);
        }

        /// <summary>
        /// Shows the context menu at the specified screen position.
        /// </summary>
        /// <param name="screenPosition">Position in screen coordinates.</param>
        public void ShowAt(Point screenPosition)
        {
            _service.Show(_items, screenPosition, _onItemClicked);
        }

        /// <summary>
        /// Shows the context menu at the specified screen position with owner for toggle support.
        /// </summary>
        /// <param name="screenPosition">Position in screen coordinates.</param>
        /// <param name="owner">The owner element for toggle support. If the same owner calls this again while open, the menu will close.</param>
        public void ShowAt(Point screenPosition, object owner)
        {
            _service.Show(_items, screenPosition, _onItemClicked, owner);
        }

        /// <summary>
        /// Gets the built items without showing the menu.
        /// </summary>
        /// <returns>The collection of menu items.</returns>
        public ObservableCollection<CustomContextMenuItem> Build()
        {
            return _items;
        }
    }

    /// <summary>
    /// Builder for sub-menu items.
    /// </summary>
    public class SubMenuBuilder
    {
        private readonly CustomContextMenuItem _parentItem;
        private string _currentSection;

        internal SubMenuBuilder(CustomContextMenuItem parentItem)
        {
            _parentItem = parentItem;
        }

        /// <summary>
        /// Adds a menu item to the sub-menu.
        /// </summary>
        /// <param name="header">The display text.</param>
        /// <param name="command">Optional command to execute.</param>
        /// <param name="commandParameter">Optional command parameter.</param>
        /// <param name="isEnabled">Whether the menu item is enabled. Default is true.</param>
        /// <returns>This builder for chaining.</returns>
        public SubMenuBuilder AddItem(string header, ICommand command = null, object commandParameter = null, bool isEnabled = true)
        {
            var item = new CustomContextMenuItem(header, command, commandParameter)
            {
                Section = _currentSection,
                IsEnabled = isEnabled
            };
            _parentItem.Children.Add(item);
            return this;
        }

        /// <summary>
        /// Adds a menu item with a click action.
        /// </summary>
        /// <param name="header">The display text.</param>
        /// <param name="onClick">Action to execute on click.</param>
        /// <param name="isEnabled">Whether the menu item is enabled. Default is true.</param>
        /// <returns>This builder for chaining.</returns>
        public SubMenuBuilder AddItem(string header, Action onClick, bool isEnabled = true)
        {
            var command = new RelayCommand(_ => onClick());
            return AddItem(header, command, null, isEnabled);
        }

        /// <summary>
        /// Adds a menu item with a click action and keyboard shortcut display.
        /// </summary>
        /// <param name="header">The display text.</param>
        /// <param name="onClick">Action to execute on click.</param>
        /// <param name="shortcut">Keyboard shortcut to display (e.g., "Ctrl+C"). Display only.</param>
        /// <param name="isEnabled">Whether the menu item is enabled. Default is true.</param>
        /// <returns>This builder for chaining.</returns>
        public SubMenuBuilder AddItem(string header, Action onClick, string shortcut, bool isEnabled = true)
        {
            var command = new RelayCommand(_ => onClick());
            var item = new CustomContextMenuItem(header, command, null)
            {
                Section = _currentSection,
                IsEnabled = isEnabled,
                Shortcut = shortcut
            };
            _parentItem.Children.Add(item);
            return this;
        }

        /// <summary>
        /// Adds a destructive menu item.
        /// </summary>
        /// <param name="header">The display text.</param>
        /// <param name="command">Optional command to execute.</param>
        /// <param name="commandParameter">Optional command parameter.</param>
        /// <param name="isEnabled">Whether the menu item is enabled. Default is true.</param>
        /// <returns>This builder for chaining.</returns>
        public SubMenuBuilder AddDestructiveItem(string header, ICommand command = null, object commandParameter = null, bool isEnabled = true)
        {
            var item = CustomContextMenuItem.CreateDestructive(header, command, commandParameter);
            item.Section = _currentSection;
            item.IsEnabled = isEnabled;
            _parentItem.Children.Add(item);
            return this;
        }

        /// <summary>
        /// Adds a destructive menu item with a click action.
        /// </summary>
        /// <param name="header">The display text.</param>
        /// <param name="onClick">Action to execute on click.</param>
        /// <param name="isEnabled">Whether the menu item is enabled. Default is true.</param>
        /// <returns>This builder for chaining.</returns>
        public SubMenuBuilder AddDestructiveItem(string header, Action onClick, bool isEnabled = true)
        {
            var command = new RelayCommand(_ => onClick());
            return AddDestructiveItem(header, command, null, isEnabled);
        }

        /// <summary>
        /// Adds a destructive menu item with a click action and keyboard shortcut display.
        /// </summary>
        /// <param name="header">The display text.</param>
        /// <param name="onClick">Action to execute on click.</param>
        /// <param name="shortcut">Keyboard shortcut to display (e.g., "Del"). Display only.</param>
        /// <param name="isEnabled">Whether the menu item is enabled. Default is true.</param>
        /// <returns>This builder for chaining.</returns>
        public SubMenuBuilder AddDestructiveItem(string header, Action onClick, string shortcut, bool isEnabled = true)
        {
            var command = new RelayCommand(_ => onClick());
            var item = CustomContextMenuItem.CreateDestructive(header, command, null);
            item.Section = _currentSection;
            item.IsEnabled = isEnabled;
            item.Shortcut = shortcut;
            _parentItem.Children.Add(item);
            return this;
        }

        /// <summary>
        /// Adds a separator.
        /// </summary>
        public SubMenuBuilder AddSeparator()
        {
            _parentItem.Children.Add(CustomContextMenuItem.CreateSeparator());
            return this;
        }

        /// <summary>
        /// Starts a new section.
        /// </summary>
        public SubMenuBuilder BeginSection(string sectionName = null)
        {
            _currentSection = sectionName ?? Guid.NewGuid().ToString();
            return this;
        }

        /// <summary>
        /// Adds a nested sub-menu (up to max depth).
        /// </summary>
        public SubMenuBuilder AddSubMenu(string header, Action<SubMenuBuilder> configure)
        {
            var item = new CustomContextMenuItem(header)
            {
                Section = _currentSection
            };

            var subBuilder = new SubMenuBuilder(item);
            configure(subBuilder);

            _parentItem.Children.Add(item);
            return this;
        }
    }

    /// <summary>
    /// Simple relay command implementation for action-based menu items.
    /// </summary>
    internal class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object parameter) => _execute(parameter);
    }
}