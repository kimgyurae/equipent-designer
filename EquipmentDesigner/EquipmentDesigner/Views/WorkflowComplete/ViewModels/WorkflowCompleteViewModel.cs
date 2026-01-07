using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models.Dtos;
using EquipmentDesigner.Models.Storage;
using EquipmentDesigner.Services;
using EquipmentDesigner.Services.Api;
using EquipmentDesigner.Services.Storage;
using EquipmentDesigner.Views.HardwareDefineWorkflow;
using EquipmentDesigner.Views.ReusableComponents.Toast;

namespace EquipmentDesigner.Views.WorkflowComplete
{
    /// <summary>
    /// Display model for hierarchical hardware nodes in WorkflowCompleteView.
    /// </summary>
    public class HardwareNodeDisplayModel
    {
        public string NodeId { get; set; }
        public HardwareLayer HardwareLayer { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TreeNodeDataDto SourceDto { get; set; }
        public ObservableCollection<HardwareNodeDisplayModel> Children { get; set; } = new ObservableCollection<HardwareNodeDisplayModel>();

        public static HardwareNodeDisplayModel FromTreeNodeDataDto(TreeNodeDataDto dto)
        {
            if (dto == null) return null;

            var model = new HardwareNodeDisplayModel
            {
                NodeId = dto.NodeId,
                HardwareLayer = dto.HardwareLayer,
                SourceDto = dto
            };

            // Extract name and description based on hardware layer
            switch (dto.HardwareLayer)
            {
                case HardwareLayer.Equipment:
                    model.Name = dto.EquipmentData?.Name ?? string.Empty;
                    model.Description = dto.EquipmentData?.Description ?? string.Empty;
                    break;
                case HardwareLayer.System:
                    model.Name = dto.SystemData?.Name ?? string.Empty;
                    model.Description = dto.SystemData?.Description ?? string.Empty;
                    break;
                case HardwareLayer.Unit:
                    model.Name = dto.UnitData?.Name ?? string.Empty;
                    model.Description = dto.UnitData?.Description ?? string.Empty;
                    break;
                case HardwareLayer.Device:
                    model.Name = dto.DeviceData?.Name ?? string.Empty;
                    model.Description = dto.DeviceData?.Description ?? string.Empty;
                    break;
            }

            // Recursively convert children
            if (dto.Children != null)
            {
                foreach (var child in dto.Children)
                {
                    var childModel = FromTreeNodeDataDto(child);
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
        private readonly WorkflowSessionDto _sessionDto;
        private ObservableCollection<HardwareNodeDisplayModel> _treeNodes;

        /// <summary>
        /// Creates a new WorkflowCompleteViewModel with the given session data.
        /// </summary>
        /// <param name="sessionDto">The workflow session DTO containing all hardware data.</param>
        /// <exception cref="ArgumentNullException">Thrown when sessionDto is null.</exception>
        public WorkflowCompleteViewModel(WorkflowSessionDto sessionDto)
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
        public string WorkflowId => _sessionDto?.WorkflowId;

        /// <summary>
        /// Gets the start type of the workflow.
        /// </summary>
        public HardwareLayer StartType => _sessionDto?.StartType ?? HardwareLayer.Equipment;

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

            if (_sessionDto?.TreeNodes == null)
                return;

            foreach (var nodeDto in _sessionDto.TreeNodes)
            {
                var displayModel = HardwareNodeDisplayModel.FromTreeNodeDataDto(nodeDto);
                if (displayModel != null)
                {
                    _treeNodes.Add(displayModel);
                }
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
            return _sessionDto != null && !string.IsNullOrEmpty(_sessionDto.WorkflowId);
        }

        private void ExecuteUpload()
        {
            UploadToServerAsync();
        }

        /// <summary>
        /// Uploads workflow data to server by:
        /// 1. Creating WorkflowSessionDto with Uploaded state
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
            var workflowData = await workflowRepo.LoadAsync();

            var session = workflowData.WorkflowSessions.FirstOrDefault(s => s.WorkflowId == _sessionDto.WorkflowId);
            if (session != null)
            {
                workflowData.WorkflowSessions.Remove(session);
                await workflowRepo.SaveAsync(workflowData);
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

            NavigationService.Instance.ResumeWorkflow(_sessionDto.WorkflowId);
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
            var workflowData = await workflowRepo.LoadAsync();

            var existingIndex = workflowData.WorkflowSessions.FindIndex(s => s.WorkflowId == _sessionDto.WorkflowId);
            if (existingIndex >= 0)
                workflowData.WorkflowSessions[existingIndex] = _sessionDto;
            else
                workflowData.WorkflowSessions.Add(_sessionDto);

            await workflowRepo.SaveAsync(workflowData);

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