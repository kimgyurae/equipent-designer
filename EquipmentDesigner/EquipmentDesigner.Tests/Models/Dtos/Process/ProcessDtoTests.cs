using System.Collections.Generic;
using System.Linq;
using EquipmentDesigner.Models;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Process
{
    /// <summary>
    /// Process DTO classes TDD tests
    /// </summary>
    public class ProcessDtoTests
    {
        #region PackMlState Enum Tests

        [Fact]
        public void PackMlState_ShouldContain_Exactly17States()
        {
            // Arrange & Act
            var stateCount = System.Enum.GetValues(typeof(PackMlState)).Length;

            // Assert
            stateCount.Should().Be(17);
        }

        [Theory]
        [InlineData(PackMlState.Idle)]
        [InlineData(PackMlState.Starting)]
        [InlineData(PackMlState.Execute)]
        [InlineData(PackMlState.Suspending)]
        [InlineData(PackMlState.Suspended)]
        [InlineData(PackMlState.Unsuspending)]
        [InlineData(PackMlState.Holding)]
        [InlineData(PackMlState.Held)]
        [InlineData(PackMlState.Unholding)]
        [InlineData(PackMlState.Aborting)]
        [InlineData(PackMlState.Aborted)]
        [InlineData(PackMlState.Clearing)]
        [InlineData(PackMlState.Resetting)]
        [InlineData(PackMlState.Stopping)]
        [InlineData(PackMlState.Stopped)]
        [InlineData(PackMlState.Completing)]
        [InlineData(PackMlState.Complete)]
        public void PackMlState_ShouldContain_AllPackMlStandardStates(PackMlState state)
        {
            // Assert - if this compiles and runs, the state exists
            state.Should().BeDefined();
        }

        #endregion

        #region ProcessNodeType Enum Tests

        [Fact]
        public void ProcessNodeType_ShouldContain_Exactly5Types()
        {
            // Arrange & Act
            var typeCount = System.Enum.GetValues(typeof(UMLNodeType)).Length;

            // Assert
            typeCount.Should().Be(5);
        }

        [Theory]
        [InlineData(UMLNodeType.Initial)]
        [InlineData(UMLNodeType.Terminal)]
        [InlineData(UMLNodeType.Action)]
        [InlineData(UMLNodeType.Decision)]
        [InlineData(UMLNodeType.PredefinedAction)]
        public void ProcessNodeType_ShouldContain_AllNodeTypes(UMLNodeType nodeType)
        {
            // Assert
            nodeType.Should().BeDefined();
        }

        #endregion

        #region ProcessConnectionDto Tests

        [Fact]
        public void ProcessConnectionDto_ShouldStore_TargetNodeId()
        {
            // Arrange
            var connection = new ProcessConnectionDto { TargetNodeId = "node-001" };

            // Assert
            connection.TargetNodeId.Should().Be("node-001");
        }

        [Fact]
        public void ProcessConnectionDto_ShouldStore_Label()
        {
            // Arrange
            var connection = new ProcessConnectionDto { Label = "Success" };

            // Assert
            connection.Label.Should().Be("Success");
        }

        #endregion

        #region ProcessNodeBase Abstract Class Tests

        [Fact]
        public void ProcessNodeBase_ShouldBeAbstract_CannotBeInstantiatedDirectly()
        {
            // Assert - ProcessNodeBase should be abstract
            typeof(ProcessNodeBase).IsAbstract.Should().BeTrue();
        }

        [Fact]
        public void ProcessNodeBase_ShouldHave_IdProperty()
        {
            // Arrange - using a concrete implementation
            var node = new InitialNodeDto { Id = "init-001" };

            // Assert
            node.Id.Should().Be("init-001");
        }

        [Fact]
        public void ProcessNodeBase_ShouldHave_NameProperty()
        {
            // Arrange
            var node = new InitialNodeDto { Label = "Start Node" };

            // Assert
            node.Label.Should().Be("Start Node");
        }

        [Fact]
        public void ProcessNodeBase_ShouldHave_DescriptionProperty()
        {
            // Arrange
            var node = new InitialNodeDto { Description = "Entry point" };

            // Assert
            node.Description.Should().Be("Entry point");
        }

        #endregion

        #region InitialNodeDto Tests

        [Fact]
        public void InitialNodeDto_ShouldInheritFrom_ProcessNodeBase()
        {
            // Assert
            typeof(InitialNodeDto).IsSubclassOf(typeof(ProcessNodeBase)).Should().BeTrue();
        }

        [Fact]
        public void InitialNodeDto_NodeType_ShouldReturn_Initial()
        {
            // Arrange
            var node = new InitialNodeDto();

            // Assert
            node.NodeType.Should().Be(UMLNodeType.Initial);
        }

        [Fact]
        public void InitialNodeDto_OutgoingConnections_ShouldBeInitializedAsEmptyList()
        {
            // Arrange
            var node = new InitialNodeDto();

            // Assert
            node.OutgoingConnections.Should().NotBeNull();
            node.OutgoingConnections.Should().BeEmpty();
        }

        [Fact]
        public void InitialNodeDto_OutgoingConnections_ShouldSupportMultipleConnections()
        {
            // Arrange
            var node = new InitialNodeDto();
            node.OutgoingConnections.Add(new ProcessConnectionDto { TargetNodeId = "node-1" });
            node.OutgoingConnections.Add(new ProcessConnectionDto { TargetNodeId = "node-2" });

            // Assert
            node.OutgoingConnections.Should().HaveCount(2);
        }

        #endregion

        #region TerminalNodeDto Tests

        [Fact]
        public void TerminalNodeDto_ShouldInheritFrom_ProcessNodeBase()
        {
            // Assert
            typeof(TerminalNodeDto).IsSubclassOf(typeof(ProcessNodeBase)).Should().BeTrue();
        }

        [Fact]
        public void TerminalNodeDto_NodeType_ShouldReturn_Terminal()
        {
            // Arrange
            var node = new TerminalNodeDto();

            // Assert
            node.NodeType.Should().Be(UMLNodeType.Terminal);
        }

        [Fact]
        public void TerminalNodeDto_ShouldNotHave_OutgoingConnectionProperty()
        {
            // Assert - TerminalNodeDto should not have OutgoingConnection or OutgoingConnections property
            var properties = typeof(TerminalNodeDto).GetProperties();
            properties.Should().NotContain(p => p.Name.Contains("Outgoing"));
        }

        #endregion

        #region ActionNodeDto Tests

        [Fact]
        public void ActionNodeDto_ShouldInheritFrom_ProcessNodeBase()
        {
            // Assert
            typeof(ActionNodeDto).IsSubclassOf(typeof(ProcessNodeBase)).Should().BeTrue();
        }

        [Fact]
        public void ActionNodeDto_NodeType_ShouldReturn_Action()
        {
            // Arrange
            var node = new ActionNodeDto();

            // Assert
            node.NodeType.Should().Be(UMLNodeType.Action);
        }

        [Fact]
        public void ActionNodeDto_OutgoingConnection_ShouldBeSingleConnection()
        {
            // Arrange
            var node = new ActionNodeDto
            {
                OutgoingConnection = new ProcessConnectionDto { TargetNodeId = "next-node" }
            };

            // Assert
            node.OutgoingConnection.Should().NotBeNull();
            node.OutgoingConnection.TargetNodeId.Should().Be("next-node");
        }

        #endregion

        #region DecisionNodeDto Tests

        [Fact]
        public void DecisionNodeDto_ShouldInheritFrom_ProcessNodeBase()
        {
            // Assert
            typeof(DecisionNodeDto).IsSubclassOf(typeof(ProcessNodeBase)).Should().BeTrue();
        }

        [Fact]
        public void DecisionNodeDto_NodeType_ShouldReturn_Decision()
        {
            // Arrange
            var node = new DecisionNodeDto();

            // Assert
            node.NodeType.Should().Be(UMLNodeType.Decision);
        }

        [Fact]
        public void DecisionNodeDto_ShouldHave_ConditionProperty()
        {
            // Arrange
            var node = new DecisionNodeDto { Condition = "IsValid == true" };

            // Assert
            node.Condition.Should().Be("IsValid == true");
        }

        [Fact]
        public void DecisionNodeDto_ShouldHave_TrueBranchProperty()
        {
            // Arrange
            var node = new DecisionNodeDto
            {
                TrueBranch = new ProcessConnectionDto { TargetNodeId = "true-node", Label = "Yes" }
            };

            // Assert
            node.TrueBranch.Should().NotBeNull();
            node.TrueBranch.TargetNodeId.Should().Be("true-node");
        }

        [Fact]
        public void DecisionNodeDto_ShouldHave_FalseBranchProperty()
        {
            // Arrange
            var node = new DecisionNodeDto
            {
                FalseBranch = new ProcessConnectionDto { TargetNodeId = "false-node", Label = "No" }
            };

            // Assert
            node.FalseBranch.Should().NotBeNull();
            node.FalseBranch.TargetNodeId.Should().Be("false-node");
        }

        #endregion

        #region PredefinedProcessNodeDto Tests

        [Fact]
        public void PredefinedProcessNodeDto_ShouldInheritFrom_ProcessNodeBase()
        {
            // Assert
            typeof(PredefinedProcessNodeDto).IsSubclassOf(typeof(ProcessNodeBase)).Should().BeTrue();
        }

        [Fact]
        public void PredefinedProcessNodeDto_NodeType_ShouldReturn_PredefinedProcess()
        {
            // Arrange
            var node = new PredefinedProcessNodeDto();

            // Assert
            node.NodeType.Should().Be(UMLNodeType.PredefinedAction);
        }

        [Fact]
        public void PredefinedProcessNodeDto_Nodes_ShouldBeInitializedAsEmptyList()
        {
            // Arrange
            var node = new PredefinedProcessNodeDto();

            // Assert
            node.Nodes.Should().NotBeNull();
            node.Nodes.Should().BeEmpty();
        }

        [Fact]
        public void PredefinedProcessNodeDto_ShouldHave_InitialNodeIdProperty()
        {
            // Arrange
            var node = new PredefinedProcessNodeDto { InitialNodeId = "init-001" };

            // Assert
            node.InitialNodeId.Should().Be("init-001");
        }

        [Fact]
        public void PredefinedProcessNodeDto_Nodes_ShouldSupportPolymorphicStorage()
        {
            // Arrange
            var node = new PredefinedProcessNodeDto();
            node.Nodes.Add(new InitialNodeDto { Id = "init-001" });
            node.Nodes.Add(new ActionNodeDto { Id = "action-001" });
            node.Nodes.Add(new DecisionNodeDto { Id = "decision-001" });
            node.Nodes.Add(new TerminalNodeDto { Id = "terminal-001" });

            // Assert
            node.Nodes.Should().HaveCount(4);
            node.Nodes.OfType<InitialNodeDto>().Should().HaveCount(1);
            node.Nodes.OfType<ActionNodeDto>().Should().HaveCount(1);
            node.Nodes.OfType<DecisionNodeDto>().Should().HaveCount(1);
            node.Nodes.OfType<TerminalNodeDto>().Should().HaveCount(1);
        }

        [Fact]
        public void PredefinedProcessNodeDto_Nodes_ShouldSupportNestedPredefinedProcess()
        {
            // Arrange - create a predefined process with a nested predefined process
            var nestedProcess = new PredefinedProcessNodeDto
            {
                Id = "nested-001",
                Label = "Nested Subprocess",
                InitialNodeId = "nested-init-001"
            };
            nestedProcess.Nodes.Add(new InitialNodeDto { Id = "nested-init-001" });
            nestedProcess.Nodes.Add(new TerminalNodeDto { Id = "nested-terminal-001" });

            var parentProcess = new PredefinedProcessNodeDto
            {
                Id = "parent-001",
                Label = "Parent Process",
                InitialNodeId = "parent-init-001"
            };
            parentProcess.Nodes.Add(new InitialNodeDto { Id = "parent-init-001" });
            parentProcess.Nodes.Add(nestedProcess);
            parentProcess.Nodes.Add(new TerminalNodeDto { Id = "parent-terminal-001" });

            // Assert
            parentProcess.Nodes.Should().HaveCount(3);
            parentProcess.Nodes.OfType<PredefinedProcessNodeDto>().Should().HaveCount(1);
            var nested = parentProcess.Nodes.OfType<PredefinedProcessNodeDto>().First();
            nested.Nodes.Should().HaveCount(2);
        }

        [Fact]
        public void PredefinedProcessNodeDto_OutgoingConnection_ShouldBeNullable()
        {
            // Arrange - without setting OutgoingConnection
            var nodeWithoutConnection = new PredefinedProcessNodeDto();

            // Arrange - with setting OutgoingConnection
            var nodeWithConnection = new PredefinedProcessNodeDto
            {
                OutgoingConnection = new ProcessConnectionDto { TargetNodeId = "next-node" }
            };

            // Assert
            nodeWithoutConnection.OutgoingConnection.Should().BeNull();
            nodeWithConnection.OutgoingConnection.Should().NotBeNull();
        }

        #endregion

        #region ProcessWorkflowDto Tests

        [Fact]
        public void ProcessWorkflowDto_ShouldHave_StateTypeProperty()
        {
            // Arrange
            var workflow = new ProcessWorkflowDto { StateType = PackMlState.Execute };

            // Assert
            workflow.StateType.Should().Be(PackMlState.Execute);
        }

        [Fact]
        public void ProcessWorkflowDto_Nodes_ShouldBeInitializedAsEmptyList()
        {
            // Arrange
            var workflow = new ProcessWorkflowDto();

            // Assert
            workflow.Nodes.Should().NotBeNull();
            workflow.Nodes.Should().BeEmpty();
        }

        [Fact]
        public void ProcessWorkflowDto_ShouldHave_InitialNodeIdProperty()
        {
            // Arrange
            var workflow = new ProcessWorkflowDto { InitialNodeId = "init-001" };

            // Assert
            workflow.InitialNodeId.Should().Be("init-001");
        }

        [Fact]
        public void ProcessWorkflowDto_Nodes_ShouldSupportPolymorphicStorage()
        {
            // Arrange
            var workflow = new ProcessWorkflowDto();
            workflow.Nodes.Add(new InitialNodeDto { Id = "init-001" });
            workflow.Nodes.Add(new ActionNodeDto { Id = "action-001" });
            workflow.Nodes.Add(new DecisionNodeDto { Id = "decision-001" });
            workflow.Nodes.Add(new TerminalNodeDto { Id = "terminal-001" });
            workflow.Nodes.Add(new PredefinedProcessNodeDto { Id = "predefined-001" });

            // Assert
            workflow.Nodes.Should().HaveCount(5);
            workflow.Nodes.OfType<InitialNodeDto>().Should().HaveCount(1);
            workflow.Nodes.OfType<ActionNodeDto>().Should().HaveCount(1);
            workflow.Nodes.OfType<DecisionNodeDto>().Should().HaveCount(1);
            workflow.Nodes.OfType<TerminalNodeDto>().Should().HaveCount(1);
            workflow.Nodes.OfType<PredefinedProcessNodeDto>().Should().HaveCount(1);
        }

        #endregion

        #region StateProcessMapDto Tests

        [Fact]
        public void StateProcessMapDto_Processes_ShouldBeInitializedAsEmptyDictionary()
        {
            // Arrange
            var map = new StateProcessMapDto();

            // Assert
            map.Processes.Should().NotBeNull();
            map.Processes.Should().BeEmpty();
        }

        [Fact]
        public void StateProcessMapDto_ShouldSupportAll17PackMlStates()
        {
            // Arrange
            var map = new StateProcessMapDto();
            var allStates = System.Enum.GetValues(typeof(PackMlState)).Cast<PackMlState>();

            // Act - add workflow for each state
            foreach (var state in allStates)
            {
                map.Processes[state] = new ProcessWorkflowDto { StateType = state };
            }

            // Assert
            map.Processes.Should().HaveCount(17);
            foreach (var state in allStates)
            {
                map.Processes.Should().ContainKey(state);
                map.Processes[state].StateType.Should().Be(state);
            }
        }

        #endregion

        #region Polymorphism Tests

        [Fact]
        public void AllNodeTypes_ShouldBeStorableIn_ProcessNodeBaseList()
        {
            // Arrange
            var nodes = new List<ProcessNodeBase>
            {
                new InitialNodeDto { Id = "1", Label = "Start" },
                new TerminalNodeDto { Id = "2", Label = "End" },
                new ActionNodeDto { Id = "3", Label = "Process" },
                new DecisionNodeDto { Id = "4", Label = "Branch" },
                new PredefinedProcessNodeDto { Id = "5", Label = "Subprocess" }
            };

            // Assert
            nodes.Should().HaveCount(5);
            nodes.Should().AllBeAssignableTo<ProcessNodeBase>();
        }

        [Fact]
        public void NodeTypes_ShouldBeIdentifiableUsing_PatternMatching()
        {
            // Arrange
            ProcessNodeBase node = new DecisionNodeDto { Id = "dec-001", Condition = "x > 0" };

            // Act & Assert
            if (node is DecisionNodeDto decision)
            {
                decision.Condition.Should().Be("x > 0");
            }
            else
            {
                Assert.True(false, "Pattern matching failed");
            }
        }

        [Fact]
        public void CastingFromBase_ToSpecificType_ShouldPreserveAllProperties()
        {
            // Arrange
            ProcessNodeBase baseNode = new ActionNodeDto
            {
                Id = "action-001",
                Label = "Test Action",
                Description = "Test Description",
                OutgoingConnection = new ProcessConnectionDto { TargetNodeId = "next" }
            };

            // Act
            var actionNode = baseNode as ActionNodeDto;

            // Assert
            actionNode.Should().NotBeNull();
            actionNode.Id.Should().Be("action-001");
            actionNode.Label.Should().Be("Test Action");
            actionNode.Description.Should().Be("Test Description");
            actionNode.OutgoingConnection.TargetNodeId.Should().Be("next");
        }

        #endregion
    }
}