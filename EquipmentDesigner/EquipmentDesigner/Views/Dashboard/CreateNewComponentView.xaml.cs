using System.Windows.Controls;
using EquipmentDesigner.ViewModels;

namespace EquipmentDesigner.Views
{
    public partial class CreateNewComponentView : UserControl
    {
        public CreateNewComponentView()
        {
            InitializeComponent();
            DataContext = new CreateNewComponentViewModel();
        }
    }
}
