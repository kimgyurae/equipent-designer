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
                IconData = "M12.6667 7.33337H3.33333C2.59695 7.33337 2 7.93033 2 8.66671V13.3334C2 14.0698 2.59695 14.6667 3.33333 14.6667H12.6667C13.403 14.6667 14 14.0698 14 13.3334V8.66671C14 7.93033 13.403 7.33337 12.6667 7.33337Z M4.6665 7.33337V4.66671C4.6665 3.78265 5.01769 2.93481 5.64281 2.30968C6.26794 1.68456 7.11578 1.33337 7.99984 1.33337C8.88389 1.33337 9.73174 1.68456 10.3569 2.30968C10.982 2.93481 11.3332 3.78265 11.3332 4.66671V7.33337"
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
                IconData = "M12.0002 7.33329V3.99996C12.0002 3.64634 11.8597 3.3072 11.6096 3.05715C11.3596 2.8071 11.0205 2.66663 10.6668 2.66663C10.3132 2.66663 9.97407 2.8071 9.72402 3.05715C9.47397 3.3072 9.3335 3.64634 9.3335 3.99996 M9.33317 6.66671V2.66671C9.33317 2.31309 9.19269 1.97395 8.94265 1.7239C8.6926 1.47385 8.35346 1.33337 7.99984 1.33337C7.64622 1.33337 7.30708 1.47385 7.05703 1.7239C6.80698 1.97395 6.6665 2.31309 6.6665 2.66671V4.00004 M6.66667 6.99996V3.99996C6.66667 3.64634 6.52619 3.3072 6.27614 3.05715C6.02609 2.8071 5.68696 2.66663 5.33333 2.66663C4.97971 2.66663 4.64057 2.8071 4.39052 3.05715C4.14048 3.3072 4 3.64634 4 3.99996V9.33329 M11.9997 5.33333C11.9997 4.97971 12.1402 4.64057 12.3903 4.39052C12.6403 4.14048 12.9795 4 13.3331 4C13.6867 4 14.0258 4.14048 14.2759 4.39052C14.5259 4.64057 14.6664 4.97971 14.6664 5.33333V9.33333C14.6664 10.7478 14.1045 12.1044 13.1043 13.1046C12.1041 14.1048 10.7476 14.6667 9.33308 14.6667H7.99974C6.13308 14.6667 4.99974 14.0933 4.00641 13.1067L1.60641 10.7067C1.37703 10.4526 1.25413 10.1201 1.26316 9.77796C1.27218 9.43581 1.41244 9.11023 1.65489 8.86864C1.89734 8.62705 2.22341 8.48794 2.56559 8.48013C2.90777 8.47232 3.23985 8.59639 3.49308 8.82667L4.66641 10"
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
                IconData = "M2.69127 3.12539C2.66496 3.06467 2.65751 2.99744 2.66989 2.93243C2.68228 2.86742 2.71392 2.80763 2.76072 2.76084C2.80751 2.71404 2.8673 2.6824 2.9323 2.67002C2.99731 2.65763 3.06454 2.66508 3.12527 2.69139L13.7919 7.02472C13.8568 7.05115 13.9117 7.09733 13.9488 7.15671C13.9859 7.2161 14.0034 7.28566 13.9988 7.35554C13.9941 7.42543 13.9676 7.49207 13.9229 7.54601C13.8782 7.59995 13.8177 7.63846 13.7499 7.65606L9.66727 8.70939C9.43659 8.76869 9.22601 8.8887 9.05742 9.05694C8.88883 9.22518 8.76838 9.43551 8.7086 9.66606L7.65593 13.7501C7.63833 13.8178 7.59983 13.8784 7.54589 13.923C7.49194 13.9677 7.4253 13.9942 7.35542 13.9989C7.28554 14.0035 7.21597 13.9861 7.15659 13.9489C7.09721 13.9118 7.05102 13.8569 7.0246 13.7921L2.69127 3.12539Z"
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
                IconData = "M8.00016 14.6667C11.6821 14.6667 14.6668 11.6819 14.6668 8.00004C14.6668 4.31814 11.6821 1.33337 8.00016 1.33337C4.31826 1.33337 1.3335 4.31814 1.3335 8.00004C1.3335 11.6819 4.31826 14.6667 8.00016 14.6667Z"
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
                IconData = "M12.6667,2 H3.33333 C2.59695,2 2,2.59695 2,3.33333 V12.6667 C2,13.403 2.59695,14 3.33333,14 H12.6667 C13.403,14 14,13.403 14,12.6667 V3.33333 C14,2.59695 13.403,2 12.6667,2 Z"
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
                IconData = "M1.79978 6.86661C1.65041 7.01583 1.53192 7.19302 1.45107 7.38806C1.37023 7.58309 1.32861 7.79215 1.32861 8.00328C1.32861 8.21441 1.37023 8.42347 1.45107 8.61851C1.53192 8.81354 1.65041 8.99073 1.79978 9.13995L6.85978 14.1999C7.00899 14.3493 7.18618 14.4678 7.38122 14.5487C7.57626 14.6295 7.78531 14.6711 7.99644 14.6711C8.20757 14.6711 8.41663 14.6295 8.61167 14.5487C8.80671 14.4678 8.9839 14.3493 9.13311 14.1999L14.1931 9.13995C14.3425 8.99073 14.461 8.81354 14.5418 8.61851C14.6227 8.42347 14.6643 8.21441 14.6643 8.00328C14.6643 7.79215 14.6227 7.58309 14.5418 7.38806C14.461 7.19302 14.3425 7.01583 14.1931 6.86661L9.13311 1.80661C8.9839 1.65725 8.80671 1.53875 8.61167 1.45791C8.41663 1.37706 8.20757 1.33545 7.99644 1.33545C7.78531 1.33545 7.57626 1.37706 7.38122 1.45791C7.18618 1.53875 7.00899 1.65725 6.85978 1.80661L1.79978 6.86661Z"
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
                IconData = "M8.00016 14.6667C11.6821 14.6667 14.6668 11.6819 14.6668 8.00004C14.6668 4.31814 11.6821 1.33337 8.00016 1.33337C4.31826 1.33337 1.3335 4.31814 1.3335 8.00004C1.3335 11.6819 4.31826 14.6667 8.00016 14.6667Z",
                OverlayIconData = "M8.00016 12.1667C10.3013 12.1667 12.1668 10.3012 12.1668 8.00004C12.1668 5.69885 10.3013 3.83337 8.00016 3.83337C5.69898 3.83337 3.8335 5.69885 3.8335 8.00004C3.8335 10.3012 5.69898 12.1667 8.00016 12.1667Z"
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
                IconData = "M12.6667 2H3.33333C2.59695 2 2 2.59695 2 3.33333V12.6667C2 13.403 2.59695 14 3.33333 14H12.6667C13.403 14 14 13.403 14 12.6667V3.33333C14 2.59695 13.403 2 12.6667 2Z",
                OverlayIconData = "M11.5 3.5H4.5C3.94772 3.5 3.5 3.94772 3.5 4.5V11.5C3.5 12.0523 3.94772 12.5 4.5 12.5H11.5C12.0523 12.5 12.5 12.0523 12.5 11.5V4.5C12.5 3.94772 12.0523 3.5 11.5 3.5Z"
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
                IconData = "M2.6665 4.66663V2.66663H13.3332V4.66663 M6 13.3334H10 M8 2.66663V13.3333"
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
                IconData = "M4.66647 14L1.7998 11.1333C1.13314 10.4667 1.13314 9.46668 1.7998 8.86668L8.1998 2.46667C8.86647 1.80001 9.86647 1.80001 10.4665 2.46667L14.1998 6.20001C14.8665 6.86668 14.8665 7.86667 14.1998 8.46667L8.66647 14 M14.6665 14H4.6665 M3.3335 7.33337L9.3335 13.3334"
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
                IconData = "M12.6667 2H3.33333C2.59695 2 2 2.59695 2 3.33333V12.6667C2 13.403 2.59695 14 3.33333 14H12.6667C13.403 14 14 13.403 14 12.6667V3.33333C14 2.59695 13.403 2 12.6667 2Z M5.99984 7.33329C6.73622 7.33329 7.33317 6.73634 7.33317 5.99996C7.33317 5.26358 6.73622 4.66663 5.99984 4.66663C5.26346 4.66663 4.6665 5.26358 4.6665 5.99996C4.6665 6.73634 5.26346 7.33329 5.99984 7.33329Z M14 9.99996L11.9427 7.94263C11.6926 7.69267 11.3536 7.55225 11 7.55225C10.6464 7.55225 10.3074 7.69267 10.0573 7.94263L4 14"
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