using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using EquipmentDesigner.Models;
using EquipmentDesigner.Services;
using EquipmentDesigner.Controls;
using EquipmentDesigner.Views;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// Display model for hierarchical hardware nodes in WorkflowCompleteView.
    /// </summary>
    public class HardwareNodeDisplayModel
    {
        public string NodeId { get; set; }
        public HardwareType HardwareType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public HardwareDefinition SourceDto { get; set; }
        public ObservableCollection<HardwareNodeDisplayModel> Children { get; set; } = new ObservableCollection<HardwareNodeDisplayModel>();

        public static HardwareNodeDisplayModel FromHardwareDefinition(HardwareDefinition dto)
        {
            if (dto == null) return null;

            var model = new HardwareNodeDisplayModel
            {
                NodeId = dto.Id,
                HardwareType = dto.HardwareType,
                SourceDto = dto,
                Name = dto.Name ?? string.Empty,
                Description = dto.Description ?? string.Empty
            };

            // Recursively convert children
            if (dto.Children != null)
            {
                foreach (var child in dto.Children)
                {
                    var childModel = FromHardwareDefinition(child);
                    if (childModel != null)
                    {
                        model.Children.Add(childModel);
                    }
                }
            }

            return model;
        }
    }

    /// <summary>
    /// ViewModel for the WorkflowCompleteView.
    /// Displays hierarchical hardware summary and provides upload functionality.
    /// </summary>
    public class WorkflowCompleteViewModel : ViewModelBase
    {
        private readonly HardwareDefinition _sessionDto;
        private ObservableCollection<HardwareNodeDisplayModel> _treeNodes;

        /// <summary>
        /// Creates a new WorkflowCompleteViewModel with the given session data.
        /// </summary>
        /// <param name="sessionDto">The workflow session DTO containing all hardware data.</param>
        /// <exception cref="ArgumentNullException">Thrown when sessionDto is null.</exception>
        public WorkflowCompleteViewModel(HardwareDefinition sessionDto)
        {
            _sessionDto = sessionDto ?? throw new ArgumentNullException(nameof(sessionDto));

            // Set state to Ready
            _sessionDto.State = ComponentState.Ready;

            // Initialize tree nodes from session
            InitializeTreeNodes();

            // Initialize commands
            InitializeCommands();
        }

        /// <summary>
        /// Parameterless constructor for design-time support.
        /// </summary>
        public WorkflowCompleteViewModel()
        {
            _treeNodes = new ObservableCollection<HardwareNodeDisplayModel>();
            InitializeCommands();
        }

        #region Properties

        /// <summary>
        /// Gets the workflow ID.
        /// </summary>
        public string WorkflowId => _sessionDto?.Id;

        /// <summary>
        /// Gets the start type of the workflow.
        /// </summary>
        public HardwareType StartType => _sessionDto?.HardwareType ?? HardwareType.Equipment;

        /// <summary>
        /// Gets the current session state.
        /// </summary>
        public ComponentState SessionState => _sessionDto?.State ?? ComponentState.Draft;

        /// <summary>
        /// Gets the hierarchical tree nodes for display.
        /// </summary>
        public ObservableCollection<HardwareNodeDisplayModel> TreeNodes
        {
            get => _treeNodes;
            private set => SetProperty(ref _treeNodes, value);
        }

        /// <summary>
        /// Gets the title text for the view.
        /// </summary>
        public string Title => "Settings Confirmation";

        /// <summary>
        /// Gets the subtitle text for the view.
        /// </summary>
        public string Subtitle => "Review the Equipment hierarchy and settings. (Click a card to view details)";

        #endregion

        #region Commands

        /// <summary>
        /// Command to upload the workflow to the server.
        /// </summary>
        public ICommand UploadCommand { get; private set; }

        /// <summary>
        /// Command to continue editing the workflow.
        /// </summary>
        public ICommand ContinueEditingCommand { get; private set; }

        /// <summary>
        /// Command to save and navigate to dashboard without uploading.
        /// </summary>
        public ICommand UploadLaterCommand { get; private set; }

        /// <summary>
        /// Command to handle card click events.
        /// </summary>
        public ICommand CardClickCommand { get; private set; }

        #endregion

        #region Initialization

        private void InitializeTreeNodes()
        {
            _treeNodes = new ObservableCollection<HardwareNodeDisplayModel>();

            if (_sessionDto == null)
                return;

            // The session itself is the root node
            var displayModel = HardwareNodeDisplayModel.FromHardwareDefinition(_sessionDto);
            if (displayModel != null)
            {
                _treeNodes.Add(displayModel);
            }
        }

        private void InitializeCommands()
        {
            UploadCommand = new RelayCommand(ExecuteUpload, CanExecuteUpload);
            ContinueEditingCommand = new RelayCommand(ExecuteContinueEditing);
            UploadLaterCommand = new RelayCommand(ExecuteUploadLater);
            CardClickCommand = new RelayCommand<HardwareNodeDisplayModel>(ExecuteCardClick);
        }

        #endregion

        #region Command Implementations

        private bool CanExecuteUpload()
        {
            return _sessionDto != null && !string.IsNullOrEmpty(_sessionDto.Id);
        }

        private void ExecuteUpload()
        {
            UploadToServerAsync();
        }

        /// <summary>
        /// Uploads workflow data to server by:
        /// 1. Creating HardwareDefinition with Uploaded state
        /// 2. Saving to UploadedWorkflowRepository (uploaded-hardwares.json)
        /// 3. Removing from WorkflowRepository (workflows.json)
        /// </summary>
        private async void UploadToServerAsync()
        {
            if (_sessionDto == null)
                return;

            // Set state to Uploaded
            _sessionDto.State = ComponentState.Uploaded;
            _sessionDto.LastModifiedAt = DateTime.Now;

            // Save to server via API service
            var apiService = ServiceLocator.GetService<IHardwareApiService>();
            var response = await apiService.SaveSessionAsync(_sessionDto);

            if (!response.Success)
            {
                ToastService.Instance.ShowError(
                    "Upload Failed",
                    response.ErrorMessage ?? "Failed to upload data to the server.");
                return;
            }

            // Remove workflow from WorkflowRepository after successful upload
            var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
            var sessions = await workflowRepo.LoadAsync();

            var session = sessions.FirstOrDefault(s => s.Id == _sessionDto.Id);
            if (session != null)
            {
                sessions.Remove(session);
                await workflowRepo.SaveAsync(sessions);
            }

            // Show success toast
            ToastService.Instance.ShowSuccess(
                "Upload Complete",
                "Data has been uploaded to the server.");

            // Navigate to dashboard
            NavigationService.Instance.NavigateToDashboard();
        }

        private void ExecuteContinueEditing()
        {
            if (_sessionDto == null)
                return;

            NavigationService.Instance.ResumeWorkflow(_sessionDto.Id);
        }

        private async void ExecuteUploadLater()
        {
            if (_sessionDto == null)
            {
                NavigationService.Instance.NavigateToDashboard();
                return;
            }

            // Save current state to WorkflowRepository
            var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
            var sessions = await workflowRepo.LoadAsync();

            var existingIndex = sessions.FindIndex(s => s.Id == _sessionDto.Id);
            if (existingIndex >= 0)
                sessions[existingIndex] = _sessionDto;
            else
                sessions.Add(_sessionDto);

            await workflowRepo.SaveAsync(sessions);

            // Navigate to dashboard
            NavigationService.Instance.NavigateToDashboard();
        }

        private void ExecuteCardClick(HardwareNodeDisplayModel node)
        {
            if (node == null)
                return;

            // Get MainWindow for backdrop control
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;

            // Show backdrop
            mainWindow?.ShowBackdrop();

            try
            {
                // Show detail dialog with proper ViewModel
                var dialog = new CardDetailDialog(node.SourceDto)
                {
                    Owner = mainWindow
                };

                dialog.ShowDialog();
            }
            finally
            {
                // Hide backdrop
                mainWindow?.HideBackdrop();
            }
        }

        #endregion
    }
}