
## Requirements
- 모든 UI는 구현하기 전 `EquipmentDesigner\EquipmentDesigner\Presentation\ReusableComponents\reusable-components.json` 을 확인하고 재사용 가능한 컴포넌트가 있다면 그것을 사용할 것.
- 새로운 UI를 구현할 때 재사용 가능한 컴포넌트가 있다면 `EquipmentDesigner\EquipmentDesigner\Presentation\ReusableComponents`에 코드를 추가하고 `reusable-components.json`에 추가할 것.
- UI의 크기나 위치는 절대 반드시 필요한 경우가 아니라면 하드코드 하지 말 것
- UI는 `Grid`, `DockPanel`, `StackPanel`, `UniformGrid` 등과 `Stretch`, `Margin`, `Padding`등의 속성을 사용하여 절대 픽셀 값 지정을 최소화 할 것.
- 매우 중요: 반드시 `EquipmentDesigner\EquipmentDesigner\Themes\DesignTokens.xaml`에 정의된 Design Token만 사용할 것.

## Figma MCP 
사용자가가 명시적으로 Figma MCP를 사용해 design을 하라고 지시하는 경우:
- Figma MCP 도구 사용실패 시 모든 작업을 중지하고 사용자에게 보고한다.
- Figma MCP 도구를 사용해서 확인한 UI가 `DesignTokens.xaml`에 정의되지 않은 Design 요소를 사용하더라도 DesignToken을 추가하지 않고 존재하는 Design Token을 활용해 최대한 유사하게 구현한다.

