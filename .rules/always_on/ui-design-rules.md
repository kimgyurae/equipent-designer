
## Requirements

* **Component Reuse:** Before implementing any `.xaml` code, check `EquipmentDesigner\EquipmentDesigner\Views\ReusableComponents\reusable-components.json`. If a reusable component exists, you must use it.
* **Avoid Hardcoding:** Do not hardcode the size or position of UI elements unless absolutely necessary.
* **Flexible Layouts:** Utilize layout controls such as `Grid`, `DockPanel`, `StackPanel`, and `UniformGrid` along with properties like `Stretch`, `Margin`, and `Padding` to minimize the use of absolute pixel values.
* **Strict Design Token Usage (Critical):** You must exclusively use the Design Tokens defined in `EquipmentDesigner\EquipmentDesigner\Themes\DesignTokens.xaml`.

## Reusable UI Components

* **Creating New Components:** When the user explicitly requests to create reusable UI components, add the code to `EquipmentDesigner\EquipmentDesigner\Views\ReusableComponents` and update the `reusable-components.json` file.
* **Registry Update:** If a new UI element is added to the `ReusableComponents` directory, it must be registered in `reusable-components.json`.

## Figma MCP

If the user explicitly instructs you to design using the **Figma MCP**:

* **Error Handling:** If the Figma MCP tool fails, stop all operations immediately and report the issue to the user.
* **Token Alignment:** Even if the UI identified via Figma MCP uses design elements not defined in `DesignTokens.xaml`, do **not** add new Design Tokens. Instead, implement the design as closely as possible using the existing Design Tokens.