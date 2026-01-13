using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Represents a connection (arrow) between two UML elements in a workflow.
    /// </summary>
    public class UMLConnection2 : INotifyPropertyChanged
    {
        private string _label = string.Empty;
        private string _targetId;
        private PortPosition _tailPort;
        private PortPosition _headPort;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Optional label displayed at the midpoint of the connection line.
        /// </summary>
        public string Label
        {
            get => _label;
            set
            {
                if (_label != value)
                {
                    _label = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The ID of the source (tail) element.
        /// </summary>
        public string TargetId
        {
            get => _targetId;
            set
            {
                if (_targetId != value)
                {
                    _targetId = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The port position on the source element where the connection starts.
        /// </summary>
        public PortPosition TailPort
        {
            get => _tailPort;
            set
            {
                if (_tailPort != value)
                {
                    _tailPort = value;
                    OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// The port position on the target element where the connection ends.
        /// </summary>
        public PortPosition HeadPort
        {
            get => _headPort;
            set
            {
                if (_headPort != value)
                {
                    _headPort = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}