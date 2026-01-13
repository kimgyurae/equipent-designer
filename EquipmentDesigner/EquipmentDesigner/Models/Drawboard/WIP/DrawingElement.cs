using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;
using EquipmentDesigner.Models.Rules;

namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Abstract base class for all drawing elements in the process editor workspace
    /// </summary>
    public abstract class DrawingElement : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private double _x;
        private double _y;
        private double _width = 1.0;
        private double _height = 1.0;
        private double _opacity = 1.0;
        private int _zIndex;
        private bool _isSelected;
        private bool _isLocked;

        // Text-related properties
        private string _text = string.Empty;
        private TextFontSize _fontSize = TextFontSize.Base;
        private TextAlignment _textAlignment = TextAlignment.Center;
        private Color _textColor = Colors.Black;
        private double _textOpacity = 1.0;

        /// <summary>
        /// 이 Element에서 나가는 화살표 목록.
        /// 이 Element가 소유하며, TargetId로 대상을 참조합니다.
        /// </summary>
        public ObservableCollection<UMLConnection2> OutgoingArrows { get; private set; }

        /// <summary>
        /// 이 Element로 들어오는 화살표의 Source Element ID 집합.
        /// 규칙 검증용으로 사용됩니다 (예: InitialElement는 IncomingSourceIds.Count == 0이어야 함).
        /// JSON 직렬화 시 제외 - OutgoingArrows에서 파생 가능.
        /// </summary>
        [JsonIgnore]
        public HashSet<string> IncomingSourceIds { get; set; } = new HashSet<string>();

        /// <summary>
        /// 현재 들어오는 화살표 개수 (검증용).
        /// </summary>
        public int CurrentIncomingCount => IncomingSourceIds.Count;

        /// <summary>
        /// 현재 나가는 화살표 개수 (검증용).
        /// </summary>
        public int CurrentOutgoingCount => OutgoingArrows.Count;

        /// <summary>
        /// Unique identifier for the drawing element
        /// </summary>
        public string Id { get; protected set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// X coordinate position
        /// </summary>
        public double X
        {
            get => _x;
            set
            {
                if (_x != value)
                {
                    _x = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Y coordinate position
        /// </summary>
        public double Y
        {
            get => _y;
            set
            {
                if (_y != value)
                {
                    _y = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Width of the element (minimum 1.0)
        /// </summary>
        public double Width
        {
            get => _width;
            set
            {
                var newValue = Math.Max(1.0, value);
                if (_width != newValue)
                {
                    _width = newValue;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Height of the element (minimum 1.0)
        /// </summary>
        public double Height
        {
            get => _height;
            set
            {
                var newValue = Math.Max(1.0, value);
                if (_height != newValue)
                {
                    _height = newValue;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Opacity of the element (0.0 to 1.0)
        /// </summary>
        public double Opacity
        {
            get => _opacity;
            set
            {
                if (_opacity != value)
                {
                    _opacity = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Z-index for layering order (higher values are on top)
        /// </summary>
        public int ZIndex
        {
            get => _zIndex;
            set
            {
                if (_zIndex != value)
                {
                    _zIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether the element is currently selected for editing
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
        /// Whether the element is locked (prevents editing)
        /// </summary>
        public bool IsLocked
        {
            get => _isLocked;
            set
            {
                if (_isLocked != value)
                {
                    _isLocked = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The text content displayed in the center of the element
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Font size of the text (XS=10, Base=14, XL=20)
        /// </summary>
        public TextFontSize FontSize
        {
            get => _fontSize;
            set
            {
                if (_fontSize != value)
                {
                    _fontSize = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Text alignment (Left, Center, Right)
        /// </summary>
        public TextAlignment TextAlign
        {
            get => _textAlignment;
            set
            {
                if (_textAlignment != value)
                {
                    _textAlignment = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Text color
        /// </summary>
        public Color TextColor
        {
            get => _textColor;
            set
            {
                if (_textColor != value)
                {
                    _textColor = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Text opacity (0.0 to 1.0)
        /// </summary>
        public double TextOpacity
        {
            get => _textOpacity;
            set
            {
                if (_textOpacity != value)
                {
                    _textOpacity = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Shape type determined by derived class
        /// </summary>
        public abstract DrawingShapeType ShapeType { get; }

        /// <summary>
        /// OutgoingArrowCount
        /// </summary>
        public abstract int OutgoingArrowCount { get; }

        /// <summary>
        /// OutgoingArrowCount
        /// </summary>
        public abstract int IncomingArrowCount { get; }

        /// <summary>
        /// Gets the bounding rectangle of this element.
        /// </summary>
        public Rect Bounds => new Rect(X, Y, Width, Height);

        /// <summary>
        /// Creates a deep copy of the element with a new Id
        /// </summary>
        public virtual DrawingElement Clone()
        {
            var clone = (DrawingElement)MemberwiseClone();
            clone.Id = Guid.NewGuid().ToString();
            // 복제 시 연결 정보는 복사하지 않음 - 새 Element는 빈 연결 상태로 시작
            clone.OutgoingArrows = new ObservableCollection<UMLConnection2>();
            clone.OutgoingArrows.CollectionChanged += clone.OnOutgoingArrowsChanged;
            clone.IncomingSourceIds = new HashSet<string>();
            return clone;
        }

        protected DrawingElement()
        {
            OutgoingArrows = new ObservableCollection<UMLConnection2>();
            OutgoingArrows.CollectionChanged += OnOutgoingArrowsChanged;
        }

        private void OnOutgoingArrowsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyViolationsChanged();
        }

        /// <summary>
        /// Adds an incoming source ID and notifies violation state change.
        /// </summary>
        /// <param name="sourceId">The source element ID to add.</param>
        /// <returns>True if the ID was added; false if it already existed.</returns>
        public bool AddIncomingSource(string sourceId)
        {
            var added = IncomingSourceIds.Add(sourceId);
            if (added)
            {
                NotifyViolationsChanged();
            }
            return added;
        }

        /// <summary>
        /// Removes an incoming source ID and notifies violation state change.
        /// </summary>
        /// <param name="sourceId">The source element ID to remove.</param>
        /// <returns>True if the ID was removed; false if it didn't exist.</returns>
        public bool RemoveIncomingSource(string sourceId)
        {
            var removed = IncomingSourceIds.Remove(sourceId);
            if (removed)
            {
                NotifyViolationsChanged();
            }
            return removed;
        }

        /// <summary>
        /// Notifies that violation-related properties have changed.
        /// Call this when arrow connections are modified externally.
        /// </summary>
        public void NotifyViolationsChanged()
        {
            OnPropertyChanged(nameof(HasViolations));
            OnPropertyChanged(nameof(Violations));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Rule Validation

        /// <summary>
        /// Gets the rules that apply to this element type.
        /// Default implementation creates rules based on OutgoingArrowCount and IncomingArrowCount properties.
        /// -1 means minimum 1, other values mean exact count.
        /// </summary>
        /// <returns>Collection of rules that apply to this element.</returns>
        public virtual IReadOnlyList<IElementRule> GetRules()
        {
            var rules = new List<IElementRule>();

            // Incoming arrow rule
            if (IncomingArrowCount == -1)
            {
                // -1 means at least 1 incoming arrow required
                rules.Add(ArrowCountRule.Minimum(ArrowDirection.Incoming, 1));
            }
            else
            {
                // Exact count required
                rules.Add(ArrowCountRule.Exact(ArrowDirection.Incoming, IncomingArrowCount));
            }

            // Outgoing arrow rule
            if (OutgoingArrowCount == -1)
            {
                // -1 means at least 1 outgoing arrow required
                rules.Add(ArrowCountRule.Minimum(ArrowDirection.Outgoing, 1));
            }
            else
            {
                // Exact count required
                rules.Add(ArrowCountRule.Exact(ArrowDirection.Outgoing, OutgoingArrowCount));
            }

            return rules;
        }

        /// <summary>
        /// Evaluates all rules and returns any violations found.
        /// </summary>
        /// <returns>List of rule violations, empty if all rules pass.</returns>
        public IReadOnlyList<RuleViolation> ValidateRules()
        {
            var violations = new List<RuleViolation>();
            var rules = GetRules();

            if (rules == null) return violations;

            foreach (var rule in rules)
            {
                var violation = rule.Evaluate(this);
                if (violation != null)
                {
                    violations.Add(violation);
                }
            }

            return violations;
        }

        /// <summary>
        /// Gets whether this element currently has any rule violations.
        /// </summary>
        [JsonIgnore]
        public bool HasViolations => ValidateRules().Count > 0;

        /// <summary>
        /// Gets the current rule violations for this element.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<RuleViolation> Violations => ValidateRules();

        #endregion
    }
}