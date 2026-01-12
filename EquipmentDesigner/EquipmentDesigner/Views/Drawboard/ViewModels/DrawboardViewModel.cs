using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using EquipmentDesigner.Controls;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models.ProcessEditor;
using EquipmentDesigner.Views.Drawboard.UMLEngine;
using EquipmentDesigner.Views.Drawboard.UMLEngine.Contexts;
using UmlDrawingContext = EquipmentDesigner.Views.Drawboard.UMLEngine.Contexts.DrawingContext;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// ViewModel for Process definition view with UML workflow editor.
    /// </summary>
    public partial class DrawboardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Fields

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
        private DrawingElement _previewElement;

        // Edit mode state - using immutable Context pattern for Engine delegation
        private DrawingElement _selectedElement;
        private EditModeState _editModeState = EditModeState.None;
        private ResizeHandleType _activeResizeHandle = ResizeHandleType.None;

        // Context objects for Engine delegation (replaces individual state fields)
        private MoveContext _moveContext;
        private ResizeContext _resizeContext;
        private UmlDrawingContext _drawingContext;

        // Multi-selection state
        private readonly ObservableCollection<DrawingElement> _selectedElements = new ObservableCollection<DrawingElement>();
        private GroupResizeContext _groupResizeContext;
        private List<Rect> _originalElementBounds;
        private Point _groupDragStartPoint;

        // Rubberband selection state
        private bool _isRubberbandSelecting;
        private Point _rubberbandStartPoint;
        private Rect _rubberbandRect;

        #endregion

        /// <summary>
        /// Canvas width based on largest connected monitor size (10x).
        /// </summary>
        public double CanvasWidth { get; }

        /// <summary>
        /// Canvas height based on largest connected monitor size (10x).
        /// </summary>
        public double CanvasHeight { get; }

        public DrawboardViewModel(bool showBackButton = true)
        {
            _showBackButton = showBackButton;

            // Canvas size = Largest connected monitor size × 10
            var largestScreen = Screen.AllScreens
                .OrderByDescending(s => s.Bounds.Width * s.Bounds.Height)
                .First();
            CanvasWidth = largestScreen.Bounds.Width * 10;
            CanvasHeight = largestScreen.Bounds.Height * 10;

            BackToHardwareDefineCommand = new RelayCommand(ExecuteBackToHardwareDefine);
            SelectToolCommand = new RelayCommand(ExecuteSelectTool);
            ShowMoreToolsCommand = new RelayCommand(ExecuteShowMoreTools);
            ZoomInCommand = new RelayCommand(_ => ZoomIn());
            ZoomOutCommand = new RelayCommand(_ => ZoomOut());
            UndoCommand = new RelayCommand(_ => Undo(), _ => CanUndo);
            RedoCommand = new RelayCommand(_ => Redo(), _ => CanRedo);
            ShowHelpCommand = new RelayCommand(_ => ShowHelp());
            DeleteSelectedCommand = new RelayCommand(_ => DeleteSelectedElement(), _ => SelectedElement != null);
            UnlockSelectedElementCommand = new RelayCommand(_ => UnlockSingleSelectedElement(), _ => ShowUnlockButton);

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
            set
            {
                if (SetProperty(ref _zoomLevel, ZoomControlEngine.ClampZoomLevel(value)))
                {
                    OnPropertyChanged(nameof(ZoomScale));
                    OnPropertyChanged(nameof(ScreenUnlockButtonX));
                    OnPropertyChanged(nameof(ScreenUnlockButtonY));
                }
            }
        }

        /// <summary>
        /// Zoom scale factor for canvas transform (ZoomLevel / 100.0).
        /// </summary>
        public double ZoomScale => ZoomControlEngine.CalculateZoomScale(ZoomLevel);

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

        /// <summary>
        /// Collection of all selected elements.
        /// </summary>
        public ObservableCollection<DrawingElement> SelectedElements => _selectedElements;

        /// <summary>
        /// True if multiple elements are selected.
        /// </summary>
        public bool IsMultiSelectionMode => _selectedElements.Count > 1;

        /// <summary>
        /// Computed bounding box of all selected elements.
        /// </summary>
        public Rect GroupBounds => DrawingElementEditingEngine.ComputeGroupBounds(_selectedElements);

        /// <summary>
        /// Current rubberband selection rectangle.
        /// </summary>
        public Rect RubberbandRect
        {
            get => _rubberbandRect;
            private set => SetProperty(ref _rubberbandRect, value);
        }

        /// <summary>
        /// True if rubberband selection is in progress.
        /// </summary>
        public bool IsRubberbandSelecting
        {
            get => _isRubberbandSelecting;
            private set => SetProperty(ref _isRubberbandSelecting, value);
        }

        /// <summary>
        /// Checks if the Selection tool is currently active.
        /// </summary>
        public bool IsSelectionToolActive => SelectedTool?.Id == "Selection";

        /// <summary>
        /// Whether to show the floating unlock button.
        /// Shows only when a single locked element is selected (not multi-selection).
        /// </summary>
        public bool ShowUnlockButton => !IsMultiSelectionMode && SelectedElement?.IsLocked == true;

        /// <summary>
        /// Scroll offset X from View (updated via ScrollChanged event).
        /// </summary>
        public double ScrollOffsetX { get; set; }

        /// <summary>
        /// Scroll offset Y from View (updated via ScrollChanged event).
        /// </summary>
        public double ScrollOffsetY { get; set; }

        /// <summary>
        /// Gets the X position for the unlock button in screen coordinates (accounting for zoom and scroll).
        /// </summary>
        public double ScreenUnlockButtonX => SelectedElement != null 
            ? SelectedElement.X * ZoomScale - ScrollOffsetX
            : 0;

        /// <summary>
        /// Gets the Y position for the unlock button in screen coordinates (accounting for zoom and scroll).
        /// </summary>
        public double ScreenUnlockButtonY => SelectedElement != null 
            ? (SelectedElement.Y - 4) * ZoomScale - ScrollOffsetY - 40
            : 0;

        /// <summary>
        /// Gets the X position for the unlock button (centered above the selected element).
        /// </summary>
        public double UnlockButtonX => SelectedElement != null 
            ? SelectedElement.X + (SelectedElement.Width / 2) - 20  // 20 = half of 40px button width
            : 0;

        /// <summary>
        /// Gets the Y position for the unlock button (above the selected element).
        /// </summary>
        public double UnlockButtonY => SelectedElement != null 
            ? SelectedElement.Y - 50  // 50 = 40px button height + 10px gap
            : 0;

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
        public ICommand UnlockSelectedElementCommand { get; }

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