namespace EquipmentDesigner.Views.HardwareDefineWorkflow
{
    /// <summary>
    /// ViewModel representing a single step in the hardware definition workflow.
    /// </summary>
    public class WorkflowStepViewModel : ViewModelBase
    {
        private bool _isActive;
        private bool _isCompleted;
        private int _filledFieldCount;
        private int _totalFieldCount;
        private string _componentName = string.Empty;

        public WorkflowStepViewModel(int stepNumber, string stepName)
        {
            StepNumber = stepNumber;
            StepName = stepName;
        }

        /// <summary>
        /// The step number (1-based).
        /// </summary>
        public int StepNumber { get; }

        /// <summary>
        /// The display name of this step.
        /// </summary>
        public string StepName { get; }

        /// <summary>
        /// The hardware layer type label (Equipment, System, Unit, Device).
        /// This is the same as StepName but provides semantic clarity.
        /// </summary>
        public string HardwareLayerType => StepName;

        /// <summary>
        /// The actual component name from the associated ViewModel.
        /// </summary>
        public string ComponentName
        {
            get => _componentName;
            set
            {
                if (SetProperty(ref _componentName, value))
                {
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        /// <summary>
        /// Display name for the step: shows ComponentName if set, otherwise "New {HardwareLayerType}".
        /// </summary>
        public string DisplayName => string.IsNullOrWhiteSpace(ComponentName) 
            ? $"New {HardwareLayerType}" 
            : ComponentName;

        /// <summary>
        /// Whether this step is currently active.
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        /// <summary>
        /// Whether this step has been completed (all required fields filled).
        /// </summary>
        public bool IsCompleted
        {
            get => _isCompleted;
            set => SetProperty(ref _isCompleted, value);
        }

        /// <summary>
        /// Number of fields that have been filled.
        /// </summary>
        public int FilledFieldCount
        {
            get => _filledFieldCount;
            set
            {
                if (SetProperty(ref _filledFieldCount, value))
                {
                    OnPropertyChanged(nameof(FieldProgressText));
                }
            }
        }

        /// <summary>
        /// Total number of fields in this step.
        /// </summary>
        public int TotalFieldCount
        {
            get => _totalFieldCount;
            set
            {
                if (SetProperty(ref _totalFieldCount, value))
                {
                    OnPropertyChanged(nameof(FieldProgressText));
                }
            }
        }

        /// <summary>
        /// Display text showing field progress (e.g., "3/7").
        /// </summary>
        public string FieldProgressText => $"{FilledFieldCount}/{TotalFieldCount}";

        /// <summary>
        /// Whether navigation to this step is allowed.
        /// All steps are always navigable.
        /// </summary>
        public bool CanNavigateTo => true;
    }
}