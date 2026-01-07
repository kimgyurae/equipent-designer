using System;
using System.Windows;
using EquipmentDesigner.Models.Storage;
using EquipmentDesigner.Views.WorkflowComplete.ViewModels;

namespace EquipmentDesigner.Views.WorkflowComplete
{
    /// <summary>
    /// Interaction logic for CardDetailDialog.xaml
    /// </summary>
    public partial class CardDetailDialog : Window
    {
        /// <summary>
        /// Creates a new CardDetailDialog with the given tree node data.
        /// </summary>
        /// <param name="treeNodeData">The tree node data to display.</param>
        public CardDetailDialog(TreeNodeDataDto treeNodeData)
        {
            InitializeComponent();

            var viewModel = new CardDetailDialogViewModel(treeNodeData);
            viewModel.RequestClose += OnRequestClose;
            DataContext = viewModel;
        }

        /// <summary>
        /// Parameterless constructor for design-time support.
        /// </summary>
        public CardDetailDialog()
        {
            InitializeComponent();
        }

        private void OnRequestClose(object sender, DialogCloseEventArgs e)
        {
            // DialogResult can only be set when the window is opened via ShowDialog()
            // If opened via Show(), setting DialogResult will throw an exception
            try
            {
                DialogResult = e.DialogResult;
            }
            catch (InvalidOperationException)
            {
                // Window was not opened as a dialog, ignore DialogResult
            }
            Close();
        }
    }
}