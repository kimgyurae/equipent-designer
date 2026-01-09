# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Project overview

This repository contains a .NET 5 (net5.0-windows) desktop application built with WPF (and some WinForms interop) following an MVVM architecture. The main application, `EquipmentDesigner`, lets users define a hierarchical hardware structure (Equipment → System → Unit → Device), edit process diagrams, and manage workflow sessions that are persisted as JSON under the user's local application data folder.

Key entry points and flows:
- `EquipmentDesigner/EquipmentDesigner/App.xaml` wires global resources and localization, and configures dependency injection via `ServiceLocator.ConfigureForProduction()` on startup.
- `EquipmentDesigner/EquipmentDesigner/MainWindow.xaml` is the single-shell window hosting the current feature in a `ContentControl` (`MainContent`) and global overlays (backdrop, spinner, toast container).
- `EquipmentDesigner/EquipmentDesigner/Services/NavigationService.cs` is the central navigation hub. It exposes events (`NavigationRequested`, `NavigateToDashboardRequested`, etc.) that `MainWindow` subscribes to in order to switch between:
  - Dashboard
  - Hardware Define Workflow
  - Drawboard (process editor)
  - Workflow Complete summary

## Build, run, and test

All commands below assume you are in the repository root (`equipment-designer`).

### Build

- Build the full solution (app + tests):
  - `dotnet build EquipmentDesigner/EquipmentDesigner.sln`

### Run the WPF application

- Run the main application project:
  - `dotnet run --project EquipmentDesigner/EquipmentDesigner/EquipmentDesigner.csproj`

This launches the `MainWindow` shell, which by default shows the Dashboard view and wires up navigation events.

### Tests (xUnit)

The test project is `EquipmentDesigner/EquipmentDesigner.Tests/EquipmentDesigner.Tests.csproj` and uses xUnit, FluentAssertions, and coverlet.

- Run the entire test suite:
  - `dotnet test EquipmentDesigner/EquipmentDesigner.sln`

- Run tests in a single test project explicitly:
  - `dotnet test EquipmentDesigner/EquipmentDesigner.Tests/EquipmentDesigner.Tests.csproj`

- Run tests for a single test class (recommended for fast feedback, reflecting the `csharp-running-tests-rules` guidance to use filters):
  - `dotnet test EquipmentDesigner/EquipmentDesigner.Tests/EquipmentDesigner.Tests.csproj --filter "FullyQualifiedName~EquipmentDesigner.Tests.Services.ServiceLocatorTests"`

- Run a single test method by name:
  - `dotnet test EquipmentDesigner/EquipmentDesigner.Tests/EquipmentDesigner.Tests.csproj --filter "Name=CreateUser_WithValidUserData_ReturnsCreatedUser"`

Adjust the fully qualified type/method names to match the actual tests you are targeting.

### Linting / analyzers

There is no separate linter configuration; rely on `dotnet build` / `dotnet test` to run compiler diagnostics and any analyzer rules configured through NuGet packages.

## High-level architecture

### MVVM layout and namespaces

The application follows MVVM with a **flat namespace** convention, decoupled from folder depth. The convention is documented in `.rules/always_on/csharp-namespace-rule.md` and reiterated in `EquipmentDesigner/EquipmentDesigner/EquipmentDesigner.csproj`:

- Folders are for physical organization only; namespaces are for logical grouping.
- Keep namespaces flat (2–3 levels max) regardless of subfolders.
- Standard namespaces:
  - `EquipmentDesigner` – root (App, common classes).
  - `EquipmentDesigner.Views` – all XAML views/windows/user controls.
  - `EquipmentDesigner.ViewModels` – all view models.
  - `EquipmentDesigner.Models` – domain models, DTOs, and storage DTOs.
  - `EquipmentDesigner.Services` – business logic, navigation, storage, and API abstractions.
  - `EquipmentDesigner.Converters` – WPF `IValueConverter` implementations.
  - `EquipmentDesigner.Controls` – custom WPF controls.
  - `EquipmentDesigner.Constants` – constant definitions.

Do **not** introduce deeply nested namespaces such as `EquipmentDesigner.Views.HardwareDefineWorkflow.ViewModels`; even if a file lives in a deep folder, its namespace should remain in the flat scheme above.

### View / ViewModel file structure

File organization for MVVM is constrained by `.rules/always_on/file-structures.md`:

- Each feature/page under `Views/` generally has its own folder (e.g., `Views/Dashboard`, `Views/HardwareDefineWorkflow`, `Views/Drawboard`, `Views/WorkflowComplete`).
- For more complex pages, each feature folder contains `Views` and `ViewModels` subfolders; simpler pages with a single view + viewmodel may place them directly in the feature folder.
- All domain `Models` and DTOs live under the `Models/` folder at the project root (where the `.csproj` resides), optionally with subfolders for organization.

When adding new features, mirror this structure rather than introducing ad-hoc layouts.

### Shell, navigation, and global overlays

- `App.xaml` merges the design token resources (see “UI and design tokens”) and registers a few global converters. `App.OnStartup`:
  - Calls `ServiceLocator.ConfigureForProduction()` to wire production services (`IWorkflowRepository`, `IUploadedWorkflowRepository`, `IHardwareApiService`).
  - Initializes `LanguageService` with Korean as the default culture.
- `MainWindow.xaml` hosts:
  - `ContentControl MainContent` – the active view (Dashboard, workflow, drawboard, etc.).
  - `BackdropOverlay` – a full-screen semi-transparent overlay used when dialogs are open.
  - `LoadingSpinnerOverlay` – a reusable global spinner.
  - `ToastContainer` – a container for toast notifications.
- `MainWindow.xaml.cs` subscribes to `NavigationService` events to:
  - Navigate to new or resumed Hardware Define Workflow sessions (`HardwareDefineWorkflowViewModel` + `HardwareDefineWorkflowView`).
  - Navigate back to the Dashboard (`DashboardView` / `DashboardViewModel`).
  - Open completed workflows in a Workflow Complete view (`WorkflowCompleteViewModel` / `WorkflowCompleteView`).
  - Open the Drawboard feature and return to the previous view.
  - Manage the backdrop overlay visibility for modal dialogs.

### Domain model and persistence

Core domain and storage types live under `EquipmentDesigner/EquipmentDesigner/Models`:

- **Hardware hierarchy and DTOs**
  - `Models/Dtos/*.cs` – DTOs for Equipment, System, Unit, Device, Commands, Parameters, etc.
  - `Models/HardwareLayer.cs` and `Models/ComponentState.cs` represent the hardware hierarchy and component lifecycle.
- **Workflow sessions and tree structures**
  - `Models/Storage/*` includes:
    - `HardwareDefinitionDataStore` – root data structure for persisted workflow data (incomplete and uploaded sessions).
    - `WorkflowSessionDto` – persisted representation of a single workflow editing session.
    - `TreeNodeDataDto` – hierarchical structure representing the Equipment/System/Unit/Device tree (mirrors the editor tree in the workflow UI).
- **Drawboard models**
  - `Models/Drawboard/*` contains the process-editing and UML-like models (shapes, elements, commands, workspace, state history). These back the Drawboard views under `Views/Drawboard`.

#### Repositories and storage locations

Persistent storage is abstracted behind typed repositories in `EquipmentDesigner/EquipmentDesigner/Services/Storage`:

- `ITypedDataRepository<T>` – generic async load/save interface.
- `TypedJsonFileRepository<T>` – JSON file implementation (camelCase, indented).
- `WorkflowRepository` – concrete repository for **incomplete** workflow data (`IWorkflowRepository`).
  - Persists to `%LOCALAPPDATA%/EquipmentDesigner/local/workflows.json` by default (directory created on demand).
- `UploadedWorkflowRepository` (not shown above but registered in `ServiceLocator`) – analogous repository for **uploaded / remote** workflows (`IUploadedWorkflowRepository`), using a `remote` folder under `%LOCALAPPDATA%/EquipmentDesigner`.
- `MemoryTypedRepository<T>`, `MemoryWorkflowRepository`, `MemoryUploadedWorkflowRepository` – in-memory repositories used for tests and non-filesystem scenarios.

`DashboardViewModel` and `HardwareDefineWorkflowViewModel` interact with these repositories via interfaces resolved from `ServiceLocator`:

- Dashboard loads and deletes local workflow sessions and can also trigger full data resets (deleting `workflows.json` and `uploaded-hardwares.json` under `%LOCALAPPDATA%/EquipmentDesigner`).
- Hardware Define Workflow saves in-progress sessions and updates component state as the user edits or completes the workflow.

### Services and API façade

Key services live under `EquipmentDesigner/EquipmentDesigner/Services`:

- **ServiceLocator**
  - A simple DI container with singleton and factory registrations.
  - `ConfigureForProduction()` registers:
    - `IWorkflowRepository` → `WorkflowRepository` (local JSON).
    - `IUploadedWorkflowRepository` → `UploadedWorkflowRepository`.
    - `IHardwareApiService` → `MockHardwareApiService` wrapping the uploaded repository.
  - `ConfigureForTesting()` registers:
    - `IWorkflowRepository` → `MemoryWorkflowRepository`.
    - `IUploadedWorkflowRepository` → `MemoryUploadedWorkflowRepository`.
    - `IHardwareApiService` → `MockHardwareApiService` backed by the memory repo.
  - Tests in `EquipmentDesigner.Tests` rely on these configurations; for new tests, prefer using `ConfigureForTesting()` and `ServiceLocator.Reset()` to avoid file system dependencies.

- **NavigationService**
  - Encapsulates all navigation-related events and methods (`NavigateToHardwareDefineWorkflow`, `ResumeWorkflow`, `NavigateToDrawboard`, etc.).
  - Consumers (e.g., `DashboardViewModel`) call its methods; `MainWindow` subscribes to the events and swaps the active view accordingly.

- **LanguageService**
  - Singleton wrapper around `WPFLocalizeExtension.Engine.LocalizeDictionary`.
  - Maintains a list of available languages (Korean and English) and exposes `SelectedLanguage` and `ChangeLanguage`.
  - `Initialize()` configures culture settings and hooks `MissingKeyEvent` for diagnostics.

- **Storage & API services**
  - `IHardwareApiService` and `MockHardwareApiService` present a REST-like API over the uploaded workflow repository (e.g., `GetSessionsByStateAsync`, `SaveSessionAsync`, `UpdateSessionStateAsync`).
  - The Dashboard uses this API to list and view existing components (in uploaded/validated states), while workflow view models use it to upload sessions and change server-side state.

### Feature-level view models and flows

#### Dashboard

Located under `Views/Dashboard`:

- `DashboardViewModel`:
  - Exposes navigation commands to start new workflows for each `HardwareLayer` via `NavigationService`.
  - Uses `IWorkflowRepository` to load **incomplete** workflow sessions and project them into `WorkflowItem` models.
  - Uses `IHardwareApiService` to fetch uploaded/validated sessions and project them into `ComponentItem` models grouped by hardware layer.
  - Provides admin commands to delete individual workflows, delete all incomplete workflows, or reset all local data (by deleting the underlying JSON files).
  - Interacts with reusable UI components:
    - `ConfirmDialog` for destructive actions.
    - MainWindow's backdrop overlay for modal experiences.

#### Hardware Define Workflow

Located under `Views/HardwareDefineWorkflow`:

- `HardwareDefineWorkflowViewModel` is the central orchestrator for the multi-step workflow:
  - Maintains a collection of `WorkflowStepViewModel` representing high-level steps (Equipment → System → Unit → Device) based on the starting `HardwareLayer`.
  - Maintains a tree of `HardwareTreeNodeViewModel` instances (one per hardware entity), each wrapping a specific `*DefineViewModel` (Equipment, System, Unit, Device) that implements `IHardwareDefineViewModel`.
  - Coordinates:
    - Navigation between steps.
    - Selection of tree nodes and mapping back to the active step.
    - Validation state across all nodes (`AllStepsRequiredFieldsFilled`).
    - Workflow completion and upload actions.
  - Implements an autosave mechanism:
    - Tracks dirty state and first-dirty timestamp.
    - Combines a debounce timer with a maximum wait time to save via `IWorkflowRepository`.
    - Shows a transient autosave indicator in the UI.
  - When resuming or viewing an existing workflow, uses `WorkflowSessionDto` (via `ToWorkflowSessionDto` / `FromWorkflowSessionDto`) to serialize/deserialize the entire tree structure.
  - Can switch between read-only and editable modes, including updating server-side state when a read-only component is transitioned back to draft.

#### Drawboard and process editor

Located under `Views/Drawboard` and `Models/Drawboard`:

- Provides a process diagram editor backed by `Models/Drawboard` entities (elements, shapes, commands, history, workspace manager, etc.).
- `DrawboardViewModel` coordinates selection, commands, and visual state; `DrawboardView` and related adorners render the canvas.
- Navigation into/out of this view is handled through `NavigationService` (`NavigateToDrawboard` / `NavigateBackFromDrawboard`), with `MainWindow` preserving the previous content.

#### Workflow Complete

Located under `Views/WorkflowComplete`:

- `WorkflowCompleteViewModel` and `WorkflowCompleteView` display a summary of a completed `WorkflowSessionDto`, including metadata about the hardware hierarchy and final state.
- `NavigationService.NavigateToWorkflowComplete` carries the relevant session DTO for display.

## UI and design tokens

Design tokens and styling are central to this project and governed by both `Views/Themes` and `.rules/always_on/ui-design-rules.md`.

### Design token structure

- `EquipmentDesigner/EquipmentDesigner/Views/Themes/DesignTokens.xaml` is the **single entry point** merged into `App.Resources`.
- It pulls in token dictionaries from `Views/Themes/Tokens`:
  - `Colors.xaml` – base color palette.
  - `Brushes.xaml` – semantic brushes (status, surfaces, inputs, buttons, etc.).
  - `Typography.xaml` – type scale and font resources.
  - `Spacing.xaml` – spacing and sizing tokens.
  - `Effects.xaml` – shadows and effects.

`Views/Themes/Tokens/README.md` documents how legacy WinForms design tokens map to WPF tokens (e.g., `StatusGroups.Operational` → `Brush.Status.Success`, `UiElements.Background` → `Brush.Background.Primary`, `ButtonColors.Start` → `Brush.Button.Success.Background`) and shows usage examples for cards, badges, buttons, and input fields.

### UI design rules

From `.rules/always_on/ui-design-rules.md`:

- **Strict token usage (critical):**
  - All new XAML must use the existing design tokens from `Views/Themes/DesignTokens.xaml` / `Views/Themes/Tokens/*.xaml` exclusively (colors, typography, spacing, shadows, etc.).
  - Do not introduce ad-hoc hard-coded colors or fonts.
- **Reusable components first:**
  - Before implementing new XAML, check `Views/ReusableComponents/reusable-components.json` for an appropriate existing component (e.g., toast notifications, confirm dialog, file drop zone, chips, context menus, loading spinner, ellipsis button).
  - If you create a new reusable component under `Views/ReusableComponents`, you must also register it in `reusable-components.json` with a description.
- **Layout constraints:**
  - Avoid hardcoded positions and absolute sizes unless unavoidable.
  - Prefer flexible layout containers (`Grid`, `DockPanel`, `StackPanel`, `UniformGrid`) combined with margins and padding.
- **Figma MCP-specific guidance** in the rule file can be ignored here unless explicitly requested by the user; however, even in MCP-driven designs you must still map as closely as possible onto existing design tokens instead of adding new ones.

## Localization

Localization is centrally managed through `Resources/Strings.resx` and WPFLocalizeExtension, with additional guidance in `.rules/always_on/localization.md`.

- `MainWindow.xaml` configures WPFLocalizeExtension:
  - `lex:ResxLocalizationProvider.DefaultAssembly="EquipmentDesigner"`.
  - `lex:ResxLocalizationProvider.DefaultDictionary="Resources.Strings"`.
- `LanguageService` manages runtime switching between Korean (`ko-KR`, default) and English (`en-US`), using `LocalizeDictionary.Instance.Culture`.
- Rules from `.rules/always_on/localization.md`:
  - All user-facing strings must be defined in `EquipmentDesigner/EquipmentDesigner/Resources/Strings.resx` (with locale-specific variants such as `Strings.ko.resx`).
  - Do **not** hardcode UI strings directly in code or XAML; always bind via localization keys (e.g., `{lex:Loc Key=AppTitle}`) or use the strongly typed resource wrapper.

When adding new UI text, ensure you update the `.resx` files and reference the new keys instead of inline literals.

## Testing structure and conventions

Testing rules are defined in `.rules/glob/csharp-unit-test-rules.md` and reflected in `EquipmentDesigner/EquipmentDesigner.Tests`.

- **Test project naming:**
  - Tests live in a project with the `.Tests` suffix: `EquipmentDesigner.Tests`.
- **Directory mirroring:**
  - Test folder structure mirrors the main project structure (e.g., tests for `Services/Storage/WorkflowRepository.cs` live under `EquipmentDesigner.Tests/Services/Storage/WorkflowRepositoryTests.cs`).
- **Test file naming:**
  - Test classes are named `<ClassName>Tests` (e.g., `ServiceLocatorTests`, `WorkflowRepositoryTests`, `DashboardViewModelTests`).
- **Test method naming:**
  - Methods follow the pattern: `[Method]_[Condition]_[ExpectedResult]` (e.g., `LoadAsync_WhenFileNotExists_ReturnsNewEmptyDataStore`).
  - Use the AAA (Arrange–Act–Assert) structure inside test methods.
- **Execution guidance:**
  - The `csharp-running-tests-rules` document emphasizes using `--filter` when running tests for faster feedback. Reflect this by preferring filtered `dotnet test` invocations when running from Warp.

When creating new tests, place them under `EquipmentDesigner/EquipmentDesigner.Tests` with mirrored folders and follow the existing xUnit + FluentAssertions style.

## How this affects future Warp agents

When modifying or extending this codebase, Warp agents should:

- Use the build/test commands above and prefer filtered `dotnet test` runs for focused feedback.
- Respect the MVVM file structure and flat namespace conventions when creating new files.
- Route persistence through the existing repository abstractions (`IWorkflowRepository`, `IUploadedWorkflowRepository`, `ITypedDataRepository<T>`) instead of accessing JSON files directly.
- Use existing services (`NavigationService`, `LanguageService`, `ServiceLocator`) rather than re-implementing navigation, localization, or DI.
- Adhere strictly to the design token and localization rules when touching XAML or user-visible text, and leverage reusable components in `Views/ReusableComponents` before introducing new UI controls.