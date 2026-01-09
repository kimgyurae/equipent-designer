using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using EquipmentDesigner.Controls;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models.ProcessEditor;
using EquipmentDesigner.Resources;
using EquipmentDesigner.Services;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// ViewModel for Process definition view with UML workflow editor.
    /// </summary>
    public class DrawboardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _showBackButton;
        private PackMlState _selectedState = PackMlState.Idle;
        private DrawboardTool _selectedTool;
        private bool _isToolLockEnabled;
        private string _currentHint = string.Empty;
        private int _zoomLevel = 100;
        private bool _canUndo;
        private bool _canRedo;
        private DrawboardToolCursorType _currentCursorType = DrawboardToolCursorType.Default;
        private int _nextZIndex = 1;
        private bool _isDrawing;
        private Point _drawStartPoint;
        private DrawingElement _previewElement;

        // Edit mode state
        private DrawingElement _selectedElement;
        private EditModeState _editModeState = EditModeState.None;
        private ResizeHandleType _activeResizeHandle = ResizeHandleType.None;
        private Point _editDragStartPoint;
        private Rect _originalBounds;
        private double _originalAspectRatio;

        public DrawboardViewModel(bool showBackButton = true)
        {
            _showBackButton = showBackButton;

            BackToHardwareDefineCommand = new RelayCommand(ExecuteBackToHardwareDefine);
            SelectToolCommand = new RelayCommand(ExecuteSelectTool);
            ShowMoreToolsCommand = new RelayCommand(ExecuteShowMoreTools);
            ZoomInCommand = new RelayCommand(_ => ZoomIn());
            ZoomOutCommand = new RelayCommand(_ => ZoomOut());
            UndoCommand = new RelayCommand(_ => Undo(), _ => CanUndo);
            RedoCommand = new RelayCommand(_ => Redo(), _ => CanRedo);
            ShowHelpCommand = new RelayCommand(_ => ShowHelp());
            DeleteSelectedCommand = new RelayCommand(_ => DeleteSelectedElement(), _ => SelectedElement != null);

            InitializeTools();
            InitializeStates();

            // Select default tool
            SelectTool(Tools.FirstOrDefault(t => t.Id == "Selection"));
        }

        #region Properties

        /// <summary>
        /// Whether to show the back button (hidden when opened in a new window).
        /// </summary>
        public bool ShowBackButton
        {
            get => _showBackButton;
            set => SetProperty(ref _showBackButton, value);
        }

        /// <summary>
        /// Available PackML states for the dropdown.
        /// </summary>
        public ObservableCollection<PackMlState> AvailableStates { get; } = new ObservableCollection<PackMlState>();

        /// <summary>
        /// Currently selected PackML state (each state has its own UML workspace).
        /// </summary>
        public PackMlState SelectedState
        {
            get => _selectedState;
            set
            {
                if (SetProperty(ref _selectedState, value))
                {
                    OnStateChanged();
                }
            }
        }

        /// <summary>
        /// All available tools for the toolbar.
        /// </summary>
        public ObservableCollection<DrawboardTool> Tools { get; } = new ObservableCollection<DrawboardTool>();

        /// <summary>
        /// Tools that appear in the main toolbar (not overflow).
        /// </summary>
        public IEnumerable<DrawboardTool> MainToolbarTools => Tools.Where(t => !t.IsOverflowTool);

        /// <summary>
        /// Tools that appear in the "More Tools" overflow menu.
        /// </summary>
        public IEnumerable<DrawboardTool> OverflowTools => Tools.Where(t => t.IsOverflowTool);

        /// <summary>
        /// Currently selected tool.
        /// </summary>
        public DrawboardTool SelectedTool
        {
            get => _selectedTool;
            private set
            {
                if (SetProperty(ref _selectedTool, value))
                {
                    UpdateCurrentHint();
                    UpdateCurrentCursorType();
                }
            }
        }

        /// <summary>
        /// Whether tool lock is enabled (keeps tool active after drawing).
        /// </summary>
        public bool IsToolLockEnabled
        {
            get => _isToolLockEnabled;
            set => SetProperty(ref _isToolLockEnabled, value);
        }

        /// <summary>
        /// Current hint text to display below the toolbar.
        /// </summary>
        public string CurrentHint
        {
            get => _currentHint;
            set => SetProperty(ref _currentHint, value);
        }

        /// <summary>
        /// Current zoom level percentage (e.g., 100 = 100%).
        /// </summary>
        public int ZoomLevel
        {
            get => _zoomLevel;
            set => SetProperty(ref _zoomLevel, Math.Clamp(value, 10, 500));
        }

        /// <summary>
        /// Whether undo operation is available.
        /// </summary>
        public bool CanUndo
        {
            get => _canUndo;
            set => SetProperty(ref _canUndo, value);
        }

        /// <summary>
        /// Whether redo operation is available.
        /// </summary>
        public bool CanRedo
        {
            get => _canRedo;
            set => SetProperty(ref _canRedo, value);
        }

        /// <summary>
        /// Current cursor type based on selected tool.
        /// </summary>
        public DrawboardToolCursorType CurrentCursorType
        {
            get => _currentCursorType;
            private set => SetProperty(ref _currentCursorType, value);
        }

        /// <summary>
        /// Collection of drawing elements on the canvas.
        /// </summary>
        public ObservableCollection<DrawingElement> Elements { get; } = new ObservableCollection<DrawingElement>();

        /// <summary>
        /// Whether a drawing operation is in progress.
        /// </summary>
        public bool IsDrawing
        {
            get => _isDrawing;
            private set => SetProperty(ref _isDrawing, value);
        }

        /// <summary>
        /// Preview element shown during drag operation.
        /// </summary>
        public DrawingElement PreviewElement
        {
            get => _previewElement;
            private set => SetProperty(ref _previewElement, value);
        }

        /// <summary>
        /// Currently selected element for editing.
        /// </summary>
        public DrawingElement SelectedElement
        {
            get => _selectedElement;
            private set => SetProperty(ref _selectedElement, value);
        }

        /// <summary>
        /// Current edit mode state.
        /// </summary>
        public EditModeState EditModeState
        {
            get => _editModeState;
            private set => SetProperty(ref _editModeState, value);
        }

        /// <summary>
        /// Currently active resize handle being dragged.
        /// </summary>
        public ResizeHandleType ActiveResizeHandle
        {
            get => _activeResizeHandle;
            private set => SetProperty(ref _activeResizeHandle, value);
        }

        #endregion

        #region Commands

        public ICommand BackToHardwareDefineCommand { get; }
        public ICommand SelectToolCommand { get; }
        public ICommand ShowMoreToolsCommand { get; }
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand ShowHelpCommand { get; }
        public ICommand DeleteSelectedCommand { get; }

        #endregion

        #region Initialization

        private void InitializeStates()
        {
            foreach (PackMlState state in Enum.GetValues(typeof(PackMlState)))
            {
                AvailableStates.Add(state);
            }
        }

        private void InitializeTools()
        {
            // Group 0: ToolLock (toggleable)
            Tools.Add(new DrawboardTool
            {
                Id = "ToolLock",
                Name = "ToolLock",
                Instruction = Strings.Drawboard_Tool_ToolLock_Instruction,
                Shortcut = "L",
                Hint = string.Empty,
                CursorType = DrawboardToolCursorType.Default,
                GroupIndex = 0,
                IsToggleable = true,
            });

            // Group 1: Hand, Selection
            Tools.Add(new DrawboardTool
            {
                Id = "Hand",
                Name = "Hand",
                Instruction = Strings.Drawboard_Tool_Hand_Instruction,
                Shortcut = "H",
                Hint = Strings.Drawboard_Tool_Hand_Hint,
                CursorType = DrawboardToolCursorType.Hand,
                GroupIndex = 1,
            });

            Tools.Add(new DrawboardTool
            {
                Id = "Selection",
                Name = "Selection",
                Instruction = Strings.Drawboard_Tool_Selection_Instruction,
                Shortcut = "V",
                Hint = Strings.Drawboard_Tool_Selection_Hint,
                CursorType = DrawboardToolCursorType.Default,
                GroupIndex = 1,
            });

            // Group 2: Node tools
            Tools.Add(new DrawboardTool
            {
                Id = "InitialNode",
                Name = "InitialNode",
                Instruction = Strings.Drawboard_Tool_InitialNode_Instruction,
                Shortcut = "1",
                Hint = string.Empty,
                CursorType = DrawboardToolCursorType.Cross,
                GroupIndex = 2,
            });

            Tools.Add(new DrawboardTool
            {
                Id = "ActionNode",
                Name = "ActionNode",
                Instruction = Strings.Drawboard_Tool_ActionNode_Instruction,
                Shortcut = "2",
                Hint = string.Empty,
                CursorType = DrawboardToolCursorType.Cross,
                GroupIndex = 2,
            });

            Tools.Add(new DrawboardTool
            {
                Id = "DecisionNode",
                Name = "DecisionNode",
                Instruction = Strings.Drawboard_Tool_DecisionNode_Instruction,
                Shortcut = "3",
                Hint = string.Empty,
                CursorType = DrawboardToolCursorType.Cross,
                GroupIndex = 2,
            });

            Tools.Add(new DrawboardTool
            {
                Id = "TerminalNode",
                Name = "TerminalNode",
                Instruction = Strings.Drawboard_Tool_TerminalNode_Instruction,
                Shortcut = "4",
                Hint = string.Empty,
                CursorType = DrawboardToolCursorType.Cross,
                GroupIndex = 2,
            });

            Tools.Add(new DrawboardTool
            {
                Id = "PredefinedActionNode",
                Name = "PredefinedActionNode",
                Instruction = Strings.Drawboard_Tool_PredefinedActionNode_Instruction,
                Shortcut = "5",
                Hint = string.Empty,
                CursorType = DrawboardToolCursorType.Cross,
                GroupIndex = 2,
            });

            // Group 3: Text, Eraser
            Tools.Add(new DrawboardTool
            {
                Id = "Textbox",
                Name = "Textbox",
                Instruction = Strings.Drawboard_Tool_Textbox_Instruction,
                Shortcut = "T",
                Hint = Strings.Drawboard_Tool_Textbox_Hint,
                CursorType = DrawboardToolCursorType.Cross,
                GroupIndex = 3,
            });

            Tools.Add(new DrawboardTool
            {
                Id = "Eraser",
                Name = "Eraser",
                Instruction = Strings.Drawboard_Tool_Eraser_Instruction,
                Shortcut = "E",
                Hint = Strings.Drawboard_Tool_Eraser_Hint,
                CursorType = DrawboardToolCursorType.Eraser,
                GroupIndex = 3,
            });

            // Group 4: More Tools (overflow)
            Tools.Add(new DrawboardTool
            {
                Id = "Image",
                Name = "Image",
                Instruction = Strings.Drawboard_Tool_Image_Instruction,
                Shortcut = "I",
                Hint = string.Empty,
                CursorType = DrawboardToolCursorType.Default,
                GroupIndex = 4,
                IsOverflowTool = true,
            });
        }

        #endregion

        #region Tool Selection

        /// <summary>
        /// Selects a tool by its instance.
        /// </summary>
        public void SelectTool(DrawboardTool tool)
        {
            if (tool == null) return;

            // Handle ToolLock specially - it's toggleable
            if (tool.Id == "ToolLock")
            {
                tool.IsSelected = !tool.IsSelected;
                IsToolLockEnabled = tool.IsSelected;
                return;
            }

            // If already selected, don't deselect (at least one tool must be selected)
            if (tool.IsSelected) return;

            // Clear selection when switching away from Selection tool
            if (SelectedTool?.Id == "Selection" && tool.Id != "Selection")
            {
                ClearSelection();
            }

            // Deselect all non-toggleable tools
            foreach (var t in Tools.Where(t => !t.IsToggleable))
            {
                t.IsSelected = false;
            }

            // Select the new tool
            tool.IsSelected = true;
            SelectedTool = tool;
        }

        /// <summary>
        /// Selects a tool by its ID.
        /// </summary>
        public void SelectToolById(string toolId)
        {
            var tool = Tools.FirstOrDefault(t => t.Id == toolId);
            if (tool != null)
            {
                SelectTool(tool);
            }
        }

        /// <summary>
        /// Ensures a valid tool is selected (fallback to Selection if needed).
        /// </summary>
        public void EnsureValidToolSelection()
        {
            var selectedNonToggleable = Tools.FirstOrDefault(t => t.IsSelected && !t.IsToggleable);
            if (selectedNonToggleable == null)
            {
                SelectToolById("Selection");
            }
        }

        private void UpdateCurrentHint()
        {
            CurrentHint = SelectedTool?.Hint ?? string.Empty;
        }

        private void UpdateCurrentCursorType()
        {
            CurrentCursorType = SelectedTool?.CursorType ?? DrawboardToolCursorType.Default;
        }

        #endregion

        #region Command Handlers

        private void ExecuteBackToHardwareDefine()
        {
            NavigationService.Instance.NavigateBackFromDrawboard();
        }

        private void ExecuteSelectTool(object parameter)
        {
            if (parameter is DrawboardTool tool)
            {
                SelectTool(tool);
            }
            else if (parameter is string toolId)
            {
                SelectToolById(toolId);
            }
        }

        private void ExecuteShowMoreTools(object parameter)
        {
            if (!(parameter is UIElement element)) return;

            var builder = ContextMenuService.Instance.Create();

            foreach (var tool in OverflowTools)
            {
                var currentTool = tool; // Capture for closure
                builder.AddItem(
                    $"{currentTool.Instruction}",
                    () => SelectTool(currentTool)
                );
            }

            // Calculate position below the More Tools button
            var point = element.PointToScreen(new Point(0, element.RenderSize.Height));
            builder.ShowAt(point, element);
        }

        private void OnStateChanged()
        {
            // Each state has its own workspace
            // Future: Load/save workspace data per state
        }

        private void ZoomIn()
        {
            ZoomLevel = Math.Min(ZoomLevel + 10, 500);
        }

        private void ZoomOut()
        {
            ZoomLevel = Math.Max(ZoomLevel - 10, 10);
        }

        private void Undo()
        {
            // UI only - no implementation
        }

        private void Redo()
        {
            // UI only - no implementation
        }

        private void ShowHelp()
        {
            // UI only - no implementation
        }

        #endregion

        #region Element Creation

        /// <summary>
        /// Starts drawing operation when mouse button is pressed.
        /// </summary>
        /// <param name="position">The mouse down position on the canvas.</param>
        /// <returns>True if drawing started, false otherwise.</returns>
        public bool TryStartDrawing(Point position)
        {
            if (SelectedTool == null) return false;

            var shapeType = GetShapeTypeFromTool(SelectedTool.Id);
            if (shapeType == null) return false;

            _drawStartPoint = position;
            IsDrawing = true;

            PreviewElement = CreateElementForShapeType(shapeType.Value);
            PreviewElement.X = position.X;
            PreviewElement.Y = position.Y;
            PreviewElement.Width = 1;
            PreviewElement.Height = 1;
            PreviewElement.ZIndex = _nextZIndex;

            return true;
        }

        /// <summary>
        /// Updates preview element during mouse drag.
        /// </summary>
        /// <param name="currentPosition">The current mouse position.</param>
        public void UpdateDrawing(Point currentPosition)
        {
            if (!IsDrawing || PreviewElement == null) return;

            double x = Math.Min(_drawStartPoint.X, currentPosition.X);
            double y = Math.Min(_drawStartPoint.Y, currentPosition.Y);
            double width = Math.Abs(currentPosition.X - _drawStartPoint.X);
            double height = Math.Abs(currentPosition.Y - _drawStartPoint.Y);

            PreviewElement.X = x;
            PreviewElement.Y = y;
            PreviewElement.Width = Math.Max(1, width);
            PreviewElement.Height = Math.Max(1, height);
        }

        /// <summary>
        /// Completes drawing and adds element to collection.
        /// </summary>
        public void FinishDrawing()
        {
            if (!IsDrawing || PreviewElement == null) return;

            PreviewElement.Opacity = 1.0;
            Elements.Add(PreviewElement);
            _nextZIndex++;

            PreviewElement = null;
            IsDrawing = false;

            if (!IsToolLockEnabled)
            {
                SelectToolById("Selection");
            }
        }

        /// <summary>
        /// Cancels current drawing operation.
        /// </summary>
        public void CancelDrawing()
        {
            PreviewElement = null;
            IsDrawing = false;
        }

        /// <summary>
        /// Maps tool ID to DrawingShapeType for shape tools (shortcuts 1-5).
        /// </summary>
        private DrawingShapeType? GetShapeTypeFromTool(string toolId)
        {
            return toolId switch
            {
                "InitialNode" => DrawingShapeType.Initial,
                "ActionNode" => DrawingShapeType.Action,
                "DecisionNode" => DrawingShapeType.Decision,
                "TerminalNode" => DrawingShapeType.Terminal,
                "PredefinedActionNode" => DrawingShapeType.PredefinedAction,
                "Textbox" => DrawingShapeType.Textbox,
                _ => null
            };
        }

        /// <summary>
        /// Creates the appropriate DrawingElement subclass for the given shape type.
        /// </summary>
        private DrawingElement CreateElementForShapeType(DrawingShapeType shapeType)
        {
            return shapeType switch
            {
                DrawingShapeType.Initial => new InitialElement(),
                DrawingShapeType.Action => new ActionElement(),
                DrawingShapeType.Decision => new DecisionElement(),
                DrawingShapeType.Terminal => new TerminalElement(),
                DrawingShapeType.PredefinedAction => new PredefinedActionElement(),
                DrawingShapeType.Textbox => new TextboxElement(),
                _ => throw new ArgumentOutOfRangeException(nameof(shapeType), shapeType, "Unknown shape type")
            };
        }

        #endregion

        #region Selection and Edit Operations

        /// <summary>
        /// Selects an element for editing.
        /// </summary>
        public void SelectElement(DrawingElement element)
        {
            if (element == null || element.IsLocked) return;

            // Clear previous selection
            if (_selectedElement != null && _selectedElement != element)
            {
                _selectedElement.IsSelected = false;
            }

            element.IsSelected = true;
            SelectedElement = element;
            EditModeState = EditModeState.Selected;
        }

        /// <summary>
        /// Clears the current selection.
        /// </summary>
        public void ClearSelection()
        {
            if (_selectedElement != null)
            {
                _selectedElement.IsSelected = false;
                SelectedElement = null;
            }
            EditModeState = EditModeState.None;
            ActiveResizeHandle = ResizeHandleType.None;
        }

        /// <summary>
        /// Deletes the currently selected element.
        /// </summary>
        public void DeleteSelectedElement()
        {
            if (_selectedElement == null || _selectedElement.IsLocked) return;

            Elements.Remove(_selectedElement);
            ClearSelection();
        }

        /// <summary>
        /// Finds an element at the specified canvas point.
        /// </summary>
        /// <param name="point">The point in canvas coordinates.</param>
        /// <returns>The topmost element at the point, or null if none found.</returns>
        public DrawingElement FindElementAtPoint(Point point)
        {
            // Search in reverse ZIndex order (topmost first)
            return Elements
                .OrderByDescending(e => e.ZIndex)
                .FirstOrDefault(e => e.Bounds.Contains(point));
        }

        /// <summary>
        /// Checks if the Selection tool is currently active.
        /// </summary>
        public bool IsSelectionToolActive => SelectedTool?.Id == "Selection";

        #region Move Operations

        /// <summary>
        /// Starts a move operation on the selected element.
        /// </summary>
        public void StartMove(Point startPoint)
        {
            if (_selectedElement == null || _selectedElement.IsLocked) return;

            _editDragStartPoint = startPoint;
            _originalBounds = _selectedElement.Bounds;
            EditModeState = EditModeState.Moving;
        }

        /// <summary>
        /// Updates the element position during a move operation.
        /// </summary>
        public void UpdateMove(Point currentPoint)
        {
            if (EditModeState != EditModeState.Moving || _selectedElement == null) return;

            double deltaX = currentPoint.X - _editDragStartPoint.X;
            double deltaY = currentPoint.Y - _editDragStartPoint.Y;

            _selectedElement.X = _originalBounds.X + deltaX;
            _selectedElement.Y = _originalBounds.Y + deltaY;
        }

        /// <summary>
        /// Ends the move operation.
        /// </summary>
        public void EndMove()
        {
            if (EditModeState != EditModeState.Moving) return;
            EditModeState = EditModeState.Selected;
        }

        #endregion

        #region Resize Operations

        /// <summary>
        /// Starts a resize operation on the selected element.
        /// </summary>
        public void StartResize(ResizeHandleType handle, Point startPoint)
        {
            if (_selectedElement == null || _selectedElement.IsLocked || handle == ResizeHandleType.None) return;

            _editDragStartPoint = startPoint;
            _originalBounds = _selectedElement.Bounds;
            _originalAspectRatio = _originalBounds.Width / Math.Max(1, _originalBounds.Height);
            ActiveResizeHandle = handle;
            EditModeState = EditModeState.Resizing;
        }

        /// <summary>
        /// Updates the element size during a resize operation.
        /// </summary>
        public void UpdateResize(Point currentPoint, bool maintainAspectRatio)
        {
            if (EditModeState != EditModeState.Resizing || _selectedElement == null) return;

            double deltaX = currentPoint.X - _editDragStartPoint.X;
            double deltaY = currentPoint.Y - _editDragStartPoint.Y;

            double newX = _originalBounds.X;
            double newY = _originalBounds.Y;
            double newWidth = _originalBounds.Width;
            double newHeight = _originalBounds.Height;

            const double MinSize = 10.0;

            switch (_activeResizeHandle)
            {
                case ResizeHandleType.TopLeft:
                    newX = _originalBounds.X + deltaX;
                    newY = _originalBounds.Y + deltaY;
                    newWidth = _originalBounds.Width - deltaX;
                    newHeight = _originalBounds.Height - deltaY;
                    break;

                case ResizeHandleType.TopRight:
                    newY = _originalBounds.Y + deltaY;
                    newWidth = _originalBounds.Width + deltaX;
                    newHeight = _originalBounds.Height - deltaY;
                    break;

                case ResizeHandleType.BottomLeft:
                    newX = _originalBounds.X + deltaX;
                    newWidth = _originalBounds.Width - deltaX;
                    newHeight = _originalBounds.Height + deltaY;
                    break;

                case ResizeHandleType.BottomRight:
                    newWidth = _originalBounds.Width + deltaX;
                    newHeight = _originalBounds.Height + deltaY;
                    break;

                case ResizeHandleType.Top:
                    newY = _originalBounds.Y + deltaY;
                    newHeight = _originalBounds.Height - deltaY;
                    break;

                case ResizeHandleType.Bottom:
                    newHeight = _originalBounds.Height + deltaY;
                    break;

                case ResizeHandleType.Left:
                    newX = _originalBounds.X + deltaX;
                    newWidth = _originalBounds.Width - deltaX;
                    break;

                case ResizeHandleType.Right:
                    newWidth = _originalBounds.Width + deltaX;
                    break;
            }

            // Maintain aspect ratio if Shift is held
            if (maintainAspectRatio && IsCornerHandle(_activeResizeHandle))
            {
                double scale = Math.Max(newWidth / _originalBounds.Width, newHeight / _originalBounds.Height);
                if (scale <= 0) scale = MinSize / Math.Min(_originalBounds.Width, _originalBounds.Height);

                double scaledWidth = _originalBounds.Width * scale;
                double scaledHeight = _originalBounds.Height * scale;

                // Adjust position based on which corner is being dragged
                switch (_activeResizeHandle)
                {
                    case ResizeHandleType.TopLeft:
                        newX = _originalBounds.Right - scaledWidth;
                        newY = _originalBounds.Bottom - scaledHeight;
                        break;
                    case ResizeHandleType.TopRight:
                        newY = _originalBounds.Bottom - scaledHeight;
                        break;
                    case ResizeHandleType.BottomLeft:
                        newX = _originalBounds.Right - scaledWidth;
                        break;
                    // BottomRight: no position adjustment needed
                }

                newWidth = scaledWidth;
                newHeight = scaledHeight;
            }

            // Enforce minimum size
            if (newWidth < MinSize)
            {
                if (IsLeftHandle(_activeResizeHandle))
                    newX = _originalBounds.Right - MinSize;
                newWidth = MinSize;
            }

            if (newHeight < MinSize)
            {
                if (IsTopHandle(_activeResizeHandle))
                    newY = _originalBounds.Bottom - MinSize;
                newHeight = MinSize;
            }

            _selectedElement.X = newX;
            _selectedElement.Y = newY;
            _selectedElement.Width = newWidth;
            _selectedElement.Height = newHeight;
        }

        /// <summary>
        /// Ends the resize operation.
        /// </summary>
        public void EndResize()
        {
            if (EditModeState != EditModeState.Resizing) return;
            EditModeState = EditModeState.Selected;
            ActiveResizeHandle = ResizeHandleType.None;
        }

        private static bool IsCornerHandle(ResizeHandleType handle) =>
            handle is ResizeHandleType.TopLeft or ResizeHandleType.TopRight
                   or ResizeHandleType.BottomLeft or ResizeHandleType.BottomRight;

        private static bool IsLeftHandle(ResizeHandleType handle) =>
            handle is ResizeHandleType.TopLeft or ResizeHandleType.BottomLeft or ResizeHandleType.Left;

        private static bool IsTopHandle(ResizeHandleType handle) =>
            handle is ResizeHandleType.TopLeft or ResizeHandleType.TopRight or ResizeHandleType.Top;

        #endregion

        #endregion

        #region INotifyPropertyChanged

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}