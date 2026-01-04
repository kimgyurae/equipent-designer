using System.Collections.ObjectModel;
using System.Windows.Input;
using EquipmentDesigner.Services;

namespace EquipmentDesigner.Views.HardwareDefineWorkflow
{
    /// <summary>
    /// Main ViewModel for the Hardware Define Workflow.
    /// Orchestrates navigation between Equipment, System, Unit, and Device definition steps.
    /// </summary>
    public class HardwareDefineWorkflowViewModel : ViewModelBase
    {
        private int _currentStepIndex;
        private EquipmentDefineViewModel _equipmentViewModel;
        private SystemDefineViewModel _systemViewModel;
        private UnitDefineViewModel _unitViewModel;
        private DeviceDefineViewModel _deviceViewModel;

        public HardwareDefineWorkflowViewModel(WorkflowStartType startType)
        {
            StartType = startType;
            WorkflowSteps = new ObservableCollection<WorkflowStepViewModel>();

            InitializeWorkflowSteps();
            InitializeViewModels();
            InitializeCommands();

            UpdateStepStates();
        }

        /// <summary>
        /// The starting type of the workflow.
        /// </summary>
        public WorkflowStartType StartType { get; }

        /// <summary>
        /// Collection of workflow steps.
        /// </summary>
        public ObservableCollection<WorkflowStepViewModel> WorkflowSteps { get; }

        /// <summary>
        /// Current step index (0-based).
        /// </summary>
        public int CurrentStepIndex
        {
            get => _currentStepIndex;
            private set
            {
                if (SetProperty(ref _currentStepIndex, value))
                {
                    OnPropertyChanged(nameof(CurrentStep));
                    OnPropertyChanged(nameof(IsFirstStep));
                    OnPropertyChanged(nameof(IsLastStep));
                    OnPropertyChanged(nameof(CanGoToNext));
                    UpdateStepStates();
                }
            }
        }

        /// <summary>
        /// Current workflow step.
        /// </summary>
        public WorkflowStepViewModel CurrentStep => WorkflowSteps[CurrentStepIndex];

        /// <summary>
        /// Returns true if at the first step.
        /// </summary>
        public bool IsFirstStep => CurrentStepIndex == 0;

        /// <summary>
        /// Returns true if at the last step.
        /// </summary>
        public bool IsLastStep => CurrentStepIndex == WorkflowSteps.Count - 1;

        /// <summary>
        /// Returns true if navigation to the next step is allowed.
        /// </summary>
        public bool CanGoToNext => !IsLastStep && GetCurrentStepViewModel()?.CanProceedToNext == true;

        /// <summary>
        /// Equipment definition ViewModel.
        /// </summary>
        public EquipmentDefineViewModel EquipmentViewModel
        {
            get => _equipmentViewModel;
            private set => SetProperty(ref _equipmentViewModel, value);
        }

        /// <summary>
        /// System definition ViewModel.
        /// </summary>
        public SystemDefineViewModel SystemViewModel
        {
            get => _systemViewModel;
            private set => SetProperty(ref _systemViewModel, value);
        }

        /// <summary>
        /// Unit definition ViewModel.
        /// </summary>
        public UnitDefineViewModel UnitViewModel
        {
            get => _unitViewModel;
            private set => SetProperty(ref _unitViewModel, value);
        }

        /// <summary>
        /// Device definition ViewModel.
        /// </summary>
        public DeviceDefineViewModel DeviceViewModel
        {
            get => _deviceViewModel;
            private set => SetProperty(ref _deviceViewModel, value);
        }

        /// <summary>
        /// Command to navigate to the next step.
        /// </summary>
        public ICommand GoToNextStepCommand { get; private set; }

        /// <summary>
        /// Command to navigate to the previous step.
        /// </summary>
        public ICommand GoToPreviousStepCommand { get; private set; }

        /// <summary>
        /// Command to exit to dashboard.
        /// </summary>
        public ICommand ExitToDashboardCommand { get; private set; }

        /// <summary>
        /// Command to navigate directly to a specific workflow step.
        /// </summary>
        public ICommand NavigateToStepCommand { get; private set; }

        private void InitializeWorkflowSteps()
        {
            int stepNumber = 1;

            if (StartType == WorkflowStartType.Equipment)
            {
                WorkflowSteps.Add(new WorkflowStepViewModel(stepNumber++, "Equipment"));
            }

            if (StartType <= WorkflowStartType.System)
            {
                WorkflowSteps.Add(new WorkflowStepViewModel(stepNumber++, "System"));
            }

            if (StartType <= WorkflowStartType.Unit)
            {
                WorkflowSteps.Add(new WorkflowStepViewModel(stepNumber++, "Unit"));
            }

            WorkflowSteps.Add(new WorkflowStepViewModel(stepNumber, "Device"));
        }

        private void InitializeViewModels()
        {
            // Initialize all ViewModels - they will be used based on the current step
            EquipmentViewModel = new EquipmentDefineViewModel();
            SystemViewModel = new SystemDefineViewModel();
            UnitViewModel = new UnitDefineViewModel();
            DeviceViewModel = new DeviceDefineViewModel();

            // Subscribe to validation changes
            EquipmentViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(EquipmentDefineViewModel.CanProceedToNext))
                    OnPropertyChanged(nameof(CanGoToNext));
            };

            SystemViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SystemDefineViewModel.CanProceedToNext))
                    OnPropertyChanged(nameof(CanGoToNext));
            };

            UnitViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(UnitDefineViewModel.CanProceedToNext))
                    OnPropertyChanged(nameof(CanGoToNext));
            };

            DeviceViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(DeviceDefineViewModel.CanProceedToNext))
                    OnPropertyChanged(nameof(CanGoToNext));
            };
        }

        private void InitializeCommands()
        {
            GoToNextStepCommand = new RelayCommand(ExecuteGoToNextStep, CanExecuteGoToNextStep);
            GoToPreviousStepCommand = new RelayCommand(ExecuteGoToPreviousStep, CanExecuteGoToPreviousStep);
            ExitToDashboardCommand = new RelayCommand(ExecuteExitToDashboard);
            NavigateToStepCommand = new RelayCommand<WorkflowStepViewModel>(ExecuteNavigateToStep, CanExecuteNavigateToStep);
        }

        private void ExecuteGoToNextStep()
        {
            if (CurrentStepIndex < WorkflowSteps.Count - 1)
            {
                CurrentStepIndex++;
            }
        }

        private bool CanExecuteGoToNextStep()
        {
            return CanGoToNext;
        }

        private void ExecuteGoToPreviousStep()
        {
            if (CurrentStepIndex > 0)
            {
                CurrentStepIndex--;
            }
        }

        private bool CanExecuteGoToPreviousStep()
        {
            return !IsFirstStep;
        }

        private void ExecuteExitToDashboard()
        {
            NavigationService.Instance.NavigateToDashboard();
        }

        private void ExecuteNavigateToStep(WorkflowStepViewModel step)
        {
            if (step == null) return;

            var targetIndex = WorkflowSteps.IndexOf(step);
            if (targetIndex < 0) return; // Step not in workflow

            // Only navigate if the step is reachable (active or completed)
            if (!step.CanNavigateTo) return;

            // Don't navigate if already at this step
            if (targetIndex == CurrentStepIndex) return;

            CurrentStepIndex = targetIndex;
        }

        private bool CanExecuteNavigateToStep(WorkflowStepViewModel step)
        {
            if (step == null) return false;
            return step.CanNavigateTo;
        }

        private void UpdateStepStates()
        {
            for (int i = 0; i < WorkflowSteps.Count; i++)
            {
                var step = WorkflowSteps[i];
                step.IsActive = i == CurrentStepIndex;
                step.IsCompleted = i < CurrentStepIndex;
            }
        }

        private dynamic GetCurrentStepViewModel()
        {
            var currentStepName = CurrentStep.StepName;

            return currentStepName switch
            {
                "Equipment" => EquipmentViewModel,
                "System" => SystemViewModel,
                "Unit" => UnitViewModel,
                "Device" => DeviceViewModel,
                _ => null
            };
        }
    }
}