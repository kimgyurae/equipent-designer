using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EquipmentDesigner.Controls
{
    /// <summary>
    /// A reusable custom context menu with support for nested sub-menus (max depth 4),
    /// section dividers, and intelligent positioning.
    /// </summary>
    public partial class CustomContextMenu : UserControl
    {
        #region Constants

        /// <summary>
        /// Maximum depth of nested sub-menus.
        /// </summary>
        public const int MaxDepth = 4;

        /// <summary>
        /// Delay before closing sub-menu when mouse leaves (milliseconds).
        /// </summary>
        private const int SubMenuCloseDelay = 300;

        #endregion

        #region Dependency Properties

        /// <summary>
        /// Identifies the Items dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(
                nameof(Items),
                typeof(ObservableCollection<CustomContextMenuItem>),
                typeof(CustomContextMenu),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the collection of menu items.
        /// </summary>
        public ObservableCollection<CustomContextMenuItem> Items
        {
            get => (ObservableCollection<CustomContextMenuItem>)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        /// <summary>
        /// Identifies the IsOpen dependency property.
        /// </summary>
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(
                nameof(IsOpen),
                typeof(bool),
                typeof(CustomContextMenu),
                new PropertyMetadata(false, OnIsOpenChanged));

        /// <summary>
        /// Gets or sets whether the context menu is open.
        /// </summary>
        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        /// <summary>
        /// Identifies the MinMenuWidth dependency property.
        /// </summary>
        public static readonly DependencyProperty MinMenuWidthProperty =
            DependencyProperty.Register(
                nameof(MinMenuWidth),
                typeof(double),
                typeof(CustomContextMenu),
                new PropertyMetadata(120.0));

        /// <summary>
        /// Gets or sets the minimum width of the menu.
        /// </summary>
        public double MinMenuWidth
        {
            get => (double)GetValue(MinMenuWidthProperty);
            set => SetValue(MinMenuWidthProperty, value);
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a menu item is clicked.
        /// </summary>
        public event EventHandler<CustomContextMenuItem> ItemClicked;

        /// <summary>
        /// Occurs when the menu is closed.
        /// </summary>
        public event EventHandler Closed;

        #endregion

        #region Fields

        private Popup _rootPopup;
        private Border _menuContainer;
        private StackPanel _itemsPanel;
        private ScrollViewer _scrollViewer;
        private Point _openPosition;
        private double _rootMenuHeight;
        private readonly List<Popup> _subMenuPopups = new List<Popup>();
        private readonly Dictionary<CustomContextMenuItem, Border> _itemContainers = new Dictionary<CustomContextMenuItem, Border>();
        private readonly Dictionary<int, CustomContextMenuItem> _openSubMenuByDepth = new Dictionary<int, CustomContextMenuItem>();
        private System.Windows.Threading.DispatcherTimer _subMenuCloseTimer;
        private CustomContextMenuItem _pendingCloseItem;
        private CustomContextMenuItem _currentOpenSubMenuItem;
        private bool _isApplicationExitRegistered;
        private bool _isGlobalMouseHandlerRegistered;

        #endregion

        #region Constructor

        public CustomContextMenu()
        {
            InitializeComponent();
            Items = new ObservableCollection<CustomContextMenuItem>();

            _subMenuCloseTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(SubMenuCloseDelay)
            };
            _subMenuCloseTimer.Tick += OnSubMenuCloseTimerTick;

            // Register for application exit to cleanup
            RegisterApplicationExitHandler();
        }

        #endregion

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

        private void UnregisterApplicationExitHandler()
        {
            if (!_isApplicationExitRegistered) return;

            if (Application.Current != null)
            {
                Application.Current.Exit -= OnApplicationExit;
                Application.Current.Dispatcher.ShutdownStarted -= OnDispatcherShutdown;
                _isApplicationExitRegistered = false;
            }
        }

        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            ForceCloseAllMenus();
        }

        private void OnDispatcherShutdown(object sender, EventArgs e)
        {
            ForceCloseAllMenus();
        }

        /// <summary>
        /// Forcefully closes all menus without animations.
        /// Used during application shutdown.
        /// </summary>
        private void ForceCloseAllMenus()
        {
            // Unregister global mouse handler
            UnregisterGlobalMouseHandler();

            _subMenuCloseTimer?.Stop();

            // Close all sub-menu popups
            foreach (var popup in _subMenuPopups)
            {
                if (popup != null)
                {
                    popup.IsOpen = false;
                }
            }
            _subMenuPopups.Clear();
            _openSubMenuByDepth.Clear();

            // Close root popup
            if (_rootPopup != null)
            {
                _rootPopup.IsOpen = false;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Opens the context menu at the specified screen position.
        /// </summary>
        /// <param name="screenPosition">Position in screen coordinates.</param>
        public void Open(Point screenPosition)
        {
            _openPosition = screenPosition;
            IsOpen = true;
        }

        /// <summary>
        /// Opens the context menu at the current mouse position.
        /// </summary>
        public void OpenAtMousePosition()
        {
            var mousePos = GetMouseScreenPosition();
            Open(mousePos);
        }

        /// <summary>
        /// Closes the context menu and all sub-menus.
        /// </summary>
        public void Close()
        {
            IsOpen = false;
        }

        #endregion

        #region Private Methods - Menu Creation

        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var menu = (CustomContextMenu)d;
            if ((bool)e.NewValue)
            {
                menu.ShowMenu();
            }
            else
            {
                menu.HideMenu();
            }
        }

        private void ShowMenu()
        {
            CloseAllSubMenus();

            if (_rootPopup == null)
            {
                CreateRootPopup();
            }

            BuildMenuItems(_itemsPanel, Items, 0);

            // Measure the menu to get its size
            _menuContainer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var menuSize = _menuContainer.DesiredSize;

            // Calculate position
            var posResult = ContextMenuPositionHelper.CalculateRootMenuPosition(
                _openPosition, menuSize.Width, menuSize.Height);

            _rootPopup.HorizontalOffset = posResult.X;
            _rootPopup.VerticalOffset = posResult.Y;

            if (posResult.NeedsScrolling)
            {
                _scrollViewer.MaxHeight = posResult.MaxHeight;
                _scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
            else
            {
                _scrollViewer.MaxHeight = double.PositiveInfinity;
                _scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }

            _rootPopup.IsOpen = true;
            _rootMenuHeight = menuSize.Height;

            // Register global mouse handler for outside click detection
            RegisterGlobalMouseHandler();
        }

        private void HideMenu()
        {
            // Unregister global mouse handler
            UnregisterGlobalMouseHandler();

            CloseAllSubMenus();

            if (_rootPopup != null)
            {
                _rootPopup.IsOpen = false;
            }

            _itemContainers.Clear();
            _openSubMenuByDepth.Clear();

            Closed?.Invoke(this, EventArgs.Empty);
        }

        private void CreateRootPopup()
        {
            _scrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                PanningMode = PanningMode.VerticalOnly
            };

            _itemsPanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            _scrollViewer.Content = _itemsPanel;

            _menuContainer = new Border
            {
                Style = (Style)FindResource("ContextMenuContainerStyle"),
                MinWidth = MinMenuWidth,
                Child = _scrollViewer
            };

            _rootPopup = new Popup
            {
                AllowsTransparency = true,
                PopupAnimation = PopupAnimation.None,
                Placement = PlacementMode.Absolute,
                StaysOpen = true, // Changed to true to allow sub-menu focus transitions
                Child = _menuContainer
            };

            _rootPopup.Closed += (s, e) =>
            {
                if (IsOpen)
                {
                    IsOpen = false;
                }
            };
        }

        private void BuildMenuItems(StackPanel panel, IEnumerable<CustomContextMenuItem> items, int depth)
        {
            panel.Children.Clear();
            string lastSection = null;

            foreach (var item in items)
            {
                // Add separator if section changed
                if (!string.IsNullOrEmpty(item.Section) && item.Section != lastSection && lastSection != null)
                {
                    var separator = CreateSeparator();
                    panel.Children.Add(separator);
                }

                if (item.IsSeparator)
                {
                    var separator = CreateSeparator();
                    panel.Children.Add(separator);
                }
                else
                {
                    var itemContainer = CreateMenuItem(item, depth);
                    panel.Children.Add(itemContainer);
                    _itemContainers[item] = itemContainer;
                }

                lastSection = item.Section;
            }
        }

        private Border CreateSeparator()
        {
            return new Border
            {
                Style = (Style)FindResource("ContextMenuSeparatorStyle")
            };
        }

        private Border CreateMenuItem(CustomContextMenuItem item, int depth)
        {
            var container = new Border
            {
                Style = (Style)FindResource("ContextMenuItemContainerStyle"),
                Tag = item,
                IsEnabled = item.IsEnabled,
                Opacity = item.IsEnabled ? 1.0 : 0.5
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Text
            var textBlock = new TextBlock
            {
                Text = item.Header,
                Style = item.ItemType == MenuItemType.Destructive
                    ? (Style)FindResource("ContextMenuTextDestructiveStyle")
                    : (Style)FindResource("ContextMenuTextStyle"),
                VerticalAlignment = VerticalAlignment.Center
            };

            if (!item.IsEnabled)
            {
                textBlock.Style = (Style)FindResource("ContextMenuTextDisabledStyle");
            }

            Grid.SetColumn(textBlock, 0);
            grid.Children.Add(textBlock);

            // Chevron for items with children (and within depth limit)
            if (item.HasChildren && depth < MaxDepth - 1)
            {
                var chevronContainer = new Border
                {
                    Width = 16,
                    Height = 16,
                    Margin = new Thickness(8, 0, 0, 0)
                };

                var chevron = new Path
                {
                    Style = (Style)FindResource("ChevronPathStyle")
                };

                chevronContainer.Child = chevron;
                Grid.SetColumn(chevronContainer, 1);
                grid.Children.Add(chevronContainer);
            }

            container.Child = grid;

            // Event handlers
            container.MouseEnter += (s, e) => OnItemMouseEnter(item, container, depth);
            container.MouseLeave += (s, e) => OnItemMouseLeave(item);
            container.MouseLeftButtonUp += (s, e) => OnItemClick(item, e);

            return container;
        }

        #endregion

        #region Private Methods - Sub Menu Management

        private void OnItemMouseEnter(CustomContextMenuItem item, Border container, int depth)
        {
            _subMenuCloseTimer.Stop();
            _pendingCloseItem = null; // Clear pending close when entering a new item

            // Check if there's an open sub-menu at this depth from a different item
            // If so, close that sub-menu (at index 'depth' in _subMenuPopups) and all deeper ones
            if (_openSubMenuByDepth.TryGetValue(depth, out var openItem) && openItem != item)
            {
                // Close the existing sub-menu at this depth and all its children
                // _subMenuPopups[depth] contains the sub-menu opened by the item at this depth
                CloseSubMenusFromDepth(depth);
            }

            if (item.HasChildren && depth < MaxDepth - 1)
            {
                // Open sub-menu immediately on hover
                OpenSubMenu(item);
            }
            else
            {
                // Item has no children, so close any sub-menus from this depth
                if (_openSubMenuByDepth.ContainsKey(depth))
                {
                    CloseSubMenusFromDepth(depth);
                    _currentOpenSubMenuItem = null;
                }
            }
        }

        private void OnItemMouseLeave(CustomContextMenuItem item)
        {
            // When mouse leaves an item with an open sub-menu, track it for explicit closing
            if (item.HasChildren && item.IsSubMenuOpen)
            {
                _pendingCloseItem = item;
                _subMenuCloseTimer.Start();
            }
        }

        private void OnSubMenuCloseTimerTick(object sender, EventArgs e)
        {
            _subMenuCloseTimer.Stop();

            // If we have a specific item whose sub-menu should be closed
            if (_pendingCloseItem != null)
            {
                var item = _pendingCloseItem;
                _pendingCloseItem = null;

                // Check if mouse is over the item or its sub-menu hierarchy
                if (IsMouseOverItemOrSubMenu(item))
                {
                    return;
                }

                // Close this item's sub-menu and all deeper ones
                int depth = GetItemDepth(item);
                if (depth >= 0)
                {
                    CloseSubMenusFromDepth(depth);
                }
                _currentOpenSubMenuItem = null;
            }
            else
            {
                // Fallback: close all if mouse is not over any sub-menu
                if (!IsMouseOverAnySubMenu())
                {
                    CloseAllSubMenus();
                    _currentOpenSubMenuItem = null;
                }
            }
        }

        /// <summary>
        /// Checks if the mouse is over the specified item or any of its open sub-menus.
        /// </summary>
        private bool IsMouseOverItemOrSubMenu(CustomContextMenuItem item)
        {
            // Check if mouse is over the item itself
            if (_itemContainers.TryGetValue(item, out var container) && container.IsMouseOver)
            {
                return true;
            }

            // Check if mouse is over this item's sub-menu or any deeper sub-menus
            int depth = GetItemDepth(item);
            if (depth >= 0)
            {
                for (int i = depth; i < _subMenuPopups.Count; i++)
                {
                    if (_subMenuPopups[i]?.Child is FrameworkElement element && element.IsMouseOver)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void OpenSubMenu(CustomContextMenuItem parentItem)
        {
            if (!_itemContainers.TryGetValue(parentItem, out var parentContainer))
                return;

            // Find the depth of this item
            int depth = GetItemDepth(parentItem);
            if (depth >= MaxDepth - 1) return;

            // ROBUSTNESS: Always close all sub-menus at this depth and deeper before opening
            // This handles edge cases where multiple menus might be open at the same depth
            // even though it shouldn't happen under normal circumstances
            CloseSubMenusFromDepth(depth);

            // Create sub-menu popup
            var subMenuScrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
            };

            var subMenuPanel = new StackPanel { Orientation = Orientation.Vertical };
            BuildMenuItems(subMenuPanel, parentItem.Children, depth + 1);
            subMenuScrollViewer.Content = subMenuPanel;

            var subMenuContainer = new Border
            {
                Style = (Style)FindResource("ContextMenuContainerStyle"),
                MinWidth = MinMenuWidth,
                Child = subMenuScrollViewer
            };

            var subMenuPopup = new Popup
            {
                AllowsTransparency = true,
                PopupAnimation = PopupAnimation.None,
                Placement = PlacementMode.Absolute,
                StaysOpen = true,
                Child = subMenuContainer
            };

            // Measure sub-menu
            subMenuContainer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var subMenuSize = subMenuContainer.DesiredSize;

            // Get parent bounds
            var parentItemBounds = ContextMenuPositionHelper.GetElementScreenBounds(parentContainer);
            var parentMenuBounds = ContextMenuPositionHelper.GetElementScreenBounds(
                GetParentMenuContainer(parentContainer));

            // Calculate position
            var posResult = ContextMenuPositionHelper.CalculateSubMenuPosition(
                parentItemBounds,
                parentMenuBounds,
                _rootMenuHeight,
                subMenuSize.Width,
                subMenuSize.Height,
                SubMenuDirection.Right);

            subMenuPopup.HorizontalOffset = posResult.X;
            subMenuPopup.VerticalOffset = posResult.Y;

            if (posResult.NeedsScrolling)
            {
                subMenuScrollViewer.MaxHeight = posResult.MaxHeight;
                subMenuScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            }

            // Track sub-menu
            while (_subMenuPopups.Count <= depth)
            {
                _subMenuPopups.Add(null);
            }
            _subMenuPopups[depth] = subMenuPopup;

            // Track which item has open sub-menu at this depth
            _openSubMenuByDepth[depth] = parentItem;

            // Handle mouse events for sub-menu
            subMenuContainer.MouseEnter += (s, e) =>
            {
                _subMenuCloseTimer.Stop();
                _pendingCloseItem = null;
            };
            subMenuContainer.MouseLeave += (s, e) =>
            {
                if (!IsMouseOverParentItem(parentItem))
                {
                    _pendingCloseItem = parentItem;
                    _subMenuCloseTimer.Start();
                }
            };

            subMenuPopup.IsOpen = true;
            _currentOpenSubMenuItem = parentItem;
            parentItem.IsSubMenuOpen = true;
        }

        private void CloseSubMenusFromDepth(int depth)
        {
            for (int i = _subMenuPopups.Count - 1; i >= depth; i--)
            {
                if (_subMenuPopups[i] != null)
                {
                    _subMenuPopups[i].IsOpen = false;
                    _subMenuPopups[i] = null;
                }
                
                // Also clean up the depth tracking
                if (_openSubMenuByDepth.ContainsKey(i))
                {
                    var item = _openSubMenuByDepth[i];
                    item.IsSubMenuOpen = false;
                    _openSubMenuByDepth.Remove(i);
                }
            }
        }

        private void CloseAllSubMenus()
        {
            CloseSubMenusFromDepth(0);
            _openSubMenuByDepth.Clear();
        }

        private int GetItemDepth(CustomContextMenuItem item)
        {
            return FindItemDepth(Items, item, 0);
        }

        private int FindItemDepth(IEnumerable<CustomContextMenuItem> items, CustomContextMenuItem target, int currentDepth)
        {
            foreach (var item in items)
            {
                if (item == target) return currentDepth;
                if (item.HasChildren)
                {
                    int foundDepth = FindItemDepth(item.Children, target, currentDepth + 1);
                    if (foundDepth >= 0) return foundDepth;
                }
            }
            return -1;
        }

        private Border GetParentMenuContainer(Border itemContainer)
        {
            DependencyObject current = itemContainer;
            while (current != null)
            {
                if (current is Border border && border.Style == (Style)FindResource("ContextMenuContainerStyle"))
                {
                    return border;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return _menuContainer;
        }

        private bool IsMouseOverAnySubMenu()
        {
            foreach (var popup in _subMenuPopups)
            {
                if (popup?.Child is FrameworkElement element && element.IsMouseOver)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsMouseOverParentItem(CustomContextMenuItem item)
        {
            if (_itemContainers.TryGetValue(item, out var container))
            {
                return container.IsMouseOver;
            }
            return false;
        }

        #endregion

        #region Private Methods - Click Handling

        private void OnItemClick(CustomContextMenuItem item, MouseButtonEventArgs e)
        {
            if (!item.IsEnabled) return;

            // If item has children, do nothing - sub-menu is handled by hover only
            if (item.HasChildren)
            {
                e.Handled = true;
                return;
            }

            // Execute command if available
            if (item.Command?.CanExecute(item.CommandParameter) == true)
            {
                item.Command.Execute(item.CommandParameter);
            }

            ItemClicked?.Invoke(this, item);

            // Close menu after click
            Close();
            e.Handled = true;
        }

        #endregion

        #region Private Methods - Mouse Position

        private Point GetMouseScreenPosition()
        {
            var point = Mouse.GetPosition(this);
            return PointToScreen(point);
        }

        #endregion

        #region Private Methods - Global Mouse Handler

        private void RegisterGlobalMouseHandler()
        {
            if (_isGlobalMouseHandlerRegistered) return;

            Mouse.AddPreviewMouseDownHandler(Application.Current.MainWindow, OnGlobalMouseDown);
            _isGlobalMouseHandlerRegistered = true;
        }

        private void UnregisterGlobalMouseHandler()
        {
            if (!_isGlobalMouseHandlerRegistered) return;

            if (Application.Current?.MainWindow != null)
            {
                Mouse.RemovePreviewMouseDownHandler(Application.Current.MainWindow, OnGlobalMouseDown);
            }
            _isGlobalMouseHandlerRegistered = false;
        }

        private void OnGlobalMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsOpen) return;

            // Check if click is inside any menu (root or sub-menus)
            if (IsMouseOverAnyMenu())
            {
                return;
            }

            // Click is outside all menus, close the context menu
            Close();
        }

        private bool IsMouseOverAnyMenu()
        {
            // Check root menu
            if (_menuContainer?.IsMouseOver == true) return true;

            // Check all sub-menu popups
            foreach (var popup in _subMenuPopups)
            {
                if (popup?.Child is FrameworkElement element && element.IsMouseOver)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }

    #region Converters

    /// <summary>
    /// Converts boolean false to Visible and true to Collapsed.
    /// </summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility != Visibility.Visible;
            }
            return false;
        }
    }

    /// <summary>
    /// Converts MenuItemType to appropriate foreground brush.
    /// </summary>
    public class MenuItemTypeToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MenuItemType itemType)
            {
                return itemType == MenuItemType.Destructive
                    ? Application.Current.FindResource("Brush.Status.Danger")
                    : Application.Current.FindResource("Brush.Text.Primary");
            }
            return Application.Current.FindResource("Brush.Text.Primary");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}