using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using EquipmentDesigner.Models;
using EquipmentDesigner.Services;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// Partial class containing autosave functionality.
    /// Handles automatic workflow state persistence with debouncing.
    /// </summary>
    public partial class HardwareDefineWorkflowViewModel
    {
        #region Autosave Fields

        private DispatcherTimer _autosaveTimer;
        private CancellationTokenSource _debounceCts;
        private bool _isDirty;
        private bool _isAutosaveEnabled;
        private static readonly TimeSpan DefaultAutosaveInterval = TimeSpan.FromSeconds(15);
        private static readonly TimeSpan DefaultDebounceDelay = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan MaxWaitTime = TimeSpan.FromSeconds(10);
        private bool _isAutosaveIndicatorVisible;
        private DispatcherTimer _autosaveIndicatorTimer;
        private DateTime? _firstDirtyTime;

        #endregion

        #region Autosave Properties

        /// <summary>
        /// Gets or sets whether the autosave indicator is visible.
        /// </summary>
        public bool IsAutosaveIndicatorVisible
        {
            get => _isAutosaveIndicatorVisible;
            private set => SetProperty(ref _isAutosaveIndicatorVisible, value);
        }

        /// <summary>
        /// Gets whether autosave is currently enabled.
        /// </summary>
        public bool IsAutosaveEnabled => _isAutosaveEnabled;

        #endregion

        #region Autosave Public Methods

        /// <summary>
        /// Enables autosave with the default interval (30 seconds).
        /// </summary>
        public void EnableAutosave()
        {
            EnableAutosave(DefaultAutosaveInterval);
        }

        /// <summary>
        /// Enables autosave with a custom interval.
        /// </summary>
        /// <param name="interval">The interval between autosave attempts.</param>
        public void EnableAutosave(TimeSpan interval)
        {
            if (_isAutosaveEnabled)
                return;

            _autosaveTimer = new DispatcherTimer
            {
                Interval = interval
            };
            _autosaveTimer.Tick += OnAutosaveTimerTick;
            _autosaveTimer.Start();
            _isAutosaveEnabled = true;
        }

        /// <summary>
        /// Disables autosave and stops both autosave and debounce timers.
        /// </summary>
        public void DisableAutosave()
        {
            if (!_isAutosaveEnabled)
                return;

            if (_autosaveTimer != null)
            {
                _autosaveTimer.Stop();
                _autosaveTimer.Tick -= OnAutosaveTimerTick;
                _autosaveTimer = null;
            }

            if (_debounceCts != null)
            {
                _debounceCts.Cancel();
                _debounceCts = null;
            }

            _isAutosaveEnabled = false;
        }

        /// <summary>
        /// Marks the workflow data as dirty (needing save).
        /// Call this when any data changes.
        /// </summary>
        public void MarkDirty()
        {
            _isDirty = true;

            // Track when the first dirty change occurred
            if (_firstDirtyTime == null)
            {
                _firstDirtyTime = DateTime.Now;
            }
        }

        #endregion

        #region Autosave Private Methods

        /// <summary>
        /// Restarts the debounce timer. Saves data 2 seconds after the last change.
        /// If max wait time has been exceeded since first change, saves immediately.
        /// </summary>
        private void RestartDebounceTimer()
        {
            if (IsReadOnly || !_isAutosaveEnabled)
                return;

            // Check if max wait time has been exceeded since first dirty change
            if (_firstDirtyTime.HasValue &&
                DateTime.Now - _firstDirtyTime.Value >= MaxWaitTime)
            {
                // Cancel any pending debounce and save immediately
                if (_debounceCts != null)
                {
                    _debounceCts.Cancel();
                    _debounceCts = null;
                }

                // Save immediately on a background thread
                Task.Run(async () =>
                {
                    if (_isDirty && !IsReadOnly)
                    {
                        await SaveWorkflowStateAsync();
                    }
                });
                return;
            }

            // Normal debounce behavior
            if (_debounceCts != null)
            {
                _debounceCts.Cancel();
                _debounceCts = null;
            }

            _debounceCts = new CancellationTokenSource();
            var token = _debounceCts.Token;

            Task.Delay(DefaultDebounceDelay, token).ContinueWith(async t =>
            {
                if (t.IsCanceled)
                    return;

                if (_isDirty && !IsReadOnly)
                {
                    await SaveWorkflowStateAsync();
                }
            }, token);
        }

        /// <summary>
        /// Handler for autosave timer tick.
        /// Saves workflow state if data is dirty.
        /// </summary>
        private async void OnAutosaveTimerTick(object sender, EventArgs e)
        {
            if (_isDirty && !IsReadOnly)
            {
                await SaveWorkflowStateAsync();
            }
        }

        /// <summary>
        /// Saves the current workflow state to repository.
        /// Uses the new WorkflowRepository (IWorkflowRepository)
        /// structure to persist in-progress workflow sessions.
        /// </summary>
        /// <param name="showAutosaveIndicator">Whether to show the autosave indicator after saving. Default is true for autosave, false for explicit saves.</param>
        private async Task SaveWorkflowStateAsync(bool showAutosaveIndicator = true)
        {
            var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
            var sessions = await workflowRepo.LoadAsync();

            // Create or update HardwareDefinition
            var sessionDto = ToHardwareDefinition();
            var existingIndex = sessions.FindIndex(s => s.Id == HardwareId);

            if (existingIndex >= 0)
                sessions[existingIndex] = sessionDto;
            else
                sessions.Add(sessionDto);

            await workflowRepo.SaveAsync(sessions);
            _isDirty = false;
            _firstDirtyTime = null; // Reset first dirty time after successful save

            // Show autosave indicator only for autosave
            if (showAutosaveIndicator)
            {
                ShowAutosaveIndicator();
            }
        }

        /// <summary>
        /// Shows the autosave indicator for 2 seconds.
        /// </summary>
        private void ShowAutosaveIndicator()
        {
            // Ensure we're on the UI thread
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher == null)
                return;

            if (!dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(new Action(ShowAutosaveIndicator));
                return;
            }

            // Stop existing timer if running
            if (_autosaveIndicatorTimer != null)
            {
                _autosaveIndicatorTimer.Stop();
                _autosaveIndicatorTimer.Tick -= OnAutosaveIndicatorTimerTick;
                _autosaveIndicatorTimer = null;
            }

            IsAutosaveIndicatorVisible = true;

            // Start 2-second timer to hide indicator
            _autosaveIndicatorTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _autosaveIndicatorTimer.Tick += OnAutosaveIndicatorTimerTick;
            _autosaveIndicatorTimer.Start();
        }

        /// <summary>
        /// Handler for autosave indicator timer tick.
        /// Hides the indicator after 2 seconds.
        /// </summary>
        private void OnAutosaveIndicatorTimerTick(object sender, EventArgs e)
        {
            if (_autosaveIndicatorTimer != null)
            {
                _autosaveIndicatorTimer.Stop();
                _autosaveIndicatorTimer.Tick -= OnAutosaveIndicatorTimerTick;
                _autosaveIndicatorTimer = null;
            }

            IsAutosaveIndicatorVisible = false;
        }

        #endregion
    }
}
