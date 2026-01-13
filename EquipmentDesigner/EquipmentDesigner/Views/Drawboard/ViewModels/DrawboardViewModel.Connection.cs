using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using EquipmentDesigner.Models;
using EquipmentDesigner.Views.Drawboard.UMLEngine;
using EquipmentDesigner.Views.Drawboard.UMLEngine.Contexts;
using EquipmentDesigner.Views.Drawboard.UMLEngine.Results;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// Partial class containing connection port and arrow connection functionality.
    /// Handles selection-based port display and arrow connection mode.
    /// Uses adjacency list pattern: each element owns its OutgoingArrows.
    /// </summary>
    public partial class DrawboardViewModel
    {
        #region Fields

        // Port display state (triggered by selection, not hover)
        private DrawingElement _portDisplayElement;
        private bool _isShowingPorts;

        // Connection state
        private ConnectionContext _connectionContext;
        private DrawingElement _connectionSourceElement;
        private string _snapTargetElementId;
        private PortPosition? _snapTargetPort;
        private Point? _snapTargetPosition;
        private Rect? _snapTargetBounds;
        private List<Point> _currentRoutePoints = new List<Point>();

        // Connection selection state (adjacency list pattern)
        private UMLConnection2 _selectedConnection;
        private DrawingElement _selectedConnectionSource; // Owner element of selected connection

        // Connection editing state
        private bool _isEditingHead; // true = editing head, false = editing tail
        private string _originalEndpointId;
        private PortPosition _originalEndpointPort;
        private string _editSnapTargetId;
        private PortPosition? _editSnapTargetPort;
        private Point? _editSnapTargetPosition;
        private Rect? _editSnapTargetBounds;
        private List<Point> _editRoutePoints = new List<Point>();

        #endregion

        #region Properties

        /// <summary>
        /// The element currently showing connection ports (based on selection).
        /// </summary>
        public DrawingElement HoverElement
        {
            get => _portDisplayElement;
            private set => SetProperty(ref _portDisplayElement, value);
        }

        /// <summary>
        /// Whether connection ports are currently showing on the hover element.
        /// </summary>
        public bool IsShowingPorts
        {
            get => _isShowingPorts;
            private set => SetProperty(ref _isShowingPorts, value);
        }

        /// <summary>
        /// Whether the ViewModel is in connection drawing mode.
        /// </summary>
        public bool IsConnecting => EditModeState == EditModeState.ConnectingArrow;

        /// <summary>
        /// The current orthogonal route points for the connection preview.
        /// </summary>
        public IReadOnlyList<Point> CurrentRoutePoints => _currentRoutePoints;

        /// <summary>
        /// Whether the connection is currently snapped to a target port.
        /// </summary>
        public bool IsSnappedToTarget => _snapTargetElementId != null;

        /// <summary>
        /// The position of the current snap target (if any).
        /// </summary>
        public Point? SnapTargetPosition => _snapTargetPosition;

        /// <summary>
        /// The currently selected connection (arrow) for editing.
        /// </summary>
        public UMLConnection2 SelectedConnection
        {
            get => _selectedConnection;
            private set
            {
                if (SetProperty(ref _selectedConnection, value))
                {
                    OnPropertyChanged(nameof(IsConnectionSelected));
                }
            }
        }

        /// <summary>
        /// The source element that owns the currently selected connection.
        /// </summary>
        public DrawingElement SelectedConnectionSource
        {
            get => _selectedConnectionSource;
            private set => SetProperty(ref _selectedConnectionSource, value);
        }

        /// <summary>
        /// Whether a connection is currently selected.
        /// </summary>
        public bool IsConnectionSelected => _selectedConnection != null;

        /// <summary>
        /// Whether a connection endpoint is currently being edited (dragged).
        /// </summary>
        public bool IsEditingConnection => EditModeState == EditModeState.EditingConnectionHead ||
                                           EditModeState == EditModeState.EditingConnectionTail;

        /// <summary>
        /// Whether the connection edit is currently snapped to a valid target.
        /// </summary>
        public bool IsEditSnappedToTarget => _editSnapTargetId != null;

        #endregion

        #region Events

        /// <summary>
        /// Raised when connection ports should be shown on the hover element.
        /// </summary>
        public event EventHandler<ConnectionPortsEventArgs> ConnectionPortsShown;

        /// <summary>
        /// Raised when connection ports should be hidden.
        /// </summary>
        public event EventHandler ConnectionPortsHidden;

        /// <summary>
        /// Raised when the connection route is updated during connection mode.
        /// </summary>
        public event EventHandler<ConnectionRouteEventArgs> ConnectionRouteUpdated;

        /// <summary>
        /// Raised when a connection is successfully created.
        /// </summary>
        public event EventHandler<ConnectionCreatedEventArgs> ConnectionCreated;

        /// <summary>
        /// Raised when a connection operation is cancelled.
        /// </summary>
        public event EventHandler ConnectionCancelled;

        /// <summary>
        /// Raised when a connection is selected for editing.
        /// </summary>
        public event EventHandler<ConnectionSelectedEventArgs> ConnectionSelected;

        /// <summary>
        /// Raised when a connection is deselected.
        /// </summary>
        public event EventHandler ConnectionDeselected;

        /// <summary>
        /// Raised when the connection edit route is updated during endpoint dragging.
        /// </summary>
        public event EventHandler<ConnectionEditRouteEventArgs> ConnectionEditRouteUpdated;

        #endregion

        #region Selection-Based Port Display

        /// <summary>
        /// Updates connection port display based on current selection state.
        /// Shows ports only when a single element is selected (not multi-selection).
        /// Called from selection methods in DrawboardViewModel.Selection.cs.
        /// </summary>
        public void UpdatePortDisplayForSelection()
        {
            // Don't update port display during connection mode
            if (IsConnecting) return;

            // Show ports only for single selection (not multi-selection)
            // TextboxElement is excluded - it cannot have connections
            if (!IsMultiSelectionMode && SelectedElement != null && SelectedElement.ShapeType != DrawingShapeType.Textbox)
            {
                ShowConnectionPorts(SelectedElement);
            }
            else
            {
                HideConnectionPorts();
            }
        }

        #endregion

        #region Connection Selection

        /// <summary>
        /// Selects a connection for editing, showing the edit adorner with head/tail thumbs.
        /// </summary>
        /// <param name="connection">The connection to select.</param>
        /// <param name="sourceElement">The source element that owns this connection.</param>
        public void SelectConnection(UMLConnection2 connection, DrawingElement sourceElement)
        {
            if (connection == null || sourceElement == null) return;

            // Clear any element selection first
            ClearAllSelections();

            // Hide connection ports if showing
            if (IsShowingPorts)
            {
                HideConnectionPorts();
            }

            // Set the selected connection and its source
            SelectedConnection = connection;
            SelectedConnectionSource = sourceElement;
            EditModeState = EditModeState.ConnectionSelected;

            // Notify view to show the edit adorner
            ConnectionSelected?.Invoke(this, new ConnectionSelectedEventArgs(connection, sourceElement));
        }

        /// <summary>
        /// Clears the current connection selection.
        /// </summary>
        public void ClearConnectionSelection()
        {
            if (_selectedConnection == null) return;

            var wasSelected = _selectedConnection;
            SelectedConnection = null;
            SelectedConnectionSource = null;

            // Reset edit mode if we were in connection selected mode
            if (EditModeState == EditModeState.ConnectionSelected)
            {
                EditModeState = EditModeState.None;
            }

            // Notify view to remove the edit adorner
            ConnectionDeselected?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Finds a connection at the specified point by checking all elements' OutgoingArrows.
        /// Returns the connection and its source element.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <param name="tolerance">The distance tolerance for hit testing.</param>
        /// <returns>The connection and source element if found, null otherwise.</returns>
        public (UMLConnection2 Connection, DrawingElement SourceElement)? FindConnectionAtPoint(Point point, double tolerance = 5.0)
        {
            foreach (var sourceElement in CurrentSteps)
            {
                foreach (var connection in sourceElement.OutgoingArrows)
                {
                    var targetElement = CurrentSteps.FirstOrDefault(e => e.Id == connection.TargetId);
                    if (targetElement == null) continue;

                    // Calculate the connection route points
                    var context = ConnectionRoutingEngine.CreateConnectionContext(sourceElement, connection.TailPort);
                    var result = ConnectionRoutingEngine.CalculateOrthogonalRouteToTarget(
                        context, connection.TargetId, connection.HeadPort, targetElement.Bounds);

                    // Check if point is near any segment of the route
                    if (IsPointNearRoute(point, result.RoutePoints, tolerance))
                    {
                        return (connection, sourceElement);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Checks if a point is near any segment of a route.
        /// </summary>
        private bool IsPointNearRoute(Point point, IReadOnlyList<Point> routePoints, double tolerance)
        {
            for (int i = 0; i < routePoints.Count - 1; i++)
            {
                var segmentStart = routePoints[i];
                var segmentEnd = routePoints[i + 1];
                var distance = DistanceToLineSegment(point, segmentStart, segmentEnd);
                if (distance <= tolerance)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Calculates the distance from a point to a line segment.
        /// </summary>
        private double DistanceToLineSegment(Point p, Point a, Point b)
        {
            var dx = b.X - a.X;
            var dy = b.Y - a.Y;
            var lengthSq = dx * dx + dy * dy;

            if (lengthSq == 0)
            {
                // Segment is a point
                return Math.Sqrt((p.X - a.X) * (p.X - a.X) + (p.Y - a.Y) * (p.Y - a.Y));
            }

            var t = Math.Max(0, Math.Min(1, ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lengthSq));
            var projX = a.X + t * dx;
            var projY = a.Y + t * dy;

            return Math.Sqrt((p.X - projX) * (p.X - projX) + (p.Y - projY) * (p.Y - projY));
        }

        #endregion

        #region Connection Editing

        /// <summary>
        /// Starts editing a connection endpoint (head or tail).
        /// </summary>
        /// <param name="isHead">True to edit the head (arrow tip), false to edit the tail (start).</param>
        public void StartConnectionEdit(bool isHead)
        {
            if (_selectedConnection == null || _selectedConnectionSource == null) return;

            _isEditingHead = isHead;

            // Store original endpoint for potential revert
            if (isHead)
            {
                _originalEndpointId = _selectedConnection.TargetId;
                _originalEndpointPort = _selectedConnection.HeadPort;
            }
            else
            {
                // For tail editing, the "endpoint" is the source element itself
                _originalEndpointId = _selectedConnectionSource.Id;
                _originalEndpointPort = _selectedConnection.TailPort;
            }

            // Clear snap state
            _editSnapTargetId = null;
            _editSnapTargetPort = null;
            _editSnapTargetPosition = null;
            _editSnapTargetBounds = null;
            _editRoutePoints.Clear();

            // Enter editing mode
            EditModeState = isHead ? EditModeState.EditingConnectionHead : EditModeState.EditingConnectionTail;
        }

        /// <summary>
        /// Updates the connection endpoint position during drag.
        /// </summary>
        /// <param name="mousePosition">The current mouse position in canvas coordinates.</param>
        public void UpdateConnectionEdit(Point mousePosition)
        {
            if (!IsEditingConnection || _selectedConnection == null || _selectedConnectionSource == null) return;

            // Determine which endpoint is fixed (the one we're NOT editing)
            DrawingElement fixedElement;
            PortPosition fixedPort;
            
            if (_isEditingHead)
            {
                // Editing head: tail (source) is fixed
                fixedElement = _selectedConnectionSource;
                fixedPort = _selectedConnection.TailPort;
            }
            else
            {
                // Editing tail: head (target) is fixed
                fixedElement = CurrentSteps.FirstOrDefault(e => e.Id == _selectedConnection.TargetId);
                fixedPort = _selectedConnection.HeadPort;
            }

            if (fixedElement == null) return;

            // Check for snap targets - exclude the fixed endpoint element
            var excludeIds = new HashSet<string> { fixedElement.Id };
            var snapTarget = FindSnapTargetExcluding(mousePosition, excludeIds);

            ConnectionRouteResult result;
            var fixedEdgePosition = ConnectionRoutingEngine.CalculateEdgePosition(fixedElement.Bounds, fixedPort);

            if (snapTarget.HasValue)
            {
                // Snap to target port
                _editSnapTargetId = snapTarget.Value.ElementId;
                _editSnapTargetPort = snapTarget.Value.Port;
                _editSnapTargetPosition = snapTarget.Value.Position;
                _editSnapTargetBounds = snapTarget.Value.Bounds;

                var targetEdgePosition = ConnectionRoutingEngine.CalculateEdgePosition(
                    snapTarget.Value.Bounds, snapTarget.Value.Port);

                if (_isEditingHead)
                {
                    // Editing head: route from fixed tail to snapped head
                    var context = ConnectionRoutingEngine.CreateConnectionContext(fixedElement, fixedPort);
                    result = ConnectionRoutingEngine.CalculateOrthogonalRouteToTarget(
                        context,
                        snapTarget.Value.ElementId,
                        snapTarget.Value.Port,
                        snapTarget.Value.Bounds);
                }
                else
                {
                    // Editing tail: route from snapped tail to fixed head
                    var snapElement = CurrentSteps.FirstOrDefault(e => e.Id == snapTarget.Value.ElementId);
                    if (snapElement == null) return;
                    
                    var context = ConnectionRoutingEngine.CreateConnectionContext(snapElement, snapTarget.Value.Port);
                    result = ConnectionRoutingEngine.CalculateOrthogonalRouteToTarget(
                        context,
                        fixedElement.Id,
                        fixedPort,
                        fixedElement.Bounds);
                }
            }
            else
            {
                // No snap - route to mouse position
                _editSnapTargetId = null;
                _editSnapTargetPort = null;
                _editSnapTargetPosition = null;
                _editSnapTargetBounds = null;

                if (_isEditingHead)
                {
                    // Editing head: route from fixed tail to mouse
                    var context = ConnectionRoutingEngine.CreateConnectionContext(fixedElement, fixedPort);
                    result = ConnectionRoutingEngine.CalculateOrthogonalRoute(context, mousePosition);
                }
                else
                {
                    // Editing tail: route from mouse to fixed head
                    var closestPort = ConnectionRoutingEngine.CalculateClosestPort(fixedElement.Bounds, mousePosition);
                    var context = ConnectionRoutingEngine.CreateConnectionContext(fixedElement, fixedPort);
                    
                    // Reverse the route: from mouse to fixed head
                    var routePoints = new List<Point> { mousePosition };
                    routePoints.Add(fixedEdgePosition);
                    result = new ConnectionRouteResult(routePoints);
                }
            }

            // Update route points
            _editRoutePoints = new List<Point>(result.RoutePoints);

            // Calculate display positions
            Point headPos, tailPos;
            if (_isEditingHead)
            {
                tailPos = fixedEdgePosition;
                headPos = _editSnapTargetPosition ?? mousePosition;
            }
            else
            {
                headPos = fixedEdgePosition;
                tailPos = _editSnapTargetPosition ?? mousePosition;
            }

            // Notify view to update preview
            ConnectionEditRouteUpdated?.Invoke(this, new ConnectionEditRouteEventArgs(
                _editRoutePoints,
                _editSnapTargetId != null,
                headPos,
                tailPos,
                _isEditingHead));
        }

        /// <summary>
        /// Completes the connection edit operation.
        /// </summary>
        public void CompleteConnectionEdit()
        {
            if (!IsEditingConnection || _selectedConnection == null || _selectedConnectionSource == null) return;

            if (_editSnapTargetId != null && _editSnapTargetPort.HasValue)
            {
                if (_isEditingHead)
                {
                    // Update target - remove from old target's IncomingSourceIds, add to new
                    var oldTargetElement = CurrentSteps.FirstOrDefault(e => e.Id == _selectedConnection.TargetId);
                    oldTargetElement?.IncomingSourceIds.Remove(_selectedConnectionSource.Id);

                    var newTargetElement = CurrentSteps.FirstOrDefault(e => e.Id == _editSnapTargetId);
                    newTargetElement?.IncomingSourceIds.Add(_selectedConnectionSource.Id);

                    // Update connection
                    _selectedConnection.TargetId = _editSnapTargetId;
                    _selectedConnection.HeadPort = _editSnapTargetPort.Value;
                }
                else
                {
                    // Editing tail means changing the source element
                    // This is a more complex operation - move connection to new source
                    var newSourceElement = CurrentSteps.FirstOrDefault(e => e.Id == _editSnapTargetId);
                    if (newSourceElement != null)
                    {
                        // Remove from old source's OutgoingArrows
                        _selectedConnectionSource.OutgoingArrows.Remove(_selectedConnection);

                        // Update target's IncomingSourceIds
                        var targetElement = CurrentSteps.FirstOrDefault(e => e.Id == _selectedConnection.TargetId);
                        if (targetElement != null)
                        {
                            targetElement.IncomingSourceIds.Remove(_selectedConnectionSource.Id);
                            targetElement.IncomingSourceIds.Add(newSourceElement.Id);
                        }

                        // Update connection and add to new source
                        _selectedConnection.TailPort = _editSnapTargetPort.Value;
                        newSourceElement.OutgoingArrows.Add(_selectedConnection);

                        // Update the source reference
                        SelectedConnectionSource = newSourceElement;
                    }
                }
            }
            // If not snapped, keep the original endpoint (no change)

            // Exit editing mode but keep selection
            ExitConnectionEditMode();

            // Check for orphan connection after edit completes
            CheckOrphanConnection(_selectedConnection, _selectedConnectionSource);
        }

        /// <summary>
        /// Cancels the connection edit operation and reverts to original state.
        /// </summary>
        public void CancelConnectionEdit()
        {
            if (!IsEditingConnection) return;

            // No changes needed - original values are still in the connection

            // Exit editing mode but keep selection
            ExitConnectionEditMode();
        }

        /// <summary>
        /// Finds snap targets excluding specified element IDs.
        /// </summary>
        private (string ElementId, PortPosition Port, Point Position, Rect Bounds)? FindSnapTargetExcluding(
            Point mousePosition,
            HashSet<string> excludeIds)
        {
            (string ElementId, PortPosition Port, Point Position, Rect Bounds, double Distance)? closest = null;

            foreach (var element in CurrentSteps)
            {
                if (excludeIds.Contains(element.Id)) continue;
                
                // TextboxElement cannot be a connection target
                if (element.ShapeType == DrawingShapeType.Textbox) continue;

                var bounds = element.Bounds;

                foreach (PortPosition port in Enum.GetValues(typeof(PortPosition)))
                {
                    var portPos = ConnectionRoutingEngine.CalculatePortPosition(bounds, port);
                    var dx = portPos.X - mousePosition.X;
                    var dy = portPos.Y - mousePosition.Y;
                    var distance = Math.Sqrt(dx * dx + dy * dy);

                    if (distance <= ConnectionRoutingEngine.SnapDistance)
                    {
                        if (closest == null || distance < closest.Value.Distance)
                        {
                            closest = (element.Id, port, portPos, bounds, distance);
                        }
                    }
                }
            }

            if (closest != null)
            {
                return (closest.Value.ElementId, closest.Value.Port, closest.Value.Position, closest.Value.Bounds);
            }

            return null;
        }

        /// <summary>
        /// Exits connection edit mode and resets editing state.
        /// </summary>
        private void ExitConnectionEditMode()
        {
            _originalEndpointId = null;
            _originalEndpointPort = default;
            _editSnapTargetId = null;
            _editSnapTargetPort = null;
            _editSnapTargetPosition = null;
            _editSnapTargetBounds = null;
            _editRoutePoints.Clear();

            // Return to connection selected state
            EditModeState = EditModeState.ConnectionSelected;
        }

        /// <summary>
        /// Checks if a connection is orphaned (target doesn't exist) and removes it if so.
        /// </summary>
        /// <param name="connection">The connection to check.</param>
        /// <param name="sourceElement">The source element owning the connection.</param>
        /// <returns>True if the connection was removed as orphan, false otherwise.</returns>
        public bool CheckOrphanConnection(UMLConnection2 connection, DrawingElement sourceElement)
        {
            if (connection == null || sourceElement == null) return false;

            // Check if target element exists
            bool isTargetOrphan = !CurrentSteps.Any(e => e.Id == connection.TargetId);

            if (isTargetOrphan)
            {
                // Clear selection if this connection was selected
                if (_selectedConnection == connection)
                {
                    ClearConnectionSelection();
                }

                // Remove the orphan connection
                RemoveConnection(connection, sourceElement);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks all connections for orphans and removes them.
        /// Called after element deletion to clean up dangling connections.
        /// </summary>
        public void CheckAllConnectionsForOrphans()
        {
            // Iterate all elements and check their OutgoingArrows
            foreach (var sourceElement in CurrentSteps.ToList())
            {
                var connectionsToCheck = sourceElement.OutgoingArrows.ToList();
                foreach (var connection in connectionsToCheck)
                {
                    CheckOrphanConnection(connection, sourceElement);
                }
            }
        }

        #endregion

        #region Port Display

        /// <summary>
        /// Shows connection ports on the specified element.
        /// </summary>
        /// <param name="element">The element to show ports on.</param>
        public void ShowConnectionPorts(DrawingElement element)
        {
            if (element == null) return;

            HoverElement = element;
            IsShowingPorts = true;
            EditModeState = EditModeState.ShowingPorts;

            ConnectionPortsShown?.Invoke(this, new ConnectionPortsEventArgs(element));
        }

        /// <summary>
        /// Hides the currently displayed connection ports.
        /// </summary>
        public void HideConnectionPorts()
        {
            if (!IsShowingPorts) return;

            IsShowingPorts = false;
            HoverElement = null;

            // Reset edit mode if we were showing ports
            if (EditModeState == EditModeState.ShowingPorts)
            {
                EditModeState = EditModeState.None;
            }

            ConnectionPortsHidden?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Connection Mode

        /// <summary>
        /// Starts arrow connection mode from the specified element and port.
        /// </summary>
        /// <param name="sourceElement">The source element.</param>
        /// <param name="sourcePort">The port to connect from.</param>
        public void StartConnection(DrawingElement sourceElement, PortPosition sourcePort)
        {
            if (sourceElement == null) return;

            // Store source element reference
            _connectionSourceElement = sourceElement;

            // Create connection context
            _connectionContext = ConnectionRoutingEngine.CreateConnectionContext(sourceElement, sourcePort);

            // Clear snap state
            _snapTargetElementId = null;
            _snapTargetPort = null;
            _snapTargetPosition = null;
            _snapTargetBounds = null;
            _currentRoutePoints.Clear();

            // Enter connection mode
            EditModeState = EditModeState.ConnectingArrow;
            
            // Hide the port adorner - set all state before firing event
            IsShowingPorts = false;
            HoverElement = null;

            // Notify view to remove the port adorner
            ConnectionPortsHidden?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Updates the connection preview based on the current mouse position.
        /// </summary>
        /// <param name="mousePosition">The current mouse position in canvas coordinates.</param>
        public void UpdateConnection(Point mousePosition)
        {
            if (!IsConnecting) return;

            // Check for snap targets
            var snapTarget = ConnectionRoutingEngine.FindSnapTarget(
                mousePosition,
                CurrentSteps,
                _connectionContext.SourceElementId,
                ConnectionRoutingEngine.SnapDistance);

            ConnectionRouteResult result;

            if (snapTarget.HasValue)
            {
                // Snap to target port
                _snapTargetElementId = snapTarget.Value.ElementId;
                _snapTargetPort = snapTarget.Value.Port;
                _snapTargetPosition = snapTarget.Value.Position;
                _snapTargetBounds = snapTarget.Value.Bounds;

                result = ConnectionRoutingEngine.CalculateOrthogonalRouteToTarget(
                    _connectionContext,
                    snapTarget.Value.ElementId,
                    snapTarget.Value.Port,
                    snapTarget.Value.Bounds);
            }
            else
            {
                // No snap - route to mouse position
                _snapTargetElementId = null;
                _snapTargetPort = null;
                _snapTargetPosition = null;
                _snapTargetBounds = null;

                result = ConnectionRoutingEngine.CalculateOrthogonalRoute(
                    _connectionContext,
                    mousePosition);
            }

            // Update route points
            _currentRoutePoints = new List<Point>(result.RoutePoints);

            // Notify view to update preview
            ConnectionRouteUpdated?.Invoke(this, new ConnectionRouteEventArgs(
                _currentRoutePoints,
                result.IsSnapped,
                _snapTargetPosition));
        }

        /// <summary>
        /// Completes the connection if snapped to a target.
        /// Called when mouse is released while snapped to a valid target port.
        /// </summary>
        public void CompleteConnection()
        {
            if (!IsConnecting || _connectionSourceElement == null) return;

            if (_snapTargetElementId != null && _snapTargetPort.HasValue)
            {
                // Create the connection (adjacency list pattern)
                var connection = new UMLConnection2
                {
                    Label = string.Empty,
                    TargetId = _snapTargetElementId,
                    TailPort = _connectionContext.SourcePort,
                    HeadPort = _snapTargetPort.Value
                };

                // Check for duplicate connections
                if (!IsDuplicateConnection(_connectionSourceElement, connection))
                {
                    // Add to source element's OutgoingArrows
                    _connectionSourceElement.OutgoingArrows.Add(connection);

                    // Update target element's IncomingSourceIds
                    var targetElement = CurrentSteps.FirstOrDefault(e => e.Id == _snapTargetElementId);
                    targetElement?.IncomingSourceIds.Add(_connectionSourceElement.Id);

                    // Notify success
                    ConnectionCreated?.Invoke(this, new ConnectionCreatedEventArgs(connection, _connectionSourceElement, true));
                }
                else
                {
                    // Duplicate connection - notify with failure
                    ConnectionCreated?.Invoke(this, new ConnectionCreatedEventArgs(null, null, false));
                }
            }
            else
            {
                // No valid target - notify failure
                ConnectionCancelled?.Invoke(this, EventArgs.Empty);
            }

            // Exit connection mode
            ExitConnectionMode();
        }

        /// <summary>
        /// Cancels the current connection operation.
        /// Called on ESC key or when mouse is released without snapping to a target.
        /// </summary>
        public void CancelConnection()
        {
            if (!IsConnecting) return;

            // Notify cancellation
            ConnectionCancelled?.Invoke(this, EventArgs.Empty);

            // Exit connection mode
            ExitConnectionMode();
        }

        /// <summary>
        /// Exits connection mode and resets state.
        /// </summary>
        private void ExitConnectionMode()
        {
            _connectionSourceElement = null;
            _connectionContext = default;
            _snapTargetElementId = null;
            _snapTargetPort = null;
            _snapTargetPosition = null;
            _snapTargetBounds = null;
            _currentRoutePoints.Clear();

            EditModeState = EditModeState.None;
        }

        /// <summary>
        /// Checks if a connection would be a duplicate of an existing one.
        /// </summary>
        private bool IsDuplicateConnection(DrawingElement sourceElement, UMLConnection2 newConnection)
        {
            return sourceElement.OutgoingArrows.Any(c => c.TargetId == newConnection.TargetId);
        }

        #endregion

        #region Data Management

        /// <summary>
        /// Removes a connection from the source element's OutgoingArrows.
        /// </summary>
        public void RemoveConnection(UMLConnection2 connection, DrawingElement sourceElement)
        {
            if (connection == null || sourceElement == null) return;

            // Remove from source element's OutgoingArrows
            sourceElement.OutgoingArrows.Remove(connection);

            // Remove source from target's IncomingSourceIds
            var targetElement = CurrentSteps.FirstOrDefault(e => e.Id == connection.TargetId);
            targetElement?.IncomingSourceIds.Remove(sourceElement.Id);
        }

        /// <summary>
        /// Deletes the currently selected connection.
        /// </summary>
        public void DeleteSelectedConnection()
        {
            if (_selectedConnection == null || _selectedConnectionSource == null) return;

            var connection = _selectedConnection;
            var source = _selectedConnectionSource;

            // Clear selection first
            ClearConnectionSelection();

            // Remove the connection
            RemoveConnection(connection, source);
        }

        /// <summary>
        /// Clears connections on state change (called as part of LoadWorkflowForCurrentState).
        /// </summary>
        private void ClearConnectionsOnStateChange()
        {
            // Cancel any ongoing connection operation
            if (IsConnecting)
            {
                CancelConnection();
            }

            // Hide ports if showing
            if (IsShowingPorts)
            {
                HideConnectionPorts();
            }

            // Clear connection selection
            if (IsConnectionSelected)
            {
                ClearConnectionSelection();
            }
        }

        /// <summary>
        /// Gets all connections in the current workflow by iterating all elements' OutgoingArrows.
        /// </summary>
        /// <returns>Enumerable of (Connection, SourceElement) pairs.</returns>
        public IEnumerable<(UMLConnection2 Connection, DrawingElement SourceElement)> GetAllConnections()
        {
            foreach (var sourceElement in CurrentSteps)
            {
                foreach (var connection in sourceElement.OutgoingArrows)
                {
                    yield return (connection, sourceElement);
                }
            }
        }

        #endregion
    }

    #region Event Args

    /// <summary>
    /// Event arguments for connection ports shown event.
    /// </summary>
    public class ConnectionPortsEventArgs : EventArgs
    {
        public DrawingElement Element { get; }

        public ConnectionPortsEventArgs(DrawingElement element)
        {
            Element = element;
        }
    }

    /// <summary>
    /// Event arguments for connection route updated event.
    /// </summary>
    public class ConnectionRouteEventArgs : EventArgs
    {
        public IReadOnlyList<Point> RoutePoints { get; }
        public bool IsSnapped { get; }
        public Point? SnapPosition { get; }

        public ConnectionRouteEventArgs(IReadOnlyList<Point> routePoints, bool isSnapped, Point? snapPosition)
        {
            RoutePoints = routePoints;
            IsSnapped = isSnapped;
            SnapPosition = snapPosition;
        }
    }

    /// <summary>
    /// Event arguments for connection created event.
    /// </summary>
    public class ConnectionCreatedEventArgs : EventArgs
    {
        public UMLConnection2 Connection { get; }
        public DrawingElement SourceElement { get; }
        public bool Success { get; }

        public ConnectionCreatedEventArgs(UMLConnection2 connection, DrawingElement sourceElement, bool success)
        {
            Connection = connection;
            SourceElement = sourceElement;
            Success = success;
        }
    }

    /// <summary>
    /// Event arguments for connection selected event.
    /// </summary>
    public class ConnectionSelectedEventArgs : EventArgs
    {
        public UMLConnection2 Connection { get; }
        public DrawingElement SourceElement { get; }

        public ConnectionSelectedEventArgs(UMLConnection2 connection, DrawingElement sourceElement)
        {
            Connection = connection;
            SourceElement = sourceElement;
        }
    }

    /// <summary>
    /// Event arguments for connection edit route updated event.
    /// </summary>
    public class ConnectionEditRouteEventArgs : EventArgs
    {
        public IReadOnlyList<Point> RoutePoints { get; }
        public bool IsSnapped { get; }
        public Point HeadPosition { get; }
        public Point TailPosition { get; }
        public bool IsEditingHead { get; }

        public ConnectionEditRouteEventArgs(
            IReadOnlyList<Point> routePoints,
            bool isSnapped,
            Point headPosition,
            Point tailPosition,
            bool isEditingHead)
        {
            RoutePoints = routePoints;
            IsSnapped = isSnapped;
            HeadPosition = headPosition;
            TailPosition = tailPosition;
            IsEditingHead = isEditingHead;
        }
    }

    #endregion
}