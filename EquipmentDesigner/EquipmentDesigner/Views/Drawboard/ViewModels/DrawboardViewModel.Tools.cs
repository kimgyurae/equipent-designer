using System.Linq;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models.ProcessEditor;
using EquipmentDesigner.Resources;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// Partial class containing tool initialization and selection logic.
    /// </summary>
    public partial class DrawboardViewModel
    {
        #region Initialization

        private void InitializeStates()
        {
            foreach (PackMlState state in System.Enum.GetValues(typeof(PackMlState)))
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

            // Clear all selections when switching away from Selection tool
            if (SelectedTool?.Id == "Selection" && tool.Id != "Selection")
            {
                ClearAllSelections();
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
    }
}