using System.Windows;
using System.Windows.Controls;
using EquipmentDesigner.ViewModels;
using System.Windows.Input;
using EquipmentDesigner.Resources;
using CustomContextMenuService = EquipmentDesigner.Controls.ContextMenuService;

namespace EquipmentDesigner.Views
{
    /// <summary>
    /// Interaction logic for HardwareTreeItemControl.xaml
    /// </summary>
    public partial class HardwareTreeItemControl : UserControl
    {
        public HardwareTreeItemControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles right-click on tree node to show custom context menu.
        /// </summary>
        private void OnNodeRightClick(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Border border)) return;
            if (!(border.Tag is HardwareDefineWorkflowViewModel viewModel)) return;
            if (!(DataContext is HardwareTreeNodeViewModel node)) return;

            e.Handled = true;

            CustomContextMenuService.Instance.Create()
                .AddItem(Strings.ContextMenu_Copy, viewModel.CopyNodeCommand, node)
                .AddSeparator()
                .AddDestructiveItem(Strings.ContextMenu_Delete, viewModel.DeleteNodeCommand, node)
                .Show();
        }
    }
}