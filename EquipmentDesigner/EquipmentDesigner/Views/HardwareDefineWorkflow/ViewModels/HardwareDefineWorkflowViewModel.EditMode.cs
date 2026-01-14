using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EquipmentDesigner.Controls;
using EquipmentDesigner.Models;
using EquipmentDesigner.Resources;
using EquipmentDesigner.Services;
using EquipmentDesigner.Views;

using MainWindow = EquipmentDesigner.MainWindow;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// Partial class containing edit mode functionality.
    /// Handles edit mode selection, version management, and session copying.
    /// </summary>
    public partial class HardwareDefineWorkflowViewModel
    {
        #region Edit Mode Commands

        private void ExecuteEnableEdit()
        {
            // Get MainWindow for backdrop control (null-safe for test environment)
            var mainWindow = System.Windows.Application.Current?.MainWindow as MainWindow;

            // If no MainWindow (test environment), directly enable edit mode
            if (mainWindow == null)
            {
                EnableEditModeDirectly();
                return;
            }

            // Show backdrop
            mainWindow.ShowBackdrop();

            try
            {
                // Show edit mode selection dialog
                var dialog = new EditModeSelectionDialog
                {
                    Owner = mainWindow,
                    CurrentVersion = TopLevelComponentVersion
                };

                if (dialog.ShowDialog() == true && dialog.IsConfirmed)
                {
                    // Store the selected mode for future use
                    var selectedMode = dialog.SelectedMode;

                    // Execute edit mode based on selection
                    switch (selectedMode)
                    {
                        case EditModeSelection.DirectEdit:
                            EnableEditModeDirectly();
                            break;

                        case EditModeSelection.CreateNewVersion:
                            EnableEditModeWithNewVersionAsync(dialog.NewVersion);
                            break;

                        case EditModeSelection.CreateNewHardware:
                            EnableEditModeWithNewHardwareAsync();
                            break;
                    }
                }
            }
            finally
            {
                // Hide backdrop
                mainWindow?.HideBackdrop();
            }
        }

        /// <summary>
        /// Enables edit mode directly on the current data.
        /// </summary>
        private void EnableEditModeDirectly()
        {
            IsReadOnly = false;

            // If a component was loaded for viewing, change its state from Uploaded/Validated to Defined
            if (!string.IsNullOrEmpty(LoadedComponentId) && LoadedHardwareType.HasValue)
            {
                ChangeComponentStateToDefinedAsync();
            }
        }

        /// <summary>
        /// Changes the loaded component's state from Uploaded/Validated to Defined in the repository.
        /// </summary>
        private async void ChangeComponentStateToDefinedAsync()
        {
            try
            {
                var apiService = ServiceLocator.GetService<IHardwareApiService>();
                var response = await apiService.UpdateSessionStateAsync(LoadedComponentId, ComponentState.Draft);

                if (!response.Success)
                {
                    // Silently fail - non-critical operation
                }
            }
            catch
            {
                // Silently fail - non-critical operation
            }
        }

        /// <summary>
        /// Creates a new version of the current workflow with updated version number.
        /// Original data remains unchanged. HardwareKey is preserved.
        /// </summary>
        /// <param name="newVersion">The new version string to apply.</param>
        private async void EnableEditModeWithNewVersionAsync(string newVersion)
        {
            try
            {
                // 1. Create HardwareDefinition from current state
                var sessionDto = ToHardwareDefinition();

                // 2. Create a copy with new session ID but same HardwareKey
                var copiedSession = CreateNewVersionSession(sessionDto, newVersion);

                // 3. Save to WorkflowRepository
                var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
                var sessions = await workflowRepo.LoadAsync();
                sessions.Add(copiedSession);
                await workflowRepo.SaveAsync(sessions);

                // 4. Navigate to the copied workflow
                NavigationService.Instance.ResumeWorkflow(copiedSession.Id);
            }
            catch (Exception)
            {
                // Show error toast
                ToastService.Instance.ShowError(
                    Strings.Toast_CopyWorkflowFailed_Title,
                    Strings.Toast_CopyWorkflowFailed_Description);
            }
        }

        /// <summary>
        /// Creates a completely new hardware with new GUID and ID from the current workflow.
        /// Original data remains unchanged. No version history is shared.
        /// </summary>
        private async void EnableEditModeWithNewHardwareAsync()
        {
            try
            {
                // 1. Create HardwareDefinition from current state
                var sessionDto = ToHardwareDefinition();

                // 2. Create a copy with new HardwareKey and regenerated IDs
                var copiedSession = CreateNewHardwareSession(sessionDto);

                // 3. Save to WorkflowRepository
                var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
                var sessions = await workflowRepo.LoadAsync();
                sessions.Add(copiedSession);
                await workflowRepo.SaveAsync(sessions);

                // 4. Navigate to the copied workflow
                NavigationService.Instance.ResumeWorkflow(copiedSession.Id);
            }
            catch (Exception)
            {
                // Show error toast
                ToastService.Instance.ShowError(
                    Strings.Toast_CopyWorkflowFailed_Title,
                    Strings.Toast_CopyWorkflowFailed_Description);
            }
        }

        #endregion

        #region Session Copy/Version Helpers

        /// <summary>
        /// Recursively regenerates all node IDs in a HardwareDefinition tree.
        /// </summary>
        /// <param name="node">The root node to regenerate IDs for.</param>
        public static void RegenerateNodeIds(HardwareDefinition node)
        {
            node.Id = Guid.NewGuid().ToString();
            foreach (var child in node.Children ?? new List<HardwareDefinition>())
            {
                RegenerateNodeIds(child);
            }
        }

        /// <summary>
        /// Applies copy suffix to the node's name.
        /// Uses the existing GenerateCopyName logic from HardwareTreeNodeViewModel.
        /// </summary>
        /// <param name="node">The node to apply copy suffix to.</param>
        public static void ApplyCopySuffixToNode(HardwareDefinition node)
        {
            node.Name = HardwareTreeNodeViewModel.GenerateCopyName(node.Name);
        }

        /// <summary>
        /// Creates a copy of a workflow session with new IDs and copy suffix applied.
        /// The copied session is set to Draft state.
        /// </summary>
        /// <param name="originalSession">The original session to copy.</param>
        /// <returns>A new HardwareDefinition with regenerated IDs and copy suffix.</returns>
        public static HardwareDefinition CreateCopySession(HardwareDefinition originalSession)
        {
            // Create a deep copy
            var copiedSession = DeepCopyHardwareDefinition(originalSession);
            copiedSession.Id = Guid.NewGuid().ToString();
            copiedSession.State = ComponentState.Draft;
            copiedSession.LastModifiedAt = DateTime.Now;

            // Regenerate all node IDs
            RegenerateNodeIds(copiedSession);

            // Apply copy suffix to root node name
            ApplyCopySuffixToNode(copiedSession);

            return copiedSession;
        }

        /// <summary>
        /// Creates a new version of a workflow session with updated version number.
        /// The copied session has a new session ID but preserves the HardwareKey.
        /// </summary>
        /// <param name="originalSession">The original session to copy.</param>
        /// <param name="newVersion">The new version string to apply.</param>
        /// <returns>A new HardwareDefinition with updated version.</returns>
        public static HardwareDefinition CreateNewVersionSession(HardwareDefinition originalSession, string newVersion)
        {
            // Create a deep copy
            var copiedSession = DeepCopyHardwareDefinition(originalSession);
            copiedSession.Id = Guid.NewGuid().ToString();
            copiedSession.State = ComponentState.Draft;
            copiedSession.LastModifiedAt = DateTime.Now;
            copiedSession.Version = newVersion;

            // Regenerate all node IDs but preserve HardwareKey
            RegenerateNodeIds(copiedSession);

            return copiedSession;
        }

        /// <summary>
        /// Creates a completely new hardware session with new GUID and ID.
        /// No version history is shared with the original.
        /// </summary>
        /// <param name="originalSession">The original session to copy.</param>
        /// <returns>A new HardwareDefinition with new HardwareKey and IDs.</returns>
        public static HardwareDefinition CreateNewHardwareSession(HardwareDefinition originalSession)
        {
            // Create a deep copy
            var copiedSession = DeepCopyHardwareDefinition(originalSession);
            copiedSession.Id = Guid.NewGuid().ToString();
            // TODO: Deepcopy Process with new Process ID
            copiedSession.State = ComponentState.Draft;
            copiedSession.LastModifiedAt = DateTime.Now;
            copiedSession.HardwareKey = Guid.NewGuid().ToString();

            // Regenerate all node IDs
            RegenerateNodeIds(copiedSession);

            // Apply copy suffix to root node name
            ApplyCopySuffixToNode(copiedSession);

            return copiedSession;
        }

        /// <summary>
        /// Creates a deep copy of a HardwareDefinition including all children.
        /// </summary>
        private static HardwareDefinition DeepCopyHardwareDefinition(HardwareDefinition original)
        {
            return new HardwareDefinition
            {
                Id = original.Id,
                HardwareKey = original.HardwareKey,
                HardwareType = original.HardwareType,
                Version = original.Version,
                State = original.State,
                Name = original.Name,
                DisplayName = original.DisplayName,
                Description = original.Description,
                Customer = original.Customer,
                ProcessId = original.ProcessId,
                ProcessInfo = original.ProcessInfo,
                EquipmentType = original.EquipmentType,
                DeviceType = original.DeviceType,
                ImplementationInstructions = original.ImplementationInstructions?.ToList() ?? new List<string>(),
                Commands = original.Commands?.Select(CopyCommandDto).ToList() ?? new List<CommandDto>(),
                IoInfo = original.IoInfo?.Select(CopyIoInfoDto).ToList() ?? new List<IoInfoDto>(),
                AttachedDocumentsIds = original.AttachedDocumentsIds?.ToList() ?? new List<string>(),
                ProgramRoot = original.ProgramRoot,
                CreatedAt = original.CreatedAt,
                UpdatedAt = original.UpdatedAt,
                LastModifiedAt = original.LastModifiedAt,
                Children = original.Children?.Select(DeepCopyHardwareDefinition).ToList() ?? new List<HardwareDefinition>()
            };
        }

        private static CommandDto CopyCommandDto(CommandDto original)
        {
            return new CommandDto
            {
                Name = original.Name,
                Description = original.Description,
                Parameters = original.Parameters?.Select(p => new ParameterDto
                {
                    Name = p.Name,
                    Type = p.Type,
                    Description = p.Description
                }).ToList() ?? new List<ParameterDto>()
            };
        }

        private static IoInfoDto CopyIoInfoDto(IoInfoDto original)
        {
            return new IoInfoDto
            {
                Name = original.Name,
                IoType = original.IoType,
                Address = original.Address,
                Description = original.Description
            };
        }

        #endregion
    }
}
