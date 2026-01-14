using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EquipmentDesigner.Controls;
using EquipmentDesigner.Models;
using EquipmentDesigner.Resources;
using EquipmentDesigner.Services;

using MainWindow = EquipmentDesigner.MainWindow;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// Partial class containing tree operations and workflow completion functionality.
    /// Handles tree CRUD operations, node selection, and workflow state management.
    /// </summary>
    public partial class HardwareDefineWorkflowViewModel
    {
        #region Tree Traversal

        /// <summary>
        /// Checks if all required fields in all tree nodes are filled.
        /// </summary>
        private bool CheckAllNodesRequiredFieldsFilled()
        {
            foreach (var node in GetAllNodes())
            {
                if (node.DataViewModel != null && !node.DataViewModel.CanProceedToNext)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns all nodes in the tree via depth-first traversal.
        /// </summary>
        private IEnumerable<HardwareTreeNodeViewModel> GetAllNodes()
        {
            foreach (var root in TreeRootNodes)
            {
                foreach (var node in GetNodesRecursive(root))
                {
                    yield return node;
                }
            }
        }

        /// <summary>
        /// Recursively traverses tree nodes.
        /// </summary>
        private IEnumerable<HardwareTreeNodeViewModel> GetNodesRecursive(HardwareTreeNodeViewModel node)
        {
            yield return node;
            foreach (var child in node.Children)
            {
                foreach (var descendant in GetNodesRecursive(child))
                {
                    yield return descendant;
                }
            }
        }

        #endregion

        #region Workflow Completion

        /// <summary>
        /// Executes workflow completion request from any DeviceViewModel.
        /// </summary>
        private async void OnWorkflowCompletedRequest(object sender, EventArgs e)
        {
            await CompleteWorkflowAsync();
        }

        /// <summary>
        /// Completes the workflow by updating the workflow state to Defined in workflows.json.
        /// Updates the existing workflow entry with ComponentState.Defined.
        /// </summary>
        private async Task CompleteWorkflowAsync()
        {
            var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
            var sessions = await workflowRepo.LoadAsync();

            // Create or update HardwareDefinition with Defined state
            var sessionDto = ToHardwareDefinition();
            sessionDto.State = ComponentState.Ready;

            var existingIndex = sessions.FindIndex(s => s.Id == HardwareId);

            if (existingIndex >= 0)
                sessions[existingIndex] = sessionDto;
            else
                sessions.Add(sessionDto);

            await workflowRepo.SaveAsync(sessions);
        }

        #endregion

        #region Device Callbacks

        /// <summary>
        /// Sets up workflow completion callbacks for the currently selected DeviceViewModel.
        /// </summary>
        private void SetupCurrentDeviceViewModelCallbacks()
        {
            if (SelectedTreeNode?.HardwareType == HardwareType.Device &&
                SelectedTreeNode?.DataViewModel is DeviceDefineViewModel deviceVm)
            {
                deviceVm.SetAllStepsRequiredFieldsFilledCheck(() => AllStepsRequiredFieldsFilled);
            }
        }

        /// <summary>
        /// Sets up workflow completion callbacks for all Device nodes in the tree.
        /// </summary>
        private void SetupAllDeviceViewModelCallbacks()
        {
            foreach (var node in GetAllNodes())
            {
                // Subscribe to PropertyChanged for data change detection on all nodes
                if (node.DataViewModel != null)
                {
                    node.DataViewModel.PropertyChanged -= OnNodeDataPropertyChanged;
                    node.DataViewModel.PropertyChanged += OnNodeDataPropertyChanged;
                }

                if (node.HardwareType == HardwareType.Device &&
                    node.DataViewModel is DeviceDefineViewModel deviceVm)
                {
                    deviceVm.SetAllStepsRequiredFieldsFilledCheck(() => AllStepsRequiredFieldsFilled);
                    deviceVm.WorkflowCompletedRequest -= OnWorkflowCompletedRequest;
                    deviceVm.WorkflowCompletedRequest += OnWorkflowCompletedRequest;
                }
            }
        }

        /// <summary>
        /// Handles PropertyChanged events from node ViewModels to detect data changes.
        /// </summary>
        private void OnNodeDataPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Skip properties that don't represent actual data changes
            if (e.PropertyName == nameof(IHardwareDefineViewModel.IsEditable) ||
                e.PropertyName == nameof(IHardwareDefineViewModel.CanProceedToNext) ||
                e.PropertyName == nameof(IHardwareDefineViewModel.FilledFieldCount) ||
                e.PropertyName == nameof(IHardwareDefineViewModel.TotalFieldCount))
            {
                return;
            }

            OnDataChanged();
        }

        /// <summary>
        /// Updates the IsEditable property on all tree node ViewModels.
        /// </summary>
        private void UpdateAllTreeNodeEditability(bool isEditable)
        {
            foreach (var node in GetAllNodes())
            {
                if (node.DataViewModel != null)
                {
                    node.DataViewModel.IsEditable = isEditable;
                }
            }
        }

        #endregion

        #region Add Child Operations

        private void ExecuteAddChild(HardwareTreeNodeViewModel parentNode)
        {
            if (parentNode == null || !parentNode.CanHaveChildren)
                return;

            var newChild = parentNode.AddChildWithFullHierarchy();
            if (newChild != null)
            {
                // Set up callbacks for any new Device nodes in the hierarchy
                SetupDeviceCallbacksForNode(newChild);

                // Set editability for new nodes
                SetEditabilityForNode(newChild, !IsReadOnly);

                // Select the new node
                ExecuteSelectTreeNode(newChild);

                // Mark data as dirty for autosave
                MarkDirty();
            }
        }

        /// <summary>
        /// Sets up Device callbacks for a node and its descendants.
        /// </summary>
        private void SetupDeviceCallbacksForNode(HardwareTreeNodeViewModel node)
        {
            if (node.HardwareType == HardwareType.Device &&
                node.DataViewModel is DeviceDefineViewModel deviceVm)
            {
                deviceVm.SetAllStepsRequiredFieldsFilledCheck(() => AllStepsRequiredFieldsFilled);
                deviceVm.WorkflowCompletedRequest += OnWorkflowCompletedRequest;
            }

            foreach (var child in node.Children)
            {
                SetupDeviceCallbacksForNode(child);
            }
        }

        /// <summary>
        /// Sets editability for a node and its descendants.
        /// </summary>
        private void SetEditabilityForNode(HardwareTreeNodeViewModel node, bool isEditable)
        {
            if (node.DataViewModel != null)
            {
                node.DataViewModel.IsEditable = isEditable;
            }

            foreach (var child in node.Children)
            {
                SetEditabilityForNode(child, isEditable);
            }
        }

        private bool CanExecuteAddChild(HardwareTreeNodeViewModel parentNode)
        {
            return parentNode != null && parentNode.CanHaveChildren && !IsReadOnly;
        }

        #endregion

        #region Selection Operations

        private void ExecuteSelectTreeNode(HardwareTreeNodeViewModel node)
        {
            if (node == null)
                return;

            // Deselect previous node
            if (SelectedTreeNode != null)
            {
                SelectedTreeNode.IsSelected = false;
            }

            // Select new node
            node.IsSelected = true;
            SelectedTreeNode = node;
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// Determines if a node can be deleted.
        /// Returns true if node is valid and not in read-only mode.
        /// Actual constraint validation happens in ExecuteDeleteNode.
        /// </summary>
        private bool CanDeleteNode(HardwareTreeNodeViewModel node)
        {
            if (node == null) return false;
            if (IsReadOnly) return false;
            return true;
        }

        /// <summary>
        /// Executes node deletion with minimum child constraint validation.
        /// Shows Toast warning if deletion would violate the constraint.
        /// Shows ConfirmDialog if deletion is allowed.
        /// </summary>
        private void ExecuteDeleteNode(HardwareTreeNodeViewModel node)
        {
            if (node == null) return;

            // Check minimum child constraint
            bool isRootNode = node.Parent == null;

            if (isRootNode)
            {
                // Root node: check if it's the only root
                if (TreeRootNodes.Count <= 1)
                {
                    ToastService.Instance.ShowError(
                        Strings.DeleteHardware_CannotDelete_Title,
                        Strings.DeleteHardware_CannotDelete_MinChild);
                    return;
                }
            }
            else
            {
                // Non-root node: check if parent has only one child
                if (node.Parent.Children.Count <= 1)
                {
                    ToastService.Instance.ShowError(
                        Strings.DeleteHardware_CannotDelete_Title,
                        Strings.DeleteHardware_CannotDelete_MinChild);
                    return;
                }
            }

            // Get all descendants for the dialog
            var descendants = node.GetAllDescendants();
            var descendantNames = descendants.Select(d => $"{d.HardwareType}: {d.DisplayName}").ToList();

            // Build description with descendants list
            var description = Strings.DeleteHardware_Description;
            if (descendantNames.Count > 0)
            {
                description += "\n\n" + Strings.DeleteHardware_DescendantsLabel + "\n• " + string.Join("\n• ", descendantNames);
            }

            // Get MainWindow for backdrop control
            var mainWindow = System.Windows.Application.Current?.MainWindow as MainWindow;

            // If no MainWindow (test environment), perform deletion directly
            if (mainWindow == null)
            {
                PerformNodeDeletion(node, isRootNode);
                return;
            }

            // Show backdrop
            mainWindow.ShowBackdrop();

            try
            {
                // Show confirmation dialog
                var dialog = new ConfirmDialog(
                    Strings.DeleteHardware_Title,
                    description,
                    "delete",
                    $"{node.HardwareType}: {node.DisplayName}")
                {
                    Owner = mainWindow,
                    ConfirmText = Strings.DeleteHardware_DeleteButton,
                    CancelText = Strings.Common_Cancel
                };

                if (dialog.ShowDialog() == true && dialog.IsConfirmed)
                {
                    PerformNodeDeletion(node, isRootNode);
                }
            }
            finally
            {
                mainWindow?.HideBackdrop();
            }
        }

        /// <summary>
        /// Performs the actual node deletion from the tree.
        /// </summary>
        private void PerformNodeDeletion(HardwareTreeNodeViewModel node, bool isRootNode)
        {
            // Remove from parent or root
            if (isRootNode)
            {
                TreeRootNodes.Remove(node);
            }
            else
            {
                node.Parent.Children.Remove(node);
            }

            // If deleted node was selected, select another node
            if (SelectedTreeNode == node)
            {
                // Try to select sibling or parent
                HardwareTreeNodeViewModel newSelection = null;

                if (isRootNode && TreeRootNodes.Count > 0)
                {
                    newSelection = TreeRootNodes[0];
                }
                else if (!isRootNode && node.Parent != null)
                {
                    newSelection = node.Parent.Children.FirstOrDefault() ?? node.Parent;
                }

                if (newSelection != null)
                {
                    ExecuteSelectTreeNode(newSelection);
                }
                else
                {
                    SelectedTreeNode = null;
                }
            }

            // Mark dirty for autosave
            MarkDirty();
        }

        #endregion

        #region Copy Operations

        /// <summary>
        /// Determines if a node can be copied.
        /// </summary>
        private bool CanCopyNode(HardwareTreeNodeViewModel node)
        {
            if (node == null) return false;
            if (IsReadOnly) return false;
            return true;
        }

        /// <summary>
        /// Executes node copy operation.
        /// Creates a deep copy of the node and adds it to the parent's children.
        /// Root nodes cannot be copied as only one top-level component is allowed per session.
        /// </summary>
        private void ExecuteCopyNode(HardwareTreeNodeViewModel node)
        {
            if (node == null) return;

            bool isRootNode = node.Parent == null;

            // Root nodes cannot be copied - only one top-level component allowed per session
            if (isRootNode)
            {
                ToastService.Instance.ShowWarning(
                    Strings.CopyHardware_CannotCopy_Title,
                    Strings.CopyHardware_CannotCopy_RootNode);
                return;
            }

            // Create deep copy with the same parent
            var copiedNode = node.DeepCopy(node.Parent);

            // Add to parent's children
            node.Parent.Children.Add(copiedNode);

            // Setup callbacks for the copied node hierarchy
            SetupDeviceCallbacksForNode(copiedNode);

            // Set editability for the copied node hierarchy
            SetEditabilityForNode(copiedNode, !IsReadOnly);

            // Subscribe to property changes for autosave
            SubscribeToNodePropertyChanges(copiedNode);

            // Select the copied node
            ExecuteSelectTreeNode(copiedNode);

            // Mark dirty for autosave
            MarkDirty();
        }

        /// <summary>
        /// Subscribes to PropertyChanged events for a node and its descendants.
        /// </summary>
        private void SubscribeToNodePropertyChanges(HardwareTreeNodeViewModel node)
        {
            if (node.DataViewModel != null)
            {
                node.DataViewModel.PropertyChanged -= OnNodeDataPropertyChanged;
                node.DataViewModel.PropertyChanged += OnNodeDataPropertyChanged;
            }

            foreach (var child in node.Children)
            {
                SubscribeToNodePropertyChanges(child);
            }
        }

        #endregion
    }
}
