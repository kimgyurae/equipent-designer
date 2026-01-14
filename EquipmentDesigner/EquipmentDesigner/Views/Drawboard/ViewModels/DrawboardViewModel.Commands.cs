using System.Collections.Generic;
using System.Windows;
using EquipmentDesigner.Controls;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models.ProcessEditor;
using EquipmentDesigner.Services;
using EquipmentDesigner.Views.Drawboard.UMLEngine;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// Partial class containing command handlers.
    /// </summary>
    public partial class DrawboardViewModel
    {
        #region Command Handlers

        private async void ExecuteBackToHardwareDefine()
        {
            await _processManager.SaveAsync(new List<Process> { Process });
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
            // Load workflow for the newly selected state
            LoadWorkflowForCurrentState();
        }

        private void ZoomIn()
        {
            ZoomLevel = ZoomControlEngine.CalculateLinearZoomIn(ZoomLevel);
        }

        private void ZoomOut()
        {
            ZoomLevel = ZoomControlEngine.CalculateLinearZoomOut(ZoomLevel);
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
    }
}