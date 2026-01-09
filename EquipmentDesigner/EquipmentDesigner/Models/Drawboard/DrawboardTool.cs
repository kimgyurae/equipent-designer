using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EquipmentDesigner.Models.ProcessEditor
{
    /// <summary>
    /// Represents a tool in the process editor toolbar.
    /// </summary>
    public class DrawboardTool : IHintable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isSelected;
        private bool _isEnabled = true;

        /// <summary>
        /// Unique identifier for the tool.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Display name of the tool.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Instruction/description shown in tooltip.
        /// </summary>
        public string Instruction { get; set; }

        /// <summary>
        /// Keyboard shortcut for the tool (single character or key name).
        /// </summary>
        public string Shortcut { get; set; }

        /// <summary>
        /// Hint text shown below the toolbar when this tool is selected.
        /// Text in brackets like [Space] will be rendered as keyboard badges.
        /// </summary>
        public string Hint { get; set; } = string.Empty;

        /// <summary>
        /// Cursor type to display when this tool is active.
        /// </summary>
        public DrawboardToolCursorType CursorType { get; set; } = DrawboardToolCursorType.Default;

        /// <summary>
        /// Group index for visual separation (tools with same index are grouped together).
        /// </summary>
        public int GroupIndex { get; set; }

        /// <summary>
        /// Whether this tool can be selected together with other tools (like ToolLock).
        /// </summary>
        public bool IsToggleable { get; set; }

        /// <summary>
        /// Whether this tool is currently selected.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether this tool is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether this tool should appear in the "More Tools" overflow menu.
        /// </summary>
        public bool IsOverflowTool { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
