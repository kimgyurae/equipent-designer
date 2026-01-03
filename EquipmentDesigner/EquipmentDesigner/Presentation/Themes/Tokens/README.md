# WPF Design Tokens 사용 가이드

## 📁 파일 구조

```
Themes/
├── DesignTokens.xaml          # 메인 진입점 (App.xaml에서 참조)
└── Tokens/
    ├── Colors.xaml            # 기본 색상 팔레트
    ├── Brushes.xaml           # 시맨틱 브러시
    ├── Typography.xaml        # 폰트 스타일
    ├── Spacing.xaml           # 간격 및 크기
    └── Effects.xaml           # 그림자 및 애니메이션
```

## 🚀 설치 방법

### App.xaml에 추가

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="Themes/DesignTokens.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

## 🔄 원본 WinForms 토큰 → WPF 매핑

### StatusGroups (상태 표시)

| 원본 (C#) | WPF 토큰 |
|-----------|----------|
| `StatusGroups.Operational` | `Brush.Status.Success` |
| `StatusGroups.Transitional` | `Brush.Status.Info` |
| `StatusGroups.Idle` | `Brush.Status.Neutral` |
| `StatusGroups.WarningHold` | `Brush.Status.Warning` |
| `StatusGroups.CriticalStop` | `Brush.Status.Danger` |

### UiElements (UI 요소)

| 원본 (C#) | WPF 토큰 |
|-----------|----------|
| `UiElements.Background` | `Brush.Background.Primary` |
| `UiElements.Sidebar` | `Brush.Surface.Primary` |
| `UiElements.CardBorder` | `Brush.Border.Primary` |
| `UiElements.TextPrimary` | `Brush.Text.Primary` |
| `UiElements.TextSecondary` | `Brush.Text.Secondary` |
| `UiElements.IconEquipment` | `Brush.Icon.Primary` |
| `UiElements.IconSystem` | `Brush.Icon.Secondary` |
| `UiElements.IconUnit` | `Brush.Icon.Warning` |
| `UiElements.HighlightActive` | `Brush.Highlight.Background` |
| `UiElements.HighlightActiveBorder` | `Brush.Highlight.Border` |

### ButtonColors (버튼)

| 원본 (C#) | WPF 토큰 |
|-----------|----------|
| `ButtonColors.Start` | `Brush.Button.Success.Background` |
| `ButtonColors.Stop` | `Brush.Button.DangerLight.Background` |
| `ButtonColors.Abort` | `Brush.Button.Danger.Background` |
| `ButtonColors.Suspend` | `Brush.Button.Warning.Background` |
| `ButtonColors.Hold` | `Brush.Button.Warning.Background` |
| `ButtonColors.Reset` | `Brush.Button.Primary.Background` |
| `ButtonColors.Clear` | `Brush.Button.Accent.Background` |
| `ButtonColors.Home` | `Brush.Button.Info.Background` |
| `ButtonColors.Disabled` | `Brush.Button.Disabled.Background` |

## 📖 사용 예시

### 배경색 및 텍스트

```xml
<Border Background="{StaticResource Brush.Background.Primary}">
    <StackPanel>
        <TextBlock Text="제목" 
                   Foreground="{StaticResource Brush.Text.Primary}"
                   Style="{StaticResource Typography.H2}"/>
        <TextBlock Text="설명" 
                   Foreground="{StaticResource Brush.Text.Secondary}"
                   Style="{StaticResource Typography.Body}"/>
    </StackPanel>
</Border>
```

### 카드 컴포넌트

```xml
<Border Background="{StaticResource Brush.Surface.Primary}"
        BorderBrush="{StaticResource Brush.Border.Primary}"
        BorderThickness="{StaticResource Border.Thin}"
        CornerRadius="{StaticResource Radius.LG}"
        Padding="{StaticResource Spacing.Padding.Card}"
        Effect="{StaticResource Shadow.MD}">
    <TextBlock Text="카드 내용"/>
</Border>
```

### 상태 표시 배지

```xml
<!-- 성공 상태 -->
<Border Background="{StaticResource Brush.Status.Success.Background}"
        BorderBrush="{StaticResource Brush.Status.Success.Border}"
        BorderThickness="1" CornerRadius="4" Padding="8,4">
    <TextBlock Text="정상 운영" 
               Foreground="{StaticResource Brush.Status.Success}"/>
</Border>

<!-- 경고 상태 -->
<Border Background="{StaticResource Brush.Status.Warning.Background}"
        BorderBrush="{StaticResource Brush.Status.Warning.Border}"
        BorderThickness="1" CornerRadius="4" Padding="8,4">
    <TextBlock Text="주의 필요" 
               Foreground="{StaticResource Brush.Status.Warning}"/>
</Border>
```

### 버튼 스타일링

```xml
<!-- Primary 버튼 -->
<Button Content="시작"
        Background="{StaticResource Brush.Button.Success.Background}"
        Foreground="{StaticResource Brush.Button.Success.Foreground}"
        Padding="{StaticResource Spacing.Padding.Button}"/>

<!-- Danger 버튼 -->
<Button Content="정지"
        Background="{StaticResource Brush.Button.Danger.Background}"
        Foreground="{StaticResource Brush.Button.Danger.Foreground}"
        Padding="{StaticResource Spacing.Padding.Button}"/>
```

### 입력 필드

```xml
<TextBox Background="{StaticResource Brush.Input.Background}"
         BorderBrush="{StaticResource Brush.Input.Border}"
         BorderThickness="{StaticResource Border.Thin}"
         Padding="{StaticResource Spacing.Padding.Input}"
         FontFamily="{StaticResource Font.Family.Primary}"
         FontSize="{StaticResource Font.Size.Base}"/>
```

## 🎨 커스텀 테마 확장

프로젝트별 토큰을 추가하려면 별도 파일 생성:

```xml
<!-- Themes/Tokens/Custom.xaml -->
<ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="Colors.xaml"/>
    </ResourceDictionary.MergedDictionaries>
    
    <!-- 프로젝트 전용 토큰 -->
    <SolidColorBrush x:Key="Brush.Brand.Primary" Color="{StaticResource Color.Primary.600}"/>
</ResourceDictionary>
```

## 🌙 다크 테마 지원

다크 테마용 Brushes 오버라이드 파일 생성:

```xml
<!-- Themes/Tokens/Brushes.Dark.xaml -->
<ResourceDictionary>
    <SolidColorBrush x:Key="Brush.Background.Primary" Color="{StaticResource Color.Gray.900}"/>
    <SolidColorBrush x:Key="Brush.Text.Primary" Color="{StaticResource Color.Gray.50}"/>
    <!-- ... -->
</ResourceDictionary>
```

런타임 테마 전환:

```csharp
var darkTheme = new ResourceDictionary { 
    Source = new Uri("Themes/Tokens/Brushes.Dark.xaml", UriKind.Relative) 
};
Application.Current.Resources.MergedDictionaries.Add(darkTheme);
```
