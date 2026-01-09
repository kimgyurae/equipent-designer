using System.Windows;
using System.Windows.Controls;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Converters
{
    /// <summary>
    /// Selects the appropriate DataTemplate for a DrawingElement based on its ShapeType.
    /// </summary>
    public class DrawingShapeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate InitialNodeTemplate { get; set; }
        public DataTemplate ActionNodeTemplate { get; set; }
        public DataTemplate DecisionNodeTemplate { get; set; }
        public DataTemplate TerminalNodeTemplate { get; set; }
        public DataTemplate PredefinedActionNodeTemplate { get; set; }
        public DataTemplate TextboxTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is DrawingElement element)
            {
                return element.ShapeType switch
                {
                    DrawingShapeType.Initial => InitialNodeTemplate,
                    DrawingShapeType.Action => ActionNodeTemplate,
                    DrawingShapeType.Decision => DecisionNodeTemplate,
                    DrawingShapeType.Terminal => TerminalNodeTemplate,
                    DrawingShapeType.PredefinedAction => PredefinedActionNodeTemplate,
                    DrawingShapeType.Textbox => TextboxTemplate,
                    _ => base.SelectTemplate(item, container)
                };
            }

            return base.SelectTemplate(item, container);
        }
    }
}
