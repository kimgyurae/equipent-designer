
# C# Project Namespace Convention

## Core Principle

**Folders are for file organization. Namespaces are for logical grouping.**

- Keep namespaces flat (2-3 levels maximum)
- Organize files into subfolders as needed for clarity
- Do not let folder depth dictate namespace depth

## Namespace Structure

| Namespace | Purpose |
|-----------|---------|
| `MyApp` | Root namespace (App.xaml, common classes, entry point) |
| `MyApp.Views` | All XAML views and windows |
| `MyApp.ViewModels` | All view models |
| `MyApp.Models` | Domain models and entities |
| `MyApp.Services` | Business logic, data access, external integrations |
| `MyApp.Converters` | IValueConverter implementations |
| `MyApp.Controls` | Custom controls and user controls |

## Folder to Namespace Mapping

Subfolders within a category share the same namespace:
```
Folder Structure                     Namespace
────────────────────────────────────────────────────────
/Views/MainView.xaml                    → MyApp.Views
/Views/Main/DashboardView.xaml          → MyApp.Views
/Views/Main/HomeView.xaml               → MyApp.Views
/Views/Settings/OptionsView.xaml        → MyApp.Views
/Views/Settings/ProfileView.xaml        → MyApp.Views

/ViewModels/MainViewModel.cs            → MyApp.ViewModels
/ViewModels/Main/DashboardViewModel.cs  → MyApp.ViewModels
/ViewModels/Main/HomeViewModel.cs       → MyApp.ViewModels
/ViewModels/Settings/OptionsViewModel.cs→ MyApp.ViewModels

/Models/User.cs                         → MyApp.Models
/Models/Order.cs                        → MyApp.Models

/Services/AuthService.cs                → MyApp.Services
/Services/Api/ApiClient.cs              → MyApp.Services
/Services/Database/DbContext.cs         → MyApp.Services

/Converters/BoolToVisibilityConverter.cs→ MyApp.Converters

/Controls/CustomButton.cs               → MyApp.Controls
/Controls/Charts/LineChart.cs           → MyApp.Controls
```

## Implementation

Explicitly declare the namespace at the top of each file, ignoring folder depth:
```csharp
// File: /ViewModels/Settings/Advanced/OptionsViewModel.cs

namespace MyApp.ViewModels;

public class OptionsViewModel : ObservableObject
{
    // ...
}
```

## What to Avoid

**Do not** create deeply nested namespaces:
```csharp
namespace MyApp.Views.Main.Dashboard.Widgets.Charts;  // Too deep
```

**Do not** let Visual Studio auto-generate namespaces based on folder structure.

**Do** manually set the namespace to match this convention.