using System.Collections.Generic;
using System.Linq;
using EquipmentDesigner.Models;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Process
{
    /// <summary>
    /// Tests for ProcessNode validation logic
    /// </summary>
    public class NodeValidationTests
    {
        #region NodeValidationResult Tests

        [Fact]
        public void NodeValidationResult_IsValid_ShouldBeTrue_WhenNoErrors()
        {
            // Arrange
            var result = NodeValidationResult.Valid("node-001", UMLNodeType.Action);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void NodeValidationResult_IsValid_ShouldBeFalse_WhenHasErrors()
        {
            // Arrange
            var result = NodeValidationResult.Invalid("node-001", UMLNodeType.Action, "Error 1", "Error 2");

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(2);
        }

        [Fact]
        public void NodeValidationResult_AddError_ShouldAppendError()
        {
            // Arrange
            var result = NodeValidationResult.Valid("node-001", UMLNodeType.Action);

            // Act
            result.AddError("New error");

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("New error");
        }

        #endregion

        #region InitialNodeDto Validation Tests

        [Fact]
        public void InitialNodeDto_Validate_ShouldFail_WhenNoOutgoingConnections()
        {
            // Arrange
            var node = new InitialNodeDto { Id = "init-001" };

            // Act
            var result = node.Validate();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Initial node must have at least 1 outgoing connection.");
        }

        [Fact]
        public void InitialNodeDto_Validate_ShouldFail_WhenOutgoingConnectionHasNoTarget()
        {
            // Arrange
            var node = new InitialNodeDto { Id = "init-001" };
            node.OutgoingConnections.Add(new ProcessConnectionDto { TargetNodeId = null });

            // Act
            var result = node.Validate();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Outgoing connection must have a valid TargetNodeId.");
        }

        [Fact]
        public void InitialNodeDto_Validate_ShouldPass_WhenHasValidOutgoingConnection()
        {
            // Arrange
            var node = new InitialNodeDto { Id = "init-001" };
            node.OutgoingConnections.Add(new ProcessConnectionDto { TargetNodeId = "action-001" });

            // Act
            var result = node.Validate();

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void InitialNodeDto_Validate_ShouldPass_WhenHasMultipleValidOutgoingConnections()
        {
            // Arrange
            var node = new InitialNodeDto { Id = "init-001" };
            node.OutgoingConnections.Add(new ProcessConnectionDto { TargetNodeId = "action-001" });
            node.OutgoingConnections.Add(new ProcessConnectionDto { TargetNodeId = "action-002" });

            // Act
            var result = node.Validate();

            // Assert
            result.IsValid.Should().BeTrue();
        }

        #endregion

        #region TerminalNodeDto Validation Tests

        [Fact]
        public void TerminalNodeDto_Validate_ShouldAlwaysPass()
        {
            // Arrange
            var node = new TerminalNodeDto { Id = "terminal-001" };

            // Act
            var result = node.Validate();

            // Assert
            result.IsValid.Should().BeTrue();
            result.NodeType.Should().Be(UMLNodeType.Terminal);
        }

        #endregion

        #region ActionNodeDto Validation Tests

        [Fact]
        public void ActionNodeDto_Validate_ShouldFail_WhenNoOutgoingConnection()
        {
            // Arrange
            var node = new ActionNodeDto { Id = "action-001" };

            // Act
            var result = node.Validate();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Action node must have exactly 1 outgoing connection.");
        }

        [Fact]
        public void ActionNodeDto_Validate_ShouldFail_WhenOutgoingConnectionHasNoTarget()
        {
            // Arrange
            var node = new ActionNodeDto
            {
                Id = "action-001",
                OutgoingConnection = new ProcessConnectionDto { TargetNodeId = "" }
            };

            // Act
            var result = node.Validate();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Outgoing connection must have a valid TargetNodeId.");
        }

        [Fact]
        public void ActionNodeDto_Validate_ShouldPass_WhenHasValidOutgoingConnection()
        {
            // Arrange
            var node = new ActionNodeDto
            {
                Id = "action-001",
                OutgoingConnection = new ProcessConnectionDto { TargetNodeId = "terminal-001" }
            };

            // Act
            var result = node.Validate();

            // Assert
            result.IsValid.Should().BeTrue();
        }

        #endregion

        #region DecisionNodeDto Validation Tests

        [Fact]
        public void DecisionNodeDto_Validate_ShouldFail_WhenNoCondition()
        {
            // Arrange
            var node = new DecisionNodeDto
            {
                Id = "decision-001",
                TrueBranch = new ProcessConnectionDto { TargetNodeId = "action-001" },
                FalseBranch = new ProcessConnectionDto { TargetNodeId = "action-002" }
            };

            // Act
            var result = node.Validate();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Decision node must have a condition expression.");
        }

        [Fact]
        public void DecisionNodeDto_Validate_ShouldFail_WhenNoTrueBranch()
        {
            // Arrange
            var node = new DecisionNodeDto
            {
                Id = "decision-001",
                Condition = "x > 0",
                FalseBranch = new ProcessConnectionDto { TargetNodeId = "action-002" }
            };

            // Act
            var result = node.Validate();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Decision node must have a TrueBranch connection.");
        }

        [Fact]
        public void DecisionNodeDto_Validate_ShouldFail_WhenNoFalseBranch()
        {
            // Arrange
            var node = new DecisionNodeDto
            {
                Id = "decision-001",
                Condition = "x > 0",
                TrueBranch = new ProcessConnectionDto { TargetNodeId = "action-001" }
            };

            // Act
            var result = node.Validate();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Decision node must have a FalseBranch connection.");
        }

        [Fact]
        public void DecisionNodeDto_Validate_ShouldFail_WhenBranchHasNoTarget()
        {
            // Arrange
            var node = new DecisionNodeDto
            {
                Id = "decision-001",
                Condition = "x > 0",
                TrueBranch = new ProcessConnectionDto { TargetNodeId = "" },
                FalseBranch = new ProcessConnectionDto { TargetNodeId = "action-002" }
            };

            // Act
            var result = node.Validate();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("TrueBranch connection must have a valid TargetNodeId.");
        }

        [Fact]
        public void DecisionNodeDto_Validate_ShouldPass_WhenFullyConfigured()
        {
            // Arrange
            var node = new DecisionNodeDto
            {
                Id = "decision-001",
                Condition = "x > 0",
                TrueBranch = new ProcessConnectionDto { TargetNodeId = "action-001" },
                FalseBranch = new ProcessConnectionDto { TargetNodeId = "action-002" }
            };

            // Act
            var result = node.Validate();

            // Assert
            result.IsValid.Should().BeTrue();
        }

        #endregion

        #region PredefinedProcessNodeDto Validation Tests

        [Fact]
        public void PredefinedProcessNodeDto_Validate_ShouldFail_WhenNoNodes()
        {
            // Arrange
            var node = new PredefinedProcessNodeDto { Id = "predefined-001" };

            // Act
            var result = node.Validate();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("PredefinedProcess must contain at least one node.");
        }

        [Fact]
        public void PredefinedProcessNodeDto_Validate_ShouldFail_WhenNoInitialNode()
        {
            // Arrange
            var node = new PredefinedProcessNodeDto
            {
                Id = "predefined-001",
                InitialNodeId = "init-001"
            };
            node.Nodes.Add(new ActionNodeDto
            {
                Id = "action-001",
                OutgoingConnection = new ProcessConnectionDto { TargetNodeId = "terminal-001" }
            });
            node.Nodes.Add(new TerminalNodeDto { Id = "terminal-001" });

            // Act
            var result = node.Validate();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("PredefinedProcess must have at least one Initial node.");
        }

        [Fact]
        public void PredefinedProcessNodeDto_Validate_ShouldFail_WhenNoTerminalNode()
        {
            // Arrange
            var node = new PredefinedProcessNodeDto
            {
                Id = "predefined-001",
                InitialNodeId = "init-001"
            };
            node.Nodes.Add(new InitialNodeDto
            {
                Id = "init-001",
                OutgoingConnections = new List<ProcessConnectionDto>
                {
                    new ProcessConnectionDto { TargetNodeId = "action-001" }
                }
            });
            node.Nodes.Add(new ActionNodeDto
            {
                Id = "action-001",
                OutgoingConnection = new ProcessConnectionDto { TargetNodeId = "terminal-001" }
            });

            // Act
            var result = node.Validate();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("PredefinedProcess must have at least one Terminal node.");
        }

        [Fact]
        public void PredefinedProcessNodeDto_Validate_ShouldFail_WhenNoIntermediateNode()
        {
            // Arrange
            var node = new PredefinedProcessNodeDto
            {
                Id = "predefined-001",
                InitialNodeId = "init-001"
            };
            node.Nodes.Add(new InitialNodeDto
            {
                Id = "init-001",
                OutgoingConnections = new List<ProcessConnectionDto>
                {
                    new ProcessConnectionDto { TargetNodeId = "terminal-001" }
                }
            });
            node.Nodes.Add(new TerminalNodeDto { Id = "terminal-001" });

            // Act
            var result = node.Validate();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("PredefinedProcess must have at least one Action, Decision, or nested PredefinedProcess node.");
        }

        [Fact]
        public void PredefinedProcessNodeDto_Validate_ShouldFail_WhenNoInitialNodeId()
        {
            // Arrange
            var node = CreateValidPredefinedProcess();
            node.InitialNodeId = null;

            // Act
            var result = node.Validate();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("PredefinedProcess must have InitialNodeId set.");
        }

        [Fact]
        public void PredefinedProcessNodeDto_Validate_ShouldFail_WhenInitialNodeIdReferencesInvalidNode()
        {
            // Arrange
            var node = CreateValidPredefinedProcess();
            node.InitialNodeId = "non-existent-node";

            // Act
            var result = node.Validate();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("InitialNodeId 'non-existent-node' does not reference a valid Initial node.");
        }

        [Fact]
        public void PredefinedProcessNodeDto_Validate_ShouldPass_WhenFullyConfigured()
        {
            // Arrange
            var node = CreateValidPredefinedProcess();

            // Act
            var result = node.Validate();

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void PredefinedProcessNodeDto_Validate_ShouldPass_WhenHasDecisionNode()
        {
            // Arrange
            var node = new PredefinedProcessNodeDto
            {
                Id = "predefined-001",
                InitialNodeId = "init-001"
            };
            node.Nodes.Add(new InitialNodeDto
            {
                Id = "init-001",
                OutgoingConnections = new List<ProcessConnectionDto>
                {
                    new ProcessConnectionDto { TargetNodeId = "decision-001" }
                }
            });
            node.Nodes.Add(new DecisionNodeDto
            {
                Id = "decision-001",
                Condition = "x > 0",
                TrueBranch = new ProcessConnectionDto { TargetNodeId = "terminal-001" },
                FalseBranch = new ProcessConnectionDto { TargetNodeId = "terminal-002" }
            });
            node.Nodes.Add(new TerminalNodeDto { Id = "terminal-001" });
            node.Nodes.Add(new TerminalNodeDto { Id = "terminal-002" });

            // Act
            var result = node.Validate();

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void PredefinedProcessNodeDto_Validate_ShouldPass_WhenHasNestedPredefinedProcess()
        {
            // Arrange
            var nestedProcess = CreateValidPredefinedProcess();
            nestedProcess.Id = "nested-001";
            nestedProcess.OutgoingConnection = new ProcessConnectionDto { TargetNodeId = "terminal-001" };

            var node = new PredefinedProcessNodeDto
            {
                Id = "predefined-001",
                InitialNodeId = "init-001"
            };
            node.Nodes.Add(new InitialNodeDto
            {
                Id = "init-001",
                OutgoingConnections = new List<ProcessConnectionDto>
                {
                    new ProcessConnectionDto { TargetNodeId = "nested-001" }
                }
            });
            node.Nodes.Add(nestedProcess);
            node.Nodes.Add(new TerminalNodeDto { Id = "terminal-001" });

            // Act
            var result = node.Validate();

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void PredefinedProcessNodeDto_Validate_ShouldReportChildNodeErrors()
        {
            // Arrange
            var node = new PredefinedProcessNodeDto
            {
                Id = "predefined-001",
                InitialNodeId = "init-001"
            };
            node.Nodes.Add(new InitialNodeDto { Id = "init-001" }); // Missing OutgoingConnections
            node.Nodes.Add(new ActionNodeDto { Id = "action-001" }); // Missing OutgoingConnection
            node.Nodes.Add(new TerminalNodeDto { Id = "terminal-001" });

            // Act
            var result = node.Validate();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.Contains("[Initial:init-001]"));
            result.Errors.Should().Contain(e => e.Contains("[Action:action-001]"));
        }

        #endregion

        #region Helper Methods

        private PredefinedProcessNodeDto CreateValidPredefinedProcess()
        {
            var node = new PredefinedProcessNodeDto
            {
                Id = "predefined-001",
                InitialNodeId = "init-001"
            };
            node.Nodes.Add(new InitialNodeDto
            {
                Id = "init-001",
                OutgoingConnections = new List<ProcessConnectionDto>
                {
                    new ProcessConnectionDto { TargetNodeId = "action-001" }
                }
            });
            node.Nodes.Add(new ActionNodeDto
            {
                Id = "action-001",
                OutgoingConnection = new ProcessConnectionDto { TargetNodeId = "terminal-001" }
            });
            node.Nodes.Add(new TerminalNodeDto { Id = "terminal-001" });
            return node;
        }

        #endregion
    }
}
