using System.Windows;
using System.Windows.Controls;

namespace EquipmentDesigner.Views.HardwareDefineWorkflow.DeviceDefine
{
    /// <summary>
    /// Interaction logic for DeviceDefineView.xaml
    /// </summary>
    public partial class DeviceDefineView : UserControl
    {
        public DeviceDefineView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is DeviceDefineViewModel oldVm)
            {
                oldVm.ShowAddIoDialogRequested -= OnShowAddIoDialogRequested;
                oldVm.ShowAddCommandDialogRequested -= OnShowAddCommandDialogRequested;
            }

            if (e.NewValue is DeviceDefineViewModel newVm)
            {
                newVm.ShowAddIoDialogRequested += OnShowAddIoDialogRequested;
                newVm.ShowAddCommandDialogRequested += OnShowAddCommandDialogRequested;
            }
        }

        private void OnShowAddIoDialogRequested(object sender, System.EventArgs e)
        {
            var dialogViewModel = new AddIoDialogViewModel();
            var dialog = new AddIoDialogWindow(dialogViewModel)
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true && DataContext is DeviceDefineViewModel deviceVm)
            {
                deviceVm.ProcessDialogResult(dialog.Result);
            }
        }

        private void OnShowAddCommandDialogRequested(object sender, System.EventArgs e)
        {
            var dialogViewModel = new AddCommandDialogViewModel();
            var dialog = new AddCommandDialogWindow(dialogViewModel)
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true && DataContext is DeviceDefineViewModel deviceVm)
            {
                deviceVm.ProcessCommandDialogResult(dialog.Result);
            }
        }
    }
}