using System.Windows.Controls;
using EquipmentDesigner.ViewModels;
using EquipmentDesigner.Controls;
using System.Windows.Input;
using System.Diagnostics;

namespace EquipmentDesigner.Views
{
    /// <summary>
    /// Interaction logic for DashboardView.xaml
    /// </summary>
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
            DataContext = new DashboardViewModel();

            // Register keyboard shortcut for Admin mode toggle
            PreviewKeyDown += DashboardView_PreviewKeyDown;
        }

        /// <summary>
        /// Handles PreviewKeyDown to toggle Admin mode with Ctrl+Shift+F10.
        /// </summary>
        private void DashboardView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // F10 is a system key in Windows, so we need to check SystemKey instead of Key
            var key = e.Key == Key.System ? e.SystemKey : e.Key;

            if (key == Key.F10 &&
                Keyboard.Modifiers.HasFlag(ModifierKeys.Control) &&
                Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                if (DataContext is DashboardViewModel viewModel)
                {
                    viewModel.ToggleAdminMode();
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Shows a comprehensive test context menu demonstrating all CustomContextMenuService features.
        /// This menu is for testing purposes only and includes:
        /// - 50+ items at depth 1
        /// - Depth 4 nesting
        /// - All AddItem overloads (ICommand, Action, Action<T>)
        /// - All AddDestructiveItem overloads
        /// - AddSeparator, BeginSection, AddSubMenu
        /// - isEnabled parameter testing
        /// - OnItemClicked callback
        /// </summary>
        private void AdminPanel_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var builder = Controls.ContextMenuService.Instance.Create();

            // ========================================
            // Section 1: Basic Items (Action overload)
            // ========================================
            builder.BeginSection("BasicActions");
            for (int i = 1; i <= 10; i++)
            {
                int index = i;
                builder.AddItem($"Basic Action Item {i}", () => Debug.WriteLine($"Basic Action {index} clicked"));
            }

            // ========================================
            // Section 2: Items with isEnabled testing
            // ========================================
            builder.BeginSection("EnabledDisabled");
            builder.AddItem("Enabled Item 1", () => Debug.WriteLine("Enabled 1"), isEnabled: true);
            builder.AddItem("Disabled Item 2", () => Debug.WriteLine("Should not execute"), isEnabled: false);
            builder.AddItem("Enabled Item 3", () => Debug.WriteLine("Enabled 3"), isEnabled: true);
            builder.AddItem("Disabled Item 4", () => Debug.WriteLine("Should not execute"), isEnabled: false);
            builder.AddItem("Disabled Item 5", () => Debug.WriteLine("Should not execute"), isEnabled: false);
            builder.AddItem("Enabled Item 6", () => Debug.WriteLine("Enabled 6"), isEnabled: true);

            // ========================================
            // Section 3: Parameterized Items (Action<T> overload)
            // ========================================
            builder.BeginSection("ParameterizedActions");
            for (int i = 1; i <= 8; i++)
            {
                string param = $"Parameter_{i}";
                builder.AddItem($"Parameterized Item {i}", (string p) => Debug.WriteLine($"Received: {p}"), param);
            }

            // ========================================
            // Section 4: Destructive Items
            // ========================================
            builder.BeginSection("DestructiveActions");
            builder.AddDestructiveItem("Delete All (Enabled)", () => Debug.WriteLine("Delete All clicked"), isEnabled: true);
            builder.AddDestructiveItem("Remove Selection (Disabled)", () => Debug.WriteLine("Should not execute"), isEnabled: false);
            builder.AddDestructiveItem("Clear Cache", () => Debug.WriteLine("Clear Cache clicked"));
            builder.AddDestructiveItem("Reset Settings (Disabled)", () => Debug.WriteLine("Should not execute"), isEnabled: false);
            builder.AddDestructiveItem("Purge Data", () => Debug.WriteLine("Purge Data clicked"));

            // ========================================
            // Section 5: Simple Separators
            // ========================================
            builder.AddSeparator();
            builder.AddItem("After Separator 1", () => Debug.WriteLine("After Sep 1"));
            builder.AddSeparator();
            builder.AddItem("After Separator 2", () => Debug.WriteLine("After Sep 2"));

            // ========================================
            // Section 6: SubMenus with Depth 2
            // ========================================
            builder.BeginSection("SubMenusDepth2");
            for (int i = 1; i <= 5; i++)
            {
                int subIndex = i;
                builder.AddSubMenu($"SubMenu Level 1 - {i}", sub1 =>
                {
                    for (int j = 1; j <= 4; j++)
                    {
                        int itemIndex = j;
                        sub1.AddItem($"L2 Item {subIndex}.{j}", () => Debug.WriteLine($"Depth 2: {subIndex}.{itemIndex}"));
                    }
                    sub1.AddSeparator();
                    sub1.AddDestructiveItem($"L2 Delete {subIndex}", () => Debug.WriteLine($"Delete at depth 2: {subIndex}"));
                });
            }

            // ========================================
            // Section 7: Deep Nesting (Depth 4)
            // ========================================
            builder.BeginSection("DeepNesting");
            builder.AddSubMenu("Deep Nest Root A", depth1 =>
            {
                depth1.AddItem("A - Direct Child", () => Debug.WriteLine("A Direct"));
                depth1.AddSubMenu("A - Level 2", depth2 =>
                {
                    depth2.AddItem("A.2 - Item 1", () => Debug.WriteLine("A.2.1"));
                    depth2.AddItem("A.2 - Item 2 (Disabled)", () => Debug.WriteLine("Should not execute"), isEnabled: false);
                    depth2.AddSubMenu("A.2 - Level 3", depth3 =>
                    {
                        depth3.AddItem("A.2.3 - Item 1", () => Debug.WriteLine("A.2.3.1"));
                        depth3.AddDestructiveItem("A.2.3 - Delete", () => Debug.WriteLine("A.2.3 Delete"));
                        depth3.AddSubMenu("A.2.3 - Level 4 (MAX)", depth4 =>
                        {
                            depth4.AddItem("A.2.3.4 - Final Item 1", () => Debug.WriteLine("MAX DEPTH 1"));
                            depth4.AddItem("A.2.3.4 - Final Item 2", () => Debug.WriteLine("MAX DEPTH 2"));
                            depth4.AddItem("A.2.3.4 - Final Item 3 (Disabled)", () => Debug.WriteLine("Should not execute"), isEnabled: false);
                            depth4.AddDestructiveItem("A.2.3.4 - Final Delete", () => Debug.WriteLine("MAX DEPTH Delete"));
                        });
                    });
                });
            });

            builder.AddSubMenu("Deep Nest Root B", depth1 =>
            {
                depth1.AddItem("B - Item 1", () => Debug.WriteLine("B.1"));
                depth1.AddItem("B - Item 2", () => Debug.WriteLine("B.2"));
                depth1.AddSubMenu("B - Nested", depth2 =>
                {
                    depth2.AddItem("B.N - Item", () => Debug.WriteLine("B.N"));
                    depth2.AddSubMenu("B.N - More Nested", depth3 =>
                    {
                        depth3.AddItem("B.N.M - Item 1", () => Debug.WriteLine("B.N.M.1"));
                        depth3.AddSubMenu("B.N.M - Deepest", depth4 =>
                        {
                            depth4.AddItem("B.N.M.D - Final 1", () => Debug.WriteLine("B Final 1"));
                            depth4.AddItem("B.N.M.D - Final 2", () => Debug.WriteLine("B Final 2"));
                        });
                    });
                });
            });

            // ========================================
            // Section 8: Mixed Content SubMenus
            // ========================================
            builder.BeginSection("MixedSubMenus");
            builder.AddSubMenu("Mixed Menu 1", sub =>
            {
                sub.BeginSection("SubSection1");
                sub.AddItem("Mixed Item 1", () => Debug.WriteLine("Mixed 1"));
                sub.AddItem("Mixed Item 2", () => Debug.WriteLine("Mixed 2"));
                sub.BeginSection("SubSection2");
                sub.AddDestructiveItem("Mixed Delete 1", () => Debug.WriteLine("Mixed Delete 1"));
                sub.AddDestructiveItem("Mixed Delete 2 (Disabled)", () => Debug.WriteLine("Should not execute"), isEnabled: false);
            });

            builder.AddSubMenu("Mixed Menu 2 (With Disabled)", sub =>
            {
                sub.AddItem("Child 1", () => Debug.WriteLine("Child 1"));
                sub.AddItem("Child 2 (Disabled)", () => Debug.WriteLine("Should not execute"), isEnabled: false);
                sub.AddItem("Child 3", () => Debug.WriteLine("Child 3"));
                sub.AddSeparator();
                sub.AddItem("Child 4", () => Debug.WriteLine("Child 4"));
            });

            // ========================================
            // Section 9: More Items to reach 50+
            // ========================================
            builder.BeginSection("BulkItems1");
            for (int i = 1; i <= 8; i++)
            {
                int index = i;
                bool enabled = i % 3 != 0; // Every 3rd item disabled
                builder.AddItem($"Bulk Item A-{i}", () => Debug.WriteLine($"Bulk A-{index}"), isEnabled: enabled);
            }

            builder.BeginSection("BulkItems2");
            for (int i = 1; i <= 8; i++)
            {
                int index = i;
                builder.AddItem($"Bulk Item B-{i}", () => Debug.WriteLine($"Bulk B-{index}"));
            }

            // ========================================
            // Section 10: Final Destructive Actions
            // ========================================
            builder.BeginSection("FinalDestructive");
            builder.AddDestructiveItem("⚠️ Danger Action 1", () => Debug.WriteLine("Danger 1"));
            builder.AddDestructiveItem("⚠️ Danger Action 2", () => Debug.WriteLine("Danger 2"));
            builder.AddDestructiveItem("⚠️ Danger Action 3 (Disabled)", () => Debug.WriteLine("Should not execute"), isEnabled: false);

            // ========================================
            // OnItemClicked callback for logging
            // ========================================
            builder.OnItemClicked(item =>
            {
                Debug.WriteLine($"[ContextMenu] Item clicked: {item.Header}, Type: {item.ItemType}, Enabled: {item.IsEnabled}");
            });

            builder.Show();
            e.Handled = true;
        }
    }
}