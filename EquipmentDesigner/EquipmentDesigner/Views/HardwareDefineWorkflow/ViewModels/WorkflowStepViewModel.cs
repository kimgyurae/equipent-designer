namespace EquipmentDesigner.Views.HardwareDefineWorkflow
{
    /// <summary>
    /// ViewModel representing a single step in the hardware definition workflow.
    /// </summary>
    public class WorkflowStepViewModel : ViewModelBase
    {
        private bool _isActive;
        private bool _isCompleted;

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
        /// Whether this step is currently active.
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (SetProperty(ref _isActive, value))
                {
                    OnPropertyChanged(nameof(CanNavigateTo));
                }
            }
        }

        /// <summary>
        /// Whether this step has been completed.
        /// </summary>
        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (SetProperty(ref _isCompleted, value))
                {
                    OnPropertyChanged(nameof(CanNavigateTo));
                }
            }
        }

        /// <summary>
        /// Whether navigation to this step is allowed.
        /// Navigation is allowed if the step is completed or currently active.
        /// </summary>
        public bool CanNavigateTo => IsActive || IsCompleted;
    }
}