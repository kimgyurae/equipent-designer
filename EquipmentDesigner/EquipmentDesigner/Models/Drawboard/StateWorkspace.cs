using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Workspace containing drawing elements for a specific PackML state
    /// </summary>
    public class StateWorkspace
    {
        private int _nextZIndex = 0;

        /// <summary>
        /// PackML state this workspace represents
        /// </summary>
        public PackMlState State { get; }

        /// <summary>
        /// Collection of drawing elements in this workspace
        /// </summary>
        public ObservableCollection<DrawingElement> Elements { get; } = new ObservableCollection<DrawingElement>();

        public StateWorkspace(PackMlState state)
        {
            State = state;
        }

        /// <summary>
        /// Adds an element to the workspace and assigns the next ZIndex.
        /// Returns false if an element with the same Id already exists.
        /// </summary>
        public bool AddElement(DrawingElement element)
        {
            if (Elements.Any(e => e.Id == element.Id))
            {
                return false;
            }

            element.ZIndex = _nextZIndex++;
            Elements.Add(element);
            return true;
        }

        /// <summary>
        /// Removes an element by Id.
        /// Returns true if element was found and removed.
        /// </summary>
        public bool RemoveElement(string elementId)
        {
            var element = Elements.FirstOrDefault(e => e.Id == elementId);
            if (element == null)
            {
                return false;
            }

            Elements.Remove(element);
            return true;
        }

        /// <summary>
        /// Gets an element by Id, or null if not found.
        /// </summary>
        public DrawingElement GetElementById(string elementId)
        {
            return Elements.FirstOrDefault(e => e.Id == elementId);
        }

        /// <summary>
        /// Removes all elements from the workspace.
        /// </summary>
        public void Clear()
        {
            Elements.Clear();
            _nextZIndex = 0;
        }

        /// <summary>
        /// Returns elements ordered by ZIndex (ascending).
        /// </summary>
        public IEnumerable<DrawingElement> GetElementsOrderedByZIndex()
        {
            return Elements.OrderBy(e => e.ZIndex);
        }
    }
}
