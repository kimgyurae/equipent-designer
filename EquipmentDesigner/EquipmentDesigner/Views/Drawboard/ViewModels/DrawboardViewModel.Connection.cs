using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private List<Point> _currentRoutePoints = new List<Point>();

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
        /// Collection of connections for the current workflow (for binding).
        /// </summary>
        public ObservableCollection<UMLConnection> Connections { get; } = new ObservableCollection<UMLConnection>();

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
            if (!IsMultiSelectionMode && SelectedElement != null)
            {
                ShowConnectionPorts(SelectedElement);
            }
            else
            {
                HideConnectionPorts();
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
                Elements,
                _connectionContext.SourceElementId,
                ConnectionRoutingEngine.SnapDistance);

            ConnectionRouteResult result;

            if (snapTarget.HasValue)
            {
                // Snap to target port
                _snapTargetElementId = snapTarget.Value.ElementId;
                _snapTargetPort = snapTarget.Value.Port;
                _snapTargetPosition = snapTarget.Value.Position;

                result = ConnectionRoutingEngine.CalculateOrthogonalRouteToTarget(
                    _connectionContext,
                    snapTarget.Value.ElementId,
                    snapTarget.Value.Port,
                    snapTarget.Value.Position);
            }
            else
            {
                // No snap - route to mouse position
                _snapTargetElementId = null;
                _snapTargetPort = null;
                _snapTargetPosition = null;

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
        /// Called on Enter key or double-click.
        /// </summary>
        public void CompleteConnection()
        {
            if (!IsConnecting) return;

            if (_snapTargetElementId != null && _snapTargetPort.HasValue)
            {
                // Create the connection
                var connection = new UMLConnection
                {
                    Label = string.Empty,
                    TailId = _connectionContext.SourceElementId,
                    TailPort = _connectionContext.SourcePort,
                    HeadId = _snapTargetElementId,
                    HeadPort = _snapTargetPort.Value
                };

                // Check for duplicate connections
                if (!IsDuplicateConnection(connection))
                {
                    // Add to workflow
                    AddConnectionToCurrentWorkflow(connection);

                    // Add to observable collection for UI binding
                    Connections.Add(connection);

                    // Notify success
                    ConnectionCreated?.Invoke(this, new ConnectionCreatedEventArgs(connection, true));
                }
                else
                {
                    // Duplicate connection - notify with failure
                    ConnectionCreated?.Invoke(this, new ConnectionCreatedEventArgs(null, false));
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
        /// Called on ESC key or click on empty space.
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
            _currentRoutePoints.Clear();

            EditModeState = EditModeState.None;
        }

        /// <summary>
        /// Checks if a connection would be a duplicate of an existing one.
        /// </summary>
        private bool IsDuplicateConnection(UMLConnection newConnection)
        {
            return Connections.Any(c =>
                c.TailId == newConnection.TailId &&
                c.HeadId == newConnection.HeadId);
        }

        #endregion

        #region Data Management

        /// <summary>
        /// Adds a connection to the current workflow in Process.
        /// </summary>
        private void AddConnectionToCurrentWorkflow(UMLConnection connection)
        {
            if (_process == null) return;

            var workflow = GetOrCreateWorkflowForCurrentState();
            workflow.Connections ??= new List<UMLConnection>();
            workflow.Connections.Add(connection);
        }

        /// <summary>
        /// Removes a connection from the current workflow.
        /// </summary>
        public void RemoveConnection(UMLConnection connection)
        {
            if (connection == null) return;

            // Remove from observable collection
            Connections.Remove(connection);

            // Remove from workflow
            if (_process?.Processes != null &&
                _process.Processes.TryGetValue(SelectedState, out var workflow) &&
                workflow?.Connections != null)
            {
                workflow.Connections.Remove(connection);
            }
        }

        /// <summary>
        /// Loads connections for the current state into the Connections collection.
        /// </summary>
        private void LoadConnectionsForCurrentState()
        {
            Connections.Clear();

            if (_process?.Processes == null) return;

            if (_process.Processes.TryGetValue(SelectedState, out var workflow) &&
                workflow?.Connections != null)
            {
                foreach (var connection in workflow.Connections)
                {
                    Connections.Add(connection);
                }
            }
        }

        /// <summary>
        /// Clears all connections when selection changes or workflow changes.
        /// Called as part of LoadWorkflowForCurrentState.
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
        public UMLConnection Connection { get; }
        public bool Success { get; }

        public ConnectionCreatedEventArgs(UMLConnection connection, bool success)
        {
            Connection = connection;
            Success = success;
        }
    }

    #endregion
}