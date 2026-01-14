using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Converters
{
    /// <summary>
    /// Custom JSON converter for polymorphic deserialization of DrawingElement.
    /// Uses ShapeType property as the type discriminator.
    /// </summary>
    public class DrawingElementJsonConverter : JsonConverter<DrawingElement>
    {
        public override DrawingElement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            // Read the shapeType discriminator
            if (!root.TryGetProperty("shapeType", out var shapeTypeElement))
            {
                throw new JsonException("Missing 'shapeType' property for DrawingElement deserialization.");
            }

            var shapeTypeString = shapeTypeElement.GetString();
            if (!Enum.TryParse<DrawingShapeType>(shapeTypeString, ignoreCase: true, out var shapeType))
            {
                throw new JsonException($"Unknown shapeType: {shapeTypeString}");
            }

            // Create the appropriate concrete instance
            DrawingElement element = shapeType switch
            {
                DrawingShapeType.Initial => new InitialElement(),
                DrawingShapeType.Action => new ActionElement(),
                DrawingShapeType.Decision => new DecisionElement(),
                DrawingShapeType.Terminal => new TerminalElement(),
                DrawingShapeType.PredefinedAction => new PredefinedActionElement(),
                DrawingShapeType.Textbox => new TextboxElement(),
                _ => throw new JsonException($"Unsupported shapeType: {shapeType}")
            };

            // Deserialize common properties
            DeserializeBaseProperties(root, element, options);

            return element;
        }

        public override void Write(Utf8JsonWriter writer, DrawingElement value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            // Write shapeType discriminator first
            writer.WriteString("shapeType", value.ShapeType.ToString());

            // Write common properties
            writer.WriteString("id", value.Id);
            writer.WriteNumber("x", value.X);
            writer.WriteNumber("y", value.Y);
            writer.WriteNumber("width", value.Width);
            writer.WriteNumber("height", value.Height);
            writer.WriteNumber("opacity", value.Opacity);
            writer.WriteNumber("zIndex", value.ZIndex);
            writer.WriteBoolean("isSelected", value.IsSelected);
            writer.WriteBoolean("isLocked", value.IsLocked);

            // Text properties
            writer.WriteString("text", value.Text ?? string.Empty);
            writer.WriteString("fontSize", value.FontSize.ToString());
            writer.WriteString("textAlign", value.TextAlign.ToString());
            writer.WriteString("textColor", value.TextColor.ToString());
            writer.WriteNumber("textOpacity", value.TextOpacity);

            // OutgoingArrows collection
            writer.WritePropertyName("outgoingArrows");
            JsonSerializer.Serialize(writer, value.OutgoingArrows, options);

            writer.WriteEndObject();
        }

        private void DeserializeBaseProperties(JsonElement root, DrawingElement element, JsonSerializerOptions options)
        {
            // Id - use reflection to set protected setter
            if (root.TryGetProperty("id", out var idElement))
            {
                var idField = typeof(DrawingElement).GetProperty("Id");
                idField?.SetValue(element, idElement.GetString());
            }

            // Position and size
            if (root.TryGetProperty("x", out var xElement))
                element.X = xElement.GetDouble();
            if (root.TryGetProperty("y", out var yElement))
                element.Y = yElement.GetDouble();
            if (root.TryGetProperty("width", out var widthElement))
                element.Width = widthElement.GetDouble();
            if (root.TryGetProperty("height", out var heightElement))
                element.Height = heightElement.GetDouble();
            if (root.TryGetProperty("opacity", out var opacityElement))
                element.Opacity = opacityElement.GetDouble();
            if (root.TryGetProperty("zIndex", out var zIndexElement))
                element.ZIndex = zIndexElement.GetInt32();

            // State
            if (root.TryGetProperty("isSelected", out var isSelectedElement))
                element.IsSelected = isSelectedElement.GetBoolean();
            if (root.TryGetProperty("isLocked", out var isLockedElement))
                element.IsLocked = isLockedElement.GetBoolean();

            // Text properties
            if (root.TryGetProperty("text", out var textElement))
                element.Text = textElement.GetString() ?? string.Empty;

            if (root.TryGetProperty("fontSize", out var fontSizeElement))
            {
                var fontSizeString = fontSizeElement.GetString();
                if (Enum.TryParse<TextFontSize>(fontSizeString, ignoreCase: true, out var fontSize))
                    element.FontSize = fontSize;
            }

            if (root.TryGetProperty("textAlign", out var textAlignElement))
            {
                var textAlignString = textAlignElement.GetString();
                if (Enum.TryParse<Models.TextAlignment>(textAlignString, ignoreCase: true, out var textAlign))
                    element.TextAlign = textAlign;
            }

            if (root.TryGetProperty("textColor", out var textColorElement))
            {
                var textColorString = textColorElement.GetString();
                if (Enum.TryParse<SupportedTextColor>(textColorString, ignoreCase: true, out var textColor))
                    element.TextColor = textColor;
            }

            if (root.TryGetProperty("textOpacity", out var textOpacityElement))
                element.TextOpacity = textOpacityElement.GetDouble();

            // OutgoingArrows
            if (root.TryGetProperty("outgoingArrows", out var arrowsElement))
            {
                var arrows = JsonSerializer.Deserialize<ObservableCollection<UMLConnection2>>(arrowsElement.GetRawText(), options);
                if (arrows != null)
                {
                    element.OutgoingArrows.Clear();
                    foreach (var arrow in arrows)
                    {
                        element.OutgoingArrows.Add(arrow);
                    }
                }
            }
        }
    }
}