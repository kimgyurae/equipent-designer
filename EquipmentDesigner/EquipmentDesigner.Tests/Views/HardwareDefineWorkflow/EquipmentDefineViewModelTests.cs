using System.Collections.ObjectModel;
using EquipmentDesigner.Models;
using EquipmentDesigner.ViewModels;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Views.HardwareDefineWorkflow
{
    public class EquipmentDefineViewModelTests
    {
        #region Property Initialization

        [Fact]
        public void Constructor_Default_InitializesWithEmptyName()
        {
            var viewModel = new EquipmentDefineViewModel();
            viewModel.Name.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyEquipmentType()
        {
            var viewModel = new EquipmentDefineViewModel();
            viewModel.EquipmentType.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyDisplayName()
        {
            var viewModel = new EquipmentDefineViewModel();
            viewModel.DisplayName.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyDescription()
        {
            var viewModel = new EquipmentDefineViewModel();
            viewModel.Description.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyCustomer()
        {
            var viewModel = new EquipmentDefineViewModel();
            viewModel.Customer.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyProcess()
        {
            var viewModel = new EquipmentDefineViewModel();
            viewModel.Process.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyAttachedDocuments()
        {
            var viewModel = new EquipmentDefineViewModel();
            viewModel.AttachedDocuments.Should().NotBeNull().And.BeEmpty();
        }

        #endregion

        #region Property Change Notification

        [Fact]
        public void Name_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new EquipmentDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(EquipmentDefineViewModel.Name))
                    raised = true;
            };

            viewModel.Name = "TestEquipment";
            raised.Should().BeTrue();
        }

        [Fact]
        public void EquipmentType_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new EquipmentDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(EquipmentDefineViewModel.EquipmentType))
                    raised = true;
            };

            viewModel.EquipmentType = "TypeA";
            raised.Should().BeTrue();
        }

        [Fact]
        public void DisplayName_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new EquipmentDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(EquipmentDefineViewModel.DisplayName))
                    raised = true;
            };

            viewModel.DisplayName = "Display Name";
            raised.Should().BeTrue();
        }

        [Fact]
        public void Description_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new EquipmentDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(EquipmentDefineViewModel.Description))
                    raised = true;
            };

            viewModel.Description = "Test description";
            raised.Should().BeTrue();
        }

        [Fact]
        public void Customer_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new EquipmentDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(EquipmentDefineViewModel.Customer))
                    raised = true;
            };

            viewModel.Customer = "Customer A";
            raised.Should().BeTrue();
        }

        [Fact]
        public void Process_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new EquipmentDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(EquipmentDefineViewModel.Process))
                    raised = true;
            };

            viewModel.Process = "Process A";
            raised.Should().BeTrue();
        }

        [Fact]
        public void Name_WhenChanged_RaisesPropertyChangedForCanProceedToNext()
        {
            var viewModel = new EquipmentDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(EquipmentDefineViewModel.CanProceedToNext))
                    raised = true;
            };

            viewModel.Name = "TestEquipment";
            raised.Should().BeTrue();
        }

        #endregion

        #region Validation

        [Fact]
        public void CanProceedToNext_WhenNameIsNull_ReturnsFalse()
        {
            var viewModel = new EquipmentDefineViewModel { Name = null };
            viewModel.CanProceedToNext.Should().BeFalse();
        }

        [Fact]
        public void CanProceedToNext_WhenNameIsEmptyString_ReturnsFalse()
        {
            var viewModel = new EquipmentDefineViewModel { Name = "" };
            viewModel.CanProceedToNext.Should().BeFalse();
        }

        [Fact]
        public void CanProceedToNext_WhenNameIsWhitespaceOnly_ReturnsFalse()
        {
            var viewModel = new EquipmentDefineViewModel { Name = "   " };
            viewModel.CanProceedToNext.Should().BeFalse();
        }

        [Fact]
        public void CanProceedToNext_WhenNameHasValidValue_ReturnsTrue()
        {
            var viewModel = new EquipmentDefineViewModel { Name = "ValidName" };
            viewModel.CanProceedToNext.Should().BeTrue();
        }

        #endregion

        #region Commands

        [Fact]
        public void LoadFromServerCommand_CanExecute_ReturnsTrue()
        {
            var viewModel = new EquipmentDefineViewModel();
            viewModel.LoadFromServerCommand.CanExecute(null).Should().BeTrue();
        }

        #endregion

        #region HardwareDefinition Conversion

        [Fact]
        public void ToHardwareDefinition_SetsHardwareTypeToEquipment()
        {
            var viewModel = new EquipmentDefineViewModel { Name = "Test" };

            var result = viewModel.ToHardwareDefinition();

            result.HardwareType.Should().Be(HardwareType.Equipment);
        }

        [Fact]
        public void ToHardwareDefinition_MapsNamePropertyCorrectly()
        {
            var viewModel = new EquipmentDefineViewModel { Name = "TestEquipment" };

            var result = viewModel.ToHardwareDefinition();

            result.Name.Should().Be("TestEquipment");
        }

        [Fact]
        public void ToHardwareDefinition_MapsDisplayNamePropertyCorrectly()
        {
            var viewModel = new EquipmentDefineViewModel
            {
                Name = "Test",
                DisplayName = "Test Display Name"
            };

            var result = viewModel.ToHardwareDefinition();

            result.DisplayName.Should().Be("Test Display Name");
        }

        [Fact]
        public void ToHardwareDefinition_MapsDescriptionPropertyCorrectly()
        {
            var viewModel = new EquipmentDefineViewModel
            {
                Name = "Test",
                Description = "Test Description"
            };

            var result = viewModel.ToHardwareDefinition();

            result.Description.Should().Be("Test Description");
        }

        [Fact]
        public void ToHardwareDefinition_MapsEquipmentTypePropertyCorrectly()
        {
            var viewModel = new EquipmentDefineViewModel
            {
                Name = "Test",
                EquipmentType = "Fab Equipment"
            };

            var result = viewModel.ToHardwareDefinition();

            result.EquipmentType.Should().Be("Fab Equipment");
        }

        [Fact]
        public void ToHardwareDefinition_MapsCustomerPropertyCorrectly()
        {
            var viewModel = new EquipmentDefineViewModel
            {
                Name = "Test",
                Customer = "ACME Corp"
            };

            var result = viewModel.ToHardwareDefinition();

            result.Customer.Should().Be("ACME Corp");
        }

        [Fact]
        public void ToHardwareDefinition_MapsProcessToProcessInfoCorrectly()
        {
            var viewModel = new EquipmentDefineViewModel
            {
                Name = "Test",
                Process = "Manufacturing Process"
            };

            var result = viewModel.ToHardwareDefinition();

            result.ProcessInfo.Should().Be("Manufacturing Process");
        }

        [Fact]
        public void ToHardwareDefinition_MapsAttachedDocumentsToAttachedDocumentsIdsCorrectly()
        {
            var viewModel = new EquipmentDefineViewModel { Name = "Test" };
            viewModel.AttachedDocuments.Add("doc1.pdf");
            viewModel.AttachedDocuments.Add("doc2.md");

            var result = viewModel.ToHardwareDefinition();

            result.AttachedDocumentsIds.Should().HaveCount(2);
            result.AttachedDocumentsIds.Should().Contain("doc1.pdf");
            result.AttachedDocumentsIds.Should().Contain("doc2.md");
        }

        [Fact]
        public void ToHardwareDefinition_MapsVersionPropertyCorrectly()
        {
            var viewModel = new EquipmentDefineViewModel
            {
                Name = "Test",
                Version = "2.0.0"
            };

            var result = viewModel.ToHardwareDefinition();

            result.Version.Should().Be("2.0.0");
        }

        [Fact]
        public void ToHardwareDefinition_MapsHardwareKeyPropertyCorrectly()
        {
            var viewModel = new EquipmentDefineViewModel
            {
                Name = "Test",
                HardwareKey = "HW-KEY-123"
            };

            var result = viewModel.ToHardwareDefinition();

            result.HardwareKey.Should().Be("HW-KEY-123");
        }

        [Fact]
        public void ToHardwareDefinition_ReturnsEmptyAttachedDocumentsIds_WhenCollectionIsEmpty()
        {
            var viewModel = new EquipmentDefineViewModel { Name = "Test" };

            var result = viewModel.ToHardwareDefinition();

            result.AttachedDocumentsIds.Should().NotBeNull();
            result.AttachedDocumentsIds.Should().BeEmpty();
        }

        [Fact]
        public void ToHardwareDefinition_HandlesNullStringProperties_ByMappingEmptyStrings()
        {
            var viewModel = new EquipmentDefineViewModel { Name = "Test" };

            var result = viewModel.ToHardwareDefinition();

            result.DisplayName.Should().Be(string.Empty);
            result.Description.Should().Be(string.Empty);
            result.EquipmentType.Should().Be(string.Empty);
            result.Customer.Should().Be(string.Empty);
            result.ProcessInfo.Should().Be(string.Empty);
        }

        [Fact]
        public void FromHardwareDefinition_CreatesViewModelWithCorrectName()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.Equipment,
                Name = "TestEquipment"
            };

            var viewModel = EquipmentDefineViewModel.FromHardwareDefinition(hw);

            viewModel.Name.Should().Be("TestEquipment");
        }

        [Fact]
        public void FromHardwareDefinition_CreatesViewModelWithCorrectDisplayName()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.Equipment,
                Name = "Test",
                DisplayName = "Test Display"
            };

            var viewModel = EquipmentDefineViewModel.FromHardwareDefinition(hw);

            viewModel.DisplayName.Should().Be("Test Display");
        }

        [Fact]
        public void FromHardwareDefinition_CreatesViewModelWithCorrectDescription()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.Equipment,
                Name = "Test",
                Description = "Test Description"
            };

            var viewModel = EquipmentDefineViewModel.FromHardwareDefinition(hw);

            viewModel.Description.Should().Be("Test Description");
        }

        [Fact]
        public void FromHardwareDefinition_CreatesViewModelWithCorrectEquipmentType()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.Equipment,
                Name = "Test",
                EquipmentType = "Assembly"
            };

            var viewModel = EquipmentDefineViewModel.FromHardwareDefinition(hw);

            viewModel.EquipmentType.Should().Be("Assembly");
        }

        [Fact]
        public void FromHardwareDefinition_CreatesViewModelWithCorrectCustomer()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.Equipment,
                Name = "Test",
                Customer = "Client Corp"
            };

            var viewModel = EquipmentDefineViewModel.FromHardwareDefinition(hw);

            viewModel.Customer.Should().Be("Client Corp");
        }

        [Fact]
        public void FromHardwareDefinition_MapsProcessInfoToProcessCorrectly()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.Equipment,
                Name = "Test",
                ProcessInfo = "Some Process"
            };

            var viewModel = EquipmentDefineViewModel.FromHardwareDefinition(hw);

            viewModel.Process.Should().Be("Some Process");
        }

        [Fact]
        public void FromHardwareDefinition_PopulatesAttachedDocumentsFromAttachedDocumentsIds()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.Equipment,
                Name = "Test",
                AttachedDocumentsIds = new System.Collections.Generic.List<string> { "file1.pdf", "file2.ppt" }
            };

            var viewModel = EquipmentDefineViewModel.FromHardwareDefinition(hw);

            viewModel.AttachedDocuments.Should().HaveCount(2);
            viewModel.AttachedDocuments.Should().Contain("file1.pdf");
            viewModel.AttachedDocuments.Should().Contain("file2.ppt");
        }

        [Fact]
        public void FromHardwareDefinition_CreatesViewModelWithCorrectVersion()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.Equipment,
                Name = "Test",
                Version = "3.0.0"
            };

            var viewModel = EquipmentDefineViewModel.FromHardwareDefinition(hw);

            viewModel.Version.Should().Be("3.0.0");
        }

        [Fact]
        public void FromHardwareDefinition_CreatesViewModelWithCorrectHardwareKey()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.Equipment,
                Name = "Test",
                HardwareKey = "KEY-456"
            };

            var viewModel = EquipmentDefineViewModel.FromHardwareDefinition(hw);

            viewModel.HardwareKey.Should().Be("KEY-456");
        }

        [Fact]
        public void FromHardwareDefinition_ThrowsArgumentNullException_WhenHardwareDefinitionIsNull()
        {
            System.Action act = () => EquipmentDefineViewModel.FromHardwareDefinition(null);

            act.Should().Throw<System.ArgumentNullException>()
               .WithParameterName("hw");
        }

        [Fact]
        public void FromHardwareDefinition_HandlesNullProperties_WithEmptyStringDefaults()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.Equipment,
                Name = null,
                DisplayName = null,
                Description = null,
                EquipmentType = null,
                Customer = null,
                ProcessInfo = null
            };

            var viewModel = EquipmentDefineViewModel.FromHardwareDefinition(hw);

            viewModel.Name.Should().Be(string.Empty);
            viewModel.DisplayName.Should().Be(string.Empty);
            viewModel.Description.Should().Be(string.Empty);
            viewModel.EquipmentType.Should().Be(string.Empty);
            viewModel.Customer.Should().Be(string.Empty);
            viewModel.Process.Should().Be(string.Empty);
        }

        [Fact]
        public void FromHardwareDefinition_HandlesNullAttachedDocumentsIds_WithEmptyCollection()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.Equipment,
                Name = "Test",
                AttachedDocumentsIds = null
            };

            var viewModel = EquipmentDefineViewModel.FromHardwareDefinition(hw);

            viewModel.AttachedDocuments.Should().NotBeNull();
            viewModel.AttachedDocuments.Should().BeEmpty();
        }

        [Fact]
        public void RoundTripConversion_PreservesAllEquipmentProperties()
        {
            var original = new EquipmentDefineViewModel
            {
                EquipmentType = "Fabrication",
                Name = "MainEquipment",
                DisplayName = "Main Equipment Display",
                Description = "Full Description",
                Customer = "Big Customer",
                Process = "Critical Process",
                Version = "1.2.3",
                HardwareKey = "UNIQUE-KEY"
            };
            original.AttachedDocuments.Add("spec.pdf");
            original.AttachedDocuments.Add("design.drawio");

            var hw = original.ToHardwareDefinition();
            var restored = EquipmentDefineViewModel.FromHardwareDefinition(hw);

            restored.EquipmentType.Should().Be(original.EquipmentType);
            restored.Name.Should().Be(original.Name);
            restored.DisplayName.Should().Be(original.DisplayName);
            restored.Description.Should().Be(original.Description);
            restored.Customer.Should().Be(original.Customer);
            restored.Process.Should().Be(original.Process);
            restored.Version.Should().Be(original.Version);
            restored.HardwareKey.Should().Be(original.HardwareKey);
            restored.AttachedDocuments.Should().BeEquivalentTo(original.AttachedDocuments);
        }

        #endregion
    }
}