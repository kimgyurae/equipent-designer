using System.Windows.Controls;
using EquipmentDesigner.ViewModels;

namespace EquipmentDesigner.Views
{
    public partial class ComponentListView : UserControl
    {
        public ComponentListView()
        {
            InitializeComponent();
            DataContext = new ComponentListViewModel();
        }
    }
}
