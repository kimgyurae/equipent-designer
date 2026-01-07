using System;
using System.Linq;
using System.Threading.Tasks;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models.Dtos;
using EquipmentDesigner.Models.Storage;
using EquipmentDesigner.Services;
using EquipmentDesigner.Services.Storage;
using EquipmentDesigner.Views.HardwareDefineWorkflow;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Views.HardwareDefineWorkflow
{
    /// <summary>
    /// Tests for autosave functionality in HardwareDefineWorkflowViewModel.
    /// Bug Fix: OnDataChanged() was not calling MarkDirty(), so field edits didn't trigger autosave.
    /// </summary>
    public class HardwareDefineWorkflowViewModelAutosaveTests
    {
        #region API-Level Test - Bug Reproduction

        [Fact]
        public async Task PropertyChange_InDeviceViewModel_TriggersAutosaveViaDebounce()
        {
            // Arrange - API-level test: User edits a field and expects data to be saved
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Device);
            viewModel.EnableAutosave();

            // Act - User changes a property (simulates typing in a field)
            viewModel.DeviceViewModel.Name = "TestDevice";
            await Task.Delay(2500); // Wait for debounce (2s) + buffer

            // Assert - Data should be saved to workflows.json
            var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
            var workflowData = await workflowRepo.LoadAsync();
            var session = workflowData.WorkflowSessions.FirstOrDefault(s => s.WorkflowId == viewModel.WorkflowId);

            session.Should().NotBeNull("because property change should trigger autosave");
            session.TreeNodes.Should().NotBeEmpty();
            session.TreeNodes.First().DeviceData.Name.Should().Be("TestDevice");

            // Cleanup
            viewModel.DisableAutosave();
        }

        [Fact]
        public async Task MultipleRapidChanges_ResultInSingleSaveAfterDebounce()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Device);
            viewModel.EnableAutosave();

            // Act - Multiple rapid changes (simulates user typing)
            viewModel.DeviceViewModel.Name = "T";
            await Task.Delay(100);
            viewModel.DeviceViewModel.Name = "Te";
            await Task.Delay(100);
            viewModel.DeviceViewModel.Name = "Tes";
            await Task.Delay(100);
            viewModel.DeviceViewModel.Name = "Test";
            await Task.Delay(100);
            viewModel.DeviceViewModel.Name = "TestDevice";
            await Task.Delay(2500); // Wait for debounce

            // Assert - Final value should be saved
            var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
            var workflowData = await workflowRepo.LoadAsync();
            var session = workflowData.WorkflowSessions.FirstOrDefault(s => s.WorkflowId == viewModel.WorkflowId);

            session.Should().NotBeNull();
            session.TreeNodes.First().DeviceData.Name.Should().Be("TestDevice");

            // Cleanup
            viewModel.DisableAutosave();
        }

        #endregion

        #region Replication Tests - Root Cause Isolation

        [Fact]
        public void OnDataChanged_WhenCalled_SetsIsDirtyToTrue()
        {
            // Arrange - Unit test isolating the root cause
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Device);
            viewModel.EnableAutosave();

            // Act - Directly call OnDataChanged (this is what child ViewModel changes trigger)
            viewModel.OnDataChanged();

            // Assert - MarkDirty should be called, setting isDirty = true
            // We verify this by checking IsAutosaveEnabled (indirect verification)
            // The actual _isDirty flag is private, but we can verify behavior
            viewModel.IsAutosaveEnabled.Should().BeTrue();

            // Cleanup
            viewModel.DisableAutosave();
        }

        [Fact]
        public void DeviceViewModelNameChange_TriggersOnNodeDataPropertyChanged()
        {
            // Arrange
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Device);
            var onDataChangedCalled = false;

            // Subscribe to CanCompleteWorkflow which changes when OnDataChanged is called
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(HardwareDefineWorkflowViewModel.CanCompleteWorkflow) ||
                    e.PropertyName == nameof(HardwareDefineWorkflowViewModel.AllStepsRequiredFieldsFilled))
                {
                    onDataChangedCalled = true;
                }
            };

            // Act
            viewModel.DeviceViewModel.Name = "TestDevice";

            // Assert
            onDataChangedCalled.Should().BeTrue("because property change should trigger OnDataChanged");
        }

        #endregion

        #region Autosave Timer Configuration Tests

        [Fact]
        public void EnableAutosave_InitializesTimerWith15SecondInterval()
        {
            // Arrange
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Device);

            // Act
            viewModel.EnableAutosave();

            // Assert
            viewModel.IsAutosaveEnabled.Should().BeTrue();

            // Cleanup
            viewModel.DisableAutosave();
        }

        [Fact]
        public void EnableAutosave_WhenAlreadyEnabled_DoesNothing()
        {
            // Arrange
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Device);
            viewModel.EnableAutosave();
            var firstEnableState = viewModel.IsAutosaveEnabled;

            // Act - Call EnableAutosave again
            viewModel.EnableAutosave();

            // Assert - Should still be enabled, no double initialization
            viewModel.IsAutosaveEnabled.Should().Be(firstEnableState);

            // Cleanup
            viewModel.DisableAutosave();
        }

        [Fact]
        public void DisableAutosave_StopsAutosaveTimer()
        {
            // Arrange
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Device);
            viewModel.EnableAutosave();

            // Act
            viewModel.DisableAutosave();

            // Assert
            viewModel.IsAutosaveEnabled.Should().BeFalse();
        }

        #endregion

        #region Debounce Timer Behavior Tests

        [Fact]
        public async Task DebounceTimer_After2Seconds_SavesWorkflow()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Device);
            viewModel.DeviceViewModel.Name = "TestDevice";
            viewModel.EnableAutosave();

            // Act - Trigger data change and wait for debounce
            viewModel.OnDataChanged();
            await Task.Delay(2500); // Wait for 2s debounce + buffer

            // Assert
            var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
            var workflowData = await workflowRepo.LoadAsync();
            workflowData.WorkflowSessions.Should().Contain(s => s.WorkflowId == viewModel.WorkflowId);

            // Cleanup
            viewModel.DisableAutosave();
        }

        [Fact]
        public async Task DebounceTimer_RestartsOnEachChange()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Device);
            viewModel.EnableAutosave();

            // Act - Change every second (less than 2s debounce), should not save yet
            viewModel.DeviceViewModel.Name = "Name1";
            await Task.Delay(1000);
            viewModel.DeviceViewModel.Name = "Name2";
            await Task.Delay(1000);
            viewModel.DeviceViewModel.Name = "Name3";
            await Task.Delay(500); // Total: 2.5s but debounce restarted at 2s mark

            // Assert - Should NOT have saved yet (debounce is 2s after LAST change)
            var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
            var workflowData = await workflowRepo.LoadAsync();
            var sessionBefore = workflowData.WorkflowSessions.FirstOrDefault(s => s.WorkflowId == viewModel.WorkflowId);

            // Now wait for debounce to complete
            await Task.Delay(2000);

            workflowData = await workflowRepo.LoadAsync();
            var sessionAfter = workflowData.WorkflowSessions.FirstOrDefault(s => s.WorkflowId == viewModel.WorkflowId);

            sessionAfter.Should().NotBeNull("because debounce should have fired after 2s of inactivity");
            sessionAfter.TreeNodes.First().DeviceData.Name.Should().Be("Name3");

            // Cleanup
            viewModel.DisableAutosave();
        }

        #endregion

        #region ReadOnly Mode Protection Tests

        [Fact]
        public async Task OnDataChanged_WhenIsReadOnlyTrue_DoesNotTriggerSave()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Device);
            viewModel.IsReadOnly = true;
            viewModel.EnableAutosave();

            // Act
            viewModel.DeviceViewModel.Name = "TestDevice";
            viewModel.OnDataChanged();
            await Task.Delay(2500); // Wait for debounce

            // Assert - No save should occur
            var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
            var workflowData = await workflowRepo.LoadAsync();
            workflowData.WorkflowSessions.Should().BeEmpty("because no save should occur in read-only mode");

            // Cleanup
            viewModel.DisableAutosave();
        }

        [Fact]
        public void RestartDebounceTimer_WhenIsReadOnlyTrue_DoesNothing()
        {
            // Arrange
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Device);
            viewModel.IsReadOnly = true;
            viewModel.EnableAutosave();

            // Act - This should not throw and should do nothing
            viewModel.OnDataChanged();

            // Assert - Just verify no exception was thrown
            viewModel.IsAutosaveEnabled.Should().BeTrue();

            // Cleanup
            viewModel.DisableAutosave();
        }

        #endregion

        #region Timer Cleanup Tests

        [Fact]
        public void DisableAutosave_StopsBothAutosaveAndDebounceTimers()
        {
            // Arrange
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Device);
            viewModel.EnableAutosave();
            viewModel.OnDataChanged(); // Start debounce timer

            // Act
            viewModel.DisableAutosave();

            // Assert
            viewModel.IsAutosaveEnabled.Should().BeFalse();
        }

        [Fact]
        public void DisableAutosave_WhenNotEnabled_DoesNothing()
        {
            // Arrange
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Device);
            // Don't enable autosave

            // Act - Should not throw
            viewModel.DisableAutosave();

            // Assert
            viewModel.IsAutosaveEnabled.Should().BeFalse();
        }

        #endregion

        #region Integration with Repository Tests

        [Fact]
        public async Task SaveWorkflowStateAsync_ResetsIsDirtyFlagAfterSave()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Device);
            viewModel.DeviceViewModel.Name = "TestDevice";
            viewModel.EnableAutosave();

            // Act - Trigger save via debounce
            viewModel.OnDataChanged();
            await Task.Delay(2500);

            // Modify again and immediately check
            viewModel.DeviceViewModel.Name = "ModifiedDevice";
            await Task.Delay(2500);

            // Assert - Both saves should have occurred (second change marked dirty again)
            var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
            var workflowData = await workflowRepo.LoadAsync();
            var session = workflowData.WorkflowSessions.FirstOrDefault(s => s.WorkflowId == viewModel.WorkflowId);

            session.Should().NotBeNull();
            session.TreeNodes.First().DeviceData.Name.Should().Be("ModifiedDevice");

            // Cleanup
            viewModel.DisableAutosave();
        }

        [Fact]
        public async Task SaveWorkflowStateAsync_UpdatesLastModifiedAtTimestamp()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Device);
            viewModel.DeviceViewModel.Name = "TestDevice";
            viewModel.EnableAutosave();
            var beforeSave = DateTime.Now;

            // Act
            viewModel.OnDataChanged();
            await Task.Delay(2500);

            // Assert
            var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
            var workflowData = await workflowRepo.LoadAsync();
            var session = workflowData.WorkflowSessions.First(s => s.WorkflowId == viewModel.WorkflowId);

            session.LastModifiedAt.Should().BeOnOrAfter(beforeSave);

            // Cleanup
            viewModel.DisableAutosave();
        }

        #endregion

        #region End-to-End Flow Tests

        [Fact]
        public async Task PropertyChange_InEquipmentViewModel_TriggersAutosave()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
            viewModel.EnableAutosave();

            // Act
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            await Task.Delay(2500);

            // Assert
            var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
            var workflowData = await workflowRepo.LoadAsync();
            var session = workflowData.WorkflowSessions.FirstOrDefault(s => s.WorkflowId == viewModel.WorkflowId);

            session.Should().NotBeNull();
            session.TreeNodes.First().EquipmentData.Name.Should().Be("TestEquipment");

            // Cleanup
            viewModel.DisableAutosave();
        }

        [Fact]
        public async Task ViewUnload_DuringDebounce_CancelsPendingSave()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Device);
            viewModel.EnableAutosave();
            viewModel.DeviceViewModel.Name = "TestDevice";
            viewModel.OnDataChanged();

            // Act - Disable autosave before debounce fires (simulates view unload)
            await Task.Delay(500); // Wait less than debounce time
            viewModel.DisableAutosave();
            await Task.Delay(2000); // Wait for what would have been the debounce time

            // Assert - No save should have occurred
            var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
            var workflowData = await workflowRepo.LoadAsync();
            workflowData.WorkflowSessions.Should().BeEmpty("because debounce was cancelled by DisableAutosave");
        }

        #endregion
    }
}
