using System.Windows;
using System.Windows.Controls;

namespace EquipmentDesigner.Views.HardwareDefineWorkflow.UnitDefine
{
    /// <summary>
    /// Interaction logic for UnitDefineView.xaml
    /// </summary>
    public partial class UnitDefineView : UserControl
    {
        public UnitDefineView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is UnitDefineViewModel oldVm)
            {
                oldVm.ShowAddCommandDialogRequested -= OnShowAddCommandDialogRequested;
            }

            if (e.NewValue is UnitDefineViewModel newVm)
            {
                newVm.ShowAddCommandDialogRequested += OnShowAddCommandDialogRequested;
            }
        }

        private void OnShowAddCommandDialogRequested(object sender, System.EventArgs e)
        {
            var dialogViewModel = new AddCommandDialogViewModel();
            var dialog = new AddCommandDialogWindow(dialogViewModel)
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true && DataContext is UnitDefineViewModel unitVm)
            {
                unitVm.ProcessCommandDialogResult(dialog.Result);
            }
        }
    }
}